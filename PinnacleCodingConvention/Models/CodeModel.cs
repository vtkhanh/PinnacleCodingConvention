using EnvDTE;
using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;

namespace PinnacleCodingConvention.Models
{
    /// <summary>
    /// This class encapsulates the representation of a document, including its code items and
    /// current state.
    /// </summary>
    internal class CodeModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeModel" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        internal CodeModel(Document document)
        {
            CodeItems = new List<BaseCodeItem>();
            Document = document;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        internal Document Document { get; }

        /// <summary>
        /// Gets or sets the code items.
        /// </summary>
        internal IList<BaseCodeItem> CodeItems { get; set; }
    }
}
