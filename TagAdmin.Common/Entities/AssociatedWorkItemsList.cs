using System;
using System.Collections.Generic;

namespace TagAdmin.Common.Entities
{
    public class AssociatedWorkItemsList
    {
        public List<AssociatedWorkItem> WorkItems { get; set; }
        public DateTime AsOf { get; set; }
    }
}
