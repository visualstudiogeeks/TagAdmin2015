/*
* Copyright (c) Microsoft Corporation. All rights reserved. This code released
* under the terms of the Microsoft Limited Public License (MS-LPL).
*/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;
using TagAdmin.UI.Services;
using TagAdmin.Common.Extensions;

namespace TagAdmin.UI.TeamExplorerIntegration.Base
{
    /// <summary>
    /// Team Explorer plugin common base class.
    /// </summary>
    public class TeamExplorerBase : IDisposable, INotifyPropertyChanged
    {
        #region Members

        private bool _contextSubscribed;

        #endregion Members

        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Get/set the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set
            {
                // Unsubscribe from Team Foundation context changes
                if (_serviceProvider != null)
                {
                    UnsubscribeContextChanges();
                }

                _serviceProvider = value;

                // Subscribe to Team Foundation context changes
                if (_serviceProvider != null)
                {
                    SubscribeContextChanges();
                }
            }
        }

        /// <summary>
        /// Get the requested service from the service provider.
        /// </summary>
        public T GetService<T>()
        {
            Debug.Assert(ServiceProvider != null, "GetService<T> called before service provider is set");
            if (ServiceProvider != null)
            {
                return (T)ServiceProvider.GetService(typeof(T));
            }

            return default(T);
        }

        protected void ClearNotification(Guid uniqueIdentifier)
        {
            var teamExplorer = GetService<ITeamExplorer>();
            if (teamExplorer != null)
            {
                teamExplorer.HideNotification(uniqueIdentifier);
            }
        }

        protected bool IsNotificationShown(Guid uniqueIdentifier)
        {
            var teamExplorer = GetService<ITeamExplorer>();
            if (teamExplorer != null)
            {
                return teamExplorer.IsNotificationVisible(uniqueIdentifier);
            }
            return false;
        }

        /// <summary>
        /// Show a notification in the Team Explorer window.
        /// </summary>
        protected Guid ShowNotification(Guid uniqueIdentifier, string message, NotificationType type)
        {
            var teamExplorer = GetService<ITeamExplorer>();
            if (teamExplorer != null && uniqueIdentifier != Guid.Empty)
            {

                teamExplorer.ShowNotification(message, type, NotificationFlags.NoTooltips, null, uniqueIdentifier);
                return uniqueIdentifier;
            }

            return Guid.Empty;
        }

        protected Guid ShowNotification(Guid uniqueIdentifier, string message, NotificationType type, ICommand command)
        {
            var teamExplorer = GetService<ITeamExplorer>();
            if (teamExplorer != null && uniqueIdentifier != Guid.Empty)
            {

                teamExplorer.ShowNotification(message, type, NotificationFlags.NoTooltips, command, uniqueIdentifier);
                return uniqueIdentifier;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Show a notification in the Team Explorer window.
        /// </summary>
        protected Guid ShowErrorNotification(Guid uniqueIdentifier, string message, ICommand command)
        {
            var teamExplorer = GetService<ITeamExplorer>();
            if (teamExplorer != null && uniqueIdentifier != Guid.Empty)
            {

                teamExplorer.ShowNotification(message, NotificationType.Error, NotificationFlags.NoTooltips, command, uniqueIdentifier);
                return uniqueIdentifier;
            }

            return Guid.Empty;
        }

        #region IDisposable

        /// <summary>
        /// Dispose.
        /// </summary>
        public virtual void Dispose()
        {
            UnsubscribeContextChanges();
        }

        #endregion IDisposable

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        #region Team Foundation Context

        /// <summary>
        /// Get the current Team Foundation context.
        /// </summary>
        protected ITeamFoundationContext CurrentContext 
        {
            get
            {
                var tfContextManager = GetService<ITeamFoundationContextManager>();
                if (tfContextManager != null)
                {
                    return tfContextManager.CurrentContext;
                }

                return null;
            }
        }

        protected ITagAdminContext TagAdminContext
        {
            get
            {
                var globalContext = ServiceProvider.GetService<STagAdminContext, ITagAdminContext>();
                return globalContext;
            }
        }

        /// <summary>
        /// ContextChanged event handler.
        /// </summary>
        protected virtual void ContextChanged(object sender, ContextChangedEventArgs e)
        {
        }

        /// <summary>
        /// Subscribe to context changes.
        /// </summary>
        protected void SubscribeContextChanges()
        {
            Debug.Assert(ServiceProvider != null, "ServiceProvider must be set before subscribing to context changes");
            if (ServiceProvider == null || _contextSubscribed)
            {
                return;
            }

            var tfContextManager = GetService<ITeamFoundationContextManager>();
            if (tfContextManager != null)
            {
                tfContextManager.ContextChanged += ContextChanged;
                _contextSubscribed = true;
            }
        }

        /// <summary>
        /// Unsubscribe from context changes.
        /// </summary>
        protected void UnsubscribeContextChanges()
        {
            if (ServiceProvider == null || !_contextSubscribed)
            {
                return;
            }

            var tfContextManager = GetService<ITeamFoundationContextManager>();
            if (tfContextManager != null)
            {
                tfContextManager.ContextChanged -= ContextChanged;
            }
        }

        #endregion Team Foundation Context
    }
}