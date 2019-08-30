using EnvDTE;
using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Helpers;

namespace PinnacleCodingConvention.Services
{
    internal sealed class CleanUpManager
    {
        private static CleanUpManager _instance;
        private CodeItemRetriever _codeItemRetrievalService;

        private readonly PinnacleCodingConventionPackage _package;

        private CleanUpManager(PinnacleCodingConventionPackage package)
        {
            _package = package;

            _codeItemRetrievalService = CodeItemRetriever.GetInstance(package);
        }

        internal static CleanUpManager GetInstance(PinnacleCodingConventionPackage package) => _instance ?? (_instance = new CleanUpManager(package));

        internal void Execute(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            new UndoTransactionHelper(_package, document.Name).Run(() =>
            {
                var codeItems = _codeItemRetrievalService.Retrieve(document);
            });
        }
    }
}
