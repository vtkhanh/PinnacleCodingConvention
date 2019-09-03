using EnvDTE;
using System;
using System.Collections.Generic;

namespace PinnacleCodingConvention.Models.CodeItems
{
    /// <summary>
    /// A base class representation of all code items that have an underlying VSX CodeElement and
    /// contain children.
    /// </summary>
    internal abstract class BaseCodeItemElementParent : BaseCodeItemElement, ICodeItemParent
    {
        protected Lazy<string> _Namespace;
        private bool _isExpanded = true;

        /// <summary>
        /// Abstract initialization code for <see cref="BaseCodeItemElementParent" />.
        /// </summary>
        protected BaseCodeItemElementParent()
        {
            Children = new List<BaseCodeItem>();

            _Namespace = new Lazy<string>(() => null);
        }

        /// <summary>
        /// Loads all lazy initialized values immediately.
        /// </summary>
        public override void LoadLazyInitializedValues()
        {
            base.LoadLazyInitializedValues();
            _ = Namespace;
        }

        /// <summary>
        /// An event raised when the IsExpanded state has changed.
        /// </summary>
        public event EventHandler IsExpandedChanged;

        /// <summary>
        /// Gets the children of this code item, may be empty.
        /// </summary>
        public List<BaseCodeItem> Children { get; private set; }

        /// <summary>
        /// Gets the insert point, may be null.
        /// </summary>
        public EditPoint InsertPoint => CodeElement?.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();

        /// <summary>
        /// Gets or sets the flag indicating if this parent item is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
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
        /// Gets the namespace.
        /// </summary>
        public string Namespace => _Namespace.Value;

    }
}