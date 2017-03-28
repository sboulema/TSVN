using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Forms;
using SamirBoulema.TSVN.Helpers;
// ReSharper disable LocalizableElement

namespace SamirBoulema.TSVN
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.9", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidTSVNPkgString)]
    [ProvideToolWindow(typeof(TSVNToolWindow))]
    public sealed class TsvnPackage : Package
    {
        public DTE Dte;
        private string _solutionDir;
        private string _currentFilePath;
        private string _tortoiseProc;

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            Dte = (DTE)GetService(typeof(DTE));

            FileHelper.Dte = Dte;
            CommandHelper.Dte = Dte;

            _tortoiseProc = FileHelper.GetTortoiseSvnProc();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null == mcs) return;

            mcs.AddCommand(CreateCommand(ShowChangesCommand, PkgCmdIdList.ShowChangesCommand));
            mcs.AddCommand(CreateCommand(UpdateCommand, PkgCmdIdList.UpdateCommand));
            mcs.AddCommand(CreateCommand(UpdateToRevisionCommand, PkgCmdIdList.UpdateToRevisionCommand));
            mcs.AddCommand(CreateCommand(CommitCommand, PkgCmdIdList.CommitCommand));
            mcs.AddCommand(CreateCommand(ShowLogCommand, PkgCmdIdList.ShowLogCommand));
            mcs.AddCommand(CreateCommand(CreatePatchCommand, PkgCmdIdList.CreatePatchCommand));
            mcs.AddCommand(CreateCommand(ApplyPatchCommand, PkgCmdIdList.ApplyPatchCommand));
            mcs.AddCommand(CreateCommand(RevertCommand, PkgCmdIdList.RevertCommand));
            mcs.AddCommand(CreateCommand(DiskBrowserCommand, PkgCmdIdList.DiskBrowser));
            mcs.AddCommand(CreateCommand(RepoBrowserCommand, PkgCmdIdList.RepoBrowser));
            mcs.AddCommand(CreateCommand(BranchCommand, PkgCmdIdList.BranchCommand));
            mcs.AddCommand(CreateCommand(SwitchCommand, PkgCmdIdList.SwitchCommand));
            mcs.AddCommand(CreateCommand(MergeCommand, PkgCmdIdList.MergeCommand));
            mcs.AddCommand(CreateCommand(DifferencesCommand, PkgCmdIdList.DifferencesCommand));
            mcs.AddCommand(CreateCommand(BlameCommand, PkgCmdIdList.BlameCommand));
            mcs.AddCommand(CreateCommand(ShowLogFileCommand, PkgCmdIdList.ShowLogFileCommand));
            mcs.AddCommand(CreateCommand(CleanupCommand, PkgCmdIdList.CleanupCommand));
            mcs.AddCommand(CreateCommand(DiskBrowserFileCommand, PkgCmdIdList.DiskBrowserFileCommand));
            mcs.AddCommand(CreateCommand(RepoBrowserFileCommand, PkgCmdIdList.RepoBrowserFileCommand));
            mcs.AddCommand(CreateCommand(MergeFileCommand, PkgCmdIdList.MergeFileCommand));
            mcs.AddCommand(CreateCommand(UpdateToRevisionFileCommand, PkgCmdIdList.UpdateToRevisionFileCommand));
            mcs.AddCommand(CreateCommand(PropertiesCommand, PkgCmdIdList.PropertiesCommand));
            mcs.AddCommand(CreateCommand(UpdateFileCommand, PkgCmdIdList.UpdateFileCommand));
            mcs.AddCommand(CreateCommand(CommitFileCommand, PkgCmdIdList.CommitFileCommand));
            mcs.AddCommand(CreateCommand(DiffPreviousCommand, PkgCmdIdList.DiffPreviousCommand));
            mcs.AddCommand(CreateCommand(RevertFileCommand, PkgCmdIdList.RevertFileCommand));
            mcs.AddCommand(CreateCommand(AddFileCommand, PkgCmdIdList.AddFileCommand));

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
            mcs.AddCommand(tsvnMenu);
            mcs.AddCommand(tsvnContextMenu);
            TSVNToolWindowCommand.Initialize(this);
        }
        #endregion

        private static void StartProcess(string application, string args)
        {
            try
            {
                Process.Start(application, args);
            }
            catch (Exception)
            {
                MessageBox.Show("TortoiseSVN not found. Did you install TortoiseSVN?", "TortoiseSVN not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
            StartProcess(_tortoiseProc, $"/command:repostatus /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void UpdateCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void UpdateFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            StartProcess(_tortoiseProc, $"/command:update /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void UpdateToRevisionCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            StartProcess(_tortoiseProc, $"/command:update /path:\"{_solutionDir}\" /rev /closeonend:0");
        }

        private void UpdateToRevisionFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            StartProcess(_tortoiseProc, $"/command:update /path:\"{_currentFilePath}\" /rev /closeonend:0");
        }

        private void PropertiesCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:properties /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void CommitCommand(object sender, EventArgs e)
        {  
            _solutionDir = CommandHelper.GetRepositoryRoot();
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            StartProcess(_tortoiseProc, $"/command:commit /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void CommitFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            StartProcess(_tortoiseProc, $"/command:commit /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void ShowLogCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:log /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void ShowLogFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:log /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void CreatePatchCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:createpatch /path:\"{_solutionDir}\" /noview /closeonend:0");
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

            StartProcess("TortoiseMerge.exe", $"/diff:\"{openFileDialog.FileName}\" /patchpath:\"{_solutionDir}\"");
        }

        private void RevertCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:revert /path:\"{_solutionDir}\" /closeonend:0");
        }

        private void RevertFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:revert /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void AddFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Dte.ActiveDocument?.Save();
            StartProcess(_tortoiseProc, $"/command:add /path:\"{_currentFilePath}\" /closeonend:0");
        }

        private void DiskBrowserCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start(_solutionDir);
        }
        private void DiskBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess("explorer.exe", _currentFilePath);
        }

        private void RepoBrowserCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_solutionDir}\"");
        }

        private void RepoBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:repobrowser /path:\"{_currentFilePath}\"");
        }

        private void BranchCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:copy /path:\"{_solutionDir}\"");
        }

        private void SwitchCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:switch /path:\"{_solutionDir}\"");
        }

        private void MergeCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:merge /path:\"{_solutionDir}\"");
        }

        private void MergeFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:merge /path:\"{_currentFilePath}\"");
        }

        private void CleanupCommand(object sender, EventArgs e)
        {
            _solutionDir = CommandHelper.GetRepositoryRoot();  
            if (string.IsNullOrEmpty(_solutionDir)) return;
            StartProcess(_tortoiseProc, $"/command:cleanup /path:\"{_solutionDir}\"");
        }

        private void DifferencesCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:diff /path:\"{_currentFilePath}\"");
        }

        private void DiffPreviousCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:prevdiff /path:\"{_currentFilePath}\"");
        }

        private void BlameCommand(object sender, EventArgs e)
        {
            _currentFilePath = Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            var currentLineIndex = ((TextDocument) Dte.ActiveDocument?.Object(string.Empty))?.Selection.CurrentLine ?? 0;  
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            StartProcess(_tortoiseProc, $"/command:blame /path:\"{_currentFilePath}\" /line:{currentLineIndex}");
        }
        #endregion
    }
}
