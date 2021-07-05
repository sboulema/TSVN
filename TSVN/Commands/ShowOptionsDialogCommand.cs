using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Options;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.showOptionsDialogCommand)]
    internal sealed class ShowOptionsDialogCommand : BaseCommand<ShowOptionsDialogCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            new OptionsDialog().ShowDialog();
        }
    }
}
