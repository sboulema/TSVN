using Community.VisualStudio.Toolkit;
using SamirBoulema.TSVN.Options;
using System;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.showOptionsDialogCommand)]
    internal sealed class ShowOptionsDialogCommand : BaseCommand<ShowOptionsDialogCommand>
    {
        protected override void Execute(object sender, EventArgs e)
        {
            new OptionsDialog().ShowDialog();
        }
    }
}
