/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/

using System;
using System.Drawing;
using Microsoft.TeamFoundation.Controls;

namespace TagAdmin.UI.TeamExplorerIntegration.Base
{
    /// <summary>
    /// Team Explorer base navigation item class.
    /// </summary>
    public class TeamExplorerBaseNavigationItem : TeamExplorerBase, ITeamExplorerNavigationItem
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TeamExplorerBaseNavigationItem(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        #region ITeamExplorerNavigationItem

        private Image _image;

        private bool _isVisible = true;

        private string _text;

        /// <summary>
        /// Get/set the item image.
        /// </summary>
        public Image Image
        {
            get { return _image; }
            set { _image = value; RaisePropertyChanged("Image"); }
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
        /// Execute the item action.
        /// </summary>
        public virtual void Execute()
        {
        }

        /// <summary>
        /// Invalidate the item state.
        /// </summary>
        public virtual void Invalidate()
        {
        }

        #endregion ITeamExplorerNavigationItem
    }
}