using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.updateCommand)]
    internal sealed class UpdateCommand : BaseCommand<UpdateCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await KnownCommands.File_SaveAll.ExecuteAsync();
            await CommandHelper.RunTortoiseSvnCommand("update");
        }
    }
}
