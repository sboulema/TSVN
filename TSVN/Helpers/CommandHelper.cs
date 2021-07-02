using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using SamirBoulema.TSVN.Properties;
using Process = System.Diagnostics.Process;
using SamirBoulema.TSVN.Options;
using EnvDTE80;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Helpers
{
    public static class CommandHelper
    {
        public static DTE2 Dte;

        public static async Task Commit()
        {
            Dte.ExecuteCommand("File.SaveAll", string.Empty);
            Commit(await GetRepositoryRoot());
        }

        public static void Commit(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var closeOnEnd = OptionsHelper.Options.CloseOnEnd ? 1 : 0;
            StartProcess(FileHelper.GetTortoiseSvnProc(), $"/command:commit /path:\"{filePath}\" /closeonend:{closeOnEnd}");
        }

        public static async Task Revert() => Revert(await GetRepositoryRoot());

        public static void Revert(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var closeOnEnd = OptionsHelper.Options.CloseOnEnd ? 1 : 0;
            StartProcess(FileHelper.GetTortoiseSvnProc(), $"/command:revert /path:\"{filePath}\" /closeonend:{closeOnEnd}");
        }

        public static void ShowDifferences(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            StartProcess(FileHelper.GetTortoiseSvnProc(), $"/command:diff /path:\"{filePath}\"");
        }

        public static List<string> GetPendingChanges()
        {
            var pendingChanges = new List<string>();

            try
            {
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
            }
            catch (Exception e)
            {
                LogHelper.Log("GetPendingChanges", e);
            }

            return pendingChanges;
        }

        public static async Task<string> GetRepositoryRoot(string path = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var svnInfo = string.Empty;

            try
            {
                // Override any logic with the solution specific Root Folder setting
                OptionsHelper.Dte = Dte;
                var options = await OptionsHelper.GetOptions();
                var rootFolder = options.RootFolder;
                if (!string.IsNullOrEmpty(rootFolder))
                {
                    return rootFolder;
                }

                // Try to find the current working folder, either by open document or by open solution
                if (string.IsNullOrEmpty(path))
                {
                    if (!string.IsNullOrEmpty(Dte.Solution.FileName))
                    {
                        path = Path.GetDirectoryName(Dte.Solution.FullName);
                    }
                    else if (Dte.ActiveDocument != null)
                    {
                        path = Path.GetDirectoryName(Dte.ActiveDocument.FullName);
                    }
                }

                // No solution or file open, we have no way of determining repository root.
                // Fail silently.
                if (string.IsNullOrEmpty(path))
                {
                    return string.Empty;
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
                    var line = await proc.StandardOutput.ReadLineAsync();
                    LogHelper.Log($"SvnInfo: {line}");
                    svnInfo += line;
                    if (line?.StartsWith("Working Copy Root Path:") ?? false)
                    {
                        rootFolder = line.Substring(24);
                    }
                }

                while (!proc.StandardError.EndOfStream)
                {
                    var line = await proc.StandardError.ReadLineAsync();
                    svnInfo += line;
                    LogHelper.Log($"SvnInfo: {line}");
                }

                options.RootFolder = rootFolder;
                await OptionsHelper.SaveOptions(options);

                if (string.IsNullOrEmpty(rootFolder))
                {
                    ShowMissingSolutionDirMessage();
                }

                return rootFolder;
            }
            catch (Exception e)
            {
                LogHelper.Log(svnInfo, e);
            }

            ShowMissingSolutionDirMessage();

            return string.Empty;
        }

        private static void ShowMissingSolutionDirMessage()
        {
            MessageBox.Show("Unable to determine the solution directory location. Please manually set the directory location in the settings.",
                "Missing Working Copy Root Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void StartProcess(string application, string args)
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
