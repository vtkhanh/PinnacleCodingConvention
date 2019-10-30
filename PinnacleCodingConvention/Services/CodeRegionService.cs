using EnvDTE;
using PinnacleCodingConvention.Common;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    internal class CodeRegionService
    {
        private static CodeRegionService _instance;

        private CodeRegionService() { }

        public static CodeRegionService GetInstance() => _instance ?? (_instance = new CodeRegionService());

        public IEnumerable<BaseCodeItem> CleanupExistingRegions(IEnumerable<BaseCodeItem> codeItems)
        {
            // Remove in descending order to reduce line number updates.
            var regionsToClean = codeItems
                .Where(codeItem => codeItem.Kind == KindCodeItem.Region)
                .OrderByDescending(region => region.StartLine);
            foreach (var region in regionsToClean)
            {
                RemoveRegion(region as CodeItemRegion);
            }

            return codeItems.Where(codeItem => codeItem.Kind != KindCodeItem.Region);
        }

        public IEnumerable<BaseCodeItem> AddRequiredRegions(IEnumerable<BaseCodeItem> codeItems, ICodeItemParent parent = null)
        {
            if (parent is null || parent.Kind != KindCodeItem.Interface)
            {
                // Regions to each Method
                AddRegionsToCodeItems(codeItems.Where(item => item.Kind == KindCodeItem.Method || item.Kind == KindCodeItem.TestMethod));
                // Regions to each Property
                AddRegionsToCodeItems(codeItems.Where(item => item.Kind == KindCodeItem.Property && item.StartLine != item.EndLine));
            }
            //Region to Class Variables
            AddBlockRegion(codeItems, KindCodeItem.Field, Resource.ClassVariablesRegion);
            // Region to Constructors
            AddBlockRegion(codeItems, KindCodeItem.Constructor, Resource.ConstructorsRegion);
            // Region to Methods
            AddBlockRegion(codeItems, KindCodeItem.Method, Resource.MethodsRegion);
            // Region to Properties
            AddBlockRegion(codeItems, KindCodeItem.Property, Resource.PropertiesRegion);
            // Region to Test Methods
            AddBlockRegion(codeItems, KindCodeItem.TestMethod, Resource.TestsRegion);
            // Region to Classes
            AddClassRegions(codeItems);

            foreach (var item in codeItems)
            {
                if (item is ICodeItemParent codeItemParent && codeItemParent.Children.Any())
                {
                    AddRequiredRegions(codeItemParent.Children, codeItemParent);
                }
            }

            return codeItems;
        }

        private void AddClassRegions(IEnumerable<BaseCodeItem> codeItems)
        {
            var codeItemClasses = codeItems.Where(item => HasClassRegion(item.Kind));
            foreach (var codeItemClass in codeItemClasses)
            {
                InsertRegionTag($": {codeItemClass.Name} :", codeItemClass.StartPoint);
                InsertEndRegionTag(codeItemClass.EndPoint);
            }

            bool HasClassRegion(KindCodeItem kind) => kind == KindCodeItem.Class || kind == KindCodeItem.Struct || kind == KindCodeItem.Interface;
        }

        private void AddBlockRegion(IEnumerable<BaseCodeItem> codeItems, KindCodeItem kind, string regionName)
        {
            var codeItemsByKind = codeItems.Where(item => item.Kind == kind).OrderBy(item => item.StartPoint.Line);
            if (!codeItemsByKind.Any())
            {
                return;
            }

            InsertRegionTag(regionName, codeItemsByKind.First().StartPoint);
            InsertEndRegionTag(codeItemsByKind.Last().EndPoint);
        }

        private void AddRegionsToCodeItems(IEnumerable<BaseCodeItem> codeItems)
        {
            var groups = codeItems.GroupBy(item => item.Name);
            foreach (var group in groups)
            {
                foreach (var codeItem in group)
                {
                    var regionName = group.Count() > 1 && codeItem is CodeItemMethod codeItemMethod
                        ? $"{group.Key}({string.Join(", ", codeItemMethod.Parameters.Select(GetParameterText))})"
                        : group.Key;
                    var regionStartPoint = InsertRegionTag(regionName, codeItem.StartPoint);
                    var regionEndPoint = InsertEndRegionTag(codeItem.EndPoint);
                    codeItem.AssociatedCodeRegion = new CodeItemRegion
                    {
                        Name = regionName,
                        StartPoint = regionStartPoint,
                        EndPoint = regionEndPoint
                    };
                }

                // Add Block region (eg: for overloading methods)
                if (group.Count() > 1)
                {
                    var sortedItems = group.ToList().OrderBy(item => item.StartPoint.Line);
                    InsertRegionTag($"{group.Key}...", sortedItems.First().StartPoint);
                    InsertEndRegionTag(sortedItems.Last().EndPoint);
                }
            }
        }

        private static string GetParameterText(CodeParameter param) => $"{param.GetStartPoint().CreateEditPoint().GetText(param.GetEndPoint())}";

        private EditPoint InsertRegionTag(string regionName, EditPoint startPoint)
        {
            var cursor = startPoint.CreateEditPoint();

            // If the cursor is not preceeded only by whitespace, insert a new line.
            var firstNonWhitespaceIndex = cursor.GetLine().TakeWhile(char.IsWhiteSpace).Count();
            if (cursor.DisplayColumn > firstNonWhitespaceIndex + 1)
            {
                cursor.Insert(Environment.NewLine);
            }

            // Insert new line if previous line is not a blank line
            if (cursor.GetLines(cursor.Line - 1, cursor.Line).Any(character => !char.IsWhiteSpace(character)))
            {
                cursor.Insert(Environment.NewLine);
            }

            cursor.Insert($"{RegionHelper.GetRegionTagText(cursor, regionName)}{Environment.NewLine}");

            cursor.LineUp();
            cursor.StartOfLine();

            startPoint.SmartFormat(cursor);

            return cursor;
        }

        private EditPoint InsertEndRegionTag(EditPoint endPoint)
        {
            var cursor = endPoint.CreateEditPoint();

            // If the cursor is not preceeded only by whitespace, insert a new line.
            var firstNonWhitespaceIndex = cursor.GetLine().TakeWhile(char.IsWhiteSpace).Count();
            if (cursor.DisplayColumn > firstNonWhitespaceIndex + 1)
            {
                cursor.Insert(Environment.NewLine);
            }

            cursor.Insert($"{RegionHelper.GetEndRegionTagText(cursor)}");

            endPoint.SmartFormat(cursor);

            // Insert new line if next line is not a blank line
            if (cursor.GetLines(cursor.Line + 1, cursor.Line + 2).Any(character => !char.IsWhiteSpace(character)))
            {
                cursor.Insert(Environment.NewLine);
                cursor.LineUp();
                cursor.EndOfLine();
            }

            return cursor;
        }

        private void RemoveRegion(CodeItemRegion region)
        {
            if (region is null || region.IsInvalidated || region.IsPseudoGroup || region.StartLine <= 0 || region.EndLine <= 0)
            {
                return;
            }

            var end = region.EndPoint.CreateEditPoint();
            end.StartOfLine();
            end.Delete(end.LineLength);
            end.DeleteWhitespace(vsWhitespaceOptions.vsWhitespaceOptionsVertical);
            end.Insert(Environment.NewLine);

            var start = region.StartPoint.CreateEditPoint();
            start.StartOfLine();
            start.Delete(start.LineLength);
            start.DeleteWhitespace(vsWhitespaceOptions.vsWhitespaceOptionsVertical);
            start.Insert(Environment.NewLine);

            region.IsInvalidated = true;
        }
    }
}
