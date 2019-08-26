using Microsoft.VisualStudio.Shell;
using PinnacleCodingConvention.Common;
using System;

namespace PinnacleCodingConvention.Helpers
{
    internal sealed class UndoTransactionHelper
    {
        private readonly PinnacleCodingConventionPackage _package;
        private readonly string _transactionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UndoTransactionHelper" /> class.
        /// </summary>
        /// <param name="package">The hosting package.</param>
        /// <param name="transactionName">The name of the transaction.</param>
        internal UndoTransactionHelper(PinnacleCodingConventionPackage package, string transactionName)
        {
            _package = package;
            _transactionName = transactionName;
        }

        /// <summary>
        /// Runs the specified try action within a try block, and conditionally the catch action
        /// within a catch block all conditionally within the context of an undo transaction.
        /// </summary>
        /// <param name="tryAction">The action to be performed within a try block.</param>
        /// <param name="catchAction">The action to be performed wihin a catch block.</param>
        internal void Run(Action tryAction, Action<Exception> catchAction = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            bool shouldCloseUndoContext = false;

            // Start an undo transaction (unless inside one already or within an auto save context).
            if (!_package.IDE.UndoContext.IsOpen)
            {
                _package.IDE.UndoContext.Open(_transactionName);
                shouldCloseUndoContext = true;
            }

            try
            {
                tryAction();
            }
            catch (Exception exception)
            {
                var message = $"{_transactionName} {Resource.Stopped}!";
                OutputWindowHelper.WriteError($"{message} {exception}");
                _package.IDE.StatusBar.Text = $"{message} {Resource.SeeOutputForMoreInformation}";

                catchAction?.Invoke(exception);

                if (shouldCloseUndoContext)
                {
                    _package.IDE.UndoContext.SetAborted();
                    shouldCloseUndoContext = false;
                }
            }
            finally
            {
                // Always close the undo transaction to prevent ongoing interference with the IDE.
                if (shouldCloseUndoContext)
                {
                    _package.IDE.UndoContext.Close();
                }
            }
        }
    }
}
