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
            Instruction previousInstruction = Instruction.GenerateTask;
            string previousLine = "$Main";
            while (true)
            {
                var (instruction, line) = ReadLine();
                if (instruction == Instruction.Repeat)
                {
                    instruction = previousInstruction;
                    line = previousLine;
                }
                switch (instruction)
                {
                    case Instruction.Clear:
                        Console.Clear();
                        break;
                    case Instruction.Quit:
                        return;
                    case Instruction.Help:
                        Console.WriteLine(@"
						h, help => show this help message.
						c, clear => clear the screen.
						s <n>, seed <n> => set the seed to integer n.
						$production, g <production>, generate <production> => generates a sentences starting at <production>.
						");
                        break;
                    case Instruction.GenerateTask:
                        PrintTree(line);
                        break;
                    case Instruction.SetSeed:
                        SetSeed(line);
                        break;
                    default:
                        Console.WriteLine("Press enter to generate a task. Type help for help.");
                        break;
                }
                if (instruction == Instruction.GenerateTask)
                {
                    previousInstruction = instruction;
                    previousLine = line;

                }
            }
        }

        private void SetSeed(string line) 
		{
			try {
				var parts = line.Split(' ');
                Options.Seed = int.Parse(parts[1]);
				Console.WriteLine();
				Console.WriteLine($"Using seed {Options.Seed}");
				Console.WriteLine();
			} catch {
                Logger.Error("incorrect syntax");
				Console.WriteLine("Try \"seed 123\"");
			}
		}

        private void PrintTree(string line)
        {
            try
            {
                string identifier;
                if (line.StartsWith('$'))
                {
                    identifier = line;
                }
                else
                {
                    var parts = line.Split(' ');
					if (parts.Length > 1) {
                        identifier = parts[1];
					} else {
						identifier = "$Main";
					}
                }
                var task = GetTask(identifier);
                if (task != null)
                {
                    PrintTask(task);
                }
            }
            catch
            {
                Logger.Error("incorrect syntax");
                Console.WriteLine("Try \"generate $Main\"");
            }
        }

        private enum Instruction {
			Unknown,
			Clear,
			Quit,
			GenerateTask,
			Help,
			SetSeed,
            Repeat
		}

        private (Instruction instruction, string line) ReadLine()
        {
            Console.Write($"{Name} (h => help, q => quit): ");
            var line = "";
			var instruction = Instruction.Unknown;
            while (instruction == Instruction.Unknown)
            {
				ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape) instruction = Instruction.Quit;
                else if (line == "")
                {
                    if (key.Key == ConsoleKey.Enter) instruction = Instruction.Repeat;
					else if (key.KeyChar == 'c') instruction = Instruction.Clear;
                    else if (key.KeyChar == 'q') instruction = Instruction.Quit;
                    else line += key.KeyChar;
				} else {
					if (key.Key == ConsoleKey.Enter) {
                        // process string command.
						if (line == "c" || line == "clear") instruction = Instruction.Clear;
						else if (line == "q" || line == "quit") instruction = Instruction.Quit;
                        else if (line == "h" || line == "help") instruction = Instruction.Help;
						else if (line.StartsWith("s") && (line.Split(' ')[0] == "s" || line.Split(' ')[0] == "seed")) instruction = Instruction.SetSeed;
                        else if (line.StartsWith("$") || line.StartsWith("g") && (line.Split(' ')[0] == "g" || line.Split(' ')[0] == "generate")) instruction = Instruction.GenerateTask;
                        else break;
                    } else {
                        line += key.KeyChar;
					}
				}
			}
            Console.WriteLine();
            return (instruction, line);
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

		private TaskNode GetTask(string identifier) {
			try
			{
                return Generator.GenerateTask(identifier);
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
