using System;
using System.Collections.Generic;

namespace TagAdmin.Common.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Url { get; set; }
        public bool IsChecked { get; set; }
        public List<AssociatedWorkItem> AssociatedWorkitems { get; set; }
        public DateTime AssociatedWorkitemsAsOf { get; set; }

        public string DisplayName { get; set; }
    }
}