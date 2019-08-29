using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PinnacleCodingConvention.Models.CodeItems
{
    /// <summary>
    /// The representation of a code region.
    /// </summary>
    internal class CodeItemRegion : BaseCodeItem, ICodeItemParent
    {
        private bool _isExpanded = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeItemRegion" /> class.
        /// </summary>
        public CodeItemRegion()
        {
            Children = new List<BaseCodeItem>();
        }

        /// <summary>
        /// Gets the kind.
        /// </summary>
        public override KindCodeItem Kind => KindCodeItem.Region;

        /// <summary>
        /// An event raised when the IsExpanded state has changed.
        /// </summary>
        public event EventHandler IsExpandedChanged;

        /// <summary>
        /// Gets the children of this code item, may be empty.
        /// </summary>
        public IList<BaseCodeItem> Children { get; private set; }

        /// <summary>
        /// Gets the insert point, may be null.
        /// </summary>
        public EditPoint InsertPoint
        {
            get
            {
                var startPoint = StartPoint;
                if (startPoint != null)
                {
                    var insertPoint = startPoint.CreateEditPoint();
                    insertPoint.LineDown();
                    return insertPoint;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the flag indicating if this parent item is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;

                    IsExpandedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets a flag indicating if this region is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (Children.Any())
                    return false;

                var start = StartPoint.CreateEditPoint();
                start.EndOfLine();

                var end = EndPoint.CreateEditPoint();
                end.StartOfLine();

                var text = start.GetText(end);

                return string.IsNullOrWhiteSpace(text);
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if this region has been invalidated.
        /// </summary>
        public bool IsInvalidated { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is a pseudo group.
        /// </summary>
        public bool IsPseudoGroup { get; set; }
    }
}