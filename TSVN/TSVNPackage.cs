using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Forms;
using SamirBoulema.TSVN.Helpers;
using SamirBoulema.TSVN.Options;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Task = System.Threading.Tasks.Task;
using Microsoft;

namespace SamirBoulema.TSVN
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.9", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidTSVNPkgString)]
    [ProvideToolWindow(typeof(TSVNToolWindow))]
    public sealed class TsvnPackage : AsyncPackage
    {
        public DTE2 Dte;
        private string _solutionDir;
        private string _currentFilePath;
        private string _tortoiseProc;
        private ProjectItemsEvents _projectItemsEvents;
        private OleMenuCommandService _mcs;

        #region Package Members
        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            await BackgroundThreadInitialization();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await MainThreadInitialization();

            return;
        }

        private async Task BackgroundThreadInitialization()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

            Dte = await GetServiceAsync(typeof(DTE2)) as DTE2;
            Assumes.Present(Dte);

            _projectItemsEvents = (Dte.Events as Events2).ProjectItemsEvents;
            _projectItemsEvents.ItemAdded += ProjectItemsEvents_ItemAdded;
            _projectItemsEvents.ItemRenamed += ProjectItemsEvents_ItemRenamed;
            _projectItemsEvents.ItemRemoved += ProjectItemsEvents_ItemRemoved;

            FileHelper.Dte = Dte;
            CommandHelper.Dte = Dte;
            OptionsHelper.Dte = Dte;

            _tortoiseProc = FileHelper.GetTortoiseSvnProc();

            TSVNToolWindowCommand.Initialize(this);
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

            var tsvnMenu = CreateCommand(null, PkgCmdIdList.TSvnMenu);
            var tsvnContextMenu = CreateCommand(null, PkgCmdIdList.TSvnContextMenu);
            switch (Dte.Version)
            {
                case "11.0":
                case "12.0":
                    tsvnMenu.Text = "TSVN";
                    tsvnContextMenu.Text = "TSVN";
                    break;
                default:
                    tsvnMenu.Text = "Tsvn";
                    tsvnContextMenu.Text = "Tsvn";
                    break;
            }
            _mcs.AddCommand(tsvnMenu);
            _mcs.AddCommand(tsvnContextMenu);
        }

        #endregion

        #region Events
        private void ProjectItemsEvents_ItemRenamed(ProjectItem projectItem, string oldName)
        {
            if (!OptionsHelper.GetOptions().OnItemRenamedRenameInSVN)
            {
                return;
            }

            var newFilePath = projectItem.Properties?.Item("FullPath").Value.ToString();
            if (string.IsNullOrEmpty(newFilePath)) return;
            var oldFilePath = Path.Combine(Path.GetDirectoryName(newFilePath), oldName);

            // Temporarily rename the new file to the old file 
            File.Move(newFilePath, oldFilePath);

            // So that we can svn rename it properly
            CommandHelper.StartProcess(FileHelper.GetSvnExec(), $"mv {oldFilePath} {newFilePath}");
        }

        private void ProjectItemsEvents_ItemAdded(ProjectItem projectItem)
        {
            if (OptionsHelper.GetOptions().OnItemAddedAddToSVN)
            {
                var filePath = projectItem.Properties?.Item("FullPath").Value.ToString();
                if (string.IsNullOrEmpty(filePath)) return;
                CommandHelper.StartProcess(_tortoiseProc, $"/command:add /path:\"{filePath}\" /closeonend:0");
            }           
        }

        private void ProjectItemsEvents_ItemRemoved(ProjectItem projectItem)
        {
            if (OptionsHelper.GetOptions().OnItemRemovedRemoveFromSVN)
            {
                var filePath = projectItem.Properties?.Item("FullPath").Value.ToString();
                if (string.IsNullOrEmpty(filePath)) return;
                CommandHelper.StartProcess(_tortoiseProc, $"/command:remove /path:\"{filePath}\" /closeonend:0");
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
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:repostatus /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void UpdateCommand(object sender, EventArgs e) => _ = Update();

        private async Task Update()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void UpdateFileCommand(object sender, EventArgs e) => _ = UpdateFile();

        private async Task UpdateFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void RenameFileCommand(object sender, EventArgs e) => _ = RenameFile();

        private async Task RenameFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:rename /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void UpdateToRevisionCommand(object sender, EventArgs e) => _ = UpdateToRevision();

        private async Task UpdateToRevision()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /rev /closeonend:0");
        }

        private void UpdateToRevisionFileCommand(object sender, EventArgs e) => _ = UpdateToRevisionFile();

        private async Task UpdateToRevisionFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_currentFilePath}\" /rev /closeonend:0");
        }

        private void PropertiesCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:properties /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void CommitCommand(object sender, EventArgs e) => _ = Commit();

        private async Task Commit()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            CommandHelper.StartProcess(_tortoiseProc, $"/command:commit /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void CommitFileCommand(object sender, EventArgs e) => _ = CommitFile();

        private async Task CommitFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:commit /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void ShowLogCommand(object sender, EventArgs e) => _ = ShowLog();

        private async Task ShowLog()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:log /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void ShowLogFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:log /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void CreatePatchCommand(object sender, EventArgs e) => _ = CreatePatch();

        private async Task CreatePatch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:createpatch /path:\"{_solutionDir}\" /noview /closeonend:0");
        }

        private void ApplyPatchCommand(object sender, EventArgs e) => _ = ApplyPatch();

        private async Task ApplyPatch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = Resources.PatchFileFilterString,
                FilterIndex = 1,
                Multiselect = false
            };
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK) return;

            CommandHelper.StartProcess("TortoiseMerge.exe", $"/diff:\"{openFileDialog.FileName}\" /patchpath:\"{_solutionDir}\"");
        }

        private void ShelveCommand(object sender, EventArgs e) => _ = Shelve();

        private async Task Shelve()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:shelve /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void UnshelveCommand(object sender, EventArgs e) => _ = Unshelve();

        private async Task Unshelve()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:unshelve /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void RevertCommand(object sender, EventArgs e) => _ = Revert();

        private async Task Revert()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:revert /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void RevertFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:revert /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void AddFileCommand(object sender, EventArgs e) => _ = AddFile();

        private async Task AddFile()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:add /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void DiskBrowserCommand(object sender, EventArgs e) => _ = DiskBrowser();

        private async Task DiskBrowser()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start(_solutionDir);
        }

        private void DiskBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess("explorer.exe", _currentFilePath);
        }

        private void RepoBrowserCommand(object sender, EventArgs e) => _ = RepoBrowser();

        private async Task RepoBrowser()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_solutionDir}\"");
        }

        private void RepoBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_currentFilePath}\"");
        }

        private void BranchCommand(object sender, EventArgs e) => _ = Branch();

        private async Task Branch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:copy /path:\"{_solutionDir}\"");
        }

        private void SwitchCommand(object sender, EventArgs e) => _ = Switch();

        private async Task Switch()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:switch /path:\"{_solutionDir}\"");
        }

        private void MergeCommand(object sender, EventArgs e) => _ = Merge();

        private async Task Merge()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:merge /path:\"{_solutionDir}\"");
        }

        private void MergeFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:merge /path:\"{_currentFilePath}\"");
        }

        private void CleanupCommand(object sender, EventArgs e) => _ = Cleanup();

        private async Task Cleanup()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:cleanup /path:\"{_solutionDir}\"");
        }

        private void LockCommand(object sender, EventArgs e) => _ = Lock();

        private async Task Lock()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:lock /path:\"{_solutionDir}\"");
        }

        private void UnlockCommand(object sender, EventArgs e) => _ = Unlock();

        private async Task Unlock()
        {
            _solutionDir = await CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:unlock /path:\"{_solutionDir}\"");
        }

        private void LockFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:lock /path:\"{_currentFilePath}\"");
        }

        private void UnlockFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:unlock /path:\"{_currentFilePath}\"");
        }

        private void DifferencesCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:diff /path:\"{_currentFilePath}\"");
        }

        private void DiffPreviousCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:prevdiff /path:\"{_currentFilePath}\"");
        }

        private void BlameCommand(object sender, EventArgs e) => _ = Blame();

        private async Task Blame()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _currentFilePath = FileHelper.GetPath();
            var currentLineIndex = ((TextDocument)Dte.ActiveDocument?.Object(string.Empty))?.Selection.CurrentLine ?? 0;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:blame /path:\"{_currentFilePath}\" /line:{currentLineIndex}");
        }

        private void DeleteFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:remove /path:\"{_currentFilePath}\"");
        }

        private void ShowOptionsDialogCommand(object sender, EventArgs e)
        {
            new OptionsDialog(Dte).ShowDialog();
        }
        #endregion
    }
}
