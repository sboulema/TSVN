using EnvDTE;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Process = System.Diagnostics.Process;

namespace SamirBoulema.TSVN.Helpers
{
    public class CommandHelper
    {
        private DTE dte;
        private string tortoiseProc;


        public CommandHelper(DTE dte)
        {
            this.dte = dte;
            tortoiseProc = GetTortoiseSVNProc();
        }

        public void Commit()
        {
            dte.ExecuteCommand("File.SaveAll", string.Empty);
            Commit(GetSolutionDir());
        }

        public void Commit(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            StartProcess(tortoiseProc, string.Format("/command:commit /path:\"{0}\" /closeonend:0", filePath));
        }

        public void Revert()
        {
            Revert(GetSolutionDir());
        }

        public void Revert(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            StartProcess(tortoiseProc, string.Format("/command:revert /path:\"{0}\" /closeonend:0", filePath));
        }

        private string GetSolutionDir()
        {
            string fileName = dte.Solution.FullName;
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Please open a solution first", "TSVN error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var path = Path.GetDirectoryName(fileName);
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
                throw new FileNotFoundException("Unable to find .svn directory.\nIs your solution under SVN source control?");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "TSVN error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return string.Empty;
        }

        private string GetTortoiseSVNProc()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN", "ProcPath", @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe");
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
