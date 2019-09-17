using EnvDTE;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System;
using System.Collections.Generic;
using CodeModel = PinnacleCodingConvention.Models.CodeModel;

namespace PinnacleCodingConvention.Services
{
    internal class CodeItemRetriever
    {
        private static CodeItemRetriever _instance;
        private CodeModelBuilder _codeModelBuilder;

        private CodeItemRetriever(PinnacleCodingConventionPackage package)
        {
            _codeModelBuilder = CodeModelBuilder.GetInstance(package);
        }

        internal static CodeItemRetriever GetInstance(PinnacleCodingConventionPackage package) =>
            _instance ?? (_instance = new CodeItemRetriever(package));

        internal IList<BaseCodeItem> Retrieve(Document document, bool loadLazyInitializedValues = false)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var codeModel = new CodeModel(document);
            BuildCodeItems(codeModel);
            if (loadLazyInitializedValues)
            {
                LoadLazyInitializedValues(codeModel);
            }

            return codeModel.CodeItems;
        }

        private void BuildCodeItems(CodeModel codeModel)
        {
            try
            {
                var codeItems = _codeModelBuilder.RetrieveAllCodeItems(codeModel.Document);
                codeModel.CodeItems = codeItems;
            }
            catch (Exception ex)
            {
                OutputWindowHelper.WriteError($"Unable to build code model for '{codeModel.Document.FullName}': {ex}");

                codeModel.CodeItems = new List<BaseCodeItem>();
            }
        }

        private void LoadLazyInitializedValues(CodeModel codeModel)
        {
            try
            {
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
