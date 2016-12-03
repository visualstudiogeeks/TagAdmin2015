using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace TagAdmin.UI.ViewModel.Base
{
    public abstract class ViewModelBase : INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected ViewModelBase()
        {
        }

        #region INotifyPropertyChanging Members
#pragma warning disable 67
        public event PropertyChangingEventHandler PropertyChanging;
#pragma warning restore
        #endregion INotifyPropertyChanging Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Members

        #region Administrative Properties

        /// <summary>
        /// Whether the view model should ignore property-change events.
        /// </summary>
        public virtual bool IgnorePropertyChangeEvents { get; set; }

        #endregion Administrative Properties


        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = propertyExpression.Body as MemberExpression;
                string propertyName = memberExpr.Member.Name;
                RaisePropertyChanged(propertyName);
            }
        }

        
    }
}
