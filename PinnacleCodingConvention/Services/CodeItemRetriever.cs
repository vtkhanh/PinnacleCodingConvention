using EnvDTE;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models;
using PinnacleCodingConvention.Models.CodeItems;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeModel = PinnacleCodingConvention.Models.CodeModel;

namespace PinnacleCodingConvention.Services
{
    internal class CodeItemRetriever
    {
        private static CodeItemRetriever _instance;
        private CodeModelCache _codeModelCache;
        private CodeModelBuilder _codeModelBuilder;
        private PinnacleCodingConventionPackage _package;

        private CodeItemRetriever(PinnacleCodingConventionPackage package)
        {
            _package = package;

            _codeModelCache = CodeModelCache.GetInstance();
            _codeModelBuilder = CodeModelBuilder.GetInstance(package);
        }

        internal static CodeItemRetriever GetInstance(PinnacleCodingConventionPackage package) =>
            _instance ?? (_instance = new CodeItemRetriever(package));

        internal IList<BaseCodeItem> Retrieve(Document document, bool loadLazyInitializedValues = false)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            OutputWindowHelper.WriteInfo($"CodeModelManager.RetrieveAllCodeItemsAsync for '{document.FullName}'");

            var codeModel = _codeModelCache.GetCodeModel(document);
            if (codeModel.IsBuilding)
            {
                if (!codeModel.IsBuiltWaitHandle.WaitOne(TimeSpan.FromSeconds(3)))
                {
                    OutputWindowHelper.WriteInfo($"Timed out waiting for code model to be built for '{codeModel.Document.FullName}'");
                    return null;
                }
            }
            else if (codeModel.IsStale)
            {
                BuildCodeItems(codeModel);

                if (loadLazyInitializedValues)
                {
                    LoadLazyInitializedValues(codeModel);
                }
            }

            return codeModel.CodeItems;
        }

        private void BuildCodeItems(CodeModel codeModel)
        {
            try
            {
                OutputWindowHelper.WriteInfo($"CodeModelManager.BuildCodeItems started for '{codeModel.Document.FullName}'");

                codeModel.IsBuilding = true;
                codeModel.IsStale = false;

                var codeItems = _codeModelBuilder.RetrieveAllCodeItems(codeModel.Document);

                if (codeModel.IsStale)
                {
                    BuildCodeItems(codeModel);
                    return;
                }

                codeModel.CodeItems = codeItems;
                codeModel.IsBuilding = false;

                OutputWindowHelper.WriteInfo($"CodeModelManager.BuildCodeItems completed for '{codeModel.Document.FullName}'");
            }
            catch (Exception ex)
            {
                OutputWindowHelper.WriteError($"Unable to build code model for '{codeModel.Document.FullName}': {ex}");

                codeModel.CodeItems = new List<BaseCodeItem>();
                codeModel.IsBuilding = false;
            }
        }

        private void LoadLazyInitializedValues(CodeModel codeModel)
        {
            try
            {
                OutputWindowHelper.WriteInfo($"CodeModelManager.LoadLazyInitializedValues for '{codeModel.Document.FullName}'");

                foreach (var codeItem in codeModel.CodeItems)
                {
                    codeItem.LoadLazyInitializedValues();
                }
            }
            catch (Exception ex)
            {
                OutputWindowHelper.WriteError($"Unable to load lazy initialized values for '{codeModel.Document.FullName}': {ex}");
            }
        }

    }
}
