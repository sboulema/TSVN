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
    [InstalledProductRegistration("#110", "#112", "1.2", IconResourceID = 400)]
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
            if ( null != mcs )
            {
                CommandID showChangesCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.ShowChangesCommand);
                MenuCommand showChangesMenuItem = new MenuCommand(ShowChangesCommand, showChangesCommandId);
                mcs.AddCommand(showChangesMenuItem);

                CommandID updateCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.UpdateCommand);
                MenuCommand menuItem = new MenuCommand(UpdateCommand, updateCommandId);
                mcs.AddCommand( menuItem );

                CommandID commitCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.CommitCommand);
                MenuCommand commitMenuItem = new MenuCommand(CommitCommand, commitCommandId);
                mcs.AddCommand(commitMenuItem);

                CommandID showLogCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.ShowLogCommand);
                MenuCommand showLogMenuItem = new MenuCommand(ShowLogCommand, showLogCommandId);
                mcs.AddCommand(showLogMenuItem);

                CommandID createPatchCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.CreatePatchCommand);
                MenuCommand createPatchMenuItem = new MenuCommand(CreatePatchCommand, createPatchCommandId);
                mcs.AddCommand(createPatchMenuItem);

                CommandID applyPatchCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.ApplyPatchCommand);
                MenuCommand applyPatchMenuItem = new MenuCommand(ApplyPatchCommand, applyPatchCommandId);
                mcs.AddCommand(applyPatchMenuItem);

                CommandID revertCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.RevertCommand);
                MenuCommand revertMenuItem = new MenuCommand(RevertCommand, revertCommandId);
                mcs.AddCommand(revertMenuItem);

                CommandID diskBrowserCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.DiskBrowser);
                MenuCommand diskBrowserMenuItem = new MenuCommand(DiskBrowserCommand, diskBrowserCommandId);
                mcs.AddCommand(diskBrowserMenuItem);

                CommandID repoBrowserCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.RepoBrowser);
                MenuCommand repoBrowserMenuItem = new MenuCommand(RepoBrowserCommand, repoBrowserCommandId);
                mcs.AddCommand(repoBrowserMenuItem);

                CommandID branchCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.BranchCommand);
                MenuCommand branchMenuItem = new MenuCommand(BranchCommand, branchCommandId);
                mcs.AddCommand(branchMenuItem);

                CommandID switchCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.SwitchCommand);
                MenuCommand switchMenuItem = new MenuCommand(SwitchCommand, switchCommandId);
                mcs.AddCommand(switchMenuItem);

                CommandID mergeCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.MergeCommand);
                MenuCommand mergeMenuItem = new MenuCommand(MergeCommand, mergeCommandId);
                mcs.AddCommand(mergeMenuItem);

                CommandID differencesCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.DifferencesCommand);
                MenuCommand differencesMenuItem = new MenuCommand(DifferencesCommand, differencesCommandId);
                mcs.AddCommand(differencesMenuItem);

                CommandID blameCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.BlameCommand);
                MenuCommand blameMenuItem = new MenuCommand(BlameCommand, blameCommandId);
                mcs.AddCommand(blameMenuItem);

                CommandID showLogFileCommandId = new CommandID(GuidList.guidTSVNCmdSet, (int)PkgCmdIdList.ShowLogFileCommand);
                MenuCommand showLogFileMenuItem = new MenuCommand(ShowLogFileCommand, showLogFileCommandId);
                mcs.AddCommand(showLogFileMenuItem);
            }
        }
        #endregion

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

        private void CommitCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:commit /path:\"{0}\" /closeonend:0", _solutionDir));
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

        private void RepoBrowserCommand(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_solutionDir)) return;
            Process.Start("TortoiseProc.exe", string.Format("/command:repobrowser /path:\"{0}\"", _solutionDir));
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
