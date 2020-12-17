using EnvDTE;
using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Services
{
    internal class CodeRegionService
    {
        private static CodeRegionService _instance;

        private CodeRegionService()
        {
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

            var start = region.StartPoint.CreateEditPoint();
            start.StartOfLine();
            start.Delete(start.LineLength);
            start.DeleteWhitespace(vsWhitespaceOptions.vsWhitespaceOptionsVertical);
        }
    }
}
