using EnvDTE;
using Newtonsoft.Json;
using System.IO;

namespace SamirBoulema.TSVN.Options
{
    public static class OptionsHelper
    {
        private const string ApplicationName = "TSVN";
        public static DTE Dte;

        public static Options GetOptions()
        {
            var solutionFilePath = Dte.Solution.FileName;

            if (!File.Exists(solutionFilePath)) return new Options();

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFilePath = Path.Combine(solutionFolder, ".vs", $"{ApplicationName}.json");
            var oldSettingFilePath = Path.Combine(solutionFolder, $"{ApplicationName}.json");

            if (File.Exists(settingFilePath))
            {
                var json = File.ReadAllText(settingFilePath);
                return JsonConvert.DeserializeObject<Options>(json);
            }

            if (File.Exists(oldSettingFilePath))
            {
                var json = File.ReadAllText(oldSettingFilePath);
                File.Delete(oldSettingFilePath);
                return JsonConvert.DeserializeObject<Options>(json);
            }

            return new Options();
        }

        public static void SaveOptions(Options options)
        {
            var json = JsonConvert.SerializeObject(options);

            var solutionFilePath = Dte.Solution.FileName;

            if (!File.Exists(solutionFilePath)) return;

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFilePath = Path.Combine(solutionFolder, ".vs", $"{ApplicationName}.json");

            File.WriteAllText(settingFilePath, json);
        }
    }
}
