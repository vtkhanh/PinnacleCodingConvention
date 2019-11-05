using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;

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
        /// Inserts blank lines before and after elements except where adjacent to a brace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codeItems">code items to pad</param>
        internal void InsertPaddingBeforeAndAfter<T>(IEnumerable<T> codeItems)
            where T : BaseCodeItem
        {
            InsertPaddingBeforeCodeElements(codeItems);
            InsertPaddingAfterCodeElements(codeItems);
        }

        /// <summary>
        /// Inserts a blank line before the specified code elements except where adjacent to a brace.
        /// </summary>
        /// <typeparam name="T">The type of the code element.</typeparam>
        /// <param name="codeItems">The code elements to pad.</param>
        internal void InsertPaddingBeforeCodeElements<T>(IEnumerable<T> codeItems)
            where T : BaseCodeItem
        {
            foreach (T codeItem in codeItems)
            {
                TextDocumentHelper.InsertBlankLineBeforePoint(codeItem.StartPoint);
            }
        }

        /// <summary>
        /// Inserts a blank line after the specified code elements except where adjacent to a brace.
        /// </summary>
        /// <typeparam name="T">The type of the code element.</typeparam>
        /// <param name="codeItems">The code elements to pad.</param>
        internal void InsertPaddingAfterCodeElements<T>(IEnumerable<T> codeItems)
            where T : BaseCodeItem
        {
            foreach (T codeItem in codeItems)
            {
                TextDocumentHelper.InsertBlankLineAfterPoint(codeItem.EndPoint);
            }
        }
    }
}
