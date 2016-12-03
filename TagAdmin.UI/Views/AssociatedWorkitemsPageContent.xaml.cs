using System;
using System.Linq;
using System.Windows.Controls;
using Microsoft.TeamFoundation.Client;
using TagAdmin.Common.Entities;
using TagAdmin.Common.Extensions;
using TagAdmin.UI.Services;
using TagAdmin.UI.ViewModel;

namespace TagAdmin.UI.Views
{
    /// <summary>
    /// Interaction logic for AssociatedWorkitemsPageContent.xaml
    /// </summary>
    public partial class AssociatedWorkitemsPageContent : UserControl
    {
        public AssociatedWorkitemsPageContent(IServiceProvider serviceProvider, ITeamFoundationContext teamFoundationContext, ITagAdminContext tagAdminContext, object context)
        {
            InitializeComponent();
            var tagsList = context as AsyncObservableCollection<Tag>;
            DataContext = new AssociatedWorkitemViewModel(serviceProvider, teamFoundationContext, tagAdminContext, tagsList?.ToList());
        }
    }
}
