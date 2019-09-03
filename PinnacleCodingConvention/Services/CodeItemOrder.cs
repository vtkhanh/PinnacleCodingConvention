using System;
using System.Collections.Generic;
using System.Linq;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models;
using PinnacleCodingConvention.Models.CodeItems;

namespace PinnacleCodingConvention.Services
{
    internal class CodeItemOrder
    {
        private static CodeItemOrder _instance;
        private CodeTreeBuilder _codeTreeBuilder;

        private CodeItemOrder()
        {
            _codeTreeBuilder = CodeTreeBuilder.GetInstance();
        }

        public static CodeItemOrder GetInstance() => _instance ?? (_instance = new CodeItemOrder());

        internal IEnumerable<BaseCodeItem> Order(IEnumerable<BaseCodeItem> codeItems, CodeSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case CodeSortOrder.Alpha:
                    return OrderByAlphabetical(codeItems);
                case CodeSortOrder.Type:
                    return OrderByType(codeItems);
                default:
                    return codeItems;
            }
        }
        private IEnumerable<BaseCodeItem> OrderByAlphabetical(IEnumerable<BaseCodeItem> codeItems)
        {
            var orderedCodeItems = new List<BaseCodeItem>();

            if (codeItems is object)
            {
                var codeItemsWithoutRegions = codeItems.Where(x => !(x is CodeItemRegion));

                var structuredCodeItems = _codeTreeBuilder.Build(codeItemsWithoutRegions);
                orderedCodeItems.AddRange(structuredCodeItems);

                Sort(orderedCodeItems, new CodeItemNameComparer());
            }

            return orderedCodeItems;
        }

        private IEnumerable<BaseCodeItem> OrderByType(IEnumerable<BaseCodeItem> codeItems)
        {
            throw new NotImplementedException();
        }

        private void Sort(List<BaseCodeItem> codeItems, IComparer<BaseCodeItem> comparer)
        {
            codeItems.Sort(comparer);

            foreach (var codeItem in codeItems.OfType<ICodeItemParent>())
            {
                Sort(codeItem.Children, comparer);
            }
        }

    }
}
