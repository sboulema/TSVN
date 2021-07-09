using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.mergeCommand)]
    internal sealed class MergeCommand : BaseCommand<MergeCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CommandHelper.RunTortoiseSvnCommand("merge");
        }
    }
}
