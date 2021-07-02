using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Options
{
    public static class OptionsHelper
    {
        private const string ApplicationName = "TSVN";
        public static DTE2 Dte;
        public static Options Options;

        static OptionsHelper()
        {
            Options = ThreadHelper.JoinableTaskFactory.Run(() => GetOptions());
        }

        public static async Task<Options> GetOptions()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solutionFilePath = Dte.Solution.FileName;

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
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var json = JsonConvert.SerializeObject(options);

            var solutionFilePath = Dte.Solution.FileName;

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
