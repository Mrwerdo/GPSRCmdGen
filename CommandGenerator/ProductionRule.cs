using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Represents a production rule of a grammar
	/// </summary>
	public class ProductionRule
	{
		/// <summary>
		/// Regular expression for Rule extraction
		/// </summary>
		private static readonly Regex rxRuleParser;

        #region Properties

        /// <summary>
        /// Gets the left side of the production rule (Non-Terminal symbol).
        /// </summary>
        public string NonTerminal { get; private set; }

        /// <summary>
        /// Gets the right side of the production rule (List of productions).
        /// </summary>
        public List<string> Replacements { get; private set; }


        public ProductionRuleAttributes Attributes { get; set; }
        #endregion

		#region Constructors

		/// <summary>
		/// Initializes the <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/> class.
		/// </summary>
		static ProductionRule()
		{
			var s = @"\s*(?<name>\$[0-9A-Za-z_]+)\s*=\s*(?<prod>[^<=>]+)(\<\=\>\s*(?<attrib>.+))?";
			rxRuleParser = new Regex(s, RegexOptions.Compiled);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/> class.
		/// </summary>
		/// <param name="nonTerminal">The non-terminal symbol of the production rule.</param>
		public ProductionRule (string nonTerminal){
			NonTerminal = nonTerminal;
			Replacements = new List<string> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/> class.
		/// </summary>
		/// <param name="nonTerminal">The non-terminal symbol of the production rule.</param>
		/// <param name="replacements">A set of replacements or productions for the non-terminal symbol.</param>
		public ProductionRule (string nonTerminal, IEnumerable<string> replacements) : this(nonTerminal)
		{
			if (replacements != null)
                Replacements.AddRange(replacements);
		}

		#endregion

		#region Methods

		public class Replacement {
			public ProductionRule Rule { get; set; }
			public string NonTerminal { get; set; }
            public string Value { get; set; }
            public int Index { get; set; }
        }

		public Replacement PickReplacement(Random random) {
			int index = random.Next(Replacements.Count);
			return new Replacement() 
			{
				Rule = this,
				NonTerminal = NonTerminal,
				Value = Replacements[index],
				Index = index
			};
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/>.</returns>
		public override string ToString ()
		{
			if (Replacements.Count == 0)
				return string.Format ("{0} has no rules", NonTerminal);
			else if (Replacements.Count == 1)
				return string.Format ("{0} -> {1}]", NonTerminal, Replacements[0]);
			int i = 0;
			StringBuilder sb = new();
			sb.Append (NonTerminal);
			sb.Append (" -> ");
			while(i < Replacements.Count-1) {
				sb.Append ('(');
				sb.AppendLine (Replacements[i++]);
				sb.Append (") | ");
			}
			sb.Append ('(');
			sb.AppendLine (Replacements[i]);
			sb.Append (')');
			return sb.ToString ();
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Creates a ProductionRule object from a string
		/// </summary>
		/// <returns>A ProductionRule object.</returns>
		/// <param name="s">the string to analyze</param>
		public static ProductionRule FromString(string s){
			Match m = rxRuleParser.Match (s);
			if (!m.Success)
				return null;
			string name = m.Result ("${name}");
			string prod = m.Result ("${prod}");
			string attr = m.Result("${attrib}");
			ProductionRule pr = new(name);
			if (attr != null && !attr.Equals("")) {
                var options = new JsonSerializerOptions() { IgnoreNullValues = true };
                pr.Attributes = JsonSerializer.Deserialize<ProductionRuleAttributes>(attr, options);
			}
			var replacements = ExpandBranchExpression(prod);
			if (replacements == null) {
				pr.Replacements = new ();
				pr.Replacements.Add(prod);
			} else {
                pr.Replacements = new List<string>(replacements);
			}
			return pr;
		}

        /// <summary>
        /// Splits a compose production rule (one with parenthesis and OR symbols)
        /// into a list of single production rules
		/// <example>
        /// Given a sentence "a b (c | d | (e | f) g) h i (j | k)" produce an array consisting of these strings:
        ///     a b c h i j
        ///     a b c h i k
        ///     a b d h i j
        ///     a b d h i k
        ///     a b e g h i j
        ///     a b e g h i k
        ///     a b f g h i j
        ///     a b f g h i k
		/// </example>
        /// </summary>
        /// <param name="s">The original production rule</param>
        /// <param name="productions">A list to store the derived poduction rules</param>
        /// <returns>Null if the input sentence contains unbalanced parenthesis.</returns>
        public static string[] ExpandBranchExpression(string sentence)
        {
            if (!Scanner.IsValidParenthesisExpression(sentence, '(', ')'))
            {
                return null;
            }
            return ExpandBranchExpresionGuarenteeValidResult(sentence);
        }

        // Guarentees non-null result, even though various methods might return null given
        // an input string with mismatched parenthesis, we make the assumption that sentence
        // has a valid parenthesis expression.
        private static string[] ExpandBranchExpresionGuarenteeValidResult(string sentence)
        {
            var results = new List<string>();
            var topLevelBranches = Scanner.SplitRespectingParenthesis(sentence, '(', ')', '|');
            foreach (var branch in topLevelBranches)
            {
                var parts = ExpandNestedBranch(branch).Select((t, i) => t.Trim());
                results.AddRange(parts);
            }
            return results.ToArray();
        }

        private static string[] ExpandNestedBranch(string sentence)
        {
            var parts = Scanner.FindParenthesisRanges(sentence, '(', ')');

            var branches = new List<string[]>();
            foreach (var (Start, End) in parts)
            {
                if (sentence[Start] == '(' && sentence[End] == ')' && End - Start > 1)
                {
                    var subsentence = sentence[(Start + 1)..End];
                    branches.Add(ExpandBranchExpression(subsentence));
                }
                else
                {
                    var subsentence = sentence[Start..(End + 1)];
                    if (subsentence.Contains('|'))
                    {
                        // Logical error.
                        throw new Exception($"{subsentence} contains a branch");
                    }
                    branches.Add(new string[] { subsentence });
                }
            }

            var results = new List<string>();
            ComputeWaysThroughBranches(results, branches);
            return results.ToArray();
        }

        private static void ComputeWaysThroughBranches(
            List<string> results,
            List<string[]> branches,
            string partialSentence = "",
            int depth = 0)
        {
            if (depth == branches.Count)
            {
                results.Add(partialSentence);
            }
            else
            {
                foreach (var child in branches[depth])
                {
                    ComputeWaysThroughBranches(results, branches, partialSentence + child, depth + 1);
                }
            }
        }

		#endregion
	}
}

