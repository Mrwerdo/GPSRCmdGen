using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using RoboCup.AtHome.CommandGenerator;

namespace RoboCup.AtHome.GPSRCmdGen
{
    public class CreateDatasetProgram
    {
        public Generator Generator { get; set; }
        public CreateDatasetOptions Options { get; set; }
        public Logger Logger { get; set; }

        public CreateDatasetProgram() { }

        private TextWriter GetOutputStream()
        {
            if (Options.Path != null) {
                return new StreamWriter(Options.Path, false, System.Text.Encoding.UTF8);
            } else {
                return Console.Out;
            }
        }

        public void Run()
        {
            using var writer = GetOutputStream();
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            if (Options.Limit is not null) {
                Logger.Info($"Generating {Options.Limit} elements in the dataset (there may be duplicates).");
                for (int index = 1; index <= Options.Limit; ++index)
                {
                    TaskNode task = Generator.GenerateTask();
                    WriteTaskToFile(csv, task, index);
                }
            } else {
                Logger.Info($"Generating all elements in the dataset.");
                var e = Generator.GetEverythingEnumerator().Select((root, index) => (root, index));
                foreach (var (task, index) in e) {
                    WriteTaskToFile(csv, task, index);
                }
            }
        }

        private static void WriteTaskToFile(CsvWriter writer, TaskNode task, int id)
        {
            if (task == null) return;
            string sTask = task.Render();
            if (sTask.Length < 1) return;
            sTask = sTask.Capitalize();
            string sentence = sTask.Trim();
            string remarks = task.MetadataDescription(false);
            string command = task.RenderCommand() ?? "";
            writer.WriteRecord(new DatasetElement() {
                Id = id,
                Sentence = sentence,
                Remarks = remarks,
                Command = command
            });
        }

        private class DatasetElement {
            public int Id { get; set; }
            public string Sentence { get; set; }
            public string Remarks { get; set; }
            public string Command { get; set; }
        }
    }
}