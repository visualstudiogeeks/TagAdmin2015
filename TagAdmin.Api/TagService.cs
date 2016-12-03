using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TagAdmin.Common.Entities;

namespace TagAdmin.Api
{
    public class TagService : VssHttpClientBase
    {
        private static Uri _baseUrl;
        private static TagService _tagService;
        private static readonly object _syncObject = new object();
        private static ILogger _log;
        private readonly VssCredentials _credentials;

        public string ApiVersion { get; set; }

        public Uri BaseUrl { get; set; }

        public bool RequiresAuthorizationHeaders { get; set; }
        public Guid Scope { get; set; }

        public string ProjectName { get; set; }

        private TagService(Uri baseUrl, VssCredentials credentials) : base(baseUrl, credentials)
        {
            _baseUrl = baseUrl;
            _credentials = credentials;

            RequiresAuthorizationHeaders = true;
            ApiVersion = "1.0";
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                $"Tag Admin for Visual Studio 2015\\TagAdminForVisualStudio2015-{DateTime.Now:dd-MM-yyyy}.log");
            _log = new LoggerConfiguration().WriteTo.File(logPath, fileSizeLimitBytes: 5000000).CreateLogger();
            _log.Information("Initializing TagService - BaseUrl: {baseUrl}", baseUrl);
        }

        private async Task<TaggingHttpClient> GetTagClient()
        {
            var connection = new VssConnection(_baseUrl, _credentials);
            return await connection.GetClientAsync<TaggingHttpClient>();
        }

        private async Task<WorkItemTrackingHttpClient> GetWITClient()
        {
            var connection = new VssConnection(_baseUrl, _credentials);
            return await connection.GetClientAsync<WorkItemTrackingHttpClient>();
        }

        public static TagService GetInstance(Uri baseUrl)
        {
            _baseUrl = baseUrl;

            // Create a default credentials object that opts in for the credentials storage to have SSO
            var credentials = new VssClientCredentials();
            credentials.Storage = new VssClientCredentialStorage();
            if (_tagService == null)
            {
                lock (_syncObject)
                {
                    _tagService = new TagService(baseUrl, credentials);
                }
            }
            return _tagService;
        }

