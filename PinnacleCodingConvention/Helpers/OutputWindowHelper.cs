using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PinnacleCodingConvention.Common;
using PinnacleCodingConvention.Models.CodeItems;

namespace PinnacleCodingConvention.Helpers
{
    internal static class OutputWindowHelper
    {
        private const string OUTPUT_PANE_TITLE = "Pinnacle Coding Convention";
        private static IVsOutputWindowPane _outputWindowPane;

        private static IVsOutputWindowPane OutputWindowPane => _outputWindowPane ?? (_outputWindowPane = GetOutputPane());

        internal static void WriteError(string message)
        {
            WriteLine(Resource.Error.ToUpper(), message);
        }

        internal static void WriteWarning(string message)
        {
            WriteLine(Resource.Warning, message);
        }

        internal static void WriteInfo(string message)
        {
            WriteLine(Resource.Info, message);
        }

        internal static void PrintCodeItems(IEnumerable<BaseCodeItem> codeItems)
        {
            foreach (var codeItem in codeItems)
            {
                WriteInfo($"{codeItem.Kind} {codeItem.Name} start: {codeItem.StartLine} end: {codeItem.EndLine}");

                if (codeItem is ICodeItemParent parent)
                {
                    PrintCodeItems(parent.Children);
                }
            }
        }

        private static void WriteLine(string category, string message)
        {
            if (OutputWindowPane is object)
            {
                string outputMessage = $"[{DateTime.Now.ToString("hh:mm:ss tt")}] {category}: {message}{Environment.NewLine}";
                OutputWindowPane.OutputString(outputMessage);
            }
        }

        private static IVsOutputWindowPane GetOutputPane()
        {
            if (!(Package.GetGlobalService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow))
                return null;

            Guid outputPaneGuid = PackageGuids.PinnacleCodingConventionOutputPane;

            outputWindow.CreatePane(ref outputPaneGuid, OUTPUT_PANE_TITLE, fInitVisible: 1, fClearWithSolution: 1);
            outputWindow.GetPane(ref outputPaneGuid, out IVsOutputWindowPane windowPane);

            return windowPane;
        }

    }
}
