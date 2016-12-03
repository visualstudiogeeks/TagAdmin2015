using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Common;
using NUnit.Framework;

namespace TagAdmin.Api.Tests
{
    [TestFixture]
    public class TagServiceApiTests
    {
        private TagService _tagService;
        private string _tagForTest;

        [OneTimeSetUp]
        public void Init()
        {
            _tagService = TagService.GetInstance(new Uri("https://account.visualstudio.com"));
            _tagService.Scope = new Guid("team-project-guid"); 

            _tagService.ApiVersion = "1.0";

            _tagForTest = "TAG-TEST";
        }
        [Test, Order(2)]
        public async Task Get_Tags_Test()
        {
            var response = await _tagService.GetTags();
            Assert.IsTrue(response != null && response.Data != null && response.Exception == null);
        }

        [Test]
        public async Task Get_Tags_IncludeInactive_Test()
        {
            var response = await _tagService.GetTags(true);
            Assert.IsTrue(response != null && response.Data != null && response.Exception == null);
        }

        [Test]
        public async Task Get_Tag_By_Name_NotFoundException_Test()
        {
            var response = await _tagService.GetTagByName("Non Existent Tag");
            Assert.IsTrue(response != null && response.Exception is TagNotFoundException);
        }
        [Test]
        public async Task Get_Tag_By_Name_Test()
        {
            var allTagsResponse = await _tagService.GetTags();
            string tagToFind = string.Empty;
            if (allTagsResponse.Data != null && allTagsResponse.Data.Tags.Count > 0)
            {
                tagToFind = allTagsResponse.Data.Tags[0].Name;
            }
            var response = await _tagService.GetTagByName(tagToFind);
            Assert.IsTrue(response != null && response.Data != null && response.Data.Name == tagToFind);
        }

        [Test, Order(1)]
        public async Task CreateTag_Test()
        {
            var response = await _tagService.CreateTag(_tagForTest);
            Assert.AreEqual(_tagForTest, response.Data.Name);
        }
        [Test, Order(4)]
        public async Task RenameTag_Test()
        {
            var tagRenamed = $"{_tagForTest}-renamed";
            var renameResponse = await _tagService.RenameTag(_tagForTest, tagRenamed);
            Assert.AreEqual(renameResponse.Data.Name, tagRenamed);
        }

        [Test]
        public async Task DeleteTag_Test()
        {
            var tagToDelete = $"{_tagForTest}-renamed";
            var deactiveTagResponse = await _tagService.DeleteTag(tagToDelete);
            Assert.AreEqual(deactiveTagResponse.IsSuccessStatusCode, true);
        }

        [Test]
        public async Task GetAssociatedWorkItems_Test()
        {
            var response = await _tagService.GetAssociatedWorkItems("ALMUtilities","Tag1");
            Assert.IsTrue(response?.Data != null);
        }

        [Test]
        public async Task GetWorkitemsForTag_And_WorkitemsDetails_Test()
        {
            var response = await _tagService.GetAssociatedWorkItems("ALMUtilities","Tag1");
            if (response?.Data != null)
            {
                foreach (var associatedWorkItem in response.Data.WorkItems)
                {
                    var wit = await _tagService.GetWorkItemDetails(associatedWorkItem.Id);
                    Assert.AreEqual(wit.Data.Id, associatedWorkItem.Id);
                }
            }
        }

        [Test]
        public async Task GetWorkItemsByIdList_Test()
        {
            var response = await _tagService.GetAssociatedWorkItems("ALMUtilities", new List<string> { "Tag1", "Tag2" });
            if (response != null)
            {
                foreach (var associatedWorkItem in response.Data.WorkItems)
                {
                    var wit = await _tagService.GetWorkItemDetails(associatedWorkItem.Id);
                    Assert.AreEqual(wit.Data.Id, associatedWorkItem.Id);
                }
            }
        }

        [Test]
        public async Task GetWorkItemDetails_ByIdList_Test()
        {
            var response = await _tagService.GetWorkItemDetailsBatch(new List<int> { 102, 106 });
            Assert.IsTrue(response.Data != null);
        }

        [Test]
        public async Task GetWorkItemDetails_ById_Test()
        {
            var response = await _tagService.GetWorkItemDetails(102);
            Assert.IsTrue(response.Data != null);
        }
    }


}
