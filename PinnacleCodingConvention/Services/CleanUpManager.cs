using EnvDTE;
using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    internal sealed class CleanUpManager
    {
        private readonly PinnacleCodingConventionPackage _package;
        private readonly CodeItemRetriever _codeItemRetriever;
        private readonly CodeItemReorganizer _codeItemReorganizer;
        private readonly CodeTreeBuilder _codeTreeBuilder;
        private readonly CodeRegionService _codeRegionService;
        private readonly BlankLineInsertService _blankLineInsertService;

        private static CleanUpManager _instance;

        private CleanUpManager(PinnacleCodingConventionPackage package)
        {
            _package = package;

            _codeItemRetriever = CodeItemRetriever.GetInstance(package);
            _codeItemReorganizer = CodeItemReorganizer.GetInstance();
            _codeTreeBuilder = CodeTreeBuilder.GetInstance();
            _codeRegionService = CodeRegionService.GetInstance();
            _blankLineInsertService = BlankLineInsertService.GetInstance();
        }

        internal static CleanUpManager GetInstance(PinnacleCodingConventionPackage package) => _instance ?? (_instance = new CleanUpManager(package));

        internal void Execute(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            new UndoTransactionHelper(_package, document.Name).Run(() =>
            {
                var codeItems = _codeItemRetriever.Retrieve(document).Where(item => !(item is CodeItemUsingStatement));
                codeItems = _codeRegionService.CleanupExistingRegions(codeItems);
                codeItems = _codeTreeBuilder.Build(codeItems);
                codeItems = _codeItemReorganizer.Reorganize(codeItems);

                var codeItemNamespace = codeItems.First(item => item is CodeItemNamespace) as ICodeItemParent;
                _codeRegionService.AddRequiredRegions(codeItemNamespace.Children, codeItemNamespace);
                codeItems = _codeItemRetriever.Retrieve(document);
                _blankLineInsertService.InsertPaddingBeforeAndAfter(codeItems.Where(item => item is CodeItemRegion || item is CodeItemNamespace));

#if DEBUG
                OutputWindowHelper.PrintCodeItems(codeItems);
#endif
            });
        }
    }
}
