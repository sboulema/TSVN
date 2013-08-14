using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;

namespace FundaRealEstateBV.TSVN
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.3", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidTSVNPkgString)]
    public sealed class TSVNPackage : Package
    {
        private DTE _dte;
        private string _solutionDir;
        private string _currentFilePath;
        private int _currentLineIndex;

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            _dte = (DTE)GetService(typeof(DTE));
            if (!string.IsNullOrEmpty(_dte.Solution.FullName))
            {
                var pathParts = _dte.Solution.FullName.Split(new[] { "\\" }, StringSplitOptions.None);
                _solutionDir = string.Format("{0}\\{1}\\{2}\\", pathParts[0], pathParts[1], pathParts[2]);
            }        

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
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
        }
        #endregion

        public MenuCommand CreateCommand(EventHandler handler, uint commandId)
        {
            CommandID menuCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)commandId);
            MenuCommand menuItem = new MenuCommand(handler, menuCommandId);
            return menuItem;
        }

        #region Button Commands
        private void ShowChangesCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:repostatus /path:\"{0}\" /closeonend:0", _solutionDir));
        }

        private void UpdateCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:update /path:\"{0}\" /closeonend:0", _solutionDir));
        }

        private void UpdateFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:update /path:\"{0}\" /closeonend:0", _currentFilePath));
        }

        private void UpdateToRevisionCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:update /path:\"{0}\" /rev /closeonend:0", _solutionDir));
        }

        private void UpdateToRevisionFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:update /path:\"{0}\" /rev /closeonend:0", _currentFilePath));
        }

        private void PropertiesCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:properties /path:\"{0}\" /closeonend:0", _currentFilePath));
        }

        private void CommitCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:commit /path:\"{0}\" /closeonend:0", _solutionDir));
        }

        private void CommitFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:commit /path:\"{0}\" /closeonend:0", _currentFilePath));
        }

        private void ShowLogCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:log /path:\"{0}\" /closeonend:0", _solutionDir));
        }

        private void ShowLogFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:log /path:\"{0}\" /closeonend:0", _currentFilePath));
        }

        private void CreatePatchCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:createpatch /path:\"{0}\" /noview /closeonend:0", _solutionDir));
        }

        private void ApplyPatchCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Resources.PatchFileFilterString,
                FilterIndex = 1,
                Multiselect = false
            };
            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK) return;

            Process.Start("TortoiseMerge.exe", string.Format("/diff:\"{0}\" /patchpath:\"{1}\"", openFileDialog.FileName, _solutionDir));
        }

        private void RevertCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:revert /path:\"{0}\" /closeonend:0", _solutionDir));
        }

        private void DiskBrowserCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start(_solutionDir);
        }
        private void DiskBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("explorer.exe", _currentFilePath);
        }

        private void RepoBrowserCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:repobrowser /path:\"{0}\"", _solutionDir));
        }

        private void RepoBrowserFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:repobrowser /path:\"{0}\"", _currentFilePath));
        }

        private void BranchCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:copy /path:\"{0}\"", _solutionDir));
        }

        private void SwitchCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:switch /path:\"{0}\"", _solutionDir));
        }

        private void MergeCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:merge /path:\"{0}\"", _solutionDir));
        }

        private void MergeFileCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:merge /path:\"{0}\"", _currentFilePath));
        }

        private void CleanupCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:cleanup /path:\"{0}\"", _solutionDir));
        }

        private void DifferencesCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:diff /path:\"{0}\"", _currentFilePath));
        }

        private void BlameCommand(object sender, EventArgs e)
        {
            _currentFilePath = _dte.ActiveDocument.FullName;
            _currentLineIndex = ((TextDocument)_dte.ActiveDocument.Object(string.Empty)).Selection.CurrentLine;  
            if (string.IsNullOrEmpty(_currentFilePath)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:blame /path:\"{0}\" /line:{1}", _currentFilePath, _currentLineIndex));
        }
        #endregion
    }
}
