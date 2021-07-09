using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.commitCommand)]
    internal sealed class CommitCommand : BaseCommand<CommitCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await KnownCommands.File_SaveAll.ExecuteAsync().ConfigureAwait(false);
            await CommandHelper.RunTortoiseSvnCommand("commit").ConfigureAwait(false);
        }
    }
}
