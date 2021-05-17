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
	public class BaseProgram
	{
        private readonly Generator generator;
		public Options Options { get; set; }
		public string Name = "cmdgen";
		public string LongName = "Command Generator";

		public BaseProgram(Options options)
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

		private void LoadData() 
		{
			Console.Write("Loading objects...");
            var container = Loader.Load<CategoryContainer, Category>("Objects", "Objects.xml", Resources.Objects);
			if (container == null) throw new Exception ("No objects found");
			foreach (var c in container) {
				generator.AllObjects.Add(c);
            }

			Console.Write("Loading names...");
            generator.AllNames = Loader.Load<NameContainer, PersonName>("Names", "Names.xml", Resources.Names);

			Console.Write("Loading locations...");
            var locations = Loader.Load<RoomContainer, Room>("Locations", "Locations.xml", Resources.Locations);
			if (locations == null) throw new Exception("No locations found");
			foreach (var l in locations) {
				generator.AllLocations.Add(l);
			}

			Console.Write("Loading gestures...");
            generator.AllGestures = Loader.Load<GestureContainer, Gesture>("Gestures", "Gestures.xml", Resources.Gestures);

			Console.Write("Loading predefined questions...");
            generator.AllQuestions = Loader.Load<QuestionsContainer, PredefinedQuestion>("Questions", "Questions.xml", Resources.Questions);

			Console.Write("Loading grammars...");
            generator.Grammar = LoadGrammars();
			Generator.Green("Done");
			generator.ValidateLocations();
		}


		private Task GetTask() {
			try
			{
                return generator.GenerateTask();
			}
			catch (StackOverflowException)
			{
				Generator.Err("Could not generate grammar. Grammar is recursive");
			}
			catch (Exception e)
			{
				Generator.Err("Could not generate grammar. Unexpected error");
				Console.WriteLine(e.Message);
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
                Task task = generator.GenerateTask();
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
