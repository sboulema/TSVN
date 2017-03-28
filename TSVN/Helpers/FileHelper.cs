using EnvDTE;
using Microsoft.Win32;

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
    }
}
