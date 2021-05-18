using System;
using System.Collections.Generic;
using System.Text;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Represents a task randomly generated from a grammar.
	/// The task is composed by list of tokens (original strings or
	/// wildcards with their replacements and metadata).
	/// </summary>
	public class Task
	{
		/// <summary>
		/// Stores the list of grammar's tokens
		/// </summary>
		public TaskNode Tree { get; set; }

        /// <summary>
        /// Gets the list of tokens that compose the task.
        /// </summary>
        public List<Token> Tokens { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.Task"/> class.
        /// </summary>
        public Task()
        {
			Tokens = new List<Token>();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Task"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Task"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new();
			for (int i = 0; i < Tokens.Count; ++i)
				sb.Append (Tokens [i].Name);
			string s = sb.ToString ();
			while(s.Contains("  "))
				s = s.Replace ("  ", " ");
			s = s.Replace (" ,", ",");
			s = s.Replace (" ;", ";");
			s = s.Replace (" .", ".");
			s = s.Replace (" :", ":");
			s = s.Replace (" ?", "?");
			return s;
		}

		/// <summary>
		/// Prints a task including metadata into the output stream.
		/// </summary>
		/// <param name="task">The task to be print</param>
		public void Print()
		{
			string sTask = ToString().Trim();
			if (sTask.Length < 1)
				return;

			// switch Console color to white, backuping the previous one
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();
			// Prints a === line
			string pad = String.Empty.PadRight(Console.BufferWidth - 1, '=');
			Console.WriteLine(pad);
			Console.WriteLine();

			// Prints task string and metadata
			sTask = sTask[0..1].ToUpper() + sTask[1..];
			do
			{
				int cut = sTask.Length;
				if (cut >= Console.BufferWidth)
					cut = sTask.LastIndexOf(' ', Console.BufferWidth-1);
				Console.WriteLine(sTask.Substring(0, cut));
				sTask = sTask[cut..].Trim();
			} while (!String.IsNullOrEmpty(sTask));
            PrintTaskMetadata();
			Console.WriteLine();
			// Prints another line
			Console.WriteLine(pad);
			// Restores Console color
			Console.ForegroundColor = pc;
			Console.WriteLine();
		}

		/// <summary>
		/// Prints the task metadata.
		/// </summary>
		/// <param name="task">The task object containing metadata to print.</param>
		private void PrintTaskMetadata()
		{
			Console.WriteLine();
			List<string> remarks = new();
            foreach (Token token in Tokens) {
                PrintMetadata(token, remarks);
			}
			if (remarks.Count > 0)
			{
				Console.WriteLine("remarks");
				foreach (string r in remarks)
					Console.WriteLine("\t{0}", r);
			}
			Console.WriteLine("parse tree:");
			Console.WriteLine(Tree.PrettyTree());
			Console.WriteLine(Tree.RenderCommand());
		}

		/// <summary>
		/// Prints the metadata of the given Token
		/// </summary>
		/// <param name="token">The token onject containing the metadata to print</param>
		/// <param name="remarks">A list to store all metadata whose token has no name</param>
		private static void PrintMetadata(Token token, List<string> remarks)
		{
			if (token.Metadata.Count < 1) return;
			// Store remarks for later
			if (String.IsNullOrEmpty(token.Name))
			{
				remarks.AddRange(token.Metadata);
				return;
			}
			// Print current token metadata
			Console.WriteLine("{0}", token.Name);
			foreach (string md in token.Metadata)
				Console.WriteLine("\t{0}", md);
		}
	}
}

