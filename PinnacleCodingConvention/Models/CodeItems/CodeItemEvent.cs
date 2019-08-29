using EnvDTE;
using EnvDTE80;
using PinnacleCodingConvention.Helpers;
using System;

namespace PinnacleCodingConvention.Models.CodeItems
{
    /// <summary>
    /// The representation of a code event.
    /// </summary>
    internal class CodeItemEvent : BaseCodeItemElement, IInterfaceItem
    {
        private readonly Lazy<bool> _isExplicitInterfaceImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeItemEvent" /> class.
        /// </summary>
        public CodeItemEvent()
        {
            // Make exceptions for explicit interface implementations - which report private access
            // but really do not have a meaningful access level.
            _Access = LazyTryDefault(() => CodeEvent != null && !IsExplicitInterfaceImplementation ? CodeEvent.Access : vsCMAccess.vsCMAccessPublic);

            _Attributes = LazyTryDefault(() => CodeEvent?.Attributes);

            _DocComment = LazyTryDefault(() => CodeEvent?.DocComment);

            _isExplicitInterfaceImplementation = LazyTryDefault(() => CodeEvent != null && ExplicitInterfaceImplementationHelper.IsExplicitInterfaceImplementation(CodeEvent));

            _IsStatic = LazyTryDefault(() => CodeEvent != null && CodeEvent.IsShared);

            _TypeString = LazyTryDefault(() => CodeEvent?.Type?.AsString);
        }

        /// <summary>
        /// Gets the kind.
        /// </summary>
        public override KindCodeItem Kind => KindCodeItem.Event;

        /// <summary>
        /// Loads all lazy initialized values immediately.
        /// </summary>
        public override void LoadLazyInitializedValues()
        {
            base.LoadLazyInitializedValues();
            _ = IsExplicitInterfaceImplementation;
        }

        /// <summary>
        /// Gets or sets the VSX CodeEvent.
        /// </summary>
        public CodeEvent CodeEvent { get; set; }

        /// <summary>
        /// Gets a flag indicating if this property is an explicit interface implementation.
        /// </summary>
        public bool IsExplicitInterfaceImplementation => _isExplicitInterfaceImplementation.Value;
    }
}