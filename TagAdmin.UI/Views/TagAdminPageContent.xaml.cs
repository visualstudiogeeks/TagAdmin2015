using System;
using System.Windows.Controls;
using Microsoft.TeamFoundation.Client;
using TagAdmin.UI.Services;
using TagAdmin.UI.ViewModel;

namespace TagAdmin.UI.Views
{
    /// <summary>
    /// Interaction logic for TagAdminPageContent.xaml
    /// </summary>
    public partial class TagAdminPageContent : UserControl
    {
        public TagAdminPageContent(IServiceProvider serviceProvider, ITeamFoundationContext currentContext, ITagAdminContext tagAdminContext)
        {
            InitializeComponent();
            DataContext = new TagAdminPageViewModel(serviceProvider, currentContext, tagAdminContext);
        }
    }
}
