using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RoboCup.AtHome.CommandGenerator.Containers;
using RoboCup.AtHome.CommandGenerator.ReplaceableTypes;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Base class for program control logic
	/// </summary>
	public class Program
	{
        private readonly Generator generator;
		public Options Options { get; set; }
		public string Name = "cmdgen";
		public string LongName = "Command Generator";

		public Program(Options options)
		{
			Options = options;
			generator = new Generator(options.Seed);
		}

		/// <summary>
		/// Starts the user input loop
		/// </summary>
		public void Run()
		{
            Info($"Selected {generator.Grammar.Name} {generator.Grammar.Tier} difficulty degree grammar.");

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
						if (task != null) PrintTask(task);
                        break;
				}
			}
        }

        private static void PrintTask(TaskNode task)
        {
            var header = new string('=', Console.BufferWidth - 1);
            var sentence = task.Render().Capitalize().Wrapped(Console.BufferWidth);
            var comments = "";
            task.EnumerateTree(t =>
            {
                comments += t.TextWildcard?.Comment ?? "";
            });
            var parseTree = $"Parse Tree:\n{task.PrettyTree()}";
            var command = $"Command:\n{task.RenderCommand()}";

            var output = $"\n{header}\n\n{sentence}\n\n{comments}\n{parseTree}\n\n{command}\n\n{header}\n";
            Console.WriteLine(output);
        }

        /// <summary>
        /// Writes the provided message string to the console in GREEN text
        /// </summary>
        /// <param name="message">The message to be written.</param>
        private static void Success(string message)
        {
            ConsoleColor pc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(message);
            Console.ForegroundColor = pc;
        }

        private static void Warning(string message)
        {
            ConsoleColor pc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = pc;
        }

		private void Info(string msg) 
		{
            if (Options.Verbose)
            {
				Console.WriteLine(msg);
			}
		}

        private void Error(string msg, Exception error = null)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.Write("error: ");
            Console.ForegroundColor = current;
            Console.Error.WriteLine(msg);
            if (Options.Verbose && error != null)
                Console.Error.WriteLine(error.Message);
        }

		private void LoadData() 
		{
            var container = Load<CategoryContainer, Category>("Objects", Options.Objects ?? "Objects.xml", Resources.Objects);
			if (container == null) throw new Exception ("No objects found");
			foreach (var c in container) {
				generator.AllObjects.Add(c);
            }

            generator.AllNames = Load<NameContainer, PersonName>("Names", Options.Names ?? "Names.xml", Resources.Names);

            var locations = Load<RoomContainer, Room>("Locations", "Locations.xml", Resources.Locations);
			if (locations == null) throw new Exception("No locations found");
			foreach (var l in locations) {
				generator.AllLocations.Add(l);
			}

            generator.AllGestures = Load<GestureContainer, Gesture>("Gestures", "Gestures.xml", Resources.Gestures);

            generator.AllQuestions = Load<QuestionsContainer, PredefinedQuestion>("Questions", "Questions.xml", Resources.Questions);

			Console.Write("Loading grammars...");
            generator.Grammar = LoadGrammars();
			Success("\tDone");
			generator.ValidateLocations();
		}

        public static List<V> Load<P, V>(string name, string path, string backup) where P : ILoadingContainer<V>
		{
			try {
				Console.Write($"Loading {name.ToLower()}...");
                P obj = Loader.LoadObject<P>(Loader.GetPath(path));
                Success("\tDone");
				return obj.Results;
			} catch {
                P obj = Loader.LoadXmlString<P>(backup);
                Warning($"Default {name} loaded");
				Console.WriteLine();
				return obj.Results;
			}
		}

		private TaskNode GetTask() {
			try
			{
                return generator.GenerateTask();
			}
			catch (StackOverflowException)
			{
                Error("Could not generate grammar. Grammar is recursive");
			}
			catch (Exception error)
			{
                Error("Could not generate grammar. Unexpected error", error);
            }
			return null;
		}

        private Grammar LoadGrammars()
        {
            var grammar = new Grammar();
            var loader = new Grammar.GrammarLoader();
            if (!Options.Files.Any())
            {
                var g = loader.LoadText(Resources.CommonRules.Split('\n'), null, false);
				foreach (var rule in g.ProductionRules) 
				{
					grammar.AddRule(rule);
				}
                g = loader.LoadText(Resources.GPSRGrammar.Split('\n'), null, false);
				foreach (var rule in g.ProductionRules) 
				{
					grammar.AddRule(rule);
				}
				grammar.Tier = g.Tier;
			}
            foreach (var file in Options.Files)
            {
                var g = loader.Load(file, false);
                foreach (var rule in g.ProductionRules)
                {
                    grammar.AddRule(rule);
                }
				if (g.Tier.CompareTo(grammar.Tier) > 0) {
					grammar.Tier = g.Tier;
				}
            }
            return grammar;
        }

		/// <summary>
		/// Initializes the random task Generator and loads data from lists and storage
		/// </summary>
		public void Setup()
        {
            if (!FilesExist()) Environment.Exit(-1);

			Console.ForegroundColor = ConsoleColor.Gray;
			Info("");
			Info("GPSR Generator 2019 Release Candidate");
			Info("");
			LoadData();
			Info("");
			Info("");
		}

		private bool FilesExist() {
			foreach (string path in Options.Files)
			{
				if (!File.Exists(path))
				{
					Error($"{path} does not exist.");
					return false;
				}
			}
			return true;
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
                TaskNode task = generator.GenerateTask();
                if (task == null) continue;
                string sTask = task.Render();
                if (sTask.Length < 1) continue;
                sTask = sTask.Capitalize();

                WriteTaskToFile(writer, task, sTask, i);
            }
        }

		private static void WriteTaskToFile(TextWriter writer, TaskNode task, string sTask, int i)
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
			writer.Write(task.MetadataDescription(false));
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
