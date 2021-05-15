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
		#region Variables

		/// <summary>
		/// Stores the non terminal symbol
		/// </summary>
		protected string nonTerminal;

		/// <summary>
		/// Stores the list of productions or replacements for the non terminal symbol
		/// </summary>
		protected List<string> replacements;

		public ProductionRuleAttributes attributes;

		/// <summary>
		/// Regular expression for Rule extraction
		/// </summary>
		private static Regex rxRuleParser;

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
		protected ProductionRule (string nonTerminal){
			this.nonTerminal = nonTerminal;
			this.replacements = new List<string> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/> class.
		/// </summary>
		/// <param name="nonTerminal">The non-terminal symbol of the production rule.</param>
		/// <param name="replacements">A set of replacements or productions for the non-terminal symbol.</param>
		public ProductionRule (string nonTerminal, IEnumerable<string> replacements) : this(nonTerminal)
		{
			if (replacements != null)
				this.replacements.AddRange (replacements);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the left side of the production rule (Non-Terminal symbol).
		/// </summary>
		public string NonTerminal{ get { return this.nonTerminal; } }

		/// <summary>
		/// Gets the right side of the production rule (List of productions).
		/// </summary>
		public List<string> Replacements{ get { return this.replacements; } }

		#endregion

		#region Methods

		/// <summary>
		/// Adds all the replacements in a homonime production rule to this instance
		/// </summary>
		/// <param name="pr">The production rule whose replacements will be added</param>
		public void AddReplacements (ProductionRule pr)
		{
			// var productions = new List<string>();
			// foreach (string r in pr.Replacements) {
			// 	ProductionRule.SplitProductions(r, productions);
			// }
			// pr.replacements = productions;
			if (pr.NonTerminal != this.NonTerminal)
				return;
			foreach (string replacement in pr.replacements) {
				if (this.replacements.Contains (replacement))
					continue;
				this.replacements.Add (replacement);
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.ProductionRule"/>.</returns>
		public override string ToString ()
		{
			if (this.replacements.Count == 0)
				return string.Format ("{0} has no rules", this.nonTerminal);
			else if (this.replacements.Count == 1)
				return string.Format ("{0} -> {1}]", this.nonTerminal, this.replacements[0]);
			int i = 0;
			StringBuilder sb = new StringBuilder ();
			sb.Append (this.nonTerminal);
			sb.Append (" -> ");
			while(i < this.replacements.Count-1) {
				sb.Append ('(');
				sb.AppendLine (this.replacements[i++]);
				sb.Append (") | ");
			}
			sb.Append ('(');
			sb.AppendLine (this.replacements[i]);
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
                pr.attributes = JsonSerializer.Deserialize<ProductionRuleAttributes>(attr, options);
				Console.WriteLine("attributes: " + pr.attributes.ToString());
			}
			SplitProductions (prod, pr.replacements);
			ExpandBranchExpression(prod);
			return pr;
		}

		// Given a sentence "a b (c | d | (e | f) g) h i (j | k)" produce an array consisting of these strings:
		//     a b c h i j
		//     a b c h i k
		//     a b d h i j
		//     a b d h i k
		//     a b e g h i j
		//     a b e g h i k
		//     a b f g h i j
		//     a b f g h i k
		//
		// Cases to handle are:
		// 1. The entire sentence is literal.
		// 2. The sentence starts and ends with a literal but contains a branch expression.
		// 3. The sentences contains multiple branching expressions.
		// 4. Nested parenthesis.
		// Undefined behaviour? What happens when parenthesis are unbalanced?
		public static string[] ExpandBranchExpression(string sentence) {
			var parts = new List<(int Start, int End)> ();

			int cc = 0;
			int bcc = 0;
			int parenthesisCount = 0;
			while (cc < sentence.Length) {
				if (sentence[cc] == '(') {
					parenthesisCount += 1;
				} else if (sentence[cc] == ')') {
					if (parenthesisCount > 0) {
						parenthesisCount -= 1;
					} else {
						return null;
					}
				}

				if (sentence[cc] != '(') {
					cc += 1;
					continue;
				}

				if (cc - bcc > 0) {
					parts.Add((bcc, cc-1));
				}

				bcc = cc;
				cc += 1;
				if (!FindClosePar(sentence, ref cc)) {
					return null;
				}
				parenthesisCount -= 1;
				parts.Add((bcc, cc));
				cc += 1;
				bcc = cc;
			}
			if (cc - bcc > 0) {
				parts.Add((bcc, cc-1));
			}

			var branches = new List<string[]>();
			foreach (var (Start, End) in parts) {
				if (sentence[Start] == '(' && sentence[End] == ')' && End - Start > 1) {
					var subsentence = sentence[(Start+1)..End];
					branches.Add(ExpandBranchExpression(subsentence));
				} else {
                    branches.Add(new string[] { sentence[Start..(End + 1)] });
				}
			}
			
			var results = new List<string>();
			ComputeWaysThroughBranches(results, branches);
			return results.ToArray();
		}

		private static string[] SplitKeepOuterSpaces(string s, char c) 
		{
			string[] parts = s.Split(c);
			if (parts.Length > 2) {
				for (int i = 1; i < parts.Length - 1; i += 1) {
					parts[i] = parts[i].Trim();
				}
			}
			if (parts.Length > 1) {
				parts[0] = parts[0].TrimEnd();
				parts[^1] = parts[^1].TrimStart();
			}
			return parts;
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

		/// <summary>
		/// if s = '(' + s1 + ')', returns s1, otherwise returns s
		/// </summary>
		/// <param name="s">String to process</param>
		protected internal static void RemoveTopLevelPar(ref string s){
			if ((s.Length < 1) || (s [0] == '('))
				return;

			int i;
			StringBuilder sb = new StringBuilder (s.Length);
			for (i = 0; i < s.Length -1; ++i) {
				if (s [i] == '\\'){
					++i;
					continue;
				}
				else if ((s [i] == '(') || (s [i] == '|') || (s [i] == ')'))
					return;
				sb.Append(s[i]);
			}
			if (s [i] == ')')
				s = sb.ToString ();
		}

		/// <summary>
		/// Splits a compose production rule (one with parenthesis and OR symbols)
		/// into a list of single production rules
		/// </summary>
		/// <param name="s">The original production rule</param>
		/// <param name="productions">A list to store the derived poduction rules</param>
		public static void SplitProductions (string s, List<string> productions)
		{
			// fixme: split s = ( a | b | (c | d )) into nested productions.
			// I can see why they abonded this algorithm in faviour of the other one.
			// This one does not handle nested parenthesis but the other one does.
			int cc = 0;
			int bcc = 0;
			string prod;

			while (cc < s.Length) {
				if (s [cc] == '\\') {
					cc+= 2;
					continue;
				}
				if (s [cc] == '(') {
					++cc;
					if (!FindClosePar (s, ref cc))
						return;
				}
				if (s [cc] == '|') {
					prod = s.Substring (bcc, cc - bcc).Trim ();
					productions.Add (prod);
					bcc = cc + 1;
				}
				++cc;
			}
			if ((cc - bcc) <= 0) {
				productions.Clear ();
				return;
			}
			prod = s.Substring (bcc, cc - bcc).Trim ();
			RemoveTopLevelPar (ref prod);
			productions.Add (prod);
		}

		/// <summary>
		/// Gets a value indicating if the provided string is an expandable production.
		/// An expandable production is a rule which can be split into two or more
		/// new production tules
		/// </summary>
		/// <param name="replacement">The replacement (right part) string of a production rule</param>
		/// <returns><c>true</c> if the provided string is an expandable production; otherwise, <c>false</c>.</returns>
		internal static bool IsExpandable (string replacement)
		{
			if(String.IsNullOrEmpty(replacement)) return false;
			for(int i = 0; i < replacement.Length; ++i){
				if (replacement [i] == '\\'){
					++i;
					continue;
				}
				else if ( replacement[i] == '(')
					return true;
			}
			return false;
		}

		/// <summary>
		/// Finds the close parenthesis.
		/// </summary>
		/// <returns><c>true</c>, if close parenthesis par was found, <c>false</c> otherwise.</returns>
		/// <param name="s">string to look inside</param>
		/// <param name="cc">Read header.
		/// Must be pointing to the next character of an open parenthesis within the string s</param>
		protected internal static bool FindClosePar(string s, ref int cc){
			int par = 1;
			while ((cc < s.Length) && (par > 0)) {
				if (s [cc] == '\\') {
					cc+= 2;
					continue;
				}
				if (s [cc] == '(') ++par;
				else if (s [cc] == ')') --par;
				++cc;
			}
			--cc;
			return par == 0;
		}

		#endregion

		static private Regex nonTerminalIdentifierMatcher = new Regex(@"(\$[0-9A-Za-z_]+)|({.+})");

		/**
		* Splits a rule , i.e. the right hand side of what this object represents, 
		* into a list of strings, where each string either does not contain $, or is a valid
		* non-terminal identifier.
		*/
		public static string[] SplitRule(string rule) {
			List<Match> matches = ProductionRule.nonTerminalIdentifierMatcher.Matches(rule).Cast<Match>().ToList();
			if (matches.Count == 0) {
				return new string[] { rule };
			}
			var tokens = new List<string>();
			var first = matches.First();
			if (first.Index != 0) {
				tokens.Add(rule.Substring(0, first.Index));
			}
			tokens.Add(rule.Substring(first.Index, first.Length));
			int nextIndex = first.Index + first.Length;
			foreach (Match m in matches.Skip(1)) {
				if (nextIndex != m.Index) {
					tokens.Add(rule.Substring(nextIndex, m.Index - nextIndex));
				}
				tokens.Add(rule.Substring(m.Index, m.Length));
				nextIndex = m.Index + m.Length;
			}
			if (nextIndex != rule.Length) {
				tokens.Add(rule.Substring(nextIndex, rule.Length - nextIndex));
			}
			return tokens.ToArray();
		}
	}
}

