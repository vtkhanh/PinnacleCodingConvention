using EnvDTE;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Models.CodeItems;
using System.Collections.Generic;
using System.Threading;

namespace PinnacleCodingConvention.Models
{
    /// <summary>
    /// This class encapsulates the representation of a document, including its code items and
    /// current state.
    /// </summary>
    internal class CodeModel
    {
        private bool _isBuilding;
        private bool _isStale;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeModel" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        internal CodeModel(Document document)
        {
            CodeItems = new List<BaseCodeItem>();
            Document = document;
            IsBuiltWaitHandle = new ManualResetEvent(false);
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        internal Document Document { get; }

        /// <summary>
        /// Gets or sets the code items.
        /// </summary>
        internal IList<BaseCodeItem> CodeItems { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this model is currently being built.
        /// </summary>
        internal bool IsBuilding
        {
            get { return _isBuilding; }
            set
            {
                if (_isBuilding != value)
                {
                    OutputWindowHelper.WriteInfo($"CodeModel.IsBuilding changing to '{value}' for '{Document.FullName}'");

                    _isBuilding = value;
                    if (_isBuilding)
                    {
                        IsBuiltWaitHandle.Reset();
                    }
                    else
                    {
                        IsBuiltWaitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a wait handle that will be signaled when building is complete.
        /// </summary>
        internal ManualResetEvent IsBuiltWaitHandle { get; }

        /// <summary>
        /// Gets or sets a flag indicating if this model is stale.
        /// </summary>
        internal bool IsStale
        {
            get { return _isStale; }
            set
            {
                if (_isStale != value)
                {
                    OutputWindowHelper.WriteInfo($"CodeModel.IsStale changing to '{value}' for '{Document.FullName}'");

                    _isStale = value;
                }
            }
        }
    }
}
