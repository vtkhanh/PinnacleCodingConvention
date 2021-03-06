using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;

namespace PinnacleCodingConvention.Models.CodeItems
{
    /// <summary>
    /// A base class representation of all code items. Includes VSX supported CodeElements as well
    /// as code regions.
    /// </summary>
    [DebuggerDisplay("{GetType().Name,nq}: {Name}")]
    internal abstract class BaseCodeItem : ICodeItem
    {
        /// <summary>
        /// Gets the kind.
        /// </summary>
        public abstract KindCodeItem Kind { get; }

        /// <summary>
        /// Gets or sets the name, may be empty.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start line.
        /// </summary>
        public int StartLine { get; set; }

        /// <summary>
        /// Gets or sets the start offset.
        /// </summary>
        public int StartOffset { get; set; }

        /// <summary>
        /// Gets or sets the start point, may be null.
        /// </summary>
        public virtual EditPoint StartPoint { get; set; }

        /// <summary>
        /// Gets or sets the end line.
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        /// Gets or sets the end offset.
        /// </summary>
        public int EndOffset { get; set; }

        /// <summary>
        /// Gets or sets the end point, may be null.
        /// </summary>
        public virtual EditPoint EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the code region of the element
        /// </summary>
        public CodeItemRegion AssociatedCodeRegion { get; set; }

        /// <summary>
        /// Gets a flag indicating if this is a code item that spans multiple lines.
        /// </summary>
        public bool IsMultiLine => StartPoint != null && EndPoint != null && StartPoint.Line != EndPoint.Line;

        /// <summary>
        /// Loads all lazy initialized values immediately.
        /// </summary>
        public virtual void LoadLazyInitializedValues()
        {
        }

        /// <summary>
        /// Refreshes the cached position and name fields on this item.
        /// </summary>
        public virtual void RefreshCachedPositionAndName()
        {
            StartLine = StartPoint.Line;
            StartOffset = StartPoint.AbsoluteCharOffset;
            EndLine = EndPoint.Line;
            EndOffset = EndPoint.AbsoluteCharOffset;
        }
    }
}