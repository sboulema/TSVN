using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.renameFileCommand)]
    internal sealed class RenameFileCommand : BaseCommand<RenameFileCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await KnownCommands.File_SaveAll.ExecuteAsync();
            await CommandHelper.RunTortoiseSvnFileCommand("rename");
        }
    }
}
