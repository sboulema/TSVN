using SamirBoulema.TSVN.Properties;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using SamirBoulema.TSVN.Helpers;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN
{
    [Guid("81a57ae8-6550-4dd0-940c-503d379550cc")]
    public class TSVNToolWindow : ToolWindowPane
    {
        private DocumentEvents _documentEvents;
        private SolutionEvents _solutionEvents;
        private readonly TSVNToolWindowControl _tsvnToolWindowControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="TSVNToolWindow"/> class.
        /// </summary>
        /// 
        public TSVNToolWindow() : base(null)
        {
            Caption = "TSVN Pending Changes";
            Content = new TSVNToolWindowControl();

            _tsvnToolWindowControl = Content as TSVNToolWindowControl;
        }

        public override void OnToolWindowCreated() => _ = ToolWindowCreated();

        private async Task ToolWindowCreated()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(); await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var tsvnPackage = Package as TsvnPackage;
            var dte = (DTE)tsvnPackage.GetServiceHelper(typeof(DTE));

            _documentEvents = dte.Events.DocumentEvents;
            _documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

            _solutionEvents = dte.Events.SolutionEvents;
            _solutionEvents.Opened += SolutionEvents_Opened;

            _tsvnToolWindowControl.HideUnversionedButton.IsChecked = Settings.Default.HideUnversioned;

            _tsvnToolWindowControl.Update(CommandHelper.GetPendingChanges(), await CommandHelper.GetRepositoryRoot());
        }

        private void SolutionEvents_Opened() => _ = Update();
        private void DocumentEvents_DocumentSaved(Document document) => _ = Update();

        private async Task Update() => _tsvnToolWindowControl.Update(CommandHelper.GetPendingChanges(), await CommandHelper.GetRepositoryRoot());
    }
}
