using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PinnacleCodingConvention.Common;

namespace PinnacleCodingConvention.Helpers
{
    internal static class OutputWindowHelper
    {
        private const string OUTPUT_PANE_TITLE = "Pinnacle Coding Convention";
        private static IVsOutputWindowPane _outputWindowPane;

        private static IVsOutputWindowPane OutputWindowPane => _outputWindowPane ?? (_outputWindowPane = GetOutputPane());

        public static void WriteLine(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (OutputWindowPane is object)
            {
                string outputMessage = $"[{DateTime.Now.ToString("hh:mm:ss tt")}] {message}{Environment.NewLine}";
                OutputWindowPane.OutputString(outputMessage);
            }
        }

        private static IVsOutputWindowPane GetOutputPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(Package.GetGlobalService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow))
                return null;

            Guid outputPaneGuid = PackageGuid.PinnacleCodingConventionOutputPaneGuid;

            outputWindow.CreatePane(ref outputPaneGuid, OUTPUT_PANE_TITLE, fInitVisible: 1, fClearWithSolution: 1);
            outputWindow.GetPane(ref outputPaneGuid, out IVsOutputWindowPane windowPane);

            return windowPane;
        }

    }
}
