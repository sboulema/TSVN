using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Forms;
using SamirBoulema.TSVN.Helpers;
using SamirBoulema.TSVN.Options;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Task = System.Threading.Tasks.Task;
using Microsoft;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using System.Net;

namespace SamirBoulema.TSVN
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.9", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidTSVNPkgString)]
    [ProvideToolWindow(typeof(TSVNToolWindow))]
    public sealed class TsvnPackage : AsyncPackage, IVsTrackProjectDocumentsEvents2
    {
        private string _solutionDir;
        private string _currentFilePath;
        private string _tortoiseProc;
        private OleMenuCommandService _mcs;
        private IVsTrackProjectDocuments2 projectDocTracker;

        #region Package Members
        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            _tortoiseProc = FileHelper.GetTortoiseSvnProc();

            TSVNToolWindowCommand.Initialize(this);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            projectDocTracker = await GetServiceAsync(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            Assumes.Present(projectDocTracker);
            projectDocTracker.AdviseTrackProjectDocumentsEvents(this, out var cookie);

            await MainThreadInitialization();

            return;
        }

        private async Task MainThreadInitialization()
        {
            _mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(_mcs);

            _mcs.AddCommand(CreateCommand(ShowChangesCommand, PkgCmdIdList.ShowChangesCommand));
            _mcs.AddCommand(CreateCommand(UpdateCommand, PkgCmdIdList.UpdateCommand));
            _mcs.AddCommand(CreateCommand(UpdateToRevisionCommand, PkgCmdIdList.UpdateToRevisionCommand));
            _mcs.AddCommand(CreateCommand(CommitCommand, PkgCmdIdList.CommitCommand));
            _mcs.AddCommand(CreateCommand(ShowLogCommand, PkgCmdIdList.ShowLogCommand));
            _mcs.AddCommand(CreateCommand(CreatePatchCommand, PkgCmdIdList.CreatePatchCommand));
            _mcs.AddCommand(CreateCommand(ApplyPatchCommand, PkgCmdIdList.ApplyPatchCommand));
            _mcs.AddCommand(CreateCommand(ShelveCommand, PkgCmdIdList.ShelveCommand));
            _mcs.AddCommand(CreateCommand(UnshelveCommand, PkgCmdIdList.UnshelveCommand));
            _mcs.AddCommand(CreateCommand(RevertCommand, PkgCmdIdList.RevertCommand));
            _mcs.AddCommand(CreateCommand(DiskBrowserCommand, PkgCmdIdList.DiskBrowser));
            _mcs.AddCommand(CreateCommand(RepoBrowserCommand, PkgCmdIdList.RepoBrowser));
            _mcs.AddCommand(CreateCommand(BranchCommand, PkgCmdIdList.BranchCommand));
            _mcs.AddCommand(CreateCommand(SwitchCommand, PkgCmdIdList.SwitchCommand));
            _mcs.AddCommand(CreateCommand(MergeCommand, PkgCmdIdList.MergeCommand));
            _mcs.AddCommand(CreateCommand(DifferencesCommand, PkgCmdIdList.DifferencesCommand));
            _mcs.AddCommand(CreateCommand(BlameCommand, PkgCmdIdList.BlameCommand));
            _mcs.AddCommand(CreateCommand(ShowLogFileCommand, PkgCmdIdList.ShowLogFileCommand));
            _mcs.AddCommand(CreateCommand(CleanupCommand, PkgCmdIdList.CleanupCommand));
            _mcs.AddCommand(CreateCommand(LockCommand, PkgCmdIdList.LockCommand));
            _mcs.AddCommand(CreateCommand(UnlockCommand, PkgCmdIdList.UnlockCommand));

            _mcs.AddCommand(CreateCommand(DiskBrowserFileCommand, PkgCmdIdList.DiskBrowserFileCommand));
            _mcs.AddCommand(CreateCommand(RepoBrowserFileCommand, PkgCmdIdList.RepoBrowserFileCommand));
            _mcs.AddCommand(CreateCommand(MergeFileCommand, PkgCmdIdList.MergeFileCommand));
            _mcs.AddCommand(CreateCommand(UpdateToRevisionFileCommand, PkgCmdIdList.UpdateToRevisionFileCommand));
            _mcs.AddCommand(CreateCommand(PropertiesCommand, PkgCmdIdList.PropertiesCommand));
            _mcs.AddCommand(CreateCommand(UpdateFileCommand, PkgCmdIdList.UpdateFileCommand));
            _mcs.AddCommand(CreateCommand(CommitFileCommand, PkgCmdIdList.CommitFileCommand));
            _mcs.AddCommand(CreateCommand(DiffPreviousCommand, PkgCmdIdList.DiffPreviousCommand));
            _mcs.AddCommand(CreateCommand(RevertFileCommand, PkgCmdIdList.RevertFileCommand));
            _mcs.AddCommand(CreateCommand(AddFileCommand, PkgCmdIdList.AddFileCommand));
            _mcs.AddCommand(CreateCommand(DeleteFileCommand, PkgCmdIdList.DeleteFileCommand));
            _mcs.AddCommand(CreateCommand(LockFileCommand, PkgCmdIdList.LockFileCommand));
            _mcs.AddCommand(CreateCommand(UnlockFileCommand, PkgCmdIdList.UnlockFileCommand));
            _mcs.AddCommand(CreateCommand(RenameFileCommand, PkgCmdIdList.RenameFileCommand));

            _mcs.AddCommand(CreateCommand(ShowOptionsDialogCommand, PkgCmdIdList.ShowOptionsDialogCommand));

            _mcs.AddCommand(CreateCommand(null, PkgCmdIdList.TSvnMenu));
            _mcs.AddCommand(CreateCommand(null, PkgCmdIdList.TSvnContextMenu));
        }

        #endregion

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
                CommandHelper.StartProcess(FileHelper.GetSvnExec(), $"mv {oldFilePaths[i]} {newFilePaths[i]}");
            }
        }

        private async Task ProjectItemsEvents_ItemAdded_Async(string[] filePaths)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemAddedAddToSVN)
            {
                return;
            }

            var closeOnEnd = options.CloseOnEnd ? 1 : 0;

            foreach (var filePath in filePaths)
            {
                CommandHelper.StartProcess(_tortoiseProc, $"/command:add /path:\"{filePath}\" /closeonend:{closeOnEnd}");
            }
        }

        private async Task ProjectItemsEvents_ItemRemoved_Async(string[] filePaths)
        {
            var options = await OptionsHelper.GetOptions();

            if (!options.OnItemRemovedRemoveFromSVN)
            {
                return;
            }

            var closeOnEnd = options.CloseOnEnd ? 1 : 0;

            foreach (var filePath in filePaths)
            {
                CommandHelper.StartProcess(_tortoiseProc, $"/command:remove /path:\"{filePath}\" /closeonend:{closeOnEnd}");
            }
        }

        #endregion

        private static OleMenuCommand CreateCommand(EventHandler handler, uint commandId)
        {
            var menuCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)commandId);
            var menuItem = new OleMenuCommand(handler, menuCommandId);
            return menuItem;
        }

        public object GetServiceHelper(Type type)
        {
            return GetService(type);
        }

        #region Button Commands
        private void ShowChangesCommand(object sender, EventArgs e) => _ = ShowChanges();

        private async Task ShowChanges()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:repostatus /path:\"{_solutionDir}\" /closeonend:{closeOnEnd}");
        }

        private void UpdateCommand(object sender, EventArgs e) => _ = Update();

        private async Task Update()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.SaveAll");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /closeonend:{closeOnEnd}");
        }

        private void UpdateFileCommand(object sender, EventArgs e) => _ = UpdateFile();

        private async Task UpdateFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.Save");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_currentFilePath}\" /closeonend:{closeOnEnd}");
        }

        private void RenameFileCommand(object sender, EventArgs e) => _ = RenameFile();

        private async Task RenameFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.Save");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:rename /path:\"{_currentFilePath}\" /closeonend:{closeOnEnd}");
        }

        private void UpdateToRevisionCommand(object sender, EventArgs e) => _ = UpdateToRevision();

        private async Task UpdateToRevision()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.SaveAll");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /rev /closeonend:{closeOnEnd}");
        }

        private void UpdateToRevisionFileCommand(object sender, EventArgs e) => _ = UpdateToRevisionFile();

        private async Task UpdateToRevisionFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.Save");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_currentFilePath}\" /rev /closeonend:{closeOnEnd}");
        }

        private void PropertiesCommand(object sender, EventArgs e) => _ = ShowProperties();

        private async Task ShowProperties()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:properties /path:\"{_currentFilePath}\" /closeonend:{closeOnEnd}");
        }

        private void CommitCommand(object sender, EventArgs e) => _ = Commit();

        private async Task Commit()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.SaveAll");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:commit /path:\"{_solutionDir}\" /closeonend:{closeOnEnd}");
        }

        private void CommitFileCommand(object sender, EventArgs e) => _ = CommitFile();

        private async Task CommitFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.Save");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:commit /path:\"{_currentFilePath}\" /closeonend:{closeOnEnd}");
        }

        private void ShowLogCommand(object sender, EventArgs e) => _ = ShowLog();

        private async Task ShowLog()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:log /path:\"{_solutionDir}\" /closeonend:{closeOnEnd}");
        }

        private void ShowLogFileCommand(object sender, EventArgs e) => _ = ShowLogFile();

        private async Task ShowLogFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:log /path:\"{_currentFilePath}\" /closeonend:{closeOnEnd}");
        }

        private void CreatePatchCommand(object sender, EventArgs e) => _ = CreatePatch();

        private async Task CreatePatch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:createpatch /path:\"{_solutionDir}\" /noview /closeonend:{closeOnEnd}");
        }

        private void ApplyPatchCommand(object sender, EventArgs e) => _ = ApplyPatch();

        private async Task ApplyPatch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = Resources.PatchFileFilterString,
                FilterIndex = 1,
                Multiselect = false
            };

            var result = openFileDialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            CommandHelper.StartProcess("TortoiseMerge.exe", $"/diff:\"{openFileDialog.FileName}\" /patchpath:\"{_solutionDir}\"");
        }

        private void ShelveCommand(object sender, EventArgs e) => _ = Shelve();

        private async Task Shelve()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:shelve /path:\"{_solutionDir}\" /closeonend:{closeOnEnd}");
        }

        private void UnshelveCommand(object sender, EventArgs e) => _ = Unshelve();

        private async Task Unshelve()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:unshelve /path:\"{_solutionDir}\" /closeonend:{closeOnEnd}");
        }

        private void RevertCommand(object sender, EventArgs e) => _ = Revert();

        private async Task Revert()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:revert /path:\"{_solutionDir}\" /closeonend:{closeOnEnd}");
        }

        private void RevertFileCommand(object sender, EventArgs e) => _ = RevertFile();

        private async Task RevertFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:revert /path:\"{_currentFilePath}\" /closeonend:{closeOnEnd}");
        }

        private void AddFileCommand(object sender, EventArgs e) => _ = AddFile();

        private async Task AddFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.Save");
            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:add /path:\"{_currentFilePath}\" /closeonend:{closeOnEnd}");
        }

        private void DiskBrowserCommand(object sender, EventArgs e) => _ = DiskBrowser();

        private async Task DiskBrowser()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            Process.Start(_solutionDir);
        }

        private void DiskBrowserFileCommand(object sender, EventArgs e)
            => _ = DiskBrowserFile();

        private async Task DiskBrowserFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess("explorer.exe", _currentFilePath);
        }

        private void RepoBrowserCommand(object sender, EventArgs e) => _ = RepoBrowser();

        private async Task RepoBrowser()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_solutionDir}\"");
        }

        private void RepoBrowserFileCommand(object sender, EventArgs e)
            => _ = RepoBrowserFile();

        private async Task RepoBrowserFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_currentFilePath}\"");
        }

        private void BranchCommand(object sender, EventArgs e) => _ = Branch();

        private async Task Branch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:copy /path:\"{_solutionDir}\"");
        }

        private void SwitchCommand(object sender, EventArgs e) => _ = Switch();

        private async Task Switch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:switch /path:\"{_solutionDir}\"");
        }

        private void MergeCommand(object sender, EventArgs e)
            => _ = Merge();

        private async Task Merge()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:merge /path:\"{_solutionDir}\"");
        }

        private void MergeFileCommand(object sender, EventArgs e)
            => _ = MergeFile();

        private async Task MergeFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:merge /path:\"{_currentFilePath}\"");
        }

        private void CleanupCommand(object sender, EventArgs e) => _ = Cleanup();

        private async Task Cleanup()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:cleanup /path:\"{_solutionDir}\"");
        }

        private void LockCommand(object sender, EventArgs e) => _ = Lock();

        private async Task Lock()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:lock /path:\"{_solutionDir}\"");
        }

        private void UnlockCommand(object sender, EventArgs e) => _ = Unlock();

        private async Task Unlock()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();

            if (string.IsNullOrEmpty(_solutionDir))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:unlock /path:\"{_solutionDir}\"");
        }

        private void LockFileCommand(object sender, EventArgs e)
            => _ = LockFile();

        private async Task LockFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:lock /path:\"{_currentFilePath}\"");
        }

        private void UnlockFileCommand(object sender, EventArgs e)
            => _ = UnlockFile();

        private async Task UnlockFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:unlock /path:\"{_currentFilePath}\"");
        }

        private void DifferencesCommand(object sender, EventArgs e)
            => _ = Differences();

        private async Task Differences()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:diff /path:\"{_currentFilePath}\"");
        }

        private void DiffPreviousCommand(object sender, EventArgs e)
            => _ = DiffPrevious();

        private async Task DiffPrevious()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:prevdiff /path:\"{_currentFilePath}\"");
        }

        private void BlameCommand(object sender, EventArgs e)
            => _ = Blame();

        private async Task Blame()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            var documentView = await VS.Documents.GetActiveDocumentViewAsync();
            var lineNumber = documentView?.TextView?.Selection.ActivePoint.Position.GetContainingLine().LineNumber;

            CommandHelper.StartProcess(_tortoiseProc, $"/command:blame /path:\"{_currentFilePath}\" /line:{lineNumber}");
        }

        private void DeleteFileCommand(object sender, EventArgs e)
            => _ = DeleteFile();

        private async Task DeleteFile()
        {
            _currentFilePath = await FileHelper.GetPath();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            CommandHelper.StartProcess(_tortoiseProc, $"/command:remove /path:\"{_currentFilePath}\"");
        }

        private void ShowOptionsDialogCommand(object sender, EventArgs e)
        {
            new OptionsDialog().ShowDialog();
        }

        public int OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            _ = ProjectItemsEvents_ItemAdded_Async(rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            _ = ProjectItemsEvents_ItemRemoved_Async(rgpszMkDocuments);
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
            _ = ProjectItemsEvents_ItemRenamedAsync(rgszMkOldNames, rgszMkNewNames);
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
