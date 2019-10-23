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
        private PinnacleCodingConventionPackage _package;
        private static CodeRegionService _instance;

        private CodeRegionService(PinnacleCodingConventionPackage package) => _package = package;

        public static CodeRegionService GetInstance(PinnacleCodingConventionPackage package) => _instance ?? (_instance = new CodeRegionService(package));

        public IEnumerable<BaseCodeItem> Cleanup(IEnumerable<BaseCodeItem> codeItems)
        {
            // 1. Clean all existing regions
            // 2. Then, add mandatory regions
            codeItems = CleanExistingRegions(codeItems);
            codeItems = AddRegions(codeItems);
            return codeItems;
        }

        private IEnumerable<BaseCodeItem> AddRegions(IEnumerable<BaseCodeItem> codeItems)
        {
            AddRegionsToMethods(codeItems.Where(item => item.Kind == KindCodeItem.Method).Select(item => item as CodeItemMethod));
            AddRegionsToProperties(codeItems.Where(item => item.Kind == KindCodeItem.Property && item.StartLine != item.EndLine));

            return codeItems;
        }

        
        private void AddRegionsToMethods(IEnumerable<CodeItemMethod> codeItems)
        {
            var methodGroups = codeItems.GroupBy(item => item.Name);
            foreach (var group in methodGroups)
            {
                foreach (var method in group)
                {
                    var regionName = group.Count() > 1
                        ? $"{group.Key}({string.Join(",", method.Parameters.Select(param => $"{param.Name}"))})"
                        : group.Key;
                    var regionStartPoint = InsertRegionTag(regionName, method.StartPoint);
                    var regionEndPoint = InsertEndRegionTag(method.EndPoint);
                    method.AssociatedCodeRegion = new CodeItemRegion
                    {
                        Name = regionName,
                        StartPoint = regionStartPoint,
                        EndPoint = regionEndPoint
                    };
                }
            }
        }

        private void AddRegionsToProperties(IEnumerable<BaseCodeItem> properties)
        {
            foreach (var property in properties)
            {
                var regionStartPoint = InsertRegionTag(property.Name, property.StartPoint);
                var regionEndPoint = InsertEndRegionTag(property.EndPoint);
                property.AssociatedCodeRegion = new CodeItemRegion
                {
                    Name = property.Name,
                    StartPoint = regionStartPoint,
                    EndPoint = regionEndPoint
                };
            }
        }

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

            startPoint.SmartFormat(cursor);

            cursor.LineUp();
            cursor.StartOfLine();

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
            }

            return cursor;
        }

        private IEnumerable<BaseCodeItem> CleanExistingRegions(IEnumerable<BaseCodeItem> codeItems)
        {
            // Remove in descending order to reduce line number updates.
            var regionsToClean = codeItems.Where(codeItem => codeItem.Kind == KindCodeItem.Region).OrderByDescending(region => region.StartLine);
            foreach (var region in regionsToClean)
            {
                RemoveRegion(region as CodeItemRegion);
            }

            return codeItems.Where(codeItem => codeItem.Kind != KindCodeItem.Region);
        }

        private void RemoveRegion(CodeItemRegion region)
        {
            if (region is null || region.IsInvalidated || region.IsPseudoGroup || region.StartLine <= 0 || region.EndLine <= 0)
            {
                return;
            }

            new UndoTransactionHelper(_package, region.Name).Run(() =>
            {
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
            });
        }
    }
}
