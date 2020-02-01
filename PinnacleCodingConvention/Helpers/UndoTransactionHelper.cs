using EnvDTE80;
using PinnacleCodingConvention.Common;
using System;

namespace PinnacleCodingConvention.Helpers
{
    internal sealed class UndoTransactionHelper
    {
        private readonly DTE2 _ide;
        private readonly string _transactionName;

        internal UndoTransactionHelper(DTE2 ide, string transactionName)
        {
            _ide = ide;
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
            bool shouldCloseUndoContext = false;

            // Start an undo transaction (unless inside one already or within an auto save context).
            if (!_ide.UndoContext.IsOpen)
            {
                _ide.UndoContext.Open(_transactionName);
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
                _ide.StatusBar.Text = $"{message} {Resource.SeeOutputForMoreInformation}";

                catchAction?.Invoke(exception);

                if (shouldCloseUndoContext)
                {
                    _ide.UndoContext.SetAborted();
                    shouldCloseUndoContext = false;
                }
            }
            finally
            {
                // Always close the undo transaction to prevent ongoing interference with the IDE.
                if (shouldCloseUndoContext)
                {
                    _ide.UndoContext.Close();
                }
            }
        }
    }
}
