using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Commands
{
    [Command(PackageGuids.guidTSVNCmdSetString, PackageIds.blameCommand)]
    internal sealed class BlameCommand : BaseCommand<BlameCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var documentView = await VS.Documents.GetActiveDocumentViewAsync();
            var lineNumber = documentView?.TextView?.Selection.ActivePoint.Position.GetContainingLine().LineNumber;

            await CommandHelper.RunTortoiseSvnFileCommand("blame", $"/line:{lineNumber}");
        }
    }
}
