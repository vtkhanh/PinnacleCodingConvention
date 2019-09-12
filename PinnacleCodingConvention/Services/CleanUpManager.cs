using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models;
using PinnacleCodingConvention.Models.CodeItems;

namespace PinnacleCodingConvention.Services
{
    internal sealed class CleanUpManager
    {
        private static CleanUpManager _instance;
        private CodeItemRetriever _codeItemRetriever;
        private CodeItemOrder _codeItemOrder;
        private CodeItemReorganizer _codeItemReorganizer;

        private readonly PinnacleCodingConventionPackage _package;

        private CleanUpManager(PinnacleCodingConventionPackage package)
        {
            _package = package;

            _codeItemRetriever = CodeItemRetriever.GetInstance(package);
            _codeItemOrder = CodeItemOrder.GetInstance();
            _codeItemReorganizer = CodeItemReorganizer.GetInstance(package);
        }

        internal static CleanUpManager GetInstance(PinnacleCodingConventionPackage package) => _instance ?? (_instance = new CleanUpManager(package));

        internal void Execute(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            new UndoTransactionHelper(_package, document.Name).Run(() =>
            {
                var codeItems = _codeItemRetriever.Retrieve(document).Where(item => !(item is CodeItemUsingStatement));
                codeItems = _codeItemOrder.Order(codeItems, CodeSortOrder.Alpha);
                codeItems = _codeItemReorganizer.Reorganize(codeItems);
                OutputWindowHelper.PrintCodeItems(codeItems);
            });
        }

    }
}
