using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable

namespace RoboCup.AtHome.GPSRCmdGen
{
    public enum Instruction
    {
        Unknown,
        Clear,
        Quit,
        GenerateTask,
        Help,
        SetSeed,
        Repeat
    }

    public static class Keyboard
    {
        public static (Instruction instruction, string line) ReadInstructionAndLine()
        {
            var line = "";
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape) return (Instruction.Quit, line);
                else if (line == "")
                {
                    if (key.Key == ConsoleKey.Enter) return (Instruction.Repeat, line);
                    else if (key.KeyChar == 'c') return (Instruction.Clear, line);
                    else if (key.KeyChar == 'q') return (Instruction.Quit, line);
                    else line += key.KeyChar;
                }
                else
                {
                    if (key.Key == ConsoleKey.Enter)
                    {
                        
                        // process string command.
                        if (line == "c" || line == "clear") return (Instruction.Clear, line);
                        else if (line == "q" || line == "quit") return (Instruction.Quit, line);
                        else if (line == "h" || line == "help") return (Instruction.Help, line);
                        else if (DetectSeed(line)) return (Instruction.SetSeed, line);
                        else if (line.StartsWith("$")) return (Instruction.GenerateTask, line);
                        else return (Instruction.Unknown, line);
                    }
                    else
                    {
                        line += key.KeyChar;
                    }
                }
            }
        }


        public static bool DetectSeed(string line) 
        {
            var regex = new Regex(@"^s(e(ed?)?)?");
            var match = regex.Match(line);
            return match.Success;
        }

        public static int? ExtractSeed(string line)
        {
            var regex = new Regex(@"^s(e(ed?)?)? +(?<seed>[0-9]+)$");
            var match = regex.Match(line);
            var seed = match.Groups["seed"];
            return seed.Success ? int.Parse(seed.Value) : null;
        }

        public static IEnumerable<(string token, int? index)> ExtractDollarCommand(string line)
        {
            var regex = new Regex(@"^\$(?<token>[a-zA-Z0-9]+)(:(?<index>[0-9]+))?( \$?(?<token>[a-zA-Z0-9]+)(:(?<index>[0-9]+))?)*$");
            var matches = regex.Matches(line);
            if (matches.Count == 0) return new List<(string, int?)>();
            return ExtractTokens(line);
        }

        private static IEnumerable<(string token, int? index)> ExtractTokens(string line)
        {
            var regex = new Regex(@"\$?(?<token>[a-zA-Z0-9]+)(:(?<index>[0-9]+))?");
            return regex.Matches(line).Select((Match t) =>
            {
                var token = t.Groups["token"];
                var index = t.Groups["index"];
                int? i = index.Success ? int.Parse(index.Value) : null;
                return (token.Value, i);
            });
        }
    }
}