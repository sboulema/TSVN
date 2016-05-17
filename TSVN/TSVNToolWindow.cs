namespace SamirBoulema.TSVN
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using System.Diagnostics;
    using System.Collections.Generic;
    using EnvDTE;
    using Helpers;
    using Process = System.Diagnostics.Process;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("81a57ae8-6550-4dd0-940c-503d379550cc")]
    public class TSVNToolWindow : ToolWindowPane
    {
        private FileHelper fileHelper;
        private DocumentEvents documentEvents;
        private SolutionEvents solutionEvents;
        private TSVNToolWindowControl tsvnToolWindowControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="TSVNToolWindow"/> class.
        /// </summary>
        /// 
        public TSVNToolWindow() : base(null)
        {
            Caption = "TSVN Pending Changes";
            Content = new TSVNToolWindowControl();

            tsvnToolWindowControl = Content as TSVNToolWindowControl;
        }

        public override void OnToolWindowCreated()
        {
            var tsvnPackage = Package as TSVNPackage;
            DTE dte = (DTE)tsvnPackage.GetServiceHelper(typeof(DTE));
            tsvnToolWindowControl.SetDTE(dte);
            fileHelper = new FileHelper(dte);

            documentEvents = dte.Events.DocumentEvents;
            documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

            solutionEvents = dte.Events.SolutionEvents;
            solutionEvents.Opened += SolutionEvents_Opened;

            tsvnToolWindowControl.Update(GetPendingChanges(), fileHelper.GetSolutionDir());
        }

        private void SolutionEvents_Opened()
        {
            tsvnToolWindowControl.Update(GetPendingChanges(), fileHelper.GetSolutionDir());
        }

        private void DocumentEvents_DocumentSaved(Document Document)
        {
            tsvnToolWindowControl.Update(GetPendingChanges(), fileHelper.GetSolutionDir());
        }

        private List<string> GetPendingChanges()
        {
            var pendingChanges = new List<string>();
            string output = string.Empty;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{fileHelper.GetSolutionDir()}\" && \"{fileHelper.GetSVNExec()}\" status",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                pendingChanges.Add(proc.StandardOutput.ReadLine());
            }

            return pendingChanges;
        }
    }
}
