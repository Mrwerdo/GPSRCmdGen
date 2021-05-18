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
        /// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.Task"/> class.
        /// </summary>
        public Task() { }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Task"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Task"/>.</returns>
		public override string ToString()
		{
			string s = Tree.Render();
			while(s.Contains("  "))
				s = s.Replace ("  ", " ");
			s = s.Replace (" ,", ",");
			s = s.Replace (" ;", ";");
			s = s.Replace (" .", ".");
			s = s.Replace (" :", ":");
			s = s.Replace (" ?", "?");
			return s;
		}
	}
}

