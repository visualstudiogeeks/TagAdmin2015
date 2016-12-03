/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/

using System;
using Microsoft.TeamFoundation.Controls;

namespace TagAdmin.UI.TeamExplorerIntegration.Base
{
    /// <summary>
    /// Team Explorer base navigation link class.
    /// </summary>
    public class TeamExplorerBaseNavigationLink : TeamExplorerBase, ITeamExplorerNavigationLink
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TeamExplorerBaseNavigationLink(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        #region ITeamExplorerNavigationLink

        private bool _isEnabled = true;

        private bool _isVisible = true;

        private string _text;

        /// <summary>
        /// Get/set the IsEnabled flag.
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; RaisePropertyChanged("IsEnabled"); }
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
        /// Get/set the item text.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; RaisePropertyChanged("Text"); }
        }

        /// <summary>
        /// Execute the link action.
        /// </summary>
        public virtual void Execute()
        {
        }

        /// <summary>
        /// Invalidate the link state.
        /// </summary>
        public virtual void Invalidate()
        {
        }

        #endregion ITeamExplorerNavigationLink
    }
}