﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Process = System.Diagnostics.Process;
using SamirBoulema.TSVN.Options;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using Settings = SamirBoulema.TSVN.Properties.Settings;
using Microsoft.VisualStudio.Shell.Interop;

namespace SamirBoulema.TSVN.Helpers
{
    public static class CommandHelper
    {
        public static async Task Commit()
        {
            await VS.Commands.ExecuteAsync("File.SaveAll");
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
            try
            {
                // Override any logic with the solution specific Root Folder setting
                var options = await OptionsHelper.GetOptions();
                if (!string.IsNullOrEmpty(options.RootFolder))
                {
                    return options.RootFolder;
                }

                // Try to find the current working folder, either by open document or by open solution
                if (string.IsNullOrEmpty(path))
                {
                    var solution = await VS.Solution.GetCurrentSolutionAsync();
                    var documentView = await VS.Documents.GetActiveDocumentViewAsync();

                    if (!string.IsNullOrEmpty(solution.FileName))
                    {
                        path = Path.GetDirectoryName(solution.FileName);
                    }
                    else if (documentView != null)
                    {
                        path = Path.GetDirectoryName(documentView?.Document?.FilePath);
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
                        Arguments = $"/c cd /D \"{path}\" && \"{FileHelper.GetSvnExec()}\" info --show-item wc-root",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();

                while (!proc.StandardOutput.EndOfStream)
                {
                    options.RootFolder = await proc.StandardOutput.ReadLineAsync();
                }

                await OptionsHelper.SaveOptions(options);

                if (string.IsNullOrEmpty(options.RootFolder))
                {
                    ShowMissingSolutionDirMessage();
                }

                return options.RootFolder;
            }
            catch (Exception e)
            {
                LogHelper.Log(e);
            }

            ShowMissingSolutionDirMessage();

            return string.Empty;
        }

        private static void ShowMissingSolutionDirMessage()
        {
            VS.MessageBox.Show("Unable to determine the solution directory location. Please manually set the directory location in the settings.",
                "Missing Working Copy Root Path", OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }

        public static void StartProcess(string application, string args)
        {
            try
            {
                Process.Start(application, args);
            }
            catch (Exception)
            {
                VS.MessageBox.Show("TortoiseSVN not found. Did you install TortoiseSVN?", "TortoiseSVN not found",
                    OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
        }

        public static async Task RunTortoiseSvnCommand(string command, string args = "")
        {
            var solutionDir = await GetRepositoryRoot();

            if (string.IsNullOrEmpty(solutionDir))
            {
                return;
            }

            var tortoiseProc = FileHelper.GetTortoiseSvnProc();

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;

            StartProcess(tortoiseProc, $"/command:{command} /path:\"{solutionDir}\" {args} /closeonend:{closeOnEnd}");
        }

        public static async Task RunTortoiseSvnFileCommand(string command, string args = "", string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = await FileHelper.GetPath();
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var tortoiseProc = FileHelper.GetTortoiseSvnProc();

            var options = await OptionsHelper.GetOptions();
            var closeOnEnd = options.CloseOnEnd ? 1 : 0;

            StartProcess(tortoiseProc, $"/command:{command} /path:\"{filePath}\" {args} /closeonend:{closeOnEnd}");
        }
    }
}
