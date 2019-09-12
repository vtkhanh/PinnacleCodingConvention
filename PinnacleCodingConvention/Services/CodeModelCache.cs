using EnvDTE;
using PinnacleCodingConvention.Helpers;
using System.Collections.Generic;
using CodeModel = PinnacleCodingConvention.Models.CodeModel;

namespace PinnacleCodingConvention.Services
{
    /// <summary>
    /// A class for encapsulating a cache of code models.
    /// </summary>
    internal class CodeModelCache
    {
        private static CodeModelCache _instance;
        private readonly Dictionary<string, CodeModel> _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeModelCache" /> class.
        /// </summary>
        private CodeModelCache() => _cache = new Dictionary<string, CodeModel>();

        internal static CodeModelCache GetInstance() => _instance ?? (_instance = new CodeModelCache());

        /// <summary>
        /// Gets a code model for the specified document. If the code model is not present in the
        /// cache, a new code model will be generated and added to the cache.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A code model representing the document.</returns>
        internal CodeModel GetCodeModel(Document document)
        {
            CodeModel codeModel;

            OutputWindowHelper.WriteInfo($"CodeModelCache.GetCodeModel for '{document.FullName}'");

            lock (_cache)
            {
                if (!_cache.TryGetValue(document.FullName, out codeModel))
                {
                    codeModel = new CodeModel(document) { IsStale = true };

                    _cache.Add(document.FullName, codeModel);
                    OutputWindowHelper.WriteInfo("  --added to cache (stale).");
                }
                else
                {
                    OutputWindowHelper.WriteInfo(codeModel.IsStale
                        ? "  --retrieved from cache (stale)."
                        : "  --retrieved from cache (not stale).");
                }
            }

            return codeModel;
        }

        /// <summary>
        /// Removes the code model associated with the specified document if it exists.
        /// </summary>
        /// <param name="document">The document.</param>
        internal void RemoveCodeModel(Document document)
        {
            lock (_cache)
            {
                if (_cache.Remove(document.FullName))
                {
                    OutputWindowHelper.WriteInfo($"CodeModelCache.RemoveCodeModel from cache for '{document.FullName}'");
                }
            }
        }

        /// <summary>
        /// Marks the code model associated with the specified document as stale if it exists.
        /// </summary>
        /// <param name="document">The document.</param>
        internal void StaleCodeModel(Document document)
        {
            if (_cache.TryGetValue(document.FullName, out CodeModel codeModel))
            {
                codeModel.IsStale = true;
                OutputWindowHelper.WriteInfo($"CodeModelCache.StaleCodeModel in cache for '{document.FullName}'");
            }
        }
    }
}
