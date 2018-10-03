using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Utilities;
using Microsoft;

namespace Transform.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(VSPackage.PackageGuidString)]
    public sealed class VSPackage : AsyncPackage
    {
        public const string PackageGuidString = "101d2a6f-97dc-4525-b0a7-2bef7ecf5c3b";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var dte = (DTE2)GetGlobalService(typeof(SDTE));

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var sp = new ServiceProvider((IServiceProvider)dte);
            var container = (IComponentModel)sp.GetService(typeof(SComponentModel));
            Assumes.Present(container);

            var contentTypeRegistry = container.GetService<IContentTypeRegistryService>();
            var fileExtensionRegistry = container.GetService<IFileExtensionRegistryService>();
            var contentType = fileExtensionRegistry.GetContentTypeForExtension("cs");

            if (contentType != contentTypeRegistry.UnknownContentType)
            {
                fileExtensionRegistry.RemoveFileExtension("cs");
            }
            fileExtensionRegistry.AddFileExtension("cs", contentTypeRegistry.GetContentType("CSharpE"));
        }
    }
}
