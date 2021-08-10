using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.updateToRevisionFileCommand)]
    internal sealed class UpdateToRevisionFileCommand : BaseCommand<UpdateToRevisionFileCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await KnownCommands.File_SaveSelectedItems.ExecuteAsync();
            await CommandHelper.RunTortoiseSvnFileCommand("update", "/rev");
        }
    }
}
