using EnvDTE;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    /// <summary>
    /// A class for encapsulating insertion of blank line padding logic.
    /// </summary>
    internal class BlankLineInsertService
    {
        /// <summary>
        /// The singleton instance of the <see cref="BlankLineInsertService" /> class.
        /// </summary>
        private static BlankLineInsertService _instance;

        /// <summary>
        /// Gets an instance of the <see cref="BlankLineInsertService" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <returns>An instance of the <see cref="BlankLineInsertService" /> class.</returns>
        internal static BlankLineInsertService GetInstance()
        {
            return _instance ?? (_instance = new BlankLineInsertService());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlankLineInsertService" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        private BlankLineInsertService() { }

        /// <summary>
        /// Determines if the specified code item instance should be preceded by a blank line.
        /// Defaults to false for unknown kinds or null objects.
        /// </summary>
        /// <param name="codeItem">The code item.</param>
        /// <returns>True if code item should be preceded by a blank line, otherwise false.</returns>
        internal bool ShouldBePrecededByBlankLine(BaseCodeItem codeItem)
        {
            if (codeItem == null)
            {
                return false;
            }

            return codeItem.AssociatedCodeRegion is object;
        }

        /// <summary>
        /// Determines if the specified code item instance should be followed by a blank line.
        /// Defaults to false for unknown kinds or null objects.
        /// </summary>
        /// <param name="codeItem">The code item.</param>
        /// <returns>True if code item should be followed by a blank line, otherwise false.</returns>
        internal bool ShouldBeFollowedByBlankLine(BaseCodeItem codeItem)
        {
            if (codeItem == null)
            {
                return false;
            }

            return codeItem.AssociatedCodeRegion is object;
        }

        /// <summary>
        /// Inserts a blank line before #region tags except where adjacent to a brace.
        /// </summary>
        /// <param name="regions">The regions to pad.</param>
        internal void InsertPaddingBeforeRegionTags(IEnumerable<CodeItemRegion> regions)
        {
            foreach (var region in regions.Where(x => !x.IsInvalidated))
            {
                var startPoint = region.StartPoint.CreateEditPoint();

                TextDocumentHelper.InsertBlankLineBeforePoint(startPoint);
            }
        }

        /// <summary>
        /// Inserts a blank line after #region tags except where adjacent to a brace.
        /// </summary>
        /// <param name="regions">The regions to pad.</param>
        internal void InsertPaddingAfterRegionTags(IEnumerable<CodeItemRegion> regions)
        {
            foreach (var region in regions.Where(x => !x.IsInvalidated))
            {
                var startPoint = region.StartPoint.CreateEditPoint();

                TextDocumentHelper.InsertBlankLineAfterPoint(startPoint);
            }
        }

        /// <summary>
        /// Inserts a blank line before #endregion tags except where adjacent to a brace.
        /// </summary>
        /// <param name="regions">The regions to pad.</param>
        internal void InsertPaddingBeforeEndRegionTags(IEnumerable<CodeItemRegion> regions)
        {
            foreach (var region in regions.Where(x => !x.IsInvalidated))
            {
                var endPoint = region.EndPoint.CreateEditPoint();

                TextDocumentHelper.InsertBlankLineBeforePoint(endPoint);
            }
        }

        /// <summary>
        /// Inserts a blank line after #endregion tags except where adjacent to a brace.
        /// </summary>
        /// <param name="regions">The regions to pad.</param>
        internal void InsertPaddingAfterEndRegionTags(IEnumerable<CodeItemRegion> regions)
        {
            foreach (var region in regions.Where(x => !x.IsInvalidated))
            {
                var endPoint = region.EndPoint.CreateEditPoint();

                TextDocumentHelper.InsertBlankLineAfterPoint(endPoint);
            }
        }

        /// <summary>
        /// Inserts a blank line before the specified code elements except where adjacent to a brace.
        /// </summary>
        /// <typeparam name="T">The type of the code element.</typeparam>
        /// <param name="codeElements">The code elements to pad.</param>
        internal void InsertPaddingBeforeCodeElements<T>(IEnumerable<T> codeElements)
            where T : BaseCodeItemElement
        {
            foreach (T codeElement in codeElements.Where(ShouldBePrecededByBlankLine))
            {
                TextDocumentHelper.InsertBlankLineBeforePoint(codeElement.StartPoint);
            }
        }

        /// <summary>
        /// Inserts a blank line after the specified code elements except where adjacent to a brace.
        /// </summary>
        /// <typeparam name="T">The type of the code element.</typeparam>
        /// <param name="codeElements">The code elements to pad.</param>
        internal void InsertPaddingAfterCodeElements<T>(IEnumerable<T> codeElements)
            where T : BaseCodeItemElement
        {
            foreach (T codeElement in codeElements.Where(ShouldBeFollowedByBlankLine))
            {
                TextDocumentHelper.InsertBlankLineAfterPoint(codeElement.EndPoint);
            }
        }

        /// <summary>
        /// Inserts a blank line between multi-line property accessors.
        /// </summary>
        /// <param name="properties">The properties.</param>
        internal void InsertPaddingBetweenMultiLinePropertyAccessors(IEnumerable<CodeItemProperty> properties)
        {
            foreach (var property in properties)
            {
                var getter = property.CodeProperty.Getter;
                var setter = property.CodeProperty.Setter;

                if (getter != null && setter != null && (getter.StartPoint.Line < getter.EndPoint.Line ||
                                                         setter.StartPoint.Line < setter.EndPoint.Line))
                {
                    EditPoint insertingPoint = setter.EndPoint.Line > getter.EndPoint.Line ? getter.EndPoint.CreateEditPoint() : setter.EndPoint.CreateEditPoint();
                    TextDocumentHelper.InsertBlankLineAfterPoint(insertingPoint);
                }
            }
        }
    }
}
