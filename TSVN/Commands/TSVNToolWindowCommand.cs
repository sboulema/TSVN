using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.showToolwindowCommand)]
    internal sealed class TSVNToolWindowCommand : BaseCommand<TSVNToolWindowCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await TSVNToolWindow.ShowAsync();
        }
    }
}
