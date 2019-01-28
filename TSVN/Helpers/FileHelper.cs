using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.Win32;

namespace SamirBoulema.TSVN.Helpers
{
    public static class FileHelper
    {
        public static DTE2 Dte;

        public static string GetTortoiseSvnProc()
        {
            var path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN", "ProcPath", @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe");

            if (string.IsNullOrEmpty(path))
            {
                path = @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe";
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
            Dte.ExecuteCommand("File.OpenFile", filePath);
        }

        /// <summary>
        /// Get the path of the file on which to act upon. 
        /// This can be different depending on where the TSVN context menu was used
        /// </summary>
        /// <returns>File path</returns>
        public static string GetPath()
        {
            // Context menu in the Solution Explorer
            if (Dte.ActiveWindow.Type == vsWindowType.vsWindowTypeSolutionExplorer)
            {
                var selectedItem = ((object[])Dte.ToolWindows.SolutionExplorer.SelectedItems)[0] as UIHierarchyItem;

                if (selectedItem == null) return string.Empty;

                if (selectedItem.Object is Project || selectedItem.Object is Solution)
                {
                    return Path.GetDirectoryName(selectedItem.Object.FileName);
                }
                else if (selectedItem.Object is ProjectItem)
                {
                    return (selectedItem.Object as ProjectItem).FileNames[0];
                }
            }

            // Context menu in the Code Editor
            return Dte.ActiveDocument.FullName;
        }
    }
}
