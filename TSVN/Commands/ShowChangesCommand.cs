using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.showChangesCommand)]
    internal sealed class ShowChangesCommand : BaseCommand<ShowChangesCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
            => await CommandHelper.RunTortoiseSvnCommand("repostatus");
    }
}
