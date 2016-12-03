using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using TagAdmin.UI.Events;
using TagAdmin.UI.TeamExplorerIntegration.Base;
using TagAdmin.UI.Views;

namespace TagAdmin.UI.TeamExplorerIntegration
{
    [TeamExplorerPage(GuidList.ASSOCIATED_WORKITEM_PAGE_GUID)]
    public class AssociatedWorkitemPage : TeamExplorerBasePage
    {
        public override void Initialize(object sender, PageInitializeEventArgs e)
        {
            Title = "Associated Workitem(s)";

            base.Initialize(sender, e);

            PageContent = new AssociatedWorkitemsPageContent(ServiceProvider, CurrentContext, TagAdminContext, e.Context);

            TagAdminContext.EventAggregator.GetEvent<ShowBusyAssociatedWorkitemPage>().Subscribe(OnBusy, ThreadOption.UIThread);
            TagAdminContext.EventAggregator.GetEvent<NotifyErrorAssociatedWorkitemPage>().Subscribe(OnNotifyError, ThreadOption.UIThread);
            TagAdminContext.EventAggregator.GetEvent<NotifyInfoAssociatedWorkitemPage>().Subscribe(OnNotifyInfo, ThreadOption.UIThread);
        }

        private void OnNotifyInfo(string message)
        {
            ShowNotification(GuidList.Notifications.NotifcationGuidAssociatedWorkitemPage, message, NotificationType.Information);
        }

        private void OnNotifyError(string message)
        {
            ShowNotification(GuidList.Notifications.NotifcationGuidAssociatedWorkitemPage, message, NotificationType.Error);
        }

        private void OnBusy(bool isBusy)
        {
            IsBusy = isBusy;
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            base.ContextChanged(sender, e);
            // If the team project collection or team project changed, refresh the data for this section
            if (e.TeamProjectCollectionChanged || e.TeamProjectChanged)
            {
                this.Close();
            }
        }
    }
}
