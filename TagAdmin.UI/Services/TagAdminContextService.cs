using System;
using Microsoft.Practices.Prism.PubSubEvents;

namespace TagAdmin.UI.Services
{
    public class TagAdminContextService : ITagAdminContext, STagAdminContext
    {
        private readonly IServiceProvider _serviceProvider;
        private EventAggregator _eventAggregator;

        public TagAdminContextService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public EventAggregator EventAggregator
        {
            get
            {
                if (_eventAggregator == null)
                {
                    _eventAggregator = new EventAggregator();
                }
                return _eventAggregator;
            }
        }
    }
}
