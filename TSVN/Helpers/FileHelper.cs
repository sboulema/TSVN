using System;
using System.IO;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Helpers
{
    public static class FileHelper
    {
        private const string DEFAULT_PROC_PATH = @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe";

        public static string GetTortoiseSvnProc()
        {
            var path = GetRegKeyValue();

            if (string.IsNullOrEmpty(path))
            {
                path = DEFAULT_PROC_PATH;
            }

            return path;
        }

        public static string GetSvnExec() 
            => GetTortoiseSvnProc().Replace("TortoiseProc.exe", "svn.exe");

        public static async Task OpenFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            await VS.Commands.ExecuteAsync("File.OpenFile", filePath);
        }

        /// <summary>
        /// Get the path of the file on which to act upon. 
        /// This can be different depending on where the TSVN context menu was used
        /// </summary>
        /// <returns>File path</returns>
        public static async Task<string> GetPath()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Context menu in the Solution Explorer
            var selectedItem = await VS.Selection.GetSelectedItemAsync();

            if (selectedItem != null)
            {
                if (selectedItem.Type == NodeType.Project ||
                    selectedItem.Type == NodeType.Solution)
                {
                    return Path.GetDirectoryName(selectedItem.FileName);
                }
                else if (selectedItem.Type == NodeType.PhysicalFile)
                {
                    return selectedItem.FileName;
                }
            }

            // Context menu in the Code Editor
            var documentView = await VS.Documents.GetActiveDocumentViewAsync();
            return documentView?.Document?.FilePath;
        }

        private static string GetRegKeyValue()
        {
            var localMachineKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);

            return localMachineKey
                .OpenSubKey(@"SOFTWARE\TortoiseSVN")
                .GetValue("ProcPath", DEFAULT_PROC_PATH)
                .ToString();
        }
    }
}
