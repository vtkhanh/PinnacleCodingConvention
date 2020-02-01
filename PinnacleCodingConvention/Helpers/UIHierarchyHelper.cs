using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Helpers
{
    /// <summary>
    /// A static helper class for working with the UI hierarchies.
    /// </summary>
    internal static class UIHierarchyHelper
    {
        /// <summary>
        /// Causes the given item and all of its expanded children to be collapsed. This may cause
        /// selections to change.
        /// </summary>
        /// <param name="parentItem">The parent item to collapse from.</param>
        internal static void CollapseRecursively(UIHierarchyItem parentItem)
        {
            if (parentItem == null)
                throw new ArgumentNullException(nameof(parentItem));

            if (!parentItem.UIHierarchyItems.Expanded) 
                return;

            // Recurse to all children first.
            foreach (UIHierarchyItem childItem in parentItem.UIHierarchyItems)
            {
                CollapseRecursively(childItem);
            }

            if (ShouldCollapseItem(parentItem))
            {
                // Attempt the direct collapse first.
                parentItem.UIHierarchyItems.Expanded = false;

                // If failed, solution folder oddity may be at play. Try an alternate path.
                if (parentItem.UIHierarchyItems.Expanded)
                {
                    parentItem.Select(vsUISelectionType.vsUISelectionTypeSelect);
                    ((DTE2)parentItem.DTE).ToolWindows.SolutionExplorer.DoDefaultAction();
                }
            }
        }

        /// <summary>
        /// Determines whether the specified item has any expanded children.
        /// </summary>
        /// <param name="parentItem">The parent item.</param>
        /// <returns>True if there are expanded children, false otherwise.</returns>
        internal static bool HasExpandedChildren(UIHierarchyItem parentItem)
        {
            if (parentItem == null)
                throw new ArgumentNullException(nameof(parentItem));

            return parentItem.UIHierarchyItems.Cast<UIHierarchyItem>().Any(childItem => childItem.UIHierarchyItems.Expanded || HasExpandedChildren(childItem));
        }

        /// <summary>
        /// Determines if the specified parent item should be collapsed.
        /// </summary>
        /// <param name="parentItem">The parent item.</param>
        /// <returns>True if the item should be collapsed, otherwise false.</returns>
        private static bool ShouldCollapseItem(UIHierarchyItem parentItem)
        {
            // Make sure not to collapse the solution, causes odd behavior.
            if (parentItem.Object is Solution)
            {
                return false;
            }

            // Conditionally skip collapsing the only project in a solution.
            // Note: Visual Studio automatically creates a second invisible project called
            //       "Miscellaneous files".
            if (parentItem.Object is Project)
            {
                var solution = parentItem.DTE.Solution;

                if (solution != null && solution.Projects.OfType<Project>().All(x => x == parentItem.Object || x.Name == "Miscellaneous Files"))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
