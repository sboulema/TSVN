using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using SamirBoulema.TSVN.Properties;
using Process = System.Diagnostics.Process;
// ReSharper disable LocalizableElement

namespace SamirBoulema.TSVN.Helpers
{
    public class CommandHelper
    {
        private readonly DTE _dte;
        private readonly string _tortoiseProc;

        public CommandHelper(DTE dte)
        {
            _dte = dte;
            _tortoiseProc = FileHelper.GetTortoiseSvnProc();
        }

        public void Commit()
        {
            _dte.ExecuteCommand("File.SaveAll", string.Empty);
            Commit(FileHelper.GetSolutionDir());
        }

        public void Commit(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            StartProcess(_tortoiseProc, $"/command:commit /path:\"{filePath}\" /closeonend:0");
        }

        public void Revert()
        {
            Revert(FileHelper.GetSolutionDir());
        }

        public void Revert(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            StartProcess(_tortoiseProc, $"/command:revert /path:\"{filePath}\" /closeonend:0");
        }

        public List<string> GetPendingChanges()
        {
            var pendingChanges = new List<string>();
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{FileHelper.GetSolutionDir()}\" && \"{FileHelper.GetSvnExec()}\" status" + (Settings.Default.HideUnversioned ? " -q" : string.Empty),
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
    }
}
