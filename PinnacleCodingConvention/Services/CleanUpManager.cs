using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    internal sealed class CleanUpManager
    {
        private readonly DTE2 _ide;
        private readonly CodeItemRetriever _codeItemRetriever;
        private readonly CodeItemReorganizer _codeItemReorganizer;
        private readonly CodeTreeBuilder _codeTreeBuilder;
        private readonly CodeRegionService _codeRegionService;
        private readonly BlankLineInsertService _blankLineInsertService;

        private static CleanUpManager _instance;

        private CleanUpManager(DTE2 ide)
        {
            _ide = ide;

            _codeItemRetriever = CodeItemRetriever.GetInstance(ide);
            _codeItemReorganizer = CodeItemReorganizer.GetInstance();
            _codeTreeBuilder = CodeTreeBuilder.GetInstance();
            _codeRegionService = CodeRegionService.GetInstance();
            _blankLineInsertService = BlankLineInsertService.GetInstance();
        }

        internal static CleanUpManager GetInstance(DTE2 ide) => _instance ?? (_instance = new CleanUpManager(ide));

        /// <summary>
        /// Execute the Pinnacle Cleanup logic
        /// </summary>
        /// <param name="document">The active document which is being worked on</param>
        internal void Execute(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            new UndoTransactionHelper(_ide, document.Name).Run(() =>
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
