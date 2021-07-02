using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN
{
    internal sealed class TSVNToolWindowCommand
    {
        public const int CommandId = 256;

        public static readonly Guid CommandSet = new Guid("fdf0dd0f-a813-4fde-a9d0-0a50dade9b94");

        private readonly Package package;

        private TSVNToolWindowCommand(Package package)
        {
            if (package == null)
            {
                return;
            }

            this.package = package;

            if (!(ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService))
            {
                return;
            }

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(ShowToolWindow, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static TSVNToolWindowCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider => package;

        public static void Initialize(Package package)
        {
            Instance = new TSVNToolWindowCommand(package);
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            _ = ShowToolWindowAsync(sender, e);
        }

        private async Task ShowToolWindowAsync(object sender, EventArgs e)
        {
            var window = package.FindToolWindow(typeof(TSVNToolWindow), 0, true);

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(((IVsWindowFrame)window.Frame).Show());
        }
    }
}
