using System;
using System.IO;
using RoboCup.AtHome.CommandGenerator;

namespace RoboCup.AtHome.GPSRCmdGen
{
	public class BulkProgram
	{
        public Generator Generator { get; set; }
		public BulkOptions Options { get; set; }
        public Logger Logger { get; set; }

		public BulkProgram() { }

        private TextWriter GetOutputStream()
		{
			if (Options.Output != null) {
				return new StreamWriter(Options.Output, false, System.Text.Encoding.UTF8);
			} else {
				return Console.Out;
			}
		}

		public void Run()
		{
            Logger.Info($"Generating {Options.Count} examples in bulk mode");
			using var writer = GetOutputStream();
            for (int i = 1; i <= Options.Count; ++i)
            {
                TaskNode task = Generator.GenerateTask();
                if (task == null) continue;
                string sTask = task.Render();
                if (sTask.Length < 1) continue;
                sTask = sTask.Capitalize();

                WriteTaskToFile(writer, task, sTask, i);
            }
        }

		private static void WriteTaskToFile(TextWriter writer, TaskNode task, string sTask, int i)
		{
			string pad = string.Empty.PadRight(79, '#');
			writer.WriteLine(pad);
			writer.WriteLine("#");
			writer.WriteLine("# Example {0}", i);
			writer.WriteLine("#");
			writer.WriteLine(pad);
			writer.WriteLine();
            writer.WriteLine(sTask.Trim());
			writer.WriteLine();
			writer.Write(task.MetadataDescription(false) ?? "\n");
			// List<string> remarks = new();
			// foreach (Token token in task.Tokens)
			// {
			// 	if (token.Metadata.Count < 1)
			// 		continue;
			// 	if (String.IsNullOrEmpty(token.Name))
			// 		remarks.AddRange(token.Metadata);
			// 	else
			// 	{
			// 		writer.WriteLine("{0}", token.Name);
			// 		foreach (string md in token.Metadata)
			// 			writer.WriteLine("\t{0}", md);
			// 	}
			// }
			// if (remarks.Count > 0)
			// {
			// 	writer.WriteLine("Remarks");
			// 	foreach (string r in remarks)
			// 		writer.WriteLine("\t{0}", r);
			// }
			// writer.WriteLine();
		}

    }
}