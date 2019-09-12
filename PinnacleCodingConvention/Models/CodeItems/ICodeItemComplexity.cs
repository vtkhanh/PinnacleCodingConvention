namespace PinnacleCodingConvention.Models.CodeItems
{
    /// <summary>
    /// An interface for code items that support complexity calculations.
    /// </summary>
    internal interface ICodeItemComplexity : ICodeItem
    {
        /// <summary>
        /// Gets the complexity.
        /// </summary>
        int Complexity { get; }
    }
}