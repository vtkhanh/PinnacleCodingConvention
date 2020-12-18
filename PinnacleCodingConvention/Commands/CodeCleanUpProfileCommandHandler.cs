using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using PinnacleCodingConvention.Helpers;
using PinnacleCodingConvention.Options;
using System;
using System.ComponentModel.Composition;

namespace PinnacleCodingConvention.Commands
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(CodeCleanUpProfileCommandHandler))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class CodeCleanUpProfileCommandHandler : ICommandHandler<CleanUpCommandArgs>
    {
        public string DisplayName => nameof(CodeCleanUpProfileCommandHandler);

        private readonly IEditorCommandHandlerServiceFactory _commandService;

        [ImportingConstructor]
        public CodeCleanUpProfileCommandHandler(IEditorCommandHandlerServiceFactory commandService) => _commandService = commandService;

        public bool ExecuteCommand(CleanUpCommandArgs args, CommandExecutionContext executionContext)
        {
            try
            {
                var service = _commandService.GetService(args.TextView);

                if (GeneralOptions.Instance.Profile == CodeCleanupProfile.Profile1)
                {
                    var command = new CodeCleanUpDefaultProfileCommandArgs(args.TextView, args.SubjectBuffer);
                    service.Execute((textView, textBuffer) => command, null);
                }
                else if (GeneralOptions.Instance.Profile == CodeCleanupProfile.Profile2)
                {
                    var command = new CodeCleanUpCustomProfileCommandArgs(args.TextView, args.SubjectBuffer);
                    service.Execute((textView, textBuffer) => command, null);
                }
            }
            catch (Exception ex)
            {
                OutputWindowHelper.WriteError(ex.Message);
            }

            return false;
        }

        public CommandState GetCommandState(CleanUpCommandArgs args) => CommandState.Available;
    }
}
