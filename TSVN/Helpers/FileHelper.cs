using EnvDTE;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SamirBoulema.TSVN.Helpers
{
    public static class FileHelper
    {
        public static DTE Dte;

        public static string GetTortoiseSvnProc()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN", "ProcPath", @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe");
        }

        public static string GetSvnExec()
        {
            return GetTortoiseSvnProc().Replace("TortoiseProc.exe", "svn.exe");
        }

        public static void SaveAllFiles()
        {
            Dte.ExecuteCommand("File.SaveAll");
        }

        public static string GetSolutionDir()
        {
            var dir = GetSolutionDir(Dte.Solution.FullName);
            if (string.IsNullOrEmpty(dir))
            {
                dir = GetSolutionDir(Dte.ActiveDocument.ActiveWindow.Project.FullName);
            }
            return dir;
        }

        private static string GetSolutionDir(string filename)
        {     
            if (!string.IsNullOrEmpty(filename))
            {
                var path = Path.GetDirectoryName(filename);
                return FindSvndir(path);
            }
            return string.Empty;
        }

        private static string FindSvndir(string path)
        {
            try
            {
                var di = new DirectoryInfo(path);
                if (di.GetDirectories().Any(d => d.Name.Equals(".svn")))
                {
                    return di.FullName;
                }
                if (di.Parent != null)
                {
                    return FindSvndir(di.Parent.FullName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "TSVN error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return string.Empty;
        }
    }
}
