using EnvDTE;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SamirBoulema.TSVN.Helpers
{
    public class FileHelper
    {
        private readonly DTE _dte;

        public FileHelper(DTE dte)
        {
            _dte = dte;
        }

        public static string GetTortoiseSvnProc()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN", "ProcPath", @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe");
        }

        public static string GetSvnExec()
        {
            return GetTortoiseSvnProc().Replace("TortoiseProc.exe", "svn.exe");
        }

        public void SaveAllFiles()
        {
            _dte.ExecuteCommand("File.SaveAll");
        }

        public string GetSolutionDir()
        {
            var fileName = _dte.Solution.FullName;
            if (!string.IsNullOrEmpty(fileName))
            {
                var path = Path.GetDirectoryName(fileName);
                return FindSVNdir(path);
            }
            return string.Empty;
        }

        private static string FindSVNdir(string path)
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
                    return FindSVNdir(di.Parent.FullName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "TGIT error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return string.Empty;
        }
    }
}
