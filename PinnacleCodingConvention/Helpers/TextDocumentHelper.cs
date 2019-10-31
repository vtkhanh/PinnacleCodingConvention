using EnvDTE;
using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Models.CodeItems;
using System;
using System.Collections.Generic;

namespace PinnacleCodingConvention.Helpers
{
    /// <summary>
    /// A static helper class for working with text documents.
    /// </summary>
    /// <remarks>
    ///
    /// Note: All POSIXRegEx text replacements search against '\n' but insert/replace with
    ///       Environment.NewLine. This handles line endings correctly.
    /// </remarks>
    internal static class TextDocumentHelper
    {
        /// <summary>
        /// The common set of options to be used for find and replace patterns.
        /// </summary>
        internal const int StandardFindOptions = (int)(vsFindOptions.vsFindOptionsRegularExpression |
                                                       vsFindOptions.vsFindOptionsMatchInHiddenText);


        /// <summary>
        /// Finds all matches of the specified pattern within the specified text document.
        /// </summary>
        /// <param name="textDocument">The text document.</param>
        /// <param name="patternString">The pattern string.</param>
        /// <returns>The set of matches.</returns>
        internal static IEnumerable<EditPoint> FindMatches(TextDocument textDocument, string patternString)
        {
            var matches = new List<EditPoint>();
            var cursor = textDocument.StartPoint.CreateEditPoint();
            EditPoint end = null;
            TextRanges dummy = null;

            while (cursor is object && cursor.FindPattern(patternString, StandardFindOptions, ref end, ref dummy))
            {
                matches.Add(cursor.CreateEditPoint());
                cursor = end;
            }

            return matches;
        }

        /// <summary>
        /// Finds all matches of the specified pattern within the specified text selection.
        /// </summary>
        /// <param name="textSelection">The text selection.</param>
        /// <param name="patternString">The pattern string.</param>
        /// <returns>The set of matches.</returns>
        internal static IEnumerable<EditPoint> FindMatches(TextSelection textSelection, string patternString)
        {
            var matches = new List<EditPoint>();
            var cursor = textSelection.TopPoint.CreateEditPoint();
            EditPoint end = null;
            TextRanges dummy = null;

            while (cursor is object && cursor.FindPattern(patternString, StandardFindOptions, ref end, ref dummy))
            {
                if (end.AbsoluteCharOffset > textSelection.BottomPoint.AbsoluteCharOffset)
                    break;

                matches.Add(cursor.CreateEditPoint());
                cursor = end;
            }

            return matches;
        }

        /// <summary>
        /// Gets the text between the specified start point and the first match.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="matchString">The match string.</param>
        /// <returns>The matching text, otherwise null.</returns>
        internal static string GetTextToFirstMatch(TextPoint startPoint, string matchString)
        {
            var startEditPoint = startPoint.CreateEditPoint();
            var endEditPoint = startEditPoint.CreateEditPoint();
            TextRanges subGroupMatches = null; // Not used - required for FindPattern.

            if (endEditPoint.FindPattern(matchString, StandardFindOptions, ref endEditPoint, ref subGroupMatches))
                return startEditPoint.GetText(endEditPoint);

            return null;
        }

        /// <summary>
        /// Inserts a blank line before the specified point except where adjacent to a brace.
        /// </summary>
        /// <param name="point">The point.</param>
        internal static void InsertBlankLineBeforePoint(EditPoint point)
        {
            if (point.Line <= 1) 
                return;

            point.LineUp(1);
            point.StartOfLine();

            string text = point.GetLine();
            if (RegexNullSafe.IsMatch(text, @"^\s*[^\s\{]")) // If it is not a scope boundary, insert newline.
            {
                point.EndOfLine();
                point.Insert(Environment.NewLine);
            }
        }

        /// <summary>
        /// Inserts a blank line after the specified point except where adjacent to a brace.
        /// </summary>
        /// <param name="point">The point.</param>
        internal static void InsertBlankLineAfterPoint(EditPoint point)
        {
            if (point.AtEndOfDocument) 
                return;

            point.LineDown(1);
            point.StartOfLine();

            string text = point.GetLine();
            if (RegexNullSafe.IsMatch(text, @"^\s*[^\s\}]"))
                point.Insert(Environment.NewLine);
        }

    }

}
