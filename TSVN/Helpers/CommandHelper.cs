using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using SamirBoulema.TSVN.Properties;
using Process = System.Diagnostics.Process;
// ReSharper disable LocalizableElement

namespace SamirBoulema.TSVN.Helpers
{
    public static class CommandHelper
    {
        public static DTE Dte;

        public static void Commit()
        {
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            Commit(GetRepositoryRoot());
        }

        public static void Commit(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            StartProcess(FileHelper.GetTortoiseSvnProc(), $"/command:commit /path:\"{filePath}\" /closeonend:0");
        }

        public static void Revert() => Revert(GetRepositoryRoot());

        public static void Revert(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            StartProcess(FileHelper.GetTortoiseSvnProc(), $"/command:revert /path:\"{filePath}\" /closeonend:0");
        }

        public static List<string> GetPendingChanges()
        {
            var pendingChanges = new List<string>();
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{GetRepositoryRoot()}\" && \"{FileHelper.GetSvnExec()}\" status" + (Settings.Default.HideUnversioned ? " -q" : string.Empty),
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

        public static string GetRepositoryRoot(string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Path.GetDirectoryName(Dte.ActiveDocument != null ? Dte.ActiveDocument.FullName : Dte.Solution.FullName);
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{path}\" && \"{FileHelper.GetSvnExec()}\" info",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                if (line?.StartsWith("Repository Root:") ?? false)
                {
                    return new Uri(line.Substring(17)).LocalPath;
                }
            }

            return string.Empty;
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
