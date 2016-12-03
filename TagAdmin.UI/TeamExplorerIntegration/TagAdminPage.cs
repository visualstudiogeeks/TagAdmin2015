using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using TagAdmin.UI.Events;
using TagAdmin.UI.TeamExplorerIntegration.Base;
using TagAdmin.UI.Views;

namespace TagAdmin.UI.TeamExplorerIntegration
{
    [TeamExplorerPage(GuidList.TAG_ADMIN_PAGE_GUID)]
    public class TagAdminPage : TeamExplorerBasePage
    {
        public override void Initialize(object sender, PageInitializeEventArgs e)
        {
            Title = "Tag Admin";
            base.Initialize(sender, e);
            PageContent = new TagAdminPageContent(ServiceProvider, CurrentContext, TagAdminContext);
            TagAdminContext.EventAggregator.GetEvent<ShowBusyTagAdminPage>().Subscribe(OnBusy, ThreadOption.UIThread);
            TagAdminContext.EventAggregator.GetEvent<NotifyErrorTagAdminPage>().Subscribe(OnNotifyError, ThreadOption.UIThread);
            TagAdminContext.EventAggregator.GetEvent<NotifyInfoTagAdminPage>().Subscribe(OnNotifyInfo, ThreadOption.UIThread);
        }

        private void OnBusy(bool isBusy)
        {
            IsBusy = isBusy;
        }

        public override void Refresh()
        {
            if (IsNotificationShown(GuidList.Notifications.NotifcationGuidTagAdminPage))
            {
                ClearNotification(GuidList.Notifications.NotifcationGuidTagAdminPage);
            }

            base.Refresh();
            TagAdminContext.EventAggregator.GetEvent<TriggerGetTags>().Publish(CurrentContext);

        }

        private void OnNotifyInfo(string message)
        {
            ShowNotification(GuidList.Notifications.NotifcationGuidTagAdminPage, message, NotificationType.Information);
        }

        private void OnNotifyError(string message)
        {
            ShowNotification(GuidList.Notifications.NotifcationGuidTagAdminPage, message, NotificationType.Error);
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            base.ContextChanged(sender, e);

            // If the team project collection or team project changed, refresh the data for this section
            if ((e.TeamProjectCollectionChanged || e.TeamProjectChanged) && !string.IsNullOrWhiteSpace(e.NewContext.TeamProjectName))
            {
                TagAdminContext.EventAggregator.GetEvent<TriggerGetTags>().Publish(CurrentContext);
            }
        }
    }
}
    