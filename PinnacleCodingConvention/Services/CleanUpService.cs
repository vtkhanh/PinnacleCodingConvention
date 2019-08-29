using EnvDTE;
using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Helpers;

namespace PinnacleCodingConvention.Services
{
    internal sealed class CleanUpService
    {
        private static CleanUpService _instance;

        private readonly PinnacleCodingConventionPackage _package;

        private CleanUpService(PinnacleCodingConventionPackage package)
        {
            _package = package;
        }

        internal static CleanUpService GetInstance(PinnacleCodingConventionPackage package) => _instance ?? (_instance = new CleanUpService(package));

        internal void Execute(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            new UndoTransactionHelper(_package, document.Name).Run(() =>
            {
                OutputWindowHelper.WriteWarning("Execute command!");
                OutputWindowHelper.WriteError("Execute command!");
            });
        }
    }
}
