using System;

namespace PinnacleCodingConvention.Common
{
    internal static class PackageGuid
    {
        public const string PinnacleCodingConventionPackageString = "2aef4ddf-562d-4264-9929-24f3be06c921";
        public const string PinnacleCodingConventionOutputPaneString = "baf6f569-4d9d-4d08-9cf9-6b98cd7a47b1";
        public const string CleanUpCommandString = "a35bce14-852c-484e-a8c0-88371dbc25ad";

        public static Guid CleanUpCommandGuid = new Guid(CleanUpCommandString);
        public static Guid PinnacleCodingConventionOutputPaneGuid = new Guid(PinnacleCodingConventionOutputPaneString);
    }
}
