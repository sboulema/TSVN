using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.branchCommand)]
    internal sealed class BranchCommand : BaseCommand<BranchCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CommandHelper.RunTortoiseSvnCommand("copy");
        }
    }
}