        public async Task<TagServiceResponse<TagsList>> GetTags(bool includeInActive = false)
        {
            var response = new TagServiceResponse<TagsList>();
            try
            {
                var tagClient = await GetTagClient();
                var tags = await tagClient.GetTagsAsync(Scope);

                var tagsList = new TagsList();
                tagsList.Count = tags.Count;
                tagsList.Tags = tags.Select(x => new Tag
                {
                    Active = x.Active ?? false,
                    Name = x.Name,
                    Id = x.Id,
                    Url = x.Url
                }).ToList();

                response.Data = tagsList;
                response.IsSuccessStatusCode = true;
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error getting Tags");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse<Tag>> GetTagByName(string tagName)
        {
            var response = new TagServiceResponse<Tag>();
            try
            {
                var tagClient = await GetTagClient();
                var tagData = await tagClient.GetTagAsync(Scope, WebUtility.HtmlEncode(tagName));

                var tag = new Tag
                {
                    Active = tagData.Active ?? false,
                    Id = tagData.Id,
                    Url = tagData.Url,
                    Name = tagData.Name
                };

                response.Data = tag;
                response.IsSuccessStatusCode = true;
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error getting TagsByName");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse<Tag>> GetTagById(Guid tagId)
        {
            var response = new TagServiceResponse<Tag>();
            try
            {
                var tagClient = await GetTagClient();
                var tagData = await tagClient.GetTagAsync(Scope, tagId);

                var tag = new Tag
                {
                    Active = tagData.Active ?? false,
                    Id = tagData.Id,
                    Url = tagData.Url,
                    Name = tagData.Name
                };

                response.Data = tag;
                response.IsSuccessStatusCode = true;
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error getting TagsById");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse<Tag>> CreateTag(string tagName)
        {
            var response = new TagServiceResponse<Tag>();
            try
            {
                var tagClient = await GetTagClient();
                var tagData = await tagClient.CreateTagAsync(Scope, tagName);

                var tag = new Tag
                {
                    Active = tagData.Active ?? false,
                    Id = tagData.Id,
                    Url = tagData.Url,
                    Name = tagData.Name
                };

                response.Data = tag;
                response.IsSuccessStatusCode = true;
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error creating tag: {tagName}");
                response.Exception = exception;
            }
            return response;
        }

        public async Task<TagServiceResponse<Tag>> RenameTag(string oldTagName, string newTagName)
        {
            var response = new TagServiceResponse<Tag>();
            try
            {
                var oldTagResponse = await GetTagByName(oldTagName);
                var oldTag = oldTagResponse.Data;
                if (oldTag != null)
                {
                    var tagClient = await GetTagClient();
                    var tagData = await tagClient.UpdateTagAsync(Scope, oldTag.Id, newTagName, oldTag.Active);

                    var tag = new Tag
                    {
                        Active = tagData.Active ?? false,
                        Id = tagData.Id,
                        Url = tagData.Url,
                        Name = tagData.Name
                    };

                    response.Data = tag;
                    response.IsSuccessStatusCode = true;
                }
                else
                {
                    throw new TagNotFoundException($"{oldTagName} is not found.", response.Exception);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error renaming tag from '{oldTagName}' to '{newTagName}'");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse> DeleteTag(string tagName)
        {
            //TODO:
            //if tag is inactive - delete tag without prompting user, else
            //search whether any work item is associated for this tag
            //if yes, confirm with user whether to delete or make it inactive

            var response = new TagServiceResponse();
            try
            {
                var oldTagResponse = await GetTagByName(tagName);
                var oldTag = oldTagResponse.Data;
                if (oldTag != null)
                {
                    var tagClient = await GetTagClient();
                    await tagClient.DeleteTagAsync(Scope, oldTag.Id);

                    response.IsSuccessStatusCode = true;
                }
                else
                {
                    throw new TagNotFoundException($"{tagName} is not found.", response.Exception);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error deleting tag '{tagName}'");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse> DeleteTag(Guid tagId)
        {
            var response = new TagServiceResponse();
            try
            {
                var oldTagResponse = await GetTagById(tagId);
                var oldTag = oldTagResponse.Data;
                if (oldTag != null)
                {
                    var tagClient = await GetTagClient();
                    await tagClient.DeleteTagAsync(Scope, oldTag.Id);

                    response.IsSuccessStatusCode = true;
                }
                else
                {
                    throw new TagNotFoundException($"Tag with ID '{tagId}' is not found.", response.Exception);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error deleting tag with id '{tagId}'");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse<AssociatedWorkItemsList>> GetAssociatedWorkItems(string projectName, List<string> tags)
        {
            var response = new TagServiceResponse<AssociatedWorkItemsList>();
            try
            {
                var witClient = await GetWITClient();

                var concat = new StringBuilder();
                tags.ToList().ForEach(x => concat.AppendFormat("AND [System.Tags] CONTAINS '{0}'", x));
                var queryString = $"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '{projectName}' {concat}";

                var query = new Wiql { Query = queryString };

                var queryResult = await witClient.QueryByWiqlAsync(query, projectName);

                var listOfWorkitems = queryResult.WorkItems.Select(x => new AssociatedWorkItem
                {
                    Id = x.Id,
                    Url = new Uri(x.Url)
                });
                response.Data = new AssociatedWorkItemsList
                {
                    WorkItems = listOfWorkitems.ToList()
                };
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error getting associated workitems for list of tags");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse<AssociatedWorkItemsList>> GetAssociatedWorkItems(string projectName, string tag)
        {
            var response = new TagServiceResponse<AssociatedWorkItemsList>();
            try
            {
                var witClient = await GetWITClient();
                var queryString = $"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '{projectName}' AND [System.Tags] CONTAINS '{tag}'";
                var query = new Wiql { Query = queryString };

                var queryResult = await witClient.QueryByWiqlAsync(query, projectName);

                var listOfWorkitems = queryResult.WorkItems.Select(x => new AssociatedWorkItem
                {
                    Id = x.Id,
                    Url = new Uri(x.Url)
                });
                response.Data = new AssociatedWorkItemsList
                {
                    WorkItems = listOfWorkitems.ToList()
                };
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error getting associated workitems for tag '{tag}'");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse<AssociatedWorkItemDetail>> GetWorkItemDetails(int workItemId)
        {
            var response = new TagServiceResponse<AssociatedWorkItemDetail>();
            try
            {
                var witClient = await GetWITClient();

                var queryResult = await witClient.GetWorkItemAsync(workItemId);
                var json = JsonConvert.SerializeObject(queryResult.Fields);
                var workitemFields = JsonConvert.DeserializeObject<WorkItemFields>(json);

                var workitem = new AssociatedWorkItemDetail
                {
                    Id = queryResult.Id ?? -1,
                    Revision = queryResult.Rev ?? -1,
                    WorkItemFields = workitemFields
                };

                response.Data = workitem;
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error getting workitem details for workitem id: '{workItemId}'");
                response.Exception = exception;
            }

            return response;
        }

        public async Task<TagServiceResponse<AssociatedWorkItemDetailList>> GetWorkItemDetailsBatch(List<int> workitemIds)
        {
            var response = new TagServiceResponse<AssociatedWorkItemDetailList>();
            try
            {
                var witClient = await GetWITClient();

                var queryResult = await witClient.GetWorkItemsAsync(workitemIds);

                var associatedWorkitemDetailList = new AssociatedWorkItemDetailList();
                associatedWorkitemDetailList.Count = queryResult.Count;
                associatedWorkitemDetailList.AssociatedWorkItemDetails = queryResult.Select(x => new AssociatedWorkItemDetail
                {
                    Id = x.Id ?? -1,
                    Revision = x.Rev ?? -1,
                    WorkItemFields = JsonConvert.DeserializeObject<WorkItemFields>(JsonConvert.SerializeObject(x.Fields))
                }).ToList();

                response.Data = associatedWorkitemDetailList;
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error getting workitem details list of workitems ids");
                response.Exception = exception;
            }

            return response;
        }
    }
}