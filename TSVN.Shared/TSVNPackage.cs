using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using SamirBoulema.TSVN.Options;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using System.Threading;
using SamirBoulema.TSVN.Commands;
using Community.VisualStudio.Toolkit;
using File = System.IO.File;

namespace SamirBoulema.TSVN
{
    [Guid(PackageGuids.guidTSVNPkgString)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideToolWindow(typeof(TSVNToolWindow.Pane))]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class TsvnPackage : ToolkitPackage, IVsTrackProjectDocumentsEvents2
    {
        private object projectDocTracker;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            TSVNToolWindow.Initialize(this);

            // Subscribe to project item events
            projectDocTracker = await GetServiceAsync(typeof(SVsTrackProjectDocuments));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await TSVNToolWindowCommand.InitializeAsync(this);
            await ShowOptionsDialogCommand.InitializeAsync(this);

            await ShowChangesCommand.InitializeAsync(this);
            await UpdateCommand.InitializeAsync(this);
            await UpdateToRevisionCommand.InitializeAsync(this);
            await CommitCommand.InitializeAsync(this);
            await ShowLogCommand.InitializeAsync(this);
            await CreatePatchCommand.InitializeAsync(this);
            await ApplyPatchCommand.InitializeAsync(this);
            await ShelveCommand.InitializeAsync(this);
            await UnshelveCommand.InitializeAsync(this);
            await RevertCommand.InitializeAsync(this);
            await DiskBrowserCommand.InitializeAsync(this);
            await RepoBrowserCommand.InitializeAsync(this);
            await BranchCommand.InitializeAsync(this);
            await SwitchCommand.InitializeAsync(this);
            await MergeCommand.InitializeAsync(this);
            await DifferencesCommand.InitializeAsync(this);
            await BlameCommand.InitializeAsync(this);
            await CommitFileCommand.InitializeAsync(this);
            await RenameFileCommand.InitializeAsync(this);
            await UnlockFileCommand.InitializeAsync(this);
            await LockFileCommand.InitializeAsync(this);
            await DeleteFileCommand.InitializeAsync(this);
            await ShowLogFileCommand.InitializeAsync(this);
            await CleanupCommand.InitializeAsync(this);
            await LockCommand.InitializeAsync(this);
            await UnlockCommand.InitializeAsync(this);
            await DiskBrowserFileCommand.InitializeAsync(this);
            await RepoBrowserFileCommand.InitializeAsync(this);
            await MergeFileCommand.InitializeAsync(this);
            await UpdateToRevisionFileCommand.InitializeAsync(this);
            await PropertiesCommand.InitializeAsync(this);
            await UpdateFileCommand.InitializeAsync(this);
            await DiffPreviousCommand.InitializeAsync(this);
            await RevertFileCommand.InitializeAsync(this);
            await AddFileCommand.InitializeAsync(this);

            if (projectDocTracker != null)
            {
                (projectDocTracker as IVsTrackProjectDocuments2).AdviseTrackProjectDocumentsEvents(this, out _);
            }
        }

        #region Events
        private async Task ProjectItemsEvents_ItemRenamedAsync(string[] oldFilePaths, string[] newFilePaths)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemRenamedRenameInSVN)
            {
                return;
            }

            for (var i = 0; i < oldFilePaths.Length - 1; i++)
            {
                // Temporarily rename the new file to the old file 
                File.Move(newFilePaths[i], oldFilePaths[i]);

                // So that we can svn rename it properly
                await CommandHelper.StartProcess(FileHelper.GetSvnExec(), $"mv {oldFilePaths[i]} {newFilePaths[i]}");
            }
        }

        private async Task ProjectItemsEvents_ItemAdded_Async(string[] filePaths)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemAddedAddToSVN)
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                await CommandHelper.RunTortoiseSvnFileCommand("add", filePath: filePath);
            }
        }

        private async Task ProjectItemsEvents_ItemRemoved_Async(string[] filePaths)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemRemovedRemoveFromSVN)
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                await CommandHelper.RunTortoiseSvnFileCommand("remove", filePath: filePath);
            }
        }

        public int OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            ProjectItemsEvents_ItemAdded_Async(rgpszMkDocuments).FireAndForget();
            return VSConstants.S_OK;
        }

        public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            ProjectItemsEvents_ItemRemoved_Async(rgpszMkDocuments).FireAndForget();
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            ProjectItemsEvents_ItemRenamedAsync(rgszMkOldNames, rgszMkNewNames).FireAndForget();
            return VSConstants.S_OK;
        }

        public int OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
