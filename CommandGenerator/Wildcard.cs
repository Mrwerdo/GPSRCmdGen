using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Represents an unified Wildcard that encapsulates all TextWildcards with a common keycode
	/// </summary>
	public class Wildcard
	{
		/// <summary>
		/// Stores the keyword associated to this wildcard
		/// </summary>
		private string keyword;

		/// <summary>
		/// Gets or sets the keyword associated to this wildcard
		/// </summary>
		public string Keyword
		{
			get{ return string.IsNullOrEmpty (keyword) ? Name : keyword; }
			set{ keyword = string.IsNullOrEmpty (value) ? null : value.ToLower (); }
		}

        /// <summary>
        /// Stores the obfuscated replacement value for the unified wildcard
        /// </summary>
        public INameable Obfuscated { get; set; }

        /// <summary>
        /// Gets or sets the replacement for all the unified wildcards
        /// </summary>
        public INameable Replacement { get; set; }

		/// <summary>
		/// Stores the list text wildcards this Wildcard unifies
		/// </summary>
		private readonly List<TextWildcard> textWildcards;

		public Wildcard(List<TextWildcard> wildcards) {
			textWildcards = wildcards;
			foreach (var child in wildcards) {
				child.Parent.SetTarget(this);
			}
		}

		/// <summary>
		/// Gets the name of the wildcard
		/// </summary>
		public string Name
		{
            get { return textWildcards.First().Name; }
		}

        /// <summary>
        /// Gets the dominant type of the TextWildcards in the collection
        /// </summary>
        public string Type
        {
            get
            {
				if (textWildcards.Count == 0) {
					return null;
				}
				var groups = textWildcards.GroupBy(t => t.Type ?? "");
				var max = groups.Aggregate((l, r) => l.Count() > r.Count() ? l : r);
                if (max.Count() == textWildcards.Count || string.IsNullOrWhiteSpace(max.First().Type)) {
					return null;
				}
                return max.First().Type;
            }
        }

        /// <summary>
        /// Gets the union (AND) of all where clauses in the collection
        /// </summary>
        public string Where
        {
            get
            {
                Queue<string> clauses = new(textWildcards.Count);
                foreach (TextWildcard t in textWildcards)
                {
                    if (!string.IsNullOrEmpty(t.Where))
                        clauses.Enqueue(t.Where);
                }
                if (clauses.Count < 1)
                    return "";
                return string.Join("AND ", clauses);
            }
        }

        public override string ToString()
        {
			var keycode = textWildcards.FirstOrDefault()?.Keycode ?? "";
            return $"{keycode} ({textWildcards.Count})";
        }
    }
}
