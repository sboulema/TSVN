using System;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

namespace SamirBoulema.TSVN.Helpers
{
    public static class FileHelper
    {
        public static DTE2 Dte;
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

        public static void SaveAllFiles()
        {
            Dte.ExecuteCommand("File.SaveAll");
        }

        public static void OpenFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Dte.ExecuteCommand("File.OpenFile", filePath);
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
            if (Dte.ActiveWindow.Type == vsWindowType.vsWindowTypeSolutionExplorer)
            {
                if (!(((object[])Dte.ToolWindows.SolutionExplorer.SelectedItems)[0] is UIHierarchyItem selectedItem))
                {
                    return string.Empty;
                }

                if (selectedItem.Object is Project || selectedItem.Object is Solution)
                {
                    return Path.GetDirectoryName((selectedItem.Object as Solution).FileName);
                }
                else if (selectedItem.Object is ProjectItem)
                {
                    return (selectedItem.Object as ProjectItem).FileNames[0];
                }
            }

            // Context menu in the Code Editor
            return Dte.ActiveDocument.FullName;
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
