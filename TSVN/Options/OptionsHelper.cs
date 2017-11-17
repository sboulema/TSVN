using EnvDTE;
using Newtonsoft.Json;
using System.IO;

namespace SamirBoulema.TSVN.Options
{
    public static class OptionsHelper
    {
        public static DTE Dte;

        public static Options GetOptions()
        {
            var solutionFolder = Path.GetDirectoryName(Dte.Solution.FileName);
            var settingFilePath = Path.Combine(solutionFolder, "TSVN.json");

            if (File.Exists(settingFilePath))
            {
                var json = File.ReadAllText(settingFilePath);
                return JsonConvert.DeserializeObject<Options>(json);
            }

            return new Options();
        }

        public static void SaveOptions(Options options)
        {
            var json = JsonConvert.SerializeObject(options);

            var solutionFolder = Path.GetDirectoryName(Dte.Solution.FileName);
            File.WriteAllText(Path.Combine(solutionFolder, "TSVN.json"), json);
        }
    }
}
