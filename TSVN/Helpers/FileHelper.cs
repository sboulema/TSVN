using System.IO;
using EnvDTE;
using Microsoft.Win32;

namespace SamirBoulema.TSVN.Helpers
{
    public static class FileHelper
    {
        public static DTE Dte;

        public static string GetTortoiseSvnProc()
        {
            var path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN", "ProcPath", @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe");
            LogHelper.Log($"TortoiseSvnProc: {path}");

            if (string.IsNullOrEmpty(path))
            {
                path = @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe";
            }

            return path;
        }

        public static string GetSvnExec()
        {
            var path = GetTortoiseSvnProc().Replace("TortoiseProc.exe", "svn.exe");
            LogHelper.Log($"SvnExec: {path}, exists: {File.Exists(path)}");
            return path;
        }

        public static void SaveAllFiles()
        {
            Dte.ExecuteCommand("File.SaveAll");
        }

        /// <summary>
        /// Get the path of the file on which to act upon. 
        /// This can be different depending on where the TSVN context menu was used
        /// </summary>
        /// <returns>File path</returns>
        public static string GetPath()
        {
            if (Dte.ActiveWindow.Type == vsWindowType.vsWindowTypeSolutionExplorer && 
                Dte.SelectedItems.Item(1).ProjectItem != null)
            {
                // Context menu in the Solution Explorer
                return Dte.SelectedItems.Item(1).ProjectItem.FileNames[0];
            }

            // Context menu in the Code Editor
            return Dte.ActiveDocument.FullName;
        }
    }
}
