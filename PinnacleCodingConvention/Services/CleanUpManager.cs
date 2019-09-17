using EnvDTE;
using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    internal sealed class CleanUpManager
    {
        private static CleanUpManager _instance;
        private CodeItemRetriever _codeItemRetriever;
        private CodeItemReorganizer _codeItemReorganizer;
        private CodeTreeBuilder _codeTreeBuilder;

        private readonly PinnacleCodingConventionPackage _package;

        private CleanUpManager(PinnacleCodingConventionPackage package)
        {
            _package = package;

            _codeItemRetriever = CodeItemRetriever.GetInstance(package);
            _codeItemReorganizer = CodeItemReorganizer.GetInstance(package);
            _codeTreeBuilder = CodeTreeBuilder.GetInstance();
        }

        internal static CleanUpManager GetInstance(PinnacleCodingConventionPackage package) => _instance ?? (_instance = new CleanUpManager(package));

        internal void Execute(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            new UndoTransactionHelper(_package, document.Name).Run(() =>
            {
                var codeItems = _codeItemRetriever.Retrieve(document).Where(item => !(item is CodeItemUsingStatement) && !(item is CodeItemRegion));
                codeItems = _codeTreeBuilder.Build(codeItems);
                codeItems = _codeItemReorganizer.Reorganize(codeItems);
                OutputWindowHelper.PrintCodeItems(codeItems);
            });
        }
    }
}
