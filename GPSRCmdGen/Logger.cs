using System;

namespace RoboCup.AtHome.GPSRCmdGen
{
	public class Logger
	{
        public bool Verbose { get; set; }
        public bool EnableColors { get; set; }

        public Logger(bool verbose = true) {
            Verbose = verbose;
            EnableColors = true;
        }

        private void SetColor(ConsoleColor color) {
            if (EnableColors) {
                Console.ForegroundColor = color;
            }
        }

        public void Success(string message)
        {
            ConsoleColor pc = Console.ForegroundColor;
            SetColor(ConsoleColor.DarkGreen);
            Console.WriteLine(message);
            SetColor(pc);
        }

        public void Warning(string message)
        {
            ConsoleColor pc = Console.ForegroundColor;
            SetColor(ConsoleColor.DarkYellow);
            Console.Error.WriteLine(message);
            SetColor(pc);
        }

		public void Info(string msg, char endline = '\n') 
		{
            if (Verbose)
            {
                Console.Write(msg + endline);
			}
		}

        public void Error(string msg, Exception error = null)
        {
            var current = Console.ForegroundColor;
            SetColor(ConsoleColor.DarkRed);
            Console.Error.Write("error: ");
            SetColor(current);
            Console.Error.WriteLine(msg);
            if (Verbose && error != null)
                Console.Error.WriteLine(error.Message);
        }

        public void ProgramInfo(string name, string longName) {
            SetColor(ConsoleColor.Gray);
            Info("");
            Info(name);
            Info(longName);
            Info("");
        }
    }
}