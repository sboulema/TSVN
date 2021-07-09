using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.commitFileCommand)]
    internal sealed class CommitFileCommand : BaseCommand<CommitFileCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await KnownCommands.File_SaveSelectedItems.ExecuteAsync();
            await CommandHelper.RunTortoiseSvnFileCommand("commit");
        }
    }
}
