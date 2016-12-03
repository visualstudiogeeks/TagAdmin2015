using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.TeamFoundation.Client;
using Serilog;
using TagAdmin.Api;
using TagAdmin.Common.Entities;
using TagAdmin.Common.Extensions;
using TagAdmin.UI.Events;
using TagAdmin.UI.Services;
using TagAdmin.UI.ViewModel.Base;

namespace TagAdmin.UI.ViewModel
{
    public class AssociatedWorkitemViewModel : ViewModelBase
    {
        #region Private Fields

        private AsyncObservableCollection<AssociatedWorkItemDetail> _associatedWorkItems;
        private EventAggregator _eventAggregator;
        private bool _isWorkInProgress;
        private IServiceProvider _serviceProvider;
        private string _statusMessage;
        private ITagAdminContext _tagAdminContext;
        private TagService _tagService;
        private List<Tag> _tagsList;
        private ITeamFoundationContext _teamFoundationContext;
        private ILogger _log;

        #endregion Private Fields

        #region Public Constructors

        public AssociatedWorkitemViewModel(IServiceProvider serviceProvider, ITeamFoundationContext teamFoundationContext, ITagAdminContext tagAdminContext, List<Tag> tagsList)
        {
            _serviceProvider = serviceProvider;
            _teamFoundationContext = teamFoundationContext;
            _tagAdminContext = tagAdminContext;
            _tagsList = tagsList;
            _eventAggregator = _tagAdminContext.EventAggregator;

            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), string.Format("Tag Admin for Visual Studio 2015\\TagAdminForVisualStudio2015-{0}.log", DateTime.Now.ToString("dd-MM-yyyy")));
            _log = new LoggerConfiguration().WriteTo.File(logPath, fileSizeLimitBytes: 5000000)
                .CreateLogger();
            _log.Information("Initializing AssociatedWorkitemViewModel - ServiceProvider: {serviceprovider}, TFSContext: {@tfs}, TagsCount: {tagsCount}",
                _serviceProvider != null,
                 new
                 {
                     Collection = _teamFoundationContext.TeamProjectCollection.Name,
                     User = _teamFoundationContext.TeamProjectCollection.ConfigurationServer.AuthorizedIdentity.UniqueName,
                     Project = _teamFoundationContext.TeamProjectName,
                 }, tagsList.Count);

            Subscriptions();
            Triggers();
        }

        #endregion Public Constructors

        #region Public Properties

        public bool IsWorkInProgress
        {
            get
            {
                return _isWorkInProgress;
            }
            set
            {
                _isWorkInProgress = value;
                RaisePropertyChanged(() => IsWorkInProgress);
            }
        }

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                RaisePropertyChanged(() => StatusMessage);
            }
        }

        public AsyncObservableCollection<AssociatedWorkItemDetail> WorkitemDetails
        {
            get { return _associatedWorkItems; }
            set
            {
                _associatedWorkItems = value;
                RaisePropertyChanged(() => WorkitemDetails);
            }
        }

        #endregion Public Properties

        #region Private Methods

        private async void GetAssociatedWorkItems(List<Tag> tagsList)
        {
            try
            {
                ShowBusy(true);
                StatusMessage = "Getting associated workitem(s)...";

                _tagService = TagService.GetInstance(_teamFoundationContext.TeamProjectCollection.Uri);
                _tagService.Scope = new Guid(_teamFoundationContext.TeamProjectUri.Segments[3]);
                var tags = tagsList.Select(t => t.Name).ToList();

                var tagServiceResponse =
                    _tagService.GetAssociatedWorkItems(_teamFoundationContext.TeamProjectName, tags).Result;
                if (tagServiceResponse.Exception != null)
                {
                    _log.Error("Error getting Associated Workitems - TagsList: {@tags}, Exception: {@exception}", tagsList, tagServiceResponse.Exception);
                    ShowBusy(false);
                    //Notify error
                    _eventAggregator.GetEvent<NotifyErrorAssociatedWorkitemPage>().Publish(tagServiceResponse.Exception.Message);
                    return;
                }
                var workitems = tagServiceResponse.Data;
                StatusMessage = "Getting workitem details...";
                var ids = workitems.WorkItems.Select(w => w.Id).ToList();
                if (!ids.Any())
                {
                    StatusMessage = string.Format("No workitem(s) found with tags {0}", tags.CommaQuibblingWithAnd());
                    ShowBusy(false);
                    return;
                }
                var details = await _tagService.GetWorkItemDetailsBatch(ids);
                if (details.Exception != null)
                {
                    _log.Error("Error getting Workitem details - Work item ids: {@tags}, Exception: {@exception}", tagsList, details.Exception);
                    //Notify error
                    ShowBusy(false);
                    _eventAggregator.GetEvent<NotifyErrorAssociatedWorkitemPage>().Publish(details.Exception.Message);
                    return;
                }
                StatusMessage = string.Format("Following workitem(s) are associated with {0}", tags.CommaQuibblingWithAnd());
                WorkitemDetails = details.Data.AssociatedWorkItemDetails.ToAsyncObservableCollection();

                ShowBusy(false);
            }

            catch (Exception exception)
            {
                _log.Information("Error getting Associated Workitems - TFSContext: {@context}, TagsCount: {count} Exception: {@exception}",
                new
                {
                    Collection = _teamFoundationContext.TeamProjectCollection.Name,
                    User = _teamFoundationContext.TeamProjectCollection.ConfigurationServer.AuthorizedIdentity.UniqueName,
                    Project = _teamFoundationContext.TeamProjectName,
                }, tagsList.Count, exception);
                _eventAggregator.GetEvent<NotifyErrorAssociatedWorkitemPage>().Publish(exception.Message);
            }
        }

        private void OnGetAssociatedWorkItems(List<Tag> tags)
        {
            GetAssociatedWorkItems(tags);
        }

        private void ShowBusy(bool workInProgress)
        {
            _eventAggregator.GetEvent<ShowBusyAssociatedWorkitemPage>().Publish(workInProgress);
            IsWorkInProgress = workInProgress;
        }

        private void Subscriptions()
        {
            _eventAggregator.GetEvent<TriggerAssociatedWorkItems>().Subscribe(OnGetAssociatedWorkItems, ThreadOption.BackgroundThread);
        }

        private void Triggers()
        {
            _tagAdminContext.EventAggregator.GetEvent<TriggerAssociatedWorkItems>().Publish(_tagsList);
        }

        #endregion Private Methods
    }
}
