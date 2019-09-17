using EnvDTE;
using PinnacleCodingConvention.Helpers;
using System;

namespace PinnacleCodingConvention.Models.CodeItems
{
    /// <summary>
    /// A base class representation of all code items that have an underlying VSX CodeElement.
    /// </summary>
    internal abstract class BaseCodeItemElement : BaseCodeItem
    {
        protected Lazy<vsCMAccess> _Access;
        protected Lazy<CodeElements> _Attributes;
        protected Lazy<string> _DocComment;
        protected Lazy<bool> _IsStatic;
        protected Lazy<string> _TypeString;

        /// <summary>
        /// Abstract initialization code for <see cref="BaseCodeItemElement" />.
        /// </summary>
        protected BaseCodeItemElement()
        {
            _Access = new Lazy<vsCMAccess>();
            _Attributes = new Lazy<CodeElements>(() => null);
            _DocComment = new Lazy<string>(() => null);
            _IsStatic = new Lazy<bool>();
            _TypeString = new Lazy<string>(() => null);
        }

        /// <summary>
        /// Gets the start point adjusted for leading comments, may be null.
        /// </summary>
        public override EditPoint StartPoint => GetStartPoint();


        /// <summary>
        /// Gets the end point, may be null.
        /// </summary>
        public override EditPoint EndPoint => GetEndPoint();

        /// <summary>
        /// Loads all lazy initialized values immediately.
        /// </summary>
        public override void LoadLazyInitializedValues()
        {
            base.LoadLazyInitializedValues();
            _ = Access;
            _ = Attributes;
            _ = DocComment;
            _ = IsStatic;
            _ = TypeString;
        }

        /// <summary>
        /// Refreshes the cached position and name fields on this item.
        /// </summary>
        public override void RefreshCachedPositionAndName()
        {
            var startPoint = CodeElement.GetStartPoint();
            var endPoint = CodeElement.GetEndPoint();

            StartLine = startPoint.Line;
            StartOffset = startPoint.AbsoluteCharOffset;
            EndLine = endPoint.Line;
            EndOffset = endPoint.AbsoluteCharOffset;
            Name = CodeElement.Name;
        }

        /// <summary>
        /// Gets or sets the code element, may be null.
        /// </summary>
        public CodeElement CodeElement { get; set; }

        /// <summary>
        /// Gets the access level.
        /// </summary>
        public vsCMAccess Access => _Access.Value;

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        public CodeElements Attributes => _Attributes.Value;

        /// <summary>
        /// Gets the doc comment.
        /// </summary>
        public string DocComment => _DocComment.Value;

        /// <summary>
        /// Gets a flag indicating if this instance is static.
        /// </summary>
        public bool IsStatic => _IsStatic.Value;

        /// <summary>
        /// Gets the type string.
        /// </summary>
        public string TypeString => _TypeString.Value;

        /// <summary>
        /// Creates a lazy initializer wrapping TryDefault around the specified function.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <returns>A lazy initializer for the specified function.</returns>
        protected static Lazy<T> LazyTryDefault<T>(Func<T> func)
        {
            return new Lazy<T>(() => TryDefault(func));
        }

        /// <summary>
        /// Tries to execute the specified function on a background thread, returning the default of
        /// the type on error or timeout.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="func">The function to execute.</param>
        /// <returns>The result of the function, otherwise the default for the result type.</returns>
        protected static T TryDefault<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                OutputWindowHelper.WriteError($"TryDefault caught an exception on '{func}': {ex}");

                return default;
            }
        }

        /// <summary>
        /// Get StartPoint adjusted for leading comment & its region if any
        /// </summary>
        /// <returns></returns>
        private EditPoint GetStartPoint()
        {
            if (CodeElement != null)
            {
                var startPointAdjustedForComment = GetStartPointAdjustedForComments(CodeElement.GetStartPoint());
                if (AssociatedCodeRegion is object && AssociatedCodeRegion.StartPoint.Line < startPointAdjustedForComment.Line)
                {
                    return AssociatedCodeRegion.StartPoint;
                }
                return startPointAdjustedForComment;
            }

            return null;
        }

        /// <summary>
        /// Get EndPoint adjusted for its region if any
        /// </summary>
        /// <returns></returns>
        private EditPoint GetEndPoint()
        {
            return AssociatedCodeRegion is object ? AssociatedCodeRegion.EndPoint : CodeElement?.GetEndPoint().CreateEditPoint();
        }

        /// <summary>
        /// Gets a starting point adjusted for leading comments.
        /// </summary>
        /// <param name="originalPoint">The original point.</param>
        /// <returns>The adjusted starting point.</returns>
        private static EditPoint GetStartPointAdjustedForComments(TextPoint originalPoint)
        {
            var commentPrefix = CodeCommentHelper.GetCommentPrefix(originalPoint.Parent);
            var point = originalPoint.CreateEditPoint();

            while (point.Line > 1)
            {
                string text = point.GetLines(point.Line - 1, point.Line);

                if (RegexNullSafe.IsMatch(text, @"^\s*" + commentPrefix))
                {
                    point.LineUp();
                    point.StartOfLine();
                }
                else
                {
                    break;
                }
            }

            return point;
        }

    }
}