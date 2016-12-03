using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagAdmin.Common.Entities
{
    public class AssociatedWorkItemDetailList
    {
        public int Count { get; set; }

        [JsonProperty("value")]
        public List<AssociatedWorkItemDetail> AssociatedWorkItemDetails { get; set; }
    }
}
