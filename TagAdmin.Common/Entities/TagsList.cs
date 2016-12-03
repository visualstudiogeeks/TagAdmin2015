using System.Collections.Generic;
using Newtonsoft.Json;

namespace TagAdmin.Common.Entities
{
    public class TagsList
    {
        public int Count { get; set; }

        [JsonProperty("value")]
        public List<Tag> Tags { get; set; }
    }
}