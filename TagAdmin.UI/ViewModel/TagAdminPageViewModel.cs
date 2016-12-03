using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using Microsoft.VisualStudio.PlatformUI;
using Ookii.Dialogs;
using Serilog;
using TagAdmin.Api;
using TagAdmin.Common.Entities;
using TagAdmin.Common.Extensions;
using TagAdmin.UI.Events;
using TagAdmin.UI.Services;
using TagAdmin.UI.ViewModel.Base;
using MessageBox = System.Windows.MessageBox;

namespace TagAdmin.UI.ViewModel
{
    public class TagAdminPageViewModel : ViewModelBase
    {

        #region Private Fields

        private DelegateCommand _clearSelectionCommand;
        private DelegateCommand _deleteTagCommand;
        private EventAggregator _eventAggregator;
        private bool _isWorkInProgress;
        private DelegateCommand _renameTagCommand;
        private AsyncObservableCollection<Tag> _selectedTags;
        private IServiceProvider _serviceProvider;
        private string _statusMessage;
        private ITagAdminContext _tagAdminContext;
        private bool _tagOperationsIsVisible;
        private AsyncObservableCollection<Tag> _tagsCollection;
        private TagService _tagService;
        private ITeamFoundationContext _teamFoundationContext;
        private DelegateCommand _toggleButtonClick;
        private DelegateCommand _viewWorkItemsTagCommand;
        private ILogger _log;

        #endregion Private Fields

        #region Public Constructors

        public TagAdminPageViewModel(IServiceProvider serviceProvider, ITeamFoundationContext teamFoundationContext, ITagAdminContext tagAdminContext)
        {
            _serviceProvider = serviceProvider;
            _teamFoundationContext = teamFoundationContext;
            _tagAdminContext = tagAdminContext;
            _eventAggregator = _tagAdminContext.EventAggregator;

            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                $"Tag Admin for Visual Studio 2015\\TagAdminForVisualStudio2015-{DateTime.Now:dd-MM-yyyy}.log");
            _log = new LoggerConfiguration().WriteTo.File(logPath, fileSizeLimitBytes: 5000000)
                .CreateLogger();
            _log.Information("Initializing TagAdminPageViewModel - ServiceProvider: {serviceprovider}, TFSContext: {@tfs}, TagAdminContext: {@tagAdmin}",
                _serviceProvider != null,
                 new
                 {
                     Collection = _teamFoundationContext.TeamProjectCollection.Name,
                     User = _teamFoundationContext.TeamProjectCollection.ConfigurationServer.AuthorizedIdentity.UniqueName,
                     Project = _teamFoundationContext.TeamProjectName,
                 }, _tagAdminContext.EventAggregator != null);

            ShowBusy(true);

            Subscriptions();

            Triggers();
        }

        #endregion Public Constructors

        #region Public Properties

        public DelegateCommand ClearSelectionCommand
        {
            get
            {
                return _clearSelectionCommand ?? (_clearSelectionCommand = new DelegateCommand(OnClearSelectionCommand));
            }
        }

        public DelegateCommand DeleteTagCommand
        {
            get
            {
                return _deleteTagCommand ?? (_deleteTagCommand = new DelegateCommand(OnDeleteTagCommand));
            }
        }

        public bool IsWorkInProgress
        {
            get
            {
                return _isWorkInProgress;
            }
            set
            {
                _isWorkInProgress = value;
                RaisePropertyChanged(() => IsWorkInProgress);
            }
        }

        public DelegateCommand RenameTagCommand
        {
            get
            {
                return _renameTagCommand ?? (_renameTagCommand = new DelegateCommand(OnRenameTagCommand));
            }
        }

