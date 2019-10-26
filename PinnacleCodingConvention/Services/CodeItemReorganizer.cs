using EnvDTE;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    internal class CodeItemReorganizer
    {
        private readonly BlankLineInsertService _blankLineInsertService;

        private static CodeItemReorganizer _instance;

        private CodeItemReorganizer() => _blankLineInsertService = BlankLineInsertService.GetInstance();

        internal static CodeItemReorganizer GetInstance() => _instance ?? (_instance = new CodeItemReorganizer());

        internal IEnumerable<BaseCodeItem> Reorganize(IEnumerable<BaseCodeItem> codeItems)
        {
            if (!codeItems.Any())
            {
                return codeItems;
            }
            
            // Get the items in their current order and their desired order.
            var currentOrder = GetReorganizableCodeItemElements(codeItems);
            var desiredOrder = new List<BaseCodeItemElement>(currentOrder);
            desiredOrder.Sort(new CodeItemTypeComparer());

            // Iterate across the items in the desired order, moving them when necessary.
            for (int desiredIndex = 0; desiredIndex < desiredOrder.Count; desiredIndex++)
            {
                var item = desiredOrder[desiredIndex];

                if (item is ICodeItemParent itemAsParent && ShouldReorganizeChildren(item))
                {
                    Reorganize(itemAsParent.Children);
                }

                int currentIndex = currentOrder.IndexOf(item);
                if (desiredIndex != currentIndex)
                {
                    // Move the item above what is in its desired position.
                    RepositionItemAboveBase(item, currentOrder[desiredIndex]);

                    // Update the current order to match the move.
                    currentOrder.RemoveAt(currentIndex);
                    currentOrder.Insert(desiredIndex > currentIndex ? desiredIndex - 1 : desiredIndex, item);
                }
            }

            return codeItems;
        }

        /// <summary>
        /// Repositions the specified item above the specified base.
        /// </summary>
        /// <param name="itemToMove">The item to move.</param>
        /// <param name="baseItem">The base item.</param>
        private void RepositionItemAboveBase(BaseCodeItem itemToMove, BaseCodeItem baseItem)
        {
            if (itemToMove == baseItem)
            {
                return;
            }

            bool separateWithNewLine = ShouldBeSeparatedByNewLine(itemToMove, baseItem);
            CutItemToMove(itemToMove, out int cursorOffset);
            PasteAboveBaseItem(baseItem, separateWithNewLine, cursorOffset);
        }

        /// <summary>
        /// Determines if the two specified items should be separated by a newline.
        /// </summary>
        /// <param name="firstItem">The first item.</param>
        /// <param name="secondItem">The second item.</param>
        /// <returns>True if the items should be separated by a newline, otherwise false.</returns>
        private bool ShouldBeSeparatedByNewLine(BaseCodeItem firstItem, BaseCodeItem secondItem)
        {
            return _blankLineInsertService.ShouldBeFollowedByBlankLine(firstItem) ||
                   _blankLineInsertService.ShouldBePrecededByBlankLine(secondItem);
        }

        /// <summary>
        /// Cut the text and removes the specified item.
        /// </summary>
        /// <param name="itemToRemove">The item to remove.</param>
        /// <param name="cursorOffset">
        /// The cursor's offset within the item being removed, otherwise -1.
        /// </param>
        private static void CutItemToMove(BaseCodeItem itemToRemove, out int cursorOffset)
        {
            var removeStartPoint = itemToRemove.StartPoint;
            var removeEndPoint = itemToRemove.EndPoint;

            // Determine the cursor's offset if within the item being removed.
            var cursorAbsoluteOffset = removeStartPoint.Parent.Selection.ActivePoint.AbsoluteCharOffset;
            if (cursorAbsoluteOffset >= removeStartPoint.AbsoluteCharOffset && cursorAbsoluteOffset <= removeEndPoint.AbsoluteCharOffset)
            {
                cursorOffset = cursorAbsoluteOffset - removeStartPoint.AbsoluteCharOffset;
            }
            else
            {
                cursorOffset = -1;
            }

            // Capture the text and cleanup whitespace.
            removeStartPoint.Cut(removeEndPoint);
            removeStartPoint.DeleteWhitespace(vsWhitespaceOptions.vsWhitespaceOptionsVertical);
        }

        /// <summary>
        /// Paste the content in the clipboard above the position of base item
        /// </summary>
        /// <param name="baseItem"></param>
        /// <param name="separateWithNewLine"></param>
        /// <param name="cursorOffset"></param>
        private static void PasteAboveBaseItem(BaseCodeItem baseItem, bool separateWithNewLine, int cursorOffset)
        {
            var baseStartPoint = baseItem.StartPoint;
            var pastePoint = baseStartPoint.CreateEditPoint();

            pastePoint.Paste();
            pastePoint.Insert(Environment.NewLine);
            if (separateWithNewLine)
            {
                pastePoint.Insert(Environment.NewLine);
            }

            pastePoint.EndOfLine();
            baseStartPoint.SmartFormat(pastePoint);

            if (cursorOffset >= 0)
            {
                baseStartPoint.Parent.Selection.MoveToAbsoluteOffset(baseStartPoint.AbsoluteCharOffset + cursorOffset);
            }
        }

        /// <summary>
        /// Gets the set of reorganizable code item elements from the specified set of code items.
        /// </summary>
        /// <param name="codeItems">The code items.</param>
        /// <returns>The set of reorganizable code item elements.</returns>
        private static IList<BaseCodeItemElement> GetReorganizableCodeItemElements(IEnumerable<BaseCodeItem> codeItems)
        {
            // Get all code item elements.
            var codeItemElements = codeItems.OfType<BaseCodeItemElement>().ToList();

            // Sort the items, pulling out the first item in a set if there are items sharing a definition (ex: fields).
            codeItemElements = codeItemElements.GroupBy(item => item.StartOffset).Select(y => y.First()).OrderBy(z => z.StartOffset).ToList();

            return codeItemElements;
        }

        /// <summary>
        /// Determines if the specified item's children should be reorganized.
        /// </summary>
        /// <param name="parent">The parent item.</param>
        /// <returns>True if the parent's children should be reorganized, otherwise false.</returns>
        private bool ShouldReorganizeChildren(BaseCodeItemElement parent)
        {
            // Enumeration values should never be reordered.
            if (parent is CodeItemEnum)
            {
                return false;
            }

            var parentAttributes = parent.Attributes;
            if (parentAttributes is object)
            {
                // Some attributes indicate that order is critical and should not be reordered.
                var attributesToIgnore = new[]
                {
                    "System.Runtime.InteropServices.ComImportAttribute",
                    "System.Runtime.InteropServices.StructLayoutAttribute"
                };

                if (parentAttributes.OfType<CodeAttribute>().Any(x => attributesToIgnore.Contains(x.FullName)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
