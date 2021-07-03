using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using Settings = SamirBoulema.TSVN.Properties.Settings;

namespace SamirBoulema.TSVN
{
    [Guid("81a57ae8-6550-4dd0-940c-503d379550cc")]
    public class TSVNToolWindow : ToolWindowPane
    {
        private readonly TSVNToolWindowControl _tsvnToolWindowControl;

        public TSVNToolWindow() : base(null)
        {
            Caption = "TSVN Pending Changes";
            Content = new TSVNToolWindowControl();

            _tsvnToolWindowControl = Content as TSVNToolWindowControl;
        }

        public override void OnToolWindowCreated() => _ = ToolWindowCreated();

        private async Task ToolWindowCreated()
        {
            VS.Events.DocumentEvents.Saved += DocumentEvents_Saved;

            VS.Events.SolutionEvents.OnAfterOpenSolution += SolutionEvents_OnAfterOpenSolution;

            _tsvnToolWindowControl.HideUnversionedButton.IsChecked = Settings.Default.HideUnversioned;

            _tsvnToolWindowControl.Update(CommandHelper.GetPendingChanges(), await CommandHelper.GetRepositoryRoot());
        }

        private void SolutionEvents_OnAfterOpenSolution(object sender, Microsoft.VisualStudio.Shell.Events.OpenSolutionEventArgs e)
        {
            _ = Update();
        }

        private void DocumentEvents_Saved(object sender, string e)
        {
            _ = Update();
        }

        private async Task Update()
            => _tsvnToolWindowControl.Update(CommandHelper.GetPendingChanges(), await CommandHelper.GetRepositoryRoot());
    }
}
