using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using TagAdmin.UI.Services;

namespace TagAdmin2015
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidTagAdmin2015PkgString)]
    [ProvideService(typeof(STagAdminContext))]
    [ProvideBindingPath]
    public sealed class TagAdmin2015Package : Package
    {
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            IServiceContainer serviceContainer = this;
            ServiceCreatorCallback creationCallback = CreateService;
            serviceContainer.AddService(typeof(STagAdminContext), creationCallback, true);
        }

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            if (container != this)
            {
                return null;
            }

            if (typeof(STagAdminContext) == serviceType)
            {
                return new TagAdminContextService(this);
            }

            return null;
        }
    }
}