        public AsyncObservableCollection<Tag> SelectedTags
        {
            get { return _selectedTags; }
            set
            {
                _selectedTags = value;
                RaisePropertyChanged(() => SelectedTags);
                RaisePropertyChanged(() => TagOperationsIsVisible);
            }
        }

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                RaisePropertyChanged(() => StatusMessage);
            }
        }

        public bool TagOperationsIsVisible
        {
            get
            {
                _tagOperationsIsVisible = SelectedTags != null && SelectedTags.Any();
                return _tagOperationsIsVisible;
            }
            set
            {
                _tagOperationsIsVisible = value;
                RaisePropertyChanged(() => TagOperationsIsVisible);
            }
        }

        public AsyncObservableCollection<Tag> TagsCollection
        {
            get { return _tagsCollection; }
            set
            {
                _tagsCollection = value;
                RaisePropertyChanged(() => TagsCollection);
            }
        }

        public DelegateCommand ToggleButtonClick
        {
            get
            {
                return _toggleButtonClick ?? (_toggleButtonClick = new DelegateCommand(OnToggleButtonClick));
            }
        }

        public DelegateCommand ViewWorkItemsTagCommand
        {
            get
            {
                return _viewWorkItemsTagCommand ?? (_viewWorkItemsTagCommand = new DelegateCommand(OnViewWorkitemCommand));
            }
        }

        #endregion Public Properties

        #region Private Methods

        private async void GetTagsFromService(ITeamFoundationContext context)
        {
            try
            {
                ShowBusy(true);

                StatusMessage = $"Getting tags in '{context.TeamProjectName}' project...";
                _tagService = TagService.GetInstance(context.TeamProjectCollection.Uri);
                _tagService.Scope = new Guid(context.TeamProjectUri.Segments[3]);
                var tagServiceResponse = await _tagService.GetTags();
                if (tagServiceResponse.Exception != null)
                {
                    _log.Error("Error getting tags response - Data: {@data}, Exception: {@exception}", tagServiceResponse.Data, tagServiceResponse.Exception);
                    //Notify error
                    ShowBusy(false);
                    _eventAggregator.GetEvent<NotifyErrorTagAdminPage>().Publish(tagServiceResponse.Exception.Message);
                    return;
                }
                var tagsList = tagServiceResponse.Data;
                Parallel.ForEach(tagsList.Tags, x => x.DisplayName = x.Name);
                TagsCollection = tagsList.Tags.ToAsyncObservableCollection();

                StatusMessage = "Getting associated work item(s)...";
                var total = tagsList.Tags.Count;
                var current = 0;
                await Task.Run(() =>
                {
                    Parallel.ForEach(tagsList.Tags, tag =>
                    {
                        var associatedWorkItems =
                            _tagService.GetAssociatedWorkItems(_teamFoundationContext.TeamProjectName, tag.Name).Result;
                        if (associatedWorkItems.Data?.WorkItems != null)
                        {
                            var displayName = $"{tag.Name} ({associatedWorkItems.Data.WorkItems.Count})";
                            var tagWithWorkItems = tagsList.Tags.Any(x => x.Id == tag.Id);
                            if (tagWithWorkItems)
                            {
                                tag.DisplayName = displayName;
                                tag.AssociatedWorkitemsAsOf = associatedWorkItems.Data.AsOf;
                                tag.AssociatedWorkitems = associatedWorkItems.Data.WorkItems;
                                TagsCollection = tagsList.Tags.ToAsyncObservableCollection();
                            }
                            StatusMessage = $"Getting associated work item(s)...completed {++current} of {total}";
                        }
                        else if (associatedWorkItems.Exception != null)
                        {
                            _log.Error("Error getting associated work item count - Data: {@data}, Exception: {@exception}", associatedWorkItems.Data, associatedWorkItems.Exception);
                        }
                    });
                });
                ShowBusy(false);
            }
            catch (Exception exception)
            {
                _eventAggregator.GetEvent<NotifyErrorTagAdminPage>().Publish(exception.Message);
                _log.Information("Error getting tags - TFSContext: {@context}, Exception: {@exception}",
                new
                {
                    Collection = _teamFoundationContext.TeamProjectCollection.Name,
                    User = _teamFoundationContext.TeamProjectCollection.ConfigurationServer.AuthorizedIdentity.UniqueName,
                    Project = _teamFoundationContext.TeamProjectName,
                }, exception);
                _eventAggregator.GetEvent<NotifyErrorTagAdminPage>().Publish(exception.Message);
            }
        }

        private void OnClearSelectionCommand(object obj)
        {
            TagsCollection?.ForEach(x => x.IsChecked = false);
            TagsCollection = TagsCollection?.ToAsyncObservableCollection();
            SelectedTags = null;
        }

        private void OnDeleteTagCommand(object e)
        {
            var selectedTags = SelectedTags;

            if (TaskDialog.OSSupportsTaskDialogs)
            {
                using (TaskDialog dialog = new TaskDialog())
                {
                    dialog.WindowTitle = "Tag Admin";
                    dialog.MainInstruction = "Are you sure to delete selected tag(s)? ";
                    dialog.Content = "Selected tag(s) will be deleted permanently. This action is irreversible.";
                    dialog.ExpandedInformation =
                        $"Tag(s) \"{selectedTags.Select(x => x.Name).ToList().CommaQuibblingWithAnd()}\" will be deleted!";
                    dialog.FooterIcon = TaskDialogIcon.Information;
                    dialog.Footer =
                        "Before you decide to delete a tag, consider that they may be associated with historical revisions of work items or other resources";
                    TaskDialogButton deleteButton = new TaskDialogButton("Yes, Delete");
                    dialog.Buttons.Add(deleteButton);
                    var cancelDeleteButton = new TaskDialogButton("Cancel delete");
                    dialog.Buttons.Add(cancelDeleteButton);
                    cancelDeleteButton.Default = true;
                    var taskDialogButton = dialog.ShowDialog();

                    if (taskDialogButton == cancelDeleteButton)
                    {
                        return;
                    }
                    if (taskDialogButton == deleteButton)
                    {
                        _eventAggregator.GetEvent<TriggerDeleteTags>().Publish(string.Empty);
                    }
                }
            }
            else
            {
                var dialogResult =
                    MessageBox.Show("Are you sure to delete selected tag(s)? This action is irreversible.", "Tag Admin",
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (dialogResult == MessageBoxResult.No)
                {
                    return;
                }
                if (dialogResult == MessageBoxResult.Yes)
                {
                    _eventAggregator.GetEvent<TriggerDeleteTags>().Publish(string.Empty);
                }
            }
        }

        private async void OnDeleteTags(string obj)
        {
            try
            {
                ShowBusy(true);

                StatusMessage = "Deleting tag...";
                var selectedTags = SelectedTags;

                if (selectedTags == null)
                    return;

                await Task.Run(() =>
                {
                    Parallel.ForEach(selectedTags, tag =>
                    {
                        ShowBusy(true);
                        StatusMessage = $"Deleting tag {tag.Name}";

                        var deleteTagResponse = _tagService.DeleteTag(tag.Id).Result;
                        if (deleteTagResponse.Exception != null)
                        {
                            _log.Error("Error deleting tags - Exception: {@exception}", deleteTagResponse.Exception);
                            _eventAggregator.GetEvent<NotifyErrorTagAdminPage>().Publish(deleteTagResponse.Exception.Message);
                        }
                    });
                });
                //trigger refresh
                StatusMessage = "Refreshing...";
                _eventAggregator.GetEvent<TriggerGetTags>().Publish(_teamFoundationContext);
                ShowBusy(false);
            }
            catch (Exception exception)
            {
                _log.Information("Error deleting tags - TFSContext: {@context}, Tags:{tags}, Exception: {@exception}",
                 new
                 {
                     Collection = _teamFoundationContext.TeamProjectCollection.Name,
                     User = _teamFoundationContext.TeamProjectCollection.ConfigurationServer.AuthorizedIdentity.UniqueName,
                     Project = _teamFoundationContext.TeamProjectName,
                 }, SelectedTags.Count, exception);
                _eventAggregator.GetEvent<NotifyErrorTagAdminPage>().Publish(exception.Message);
            }
        }

        private void OnGetTags(ITeamFoundationContext context)
        {
            OnClearSelectionCommand(null);
            //if context has changed by user update our local context
            // also fixes issue: The project specified is not found in hierarchy issue
            _teamFoundationContext = context;
            GetTagsFromService(context);
        }

        private void OnRenameTagCommand(object e)
        {
            var inputDialog = new InputDialog();
            inputDialog.WindowTitle = "Tag Admin";
            inputDialog.MainInstruction = "Please enter a new tag name";
            inputDialog.Content =
                "NOTE: If the tag already does not exist, it will be created for you. You cannot rename to an existing tag.";
            if (inputDialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(inputDialog.Input))
                {
                    if (TaskDialog.OSSupportsTaskDialogs)
                    {
                        using (TaskDialog dialog = new TaskDialog())
                        {
                            dialog.WindowTitle = "Tag Admin";
                            dialog.MainInstruction = "Tag name cannot be empty!";
                            dialog.Content = "Please provide a name new tag name.";
                            TaskDialogButton okButton = new TaskDialogButton(ButtonType.Ok);
                            dialog.Buttons.Add(okButton);
                            dialog.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageDialog.Show("Tag Admin", "Tag name cannot be empty. Please provide a new tag name.",
                          MessageDialogCommandSet.Ok);
                    }

                    OnRenameTagCommand(null);
                    return;
                }
                //Rename tags
                _eventAggregator.GetEvent<TriggerRenameTags>().Publish(inputDialog.Input);
            }
        }

        private void OnRenameTags(string newName)
        {
            ShowBusy(true);

            //check if new name provided is a new tag or existing
            var tagExistResponse = _tagService.GetTagByName(newName).Result;
            if (tagExistResponse.Exception != null)
            {
                StatusMessage = "Renaming tag...";
                var selectedTag = SelectedTags?.FirstOrDefault();
                if (selectedTag == null)
                    return;

                StatusMessage = $"Renaming tag...{selectedTag.Name}";

                var response = _tagService.RenameTag(selectedTag.Name, newName.Trim()).Result;
                if (response.Exception != null)
                {
                    _log.Error("Error renaming tag - Data: {@data}, Exception: {@exception}", response.Data, response.Exception);
                    ShowBusy(false);
                    _eventAggregator.GetEvent<NotifyErrorTagAdminPage>().Publish(response.Exception.Message);
                    return;
                }
                //trigger refresh
                StatusMessage = "Refreshing...";
                _eventAggregator.GetEvent<TriggerGetTags>().Publish(_teamFoundationContext);
            }
            else if (tagExistResponse.Data != null)
            {
                //tag already exist
                ShowBusy(false);
                if (TaskDialog.OSSupportsTaskDialogs)
                {
                    using (TaskDialog dialog = new TaskDialog())
                    {
                        dialog.WindowTitle = "Tag Admin";
                        dialog.MainInstruction = "Tag name already exists!";
                        dialog.Content = $"Tag '{newName}' already exists, please choose another name.";
                        TaskDialogButton okButton = new TaskDialogButton("Try new name");
                        dialog.Buttons.Add(okButton);
                        var cancelButton = new TaskDialogButton("Cancel rename");
                        dialog.Buttons.Add(cancelButton);
                        var taskDialogButton = dialog.ShowDialog();
                        if (taskDialogButton == okButton)
                        {
                            OnRenameTagCommand(null);
                            return;
                        }
                        if (taskDialogButton == cancelButton)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    var messageBoxResult = MessageBox.Show(
                        $"Tag '{newName}' already exists, please choose another name.", "Tag Admin", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (messageBoxResult == MessageBoxResult.OK)
                    {
                        OnRenameTagCommand(null);
                        return;
                    }
                    if (messageBoxResult == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
            }


            ShowBusy(false);
        }

        private void OnToggleButtonClick(object selectedTag)
        {
            SelectedTags = TagsCollection.Where(t => t.IsChecked).ToAsyncObservableCollection();
        }

        private void OnViewWorkitemCommand(object obj)
        {
            var teamExplorer = _serviceProvider.GetService(typeof(ITeamExplorer)) as ITeamExplorer;
            teamExplorer?.NavigateToPage(new Guid(GuidList.ASSOCIATED_WORKITEM_PAGE_GUID), SelectedTags);
        }

        private void ShowBusy(bool workInProgress)
        {
            _eventAggregator.GetEvent<ShowBusyTagAdminPage>().Publish(workInProgress);
            IsWorkInProgress = workInProgress;
        }

        private void Subscriptions()
        {
            _eventAggregator.GetEvent<TriggerGetTags>().Subscribe(OnGetTags, ThreadOption.BackgroundThread);
            _eventAggregator.GetEvent<TriggerRenameTags>().Subscribe(OnRenameTags, ThreadOption.BackgroundThread);
            _eventAggregator.GetEvent<TriggerDeleteTags>().Subscribe(OnDeleteTags, ThreadOption.UIThread);
        }
        private void Triggers()
        {
            _tagAdminContext.EventAggregator.GetEvent<TriggerGetTags>().Publish(_teamFoundationContext);
        }

        #endregion Private Methods

    }
}
