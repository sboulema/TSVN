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
using SamirBoulema.TSVN.Helpers.AsyncPackageHelpers;
using ProvideAutoLoad = SamirBoulema.TSVN.Helpers.AsyncPackageHelpers.ProvideAutoLoadAttribute;

namespace SamirBoulema.TSVN
{
    [AsyncPackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.9", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidTSVNPkgString)]
    [ProvideToolWindow(typeof(TSVNToolWindow))]
    public sealed class TsvnPackage : Package, IAsyncLoadablePackageInitialize
    {
        public DTE Dte;
        private string _solutionDir;
        private string _currentFilePath;
        private string _tortoiseProc;
        private ProjectItemsEvents _projectItemsEvents;
        private bool isAsyncLoadSupported;
        private OleMenuCommandService _mcs;

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            isAsyncLoadSupported = this.IsAsyncPackageSupported();

            // Only perform initialization if async package framework is not supported
            if (!isAsyncLoadSupported)
            {
                BackgroundThreadInitialization();
                MainThreadInitialization();
            }
        }

        /// <summary>
        /// Performs the asynchronous initialization for the package in cases where IDE supports AsyncPackage.
        /// 
        /// This method is always called from background thread initially.
        /// </summary>
        /// <param name="asyncServiceProvider">Async service provider instance to query services asynchronously</param>
        /// <param name="pProfferService">Async service proffer instance</param>
        /// <param name="IAsyncProgressCallback">Progress callback instance</param>
        /// <returns></returns>
        public IVsTask Initialize(Microsoft.VisualStudio.Shell.Interop.IAsyncServiceProvider asyncServiceProvider,
            IProfferAsyncService pProfferService, IAsyncProgressCallback pProgressCallback)
        {
            if (!isAsyncLoadSupported)
            {
                throw new InvalidOperationException("Async Initialize method should not be called when async load is not supported.");
            }

            return ThreadHelper.JoinableTaskFactory.RunAsync<object>(async () =>
            {
                BackgroundThreadInitialization();

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                MainThreadInitialization();
                return null;
            }).AsVsTask();
        }

        private void BackgroundThreadInitialization()
        {
            Dte = (DTE)GetService(typeof(DTE));

            _projectItemsEvents = (Dte.Events as Events2).ProjectItemsEvents;
            _projectItemsEvents.ItemAdded += ProjectItemsEvents_ItemAdded;
            _projectItemsEvents.ItemRenamed += ProjectItemsEvents_ItemRenamed;
            _projectItemsEvents.ItemRemoved += ProjectItemsEvents_ItemRemoved;

            FileHelper.Dte = Dte;
            CommandHelper.Dte = Dte;
            OptionsHelper.Dte = Dte;

            _tortoiseProc = FileHelper.GetTortoiseSvnProc();

            Logger.Initialize(this, "TSVN");

            // Add our command handlers for menu (commands must exist in the .vsct file)
            _mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            TSVNToolWindowCommand.Initialize(this);
        }

        private void MainThreadInitialization()
        {
            if (null == _mcs) return;

            _mcs.AddCommand(CreateCommand(ShowChangesCommand, PkgCmdIdList.ShowChangesCommand));
            _mcs.AddCommand(CreateCommand(UpdateCommand, PkgCmdIdList.UpdateCommand));
            _mcs.AddCommand(CreateCommand(UpdateToRevisionCommand, PkgCmdIdList.UpdateToRevisionCommand));
            _mcs.AddCommand(CreateCommand(CommitCommand, PkgCmdIdList.CommitCommand));
            _mcs.AddCommand(CreateCommand(ShowLogCommand, PkgCmdIdList.ShowLogCommand));
            _mcs.AddCommand(CreateCommand(CreatePatchCommand, PkgCmdIdList.CreatePatchCommand));
            _mcs.AddCommand(CreateCommand(ApplyPatchCommand, PkgCmdIdList.ApplyPatchCommand));
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
            if (OptionsHelper.GetOptions().OnItemRenamedRenameInSVN)
            {
                var newFilePath = projectItem.Properties?.Item("FullPath").Value;
                if (string.IsNullOrEmpty(newFilePath)) return;
                var oldFilePath = Path.Combine(Path.GetDirectoryName(newFilePath), oldName);

                // Temporarily rename the new file to the old file 
                File.Move(newFilePath, oldFilePath);
                
                // So that we can svn rename it properly
                CommandHelper.StartProcess(FileHelper.GetSvnExec(), $"mv {oldFilePath} {newFilePath}");
            }
        }

        private void ProjectItemsEvents_ItemAdded(ProjectItem projectItem)
        {
            if (OptionsHelper.GetOptions().OnItemAddedAddToSVN)
            {
                var filePath = projectItem.Properties?.Item("FullPath").Value;
                if (string.IsNullOrEmpty(filePath)) return;
                CommandHelper.StartProcess(_tortoiseProc, $"/command:add /path:\"{filePath}\" /closeonend:0");
            }           
        }

        private void ProjectItemsEvents_ItemRemoved(ProjectItem projectItem)
        {
            if (OptionsHelper.GetOptions().OnItemRemovedRemoveFromSVN)
            {
                var filePath = projectItem.Properties?.Item("FullPath").Value;
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
        private void ShowChangesCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:repostatus /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void UpdateCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void UpdateFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void UpdateToRevisionCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            CommandHelper.StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /rev /closeonend:0");
        }

        private void UpdateToRevisionFileCommand(object sender, EventArgs e)
        {
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

        private void CommitCommand(object sender, EventArgs e)
        {  
            _solutionDir = CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            CommandHelper.StartProcess(_tortoiseProc, $"/command:commit /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void CommitFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:commit /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void ShowLogCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:log /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void ShowLogFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:log /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void CreatePatchCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:createpatch /path:\"{_solutionDir}\" /noview /closeonend:0");
        }

        private void ApplyPatchCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
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

        private void RevertCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:revert /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void RevertFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:revert /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void AddFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            CommandHelper.StartProcess(_tortoiseProc, $"/command:add /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void DiskBrowserCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start(_solutionDir);
        }
        private void DiskBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess("explorer.exe", _currentFilePath);
        }

        private void RepoBrowserCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_solutionDir}\"");
        }

        private void RepoBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_currentFilePath}\"");
        }

        private void BranchCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:copy /path:\"{_solutionDir}\"");
        }

        private void SwitchCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:switch /path:\"{_solutionDir}\"");
        }

        private void MergeCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:merge /path:\"{_solutionDir}\"");
        }

        private void MergeFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:merge /path:\"{_currentFilePath}\"");
        }

        private void CleanupCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            CommandHelper.StartProcess(_tortoiseProc, $"/command:cleanup /path:\"{_solutionDir}\"");
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

        private void BlameCommand(object sender, EventArgs e)
        {
            _currentFilePath = FileHelper.GetPath();
            var currentLineIndex = ((TextDocument) Dte.ActiveDocument?.Object(string.Empty))?.Selection.CurrentLine ?? 0;  
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
