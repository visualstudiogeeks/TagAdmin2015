using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.Shell;
using TagAdmin.UI.TeamExplorerIntegration.Base;

namespace TagAdmin.UI.TeamExplorerIntegration
{
    [TeamExplorerNavigationItem(LINK_ID, 100)]
    public class TagAdminNavigationItem : TeamExplorerBaseNavigationItem
    {
        public const string LINK_ID = "7F902432-44E5-451D-9342-D72839663C21";
        private readonly IServiceProvider _serviceProvider;
        [ImportingConstructor]
        public TagAdminNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            IsVisible = true;
            Text = "Tag Admin";
            Image = Resources.Tag;
        }

        public override void Execute()
        {
            if (CurrentContext != null && CurrentContext.HasCollection && CurrentContext.HasTeamProject)
            {
                var teamExplorer = GetService<ITeamExplorer>();
                teamExplorer?.NavigateToPage(new Guid(GuidList.TAG_ADMIN_PAGE_GUID), null);
            }
            else
            {
                ShowNotification(Guid.Parse(GuidList.TAG_ADMIN_PAGE_GUID), "You are currently not connected to a Team Project hosted by Visual Studio Team Services or TFS. Please try after you are connected.", NotificationType.Error);
            }
            
        }

        public override void Invalidate()
        {
            IsVisible = true;
        }

        protected override void ContextChanged(object sender, ContextChangedEventArgs e)
        {
            base.ContextChanged(sender, e);
        }

    }
}