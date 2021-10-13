using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using SamirBoulema.TSVN.Options;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using System.Threading;
using Community.VisualStudio.Toolkit;
using File = System.IO.File;
using System.Collections.Generic;

namespace SamirBoulema.TSVN
{
    [Guid(PackageGuids.guidTSVNPkgString)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideToolWindow(typeof(TSVNToolWindow.Pane))]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class TsvnPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            this.RegisterToolWindows();

            await this.RegisterCommandsAsync();

            VS.Events.ProjectItemsEvents.AfterAddProjectItems += ProjectItemsEvents_AfterAddProjectItems;
            VS.Events.ProjectItemsEvents.AfterRenameProjectItems += ProjectItemsEvents_AfterRenameProjectItems;
            VS.Events.ProjectItemsEvents.AfterRemoveProjectItems += ProjectItemsEvents_AfterRemoveProjectItems;
        }

        #region Events
        private void ProjectItemsEvents_AfterRenameProjectItems(AfterRenameProjectItemEventArgs obj)
        {
            ProjectItemsEvents_ItemRenamedAsync(obj).FireAndForget();
        }

        private async Task ProjectItemsEvents_ItemRenamedAsync(AfterRenameProjectItemEventArgs obj)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemRenamedRenameInSVN)
            {
                return;
            }

            for (var i = 0; i < obj.ProjectItemRenames.Length - 1; i++)
            {
                var newPath = obj.ProjectItemRenames[i].SolutionItem.FullPath;
                var oldPath = obj.ProjectItemRenames[i].OldName;

                // Temporarily rename the new file to the old file 
                File.Move(newPath, oldPath);

                // So that we can svn rename it properly
                await CommandHelper.StartProcess(FileHelper.GetSvnExec(), $"mv {oldPath} {newPath}");
            }
        }

        private void ProjectItemsEvents_AfterAddProjectItems(IEnumerable<SolutionItem> solutionItems)
        {
            ProjectItemsEvents_ItemAdded_Async(solutionItems).FireAndForget();
        }

        private async Task ProjectItemsEvents_ItemAdded_Async(IEnumerable<SolutionItem> solutionItems)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemAddedAddToSVN)
            {
                return;
            }

            foreach (var item in solutionItems)
            {
                await CommandHelper.RunTortoiseSvnFileCommand("add", filePath: item.FullPath);
            }
        }

        private void ProjectItemsEvents_AfterRemoveProjectItems(AfterRemoveProjectItemEventArgs obj)
        {
            ProjectItemsEvents_ItemRemoved_Async(obj).FireAndForget();
        }

        private async Task ProjectItemsEvents_ItemRemoved_Async(AfterRemoveProjectItemEventArgs obj)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemRemovedRemoveFromSVN)
            {
                return;
            }

            foreach (var item in obj.ProjectItemRemoves)
            {
                await CommandHelper.RunTortoiseSvnFileCommand("remove", filePath: item.RemovedItemName);
            }
        }

        #endregion
    }
}
