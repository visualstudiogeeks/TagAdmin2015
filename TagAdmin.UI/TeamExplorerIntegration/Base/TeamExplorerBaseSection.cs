/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/

using System;
using Microsoft.TeamFoundation.Controls;

namespace TagAdmin.UI.TeamExplorerIntegration.Base
{
    /// <summary>
    /// Team Explorer base section class.
    /// </summary>
    public class TeamExplorerBaseSection : TeamExplorerBase, ITeamExplorerSection
    {
        #region ITeamExplorerSection

        private bool _isBusy;

        private bool _isExpanded = true;

        private bool _isVisible = true;

        private object _sectionContent;

        private string _title;

        /// <summary>
        /// Get/set the IsBusy flag.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged("IsBusy"); }
        }

        /// <summary>
        /// Get/set the IsExpanded flag.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; RaisePropertyChanged("IsExpanded"); }
        }

        /// <summary>
        /// Get/set the IsVisible flag.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; RaisePropertyChanged("IsVisible"); }
        }

        /// <summary>
        /// Get/set the section content.
        /// </summary>
        public object SectionContent
        {
            get { return _sectionContent; }
            set { _sectionContent = value; RaisePropertyChanged("SectionContent"); }
        }

        /// <summary>
        /// Get/set the section title.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged("Title"); }
        }

        /// <summary>
        /// Cancel any running operations.
        /// </summary>
        public virtual void Cancel()
        {
        }

        /// <summary>
        /// Get the requested extensibility service from the section.  Return
        /// null if the service is not offered by this section.
        /// </summary>
        public virtual object GetExtensibilityService(Type serviceType)
        {
            return null;
        }

        /// <summary>
        /// Initialize the section.
        /// </summary>
        public virtual void Initialize(object sender, SectionInitializeEventArgs e)
        {
            ServiceProvider = e.ServiceProvider;
        }

        /// <summary>
        /// Called when the section is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void Loaded(object sender, SectionLoadedEventArgs e)
        {
        }

        /// <summary>
        /// Refresh the section contents.
        /// </summary>
        public virtual void Refresh()
        {
        }

        /// <summary>
        /// Save context handler that is called before a section is unloaded.
        /// </summary>
        public virtual void SaveContext(object sender, SectionSaveContextEventArgs e)
        {
        }

        #endregion ITeamExplorerSection
    }
}