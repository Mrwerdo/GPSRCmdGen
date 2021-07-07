using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RoboCup.AtHome.CommandGenerator;

namespace RoboCup.AtHome.GPSRCmdGen
{
    public class CreateDatasetProgram
    {
        public Generator Generator { get; set; }
        public CreateDatasetOptions Options { get; set; }
        public Logger Logger { get; set; }

        public CreateDatasetProgram() { }

        private Stream GetOutputStream()
        {
            if (Options?.Path != null) {
                string path = Options.Path;
                return new FileStream(path, FileMode.OpenOrCreate);
            } else {
                return Console.OpenStandardOutput();
            }
        }

        public void Run()
        {
            IEnumerable<(TaskNode task, int index)> sequence;
            if ((Options?.Limit).HasValue) {
                Logger?.Info($"Generating {Options.Limit.Value} elements in the dataset (there may be duplicates).");
                sequence = FiniteAmountIterator(1, Options.Limit.Value).Select((index) => (Generator.GenerateTask(), index));
            } else {
                Logger?.Info($"Generating all elements in the dataset.");
                sequence = Generator.GetEverythingEnumerator().Select((root, index) => (root, index));
            }

            var options = new JsonWriterOptions() { 
                Indented = true
            };
            using var stream = GetOutputStream();
            using var jsonWriter = new Utf8JsonWriter(stream, options);
            jsonWriter.WriteStartArray();
            try {
                foreach (var (task, index) in sequence)
                {
                    var element = ConvertToDatasetElement(task, index);
                    JsonSerializer.Serialize(jsonWriter, element, new JsonSerializerOptions() { WriteIndented = true });
                    jsonWriter.Flush();
                }
            } catch {
                throw;
            } finally {
                jsonWriter.WriteEndArray();
                jsonWriter.Flush();
            }
        }

        private static IEnumerable<int> FiniteAmountIterator(int start, int end) {
            for (int index = start; index < end; index +=1) {
                yield return index;
            }
        }

        private static DatasetElement ConvertToDatasetElement(TaskNode task, int id)
        {
            if (task == null) return null;
            string sTask = task.Render();
            if (sTask.Length < 1) return null;
            sTask = sTask.Capitalize();
            string sentence = sTask.Trim();
            string remarks = task.MetadataDescription(false);
            string command = task.RenderCommand() ?? "";
            return new DatasetElement(id, sentence, remarks, command);
        }

        private class DatasetElement {
            public DatasetElement(int id, string sentence, string remarks, string command)
            {
                Id = id;
                Sentence = sentence;
                Remarks = remarks;
                Command = command;
            }

            public int Id { get; set; }
            public string Sentence { get; set; }
            public string Remarks { get; set; }
            public string Command { get; set; }
        }
    }
}