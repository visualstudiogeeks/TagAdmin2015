using System.Collections.Generic;
using Microsoft.Practices.Prism.PubSubEvents;
using TagAdmin.Common.Entities;

namespace TagAdmin.UI.Events
{
    public class TriggerAssociatedWorkItems : PubSubEvent<List<Tag>>
    {
    }
}
