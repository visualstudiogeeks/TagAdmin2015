using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.TeamFoundation.Client;

namespace TagAdmin.UI.Events
{
    public class TriggerGetTags : PubSubEvent<ITeamFoundationContext>
    {
    }
}
