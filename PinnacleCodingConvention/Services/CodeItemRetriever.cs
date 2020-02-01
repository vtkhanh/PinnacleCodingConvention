using EnvDTE;
using EnvDTE80;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System;
using System.Collections.Generic;
using CodeModel = PinnacleCodingConvention.Models.CodeModel;

namespace PinnacleCodingConvention.Services
{
    internal class CodeItemRetriever
    {
        private readonly CodeModelBuilder _codeModelBuilder;

        private static CodeItemRetriever _instance;

        private CodeItemRetriever(DTE2 ide) => _codeModelBuilder = CodeModelBuilder.GetInstance(ide);

        internal static CodeItemRetriever GetInstance(DTE2 ide) => _instance ?? (_instance = new CodeItemRetriever(ide));

        /// <summary>
        /// Parse the document to get list of BaseCodeItem
        /// </summary>
        /// <param name="document"></param>
        /// <param name="loadLazyInitializedValues"></param>
        /// <returns></returns>
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
