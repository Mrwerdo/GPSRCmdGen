using System;
using RoboCup.AtHome.CommandGenerator;

namespace RoboCup.AtHome.GPSRCmdGen
{
	public class InteractiveProgram
	{
        public Generator Generator { get; set; }
		public InteractiveOptions Options { get; set; }
		public string Name = "cmdgen";
		public Logger Logger { get; set; }

        public InteractiveProgram() { }

		public void Run()
		{
			Logger.Info("");
			Logger.Info($"Using seed: {Options.Seed}");
			Logger.Info("");
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

		private TaskNode GetTask() {
			try
			{
                return Generator.GenerateTask();
			}
			catch (StackOverflowException)
			{
                Logger.Error("Could not generate grammar. Grammar is recursive");
			}
			catch (Exception error)
			{
                Logger.Error("Could not generate grammar. Unexpected error", error);
            }
			return null;
		}
	}
}
