using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// A grammar which can be used to produce random sentences.
	/// </summary>
	public partial class Grammar : ITiered
	{
		#region Variables

		/// <summary>
		/// Stores the set of production rules (accessible by rule name)
		/// </summary>
		private readonly Dictionary<string, List<ProductionRule>> productionRules;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.Grammar"/> class.
        /// </summary>
        public Grammar()
        {
            productionRules = new();
			Tier = DifficultyDegree.Unknown;
		}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the grammar.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the difficulty degree (tier) of the grammar
        /// </summary>
        public DifficultyDegree Tier { get; set; }

		public IEnumerable<ProductionRule> ProductionRules {
			get {
				return productionRules.Values.SelectMany(t => t);
			}
		}

		#endregion

		#region Methods

		public ProductionRule GetRuleWithoutAttributes(string nonTerminalIdentifier) {
			var rules = productionRules[nonTerminalIdentifier];
			var replacements = new List<string>();
			foreach (var r in rules) {
				replacements.AddRange(r.Replacements);
			}
            return new ProductionRule(nonTerminalIdentifier, replacements);
        }

		public bool ContainsRule(string nonTerminalIdentifier) {
			return productionRules.ContainsKey(nonTerminalIdentifier);
		}

		public void AddRule(ProductionRule rule) {
			if (!productionRules.ContainsKey(rule.NonTerminal)) {
				productionRules[rule.NonTerminal] = new List<ProductionRule>();
			}
			productionRules[rule.NonTerminal].Add(rule);
		}

		/// <summary>
		/// Retrieves the non-terminal symbol within the input string pointed by cc
		/// </summary>
		/// <param name="s">S.</param>
		/// <param name="cc">A read header that points to the first character at
		/// the right of.</param>
		/// <returns>The non-terminal symbol found.</returns>
		private static string FetchNonTerminal (string s, ref int cc)
		{
			char c;
			int bcc = cc++;
			while (cc < s.Length) {
				c = s [cc];
				if (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c <= 'z')) || (c == '_') )
					++cc;
				else
					break;
			}
			return s[bcc..cc];
		}

        /// <summary>
        /// Gets a random replacement for the provided non-terminal symbol.
        /// </summary>
        /// <param name="nonTerminal">The non-terminal symbol for which a
        /// random replacement will be searched</param>
        /// <param name="random">Random number generator used to choose a replacement</param>
        /// <returns>A replacement string. If the non-terminal symbol does not
        /// belong to the grammar or contains no productions, an empty string
        /// is returned.</returns>
        private ProductionRule.Replacement FindReplacement(string nonTerminal, Random random)
        {
            if (!productionRules.ContainsKey(nonTerminal)) return null;
            var rule = productionRules[nonTerminal].SelectUniform(random);
			return rule.PickReplacement(random);
		}

        /// <summary>
        /// Generates a random sentence.
        /// </summary>
        /// <param name="rnd">Random number generator used to choose the
        /// productions and generate the sentence</param>
        /// <returns>A randomly generated sentence.</returns>
        public TaskNode GenerateSentence(Random rnd)
        {
			TaskNode root = SolveNonTerminals("$Main", rnd);
			root.Tier = Tier;
			return root;
		}

		/// <summary>
		/// Solves all the non-terminal symbols within the given sentence.
		/// </summary>
		/// <param name="sentence">A string with non-terminal symbols to replace.</param>
		/// <param name="rnd">Random number generator used to choose the
		/// productions and generate the sentence</param>
		/// <param name="stackCounter">A counter that indicates how many times
		/// this function has called itself. It is used to prevent a stack overflow.
		/// When it reach 1000 the production is aborted.</param>
		/// <returns>A string with terminal symbols only</returns>
		/// <remarks>Recursive function</remarks>
		private TaskNode SolveNonTerminals (string nonTerminal, Random rnd, int stackCounter = 0)
		{
			if (++stackCounter > 999)
				throw new StackOverflowException ();

            TaskNode node = new(nonTerminal, isNonTerminal: true);
			node.Replacement = FindReplacement(nonTerminal, rnd);
			if (node.Replacement == null) return node;
            string[] parts = Scanner.SplitRule(node.Replacement?.Value ?? "");
			foreach (string part in parts) {
				if (part.Contains("$")) {
					TaskNode child = SolveNonTerminals(part, rnd, stackCounter + 1);
					child.Parent.SetTarget(node);
					node.Children.Add(child);
				} else {
					TaskNode child = new(part, isNonTerminal: false);
					child.Parent.SetTarget(node);
					node.Children.Add(child);
				}
			}
			return node;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Grammar"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Grammar"/>.</returns>
		public override string ToString ()
		{
			return string.Format ("[Grammar: Name={0}, Tier={1}]", Name, Tier);
		}

		#endregion

		#region Static variables

		/// <summary>
		/// Regular expression used to extract the name of a grammar
		/// </summary>
		private static readonly Regex rxGrammarNameXtractor;

		/// <summary>
		/// Regular expression used to extract the difficulty degree of a grammar
		/// </summary>
		private static readonly Regex rxGrammarTierXtractor;

		/// <summary>
		/// Regular expression used to extract import directives
		/// </summary>
		private static readonly Regex rxGrammarImportXtractor;

		#endregion

		#region Static constructor

		/// <summary>
		/// Initializes the <see cref="RoboCup.AtHome.CommandGenerator.Grammar"/> class.
		/// </summary>
		static Grammar(){
			rxGrammarNameXtractor = new Regex(@"^\s*grammar\s+name\s+(?<name>.*)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			rxGrammarTierXtractor = new Regex(@"^\s*grammar\s+tier\s+(?<tier>\w+)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			rxGrammarImportXtractor = new Regex(@"^\s*(?<directive>(load)|(import))\s+(?<path>(\S+|(""([^""]|(\\""))+"")))(\s+as\s+(?<nt>\$[A-Za-z_][0-9A-Za-z_]+))?\s*", RegexOptions.Compiled);
		}

		#endregion
	}
}
	