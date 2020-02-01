using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Options;
using System;
using System.Runtime.InteropServices;

namespace PinnacleCodingConvention
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(DialogPageProvider.General), "Pinnacle Coding Convention", "General", 0, 0, true)]
    [Guid(PackageGuids.PinnacleCodingConventionPackageString)]
    [ProvideBindingPath]
    public sealed class PinnacleCodingConventionPackage : AsyncPackage
    {
    }
}
