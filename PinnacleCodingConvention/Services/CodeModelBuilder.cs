using EnvDTE;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinnacleCodingConvention.Services
{
    internal class CodeModelBuilder
    {
        private readonly PinnacleCodingConventionPackage _package;
        private readonly CodeModelService _codeModelService;

        /// <summary>
        /// The singleton instance of the <see cref="CodeModelBuilder" /> class.
        /// </summary>
        private static CodeModelBuilder _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeModelBuilder" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private CodeModelBuilder(PinnacleCodingConventionPackage package)
        {
            _package = package;

            _codeModelService = CodeModelService.GetInstance();
        }

        /// <summary>
        /// Gets an instance of the <see cref="CodeModelBuilder" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="CodeModelBuilder" /> class.</returns>
        internal static CodeModelBuilder GetInstance(PinnacleCodingConventionPackage package) => _instance ?? (_instance = new CodeModelBuilder(package));

        /// <summary>
        /// Walks the given document and constructs a <see cref="IList<BaseCodeItem>" /> of CodeItems
        /// within it including regions.
        /// </summary>
        /// <param name="document">The document to walk.</param>
        /// <returns>The set of code items within the document, including regions.</returns>
        internal IList<BaseCodeItem> RetrieveAllCodeItems(Document document)
        {
            var codeItems = new List<BaseCodeItem>();

            var fileCodeModel = RetrieveFileCodeModel(document.ProjectItem);
            RetrieveCodeItems(codeItems, fileCodeModel);

            var codeRegions = _codeModelService.RetrieveCodeRegions(document.GetTextDocument());
            MatchCodeItemsWithRegions(codeItems, codeRegions);
            codeItems.AddRange(codeRegions);

            return codeItems;
        }

        private void MatchCodeItemsWithRegions(List<BaseCodeItem> codeItems, IEnumerable<CodeItemRegion> codeRegions)
        {
            foreach (var codeItem in codeItems)
            {
                var matchedCodeRegion = codeRegions.FirstOrDefault(region => region.Name == codeItem.Name);
                if (matchedCodeRegion is object)
                {
                    codeItem.StartLine = matchedCodeRegion.StartLine;
                    codeItem.StartOffset = matchedCodeRegion.StartOffset;
                    codeItem.StartPoint = matchedCodeRegion.StartPoint;
                    codeItem.EndLine = matchedCodeRegion.EndLine;
                    codeItem.EndOffset = matchedCodeRegion.EndOffset;
                    codeItem.EndPoint = matchedCodeRegion.EndPoint;
                }
            }
        }

        /// <summary>
        /// Attempts to return the FileCodeModel associated with the specified project item.
        /// </summary>
        /// <param name="projectItem">The project item.</param>
        /// <returns>The associated FileCodeModel, otherwise null.</returns>
        private FileCodeModel RetrieveFileCodeModel(ProjectItem projectItem)
        {
            if (projectItem == null)
                return null;

            if (projectItem.FileCodeModel != null)
                return projectItem.FileCodeModel;

            // If this project item is part of a shared project, retrieve the FileCodeModel via a similar platform project item.
            const string sharedProjectTypeGUID = "{d954291e-2a0b-460d-934e-dc6b0785db48}";
            var containingProject = projectItem.ContainingProject;

            if (containingProject != null && containingProject.Kind != null &&
                containingProject.Kind.ToLowerInvariant() == sharedProjectTypeGUID)
            {
                var similarProjectItems = SolutionHelper.GetSimilarProjectItems(_package, projectItem);
                var fileCodeModel = similarProjectItems.FirstOrDefault(x => x.FileCodeModel is object)?.FileCodeModel;

                return fileCodeModel;
            }

            return null;
        }

        /// <summary>
        /// Walks the given FileCodeModel, turning CodeElements into code items within the specified
        /// code items set.
        /// </summary>
        /// <param name="codeItems">The code items set for accumulation.</param>
        /// <param name="fileCodeModel">The FileCodeModel to walk.</param>
        private static void RetrieveCodeItems(IList<BaseCodeItem> codeItems, FileCodeModel fileCodeModel)
        {
            if (fileCodeModel != null && fileCodeModel.CodeElements != null)
                RetrieveCodeItemsFromElements(codeItems, fileCodeModel.CodeElements);
        }

        /// <summary>
        /// Retrieves code items from each specified code element into the specified code items set.
        /// </summary>
        /// <param name="codeItems">The code items set for accumulation.</param>
        /// <param name="codeElements">The CodeElements to walk.</param>
        private static void RetrieveCodeItemsFromElements(IList<BaseCodeItem> codeItems, CodeElements codeElements)
        {
            foreach (CodeElement child in codeElements)
            {
                RetrieveCodeItemsRecursively(codeItems, child);
            }
        }

        /// <summary>
        /// Recursive method for creating a code item for the specified code element, adding it to
        /// the specified code items set and recursing into all of the code element's children.
        /// </summary>
        /// <param name="codeItems">The code items set for accumulation.</param>
        /// <param name="codeElement">The CodeElement to walk (add and recurse).</param>
        private static void RetrieveCodeItemsRecursively(IList<BaseCodeItem> codeItems, CodeElement codeElement)
        {
            var codeItem = FactoryCodeItems.CreateCodeItemElement(codeElement);

            if (codeItem != null)
                codeItems.Add(codeItem);

            if (codeElement.Children != null)
                RetrieveCodeItemsFromElements(codeItems, codeElement.Children);
        }
    }
}
