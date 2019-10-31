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
        private BlankLineInsertService _blankLineInsertService;

        private CodeRegionService() 
        {
            _blankLineInsertService = BlankLineInsertService.GetInstance();
        }

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

        public void AddRequiredRegions(IEnumerable<BaseCodeItem> codeItems, ICodeItemParent parent)
        {
            if (parent.Kind != KindCodeItem.Interface)
            {
                // Regions to each Method, Test Method, or Property
                AddRegionsToCodeItems(codeItems);
            }
            if (parent.Kind == KindCodeItem.Class || parent.Kind == KindCodeItem.Struct)
            {
                //Region to Class Variables
                AddBlockRegion(codeItems, KindCodeItem.Field, Resource.ClassVariablesRegion);
            }
            // Region to Constructors
            AddBlockRegion(codeItems, KindCodeItem.Constructor, Resource.ConstructorsRegion);
            // Region to Methods
            AddBlockRegion(codeItems, KindCodeItem.Method, Resource.MethodsRegion);
            // Region to Properties
            AddBlockRegion(codeItems, KindCodeItem.Property, Resource.PropertiesRegion);
            // Region to Test Methods
            AddBlockRegion(codeItems, KindCodeItem.TestMethod, Resource.TestsRegion);
            // Region to Classes
            if (parent.Kind == KindCodeItem.Namespace)
            {
                AddClassRegions(codeItems);
            }

            foreach (var item in codeItems)
            {
                if (item is ICodeItemParent codeItemParent && codeItemParent.Children.Any())
                {
                    AddRequiredRegions(codeItemParent.Children, codeItemParent);
                }
            }
        }

        public void InsertBlankLines(IEnumerable<CodeItemRegion> codeItemRegions)
        {
            _blankLineInsertService.InsertPaddingBeforeRegionTags(codeItemRegions);
            _blankLineInsertService.InsertPaddingAfterEndRegionTags(codeItemRegions);
        }

        private void AddClassRegions(IEnumerable<BaseCodeItem> codeItems)
        {
            foreach (var codeItem in codeItems)
            {
                var regionName = $": {codeItem.Name} :";
                InsertRegionTag(regionName, codeItem.StartPoint);
                InsertEndRegionTag(codeItem.EndPoint);
            }
        }

        private void AddBlockRegion(IEnumerable<BaseCodeItem> codeItems, KindCodeItem kind, string regionName)
        {
            var codeItemsByKind = codeItems.Where(item => item.Kind == kind).OrderBy(item => item.StartPoint.Line);
            if (!codeItemsByKind.Any())
            {
                return;
            }

            var firstItem = codeItemsByKind.First();
            var lastItem = codeItemsByKind.Last();
            InsertRegionTag(regionName, firstItem.StartPoint);
            InsertEndRegionTag(lastItem.EndPoint);
        }

        private void AddRegionsToCodeItems(IEnumerable<BaseCodeItem> codeItems)
        {
            var groups = codeItems.Where(HasItemRegion).GroupBy(item => item.Name);
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
                    var regionName = $"{group.Key}...";
                    InsertRegionTag(regionName, sortedItems.First().StartPoint);
                    InsertEndRegionTag(sortedItems.Last().EndPoint);
                }
            }

            bool HasItemRegion(BaseCodeItem codeItem) => 
                codeItem.Kind == KindCodeItem.Method
                || codeItem.Kind == KindCodeItem.TestMethod
                || (codeItem.Kind == KindCodeItem.Property && codeItem.StartLine != codeItem.EndLine);
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

            return cursor;
        }

        private void RemoveRegion(CodeItemRegion region)
        {
            if (region is null)
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
