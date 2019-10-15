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
            throw new NotImplementedException();
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
