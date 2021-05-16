using System;
using System.Collections.Generic;
using System.IO;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Base class for program control logic
	/// </summary>
	public class BaseProgram
	{
        private readonly Generator generator;
		public Options Options { get; set; }
		public string Name = "cmdgen";
		public string LongName = "Command Generator";

		public BaseProgram(Generator g, Options options)
		{
			Options = options;
			generator = g;
		}

		/// <summary>
		/// Loads data from lists and storage
		/// </summary>
		protected void LoadData()
		{
			Console.Write("Loading objects...");
			generator.LoadObjects();
			Console.Write("Loading names...");
			generator.LoadNames();
			Console.Write("Loading locations...");
			generator.LoadLocations();
			Console.Write("Loading gestures...");
			generator.LoadGestures();
			Console.Write("Loading predefined questions...");
			generator.LoadQuestions();
			Console.Write("Loading grammars...");
			generator.LoadGrammars();
			generator.ValidateLocations();
		}


		/// <summary>
		/// Starts the user input loop
		/// </summary>
		public void Run()
		{
			while (true) {
				Console.Write($"{Name} (press q to quit): ");
				ConsoleKeyInfo key = Console.ReadKey();
				Console.WriteLine();
                if (key.Key == ConsoleKey.Escape) return;
				switch (key.KeyChar) {
					case 'c':
						Console.Clear();
						break;
					case 'q':
						return;
					default:
						var task = GetTask();
						if (task != null) task.Print();
						break;
				}
			}
        }
		private void Info(string msg) 
		{
            if (Options.Verbose)
            {
				Console.WriteLine(msg);
			}
		}

		private Task GetTask()
		{
			return generator.GenerateTask(DifficultyDegree.High);
		}

		/// <summary>
		/// Initializes the random task Generator and loads data from lists and storage
		/// </summary>
		public void Setup()
        {
			Console.ForegroundColor = ConsoleColor.Gray;
			Info("");
			Info("GPSR Generator 2019 Release Candidate");
			Info("");
			LoadData();
			Info("");
			Info("");
		}

		private TextWriter GetOutputStream()
		{
			if (Options.Output != null) {
				return new StreamWriter(Options.Output, false, System.Text.Encoding.UTF8);
			} else {
				return Console.Out;
			}
		}

		public void RunBulk()
		{
            Info($"Generating {Options.Bulk} examples in bulk mode");
			using var writer = GetOutputStream();
            for (int i = 1; i <= Options.Bulk; ++i)
            {
                Task task = GetTask();
                if (task == null) continue;
                string sTask = task.ToString().Trim();
                if (sTask.Length < 1) continue;
                sTask = sTask.Capitalize();

                WriteTaskToFile(writer, task, sTask, i);
            }
        }

		private static void WriteTaskToFile(TextWriter writer, Task task, string sTask, int i)
		{
			string pad = String.Empty.PadRight(79, '#');
			writer.WriteLine(pad);
			writer.WriteLine("#");
			writer.WriteLine("# Example {0}", i);
			writer.WriteLine("#");
			writer.WriteLine(pad);
			writer.WriteLine();
			writer.WriteLine(sTask);
			writer.WriteLine();
			List<string> remarks = new();
			foreach (Token token in task.Tokens)
			{
				if (token.Metadata.Count < 1)
					continue;
				if (String.IsNullOrEmpty(token.Name))
					remarks.AddRange(token.Metadata);
				else
				{
					writer.WriteLine("{0}", token.Name);
					foreach (string md in token.Metadata)
						writer.WriteLine("\t{0}", md);
				}
			}
			if (remarks.Count > 0)
			{
				writer.WriteLine("Remarks");
				foreach (string r in remarks)
					writer.WriteLine("\t{0}", r);
			}
			writer.WriteLine();
		}

	}
}
