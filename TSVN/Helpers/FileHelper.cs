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
        private DTE dte;

        public FileHelper(DTE dte)
        {
            this.dte = dte;
        }

        public string GetTortoiseSVNProc()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN", "ProcPath", @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe");
        }

        public string GetSVNExec()
        {
            return GetTortoiseSVNProc().Replace("TortoiseProc.exe", "svn.exe");
        }

        public void SaveAllFiles()
        {
            dte.ExecuteCommand("File.SaveAll");
        }

        public string GetSolutionDir()
        {
            string fileName = dte.Solution.FullName;
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
