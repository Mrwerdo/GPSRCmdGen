using System;
using System.Collections.Generic;
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
                Console.Write($"{Name} (h => help, q => quit): ");
                var (instruction, line) = Keyboard.ReadInstructionAndLine();
                Console.WriteLine();
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
						$Main => generates a new random sentence,
                        $task:13 => uses the 13th task production rule to start.
                        $task:13 $single:3 => uses the 13th task, and uses the 3rd definition of $single when expanding the $tasks's sentence.
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
            var seed = Keyboard.ExtractSeed(line);
            if (seed is not null)
            {
                Console.WriteLine();
                Console.WriteLine($"Using seed {Options.Seed}");
                Console.WriteLine();
            }
            else
            {
                Logger.Error("incorrect syntax");
                Console.WriteLine("syntax: seed <natural number>");
            }
		}

        private void PrintTree(string line)
        {
            var tokens = new List<(string, int?)>(Keyboard.ExtractDollarCommand(line));
            var task = tokens.Count > 0 ? Generator.GuidedGenerateTask(tokens) : GetTask("$Main");
            if (task != null)
            {
                PrintTask(task);
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
            var renderedCommand = task.RenderCommand() ?? "Failed to render command: an alternative expression was missing in the parse tree.";
            var command = $"Command:\n{renderedCommand}";
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
