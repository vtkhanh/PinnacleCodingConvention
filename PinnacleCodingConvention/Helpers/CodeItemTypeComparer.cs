using EnvDTE;
using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;

namespace PinnacleCodingConvention.Helpers
{
    /// <summary>
    /// A helper for comparing code items by type, access level, etc.
    /// </summary>
    internal class CodeItemTypeComparer : Comparer<BaseCodeItem>
    {
        private readonly bool _primaryOrderByAccessLevel;
        private readonly bool _secondaryOrderByName;

        internal CodeItemTypeComparer() : this(false, true) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeItemTypeComparer"/> class.
        /// </summary>
        /// <param name="secondaryOrderByName">Determines whether a secondary sort by name is performed or not.</param>
        internal CodeItemTypeComparer(bool primaryOrderByAccessLevel, bool secondaryOrderByName)
        {
            _primaryOrderByAccessLevel = primaryOrderByAccessLevel;
            _secondaryOrderByName = secondaryOrderByName;
        }

        /// <summary>
        /// Performs a comparison of two objects of the same type and returns a value indicating
        /// whether one object is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Less than zero: <paramref name="x" /> is less than <paramref name="y" />.
        /// Zero: <paramref name="x" /> equals <paramref name="y" />.
        /// Greater than zero: <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public override int Compare(BaseCodeItem x, BaseCodeItem y)
        {
            int first = CalculateNumericRepresentation(x);
            int second = CalculateNumericRepresentation(y);

            if (first == second)
            {
                // Check if secondary sort by name should occur.
                if (_secondaryOrderByName)
                {
                    int nameComparison = NormalizeName(x).CompareTo(NormalizeName(y));
                    if (nameComparison != 0)
                        return nameComparison;
                }

                // Fall back to position comparison for matching elements.
                return x.StartOffset.CompareTo(y.StartOffset);
            }

            return first.CompareTo(second);
        }

        private int CalculateNumericRepresentation(BaseCodeItem codeItem)
        {
            int typeOffset = CalculateTypeOffset(codeItem);
            int accessOffset = CalculateAccessOffset(codeItem);
            int constantOffset = CalculateConstantOffset(codeItem);
            int staticOffset = CalculateStaticOffset(codeItem);
            int readOnlyOffset = CalculateReadOnlyOffset(codeItem);

            int calc = 0;

            if (!_primaryOrderByAccessLevel)
            {
                calc += typeOffset * 10000;
                calc += accessOffset * 1000;
            }
            else
            {
                calc += accessOffset * 10000;
                calc += typeOffset * 1000;
            }

            calc += (constantOffset * 100) + (staticOffset * 10) + readOnlyOffset;

            return calc;
        }

        private static int CalculateTypeOffset(BaseCodeItem codeItem)
        {
            var itemsOrder = new List<KindCodeItem> 
            {
                KindCodeItem.Class,
                KindCodeItem.Interface,
                KindCodeItem.Struct,
                KindCodeItem.Constructor,
                KindCodeItem.Delegate,
                KindCodeItem.Event,
                KindCodeItem.Enum,
                KindCodeItem.Field,
                KindCodeItem.Method,
                KindCodeItem.Property
            };
            return itemsOrder.IndexOf(codeItem.Kind) + 1;
        }

        private static int CalculateAccessOffset(BaseCodeItem codeItem)
        {
            var codeItemElement = codeItem as BaseCodeItemElement;
            if (codeItemElement == null) 
                return 0;

            var itemsOrder = new List<vsCMAccess>
            {
                vsCMAccess.vsCMAccessPublic,
                vsCMAccess.vsCMAccessAssemblyOrFamily,
                vsCMAccess.vsCMAccessProject,
                vsCMAccess.vsCMAccessProjectOrProtected,
                vsCMAccess.vsCMAccessProtected,
                vsCMAccess.vsCMAccessPrivate
            };

            return itemsOrder.IndexOf(codeItemElement.Access) + 1;
        }

        private static int CalculateConstantOffset(BaseCodeItem codeItem)
        {
            var codeItemField = codeItem as CodeItemField;
            if (codeItemField == null) 
                return 0;

            return codeItemField.IsConstant ? 0 : 1;
        }

        private static int CalculateStaticOffset(BaseCodeItem codeItem)
        {
            var codeItemElement = codeItem as BaseCodeItemElement;
            if (codeItemElement == null) 
                return 0;

            return codeItemElement.IsStatic ? 0 : 1;
        }

        private static int CalculateReadOnlyOffset(BaseCodeItem codeItem)
        {
            var codeItemField = codeItem as CodeItemField;
            if (codeItemField == null) 
                return 0;

            return codeItemField.IsReadOnly ? 0 : 1;
        }

        private static string NormalizeName(BaseCodeItem codeItem)
        {
            string name = codeItem.Name;
            var interfaceItem = codeItem as IInterfaceItem;

            if ((interfaceItem != null) && interfaceItem.IsExplicitInterfaceImplementation)
            {
                // Try to find where the interface ends and the method starts
                int dotPosition = name.LastIndexOf('.') + 1;

                if (0 < dotPosition && dotPosition < name.Length)
                    return name.Substring(dotPosition);
            }

            return name;
        }
    }
}
