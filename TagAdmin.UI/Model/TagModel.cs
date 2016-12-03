using System.Threading.Tasks;
using TagAdmin.Api;
using TagAdmin.Common.Entities;

namespace TagAdmin.UI.Model
{
    public sealed class TagModel
    {
        private static readonly TagModel _instance = new TagModel();
        private static TagService _tagService;

        private TagModel()
        {

        }

        public static TagModel GetInstance(TagService service)
        {
            _tagService = service;
            return _instance;
        }

        public async Task<TagServiceResponse<Tag>> UpdateTag(string oldTagName, string newTagName)
        {
            return await _tagService.RenameTag(oldTagName, newTagName);
        }
    }
}
