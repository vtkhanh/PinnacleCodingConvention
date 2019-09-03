using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    internal class CodeTreeBuilder
    {
        private static CodeTreeBuilder _instance;

        private CodeTreeBuilder() { }

        internal static CodeTreeBuilder GetInstance() => _instance ?? (_instance = new CodeTreeBuilder());

        internal IEnumerable<BaseCodeItem> Build(IEnumerable<BaseCodeItem> codeItems)
        {
            var codeTree = new List<BaseCodeItem>();

            if (codeItems != null)
            {
                // Sort the raw list of code items by starting position.
                var orderedCodeItems = codeItems.OrderBy(x => x.StartOffset);
                var codeItemStack = new Stack<BaseCodeItem>();

                foreach (var codeItem in orderedCodeItems)
                {
                    while (true)
                    {
                        if (!codeItemStack.Any())
                        {
                            codeTree.Add(codeItem);
                            codeItemStack.Push(codeItem);
                            break;
                        }

                        var top = codeItemStack.Peek();
                        if (codeItem.EndOffset < top.EndOffset)
                        {
                            if (top is ICodeItemParent topParent)
                            {
                                topParent.Children.Add(codeItem);
                                codeItemStack.Push(codeItem);
                                break;
                            }

                            if (codeItem is CodeItemRegion)
                            {
                                // Skip regions within non-parentable items (e.g. in methods).
                                break;
                            }
                        }

                        codeItemStack.Pop();
                    }
                }
            }

            return codeTree;
        }
    }
}
