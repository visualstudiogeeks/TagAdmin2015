using System.Runtime.InteropServices;
using Microsoft.Practices.Prism.PubSubEvents;

namespace TagAdmin.UI.Services
{
    [Guid("F0DACFE2-1E52-4BBD-9293-910AE2F1FAED")]
    [ComVisible(true)]
    public interface ITagAdminContext
    {
        EventAggregator EventAggregator { get; }

    }

    [Guid("E73A9318-163A-4FA8-BD29-481C8CB89E69")]
    public interface STagAdminContext
    {
    }
}
