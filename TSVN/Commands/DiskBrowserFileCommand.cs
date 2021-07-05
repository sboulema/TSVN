using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.diskBrowserFileCommand)]
    internal sealed class DiskBrowserFileCommand : BaseCommand<DiskBrowserFileCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var filePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            CommandHelper.StartProcess("explorer.exe", filePath);
        }
    }
}
