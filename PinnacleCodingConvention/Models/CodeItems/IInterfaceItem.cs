namespace PinnacleCodingConvention.Models.CodeItems
{
    /// <summary>
    /// Represents an item that can implement an interface member.
    /// </summary>
    internal interface IInterfaceItem
    {
        /// <summary>
        /// Gets a flag indicating if this is an explicit interface implementation.
        /// </summary>
        bool IsExplicitInterfaceImplementation { get; }
    }
}