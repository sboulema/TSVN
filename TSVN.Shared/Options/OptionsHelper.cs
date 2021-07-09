using Community.VisualStudio.Toolkit;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using File = System.IO.File;

namespace SamirBoulema.TSVN.Options
{
    public static class OptionsHelper
    {
        private const string ApplicationName = "TSVN";

        public static async Task<Options> GetOptions()
        {
            var solution = await VS.Solution.GetCurrentSolutionAsync();
            var solutionFilePath = solution?.FileName;

            if (!File.Exists(solutionFilePath))
            {
                return new Options();
            }

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

        public static async Task SaveOptions(Options options)
        {
            var json = JsonConvert.SerializeObject(options);

            var solution = await VS.Solution.GetCurrentSolutionAsync().ConfigureAwait(false);
            var solutionFilePath = solution?.FileName;

            if (!File.Exists(solutionFilePath))
            {
                return;
            }

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            var settingFilePath = Path.Combine(solutionFolder, ".vs", $"{ApplicationName}.json");

            File.WriteAllText(settingFilePath, json);
        }
    }
}
