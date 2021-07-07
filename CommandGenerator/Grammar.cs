using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// A grammar which can be used to produce random sentences.
	/// </summary>
	public class Grammar : ITiered
	{
		public string? Name { get; set; }
		public DifficultyDegree Tier { get; set; }
		private readonly Dictionary<string, List<ProductionRule>> productionRules;

		public IEnumerable<ProductionRule> ProductionRules => productionRules.Values.SelectMany(t => t);

		public Grammar()
		{
			productionRules = new();
			Tier = DifficultyDegree.Unknown;
		}

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
		/// Gets a random replacement for the provided non-terminal symbol.
		/// </summary>
		/// <param name="nonTerminal">The non-terminal symbol for which a random replacement will be searched</param>
		/// <param name="random">Random number generator used to choose a replacement</param>
		/// <param name="path">
		/// 	A sequence of tuples where the second element is the index for a specific version 
		/// 	of the non-terminal identified by the first element that should be substituted for 
		/// 	any occurances of the first element in the expanded string for the node represented 
		/// 	by the nonTerminal argument.
		/// </param>
		/// <returns>A replacement object.</returns>
		private ProductionRule.Replacement? FindReplacement(
			string nonTerminal, 
			Random random, 
			List<(string nonTerminal, int? index)>? path, 
			int depth)
		{
			if (!productionRules.ContainsKey(nonTerminal)) return null;
			if (path is not null)
			{
				if (path.Count > depth)
				{
					var element = path[depth];
					if ("$" + element.nonTerminal == nonTerminal && element.index != null) {
						return productionRules[nonTerminal][element.index ?? 0].PickReplacement(random);
					}
				}
			}
			var rule = productionRules[nonTerminal].SelectUniform(random);
			return rule.PickReplacement(random);
		}

		/// <summary>
		/// Solves all the non-terminal symbols within the given sentence.
		/// </summary>
		/// <param name="nonTerminal">The non-terminal symbol for which a random replacement will be searched</param>
		/// <param name="random">Random number generator used to choose a replacement</param>
		/// <param name="path">
		/// 	A sequence of tuples where the second element is the index for a specific version 
		/// 	of the non-terminal identified by the first element that should be substituted for 
		/// 	any occurances of the first element in the expanded string for the node represented 
		/// 	by the nonTerminal argument.
		/// </param>
		/// <param name="depth">
		/// 	The number of times the function has been called recursively. 
		/// 	You shouldn't need to provide any value other than the default of 0.</param>
		/// <returns>A TaskNode that represents a random parse tree through this grammar.</returns>
		/// <remarks>This function is recursive. If the grammar has recursive symbols, it may never terminate.</remarks>
		public TaskNode GenerateSentence(string nonTerminal, Random random, List<(string nonTerminal, int? index)>? path = null, int depth = 0)
		{
			TaskNode node = new(nonTerminal, isNonTerminal: true);
			node.Replacement = FindReplacement(nonTerminal, random, path, depth);
			if (node.Replacement == null) return node;
			string[] parts = Scanner.SplitRule(node.Replacement?.Value ?? "");
			foreach (string part in parts) {
				if (part.Contains("$")) {
					TaskNode child = GenerateSentence(part, random, path, depth + 1);
					child.Parent = node;
					node.Children.Add(child);
				} else {
					TaskNode child = new(part, isNonTerminal: false);
					child.Parent = node;
					node.Children.Add(child);
				}
			}
			return node;
		}

		public IEnumerable<TaskNode> EverythingEnumerator(string nonTerminal)
		{
			var rules = productionRules[nonTerminal];
			foreach (var rule in rules) {
				for (int index = 0; index < rule.Replacements.Count; index++) {
					var replacement = new ProductionRule.Replacement() {
						Rule = rule,
						NonTerminal = rule.NonTerminal,
						Value = rule.Replacements[index],
						Index = index
					};
					string[] parts = Scanner.SplitRule(replacement.Value ?? "");
					List<IEnumerable<TaskNode>> iterators = parts.Select(p => {
						if (p.StartsWith("$")) {
							return EverythingEnumerator(p);
						} else {
							return new List<TaskNode>() { new TaskNode(p, isNonTerminal: false) };
						}
					}).ToList();
					var ways = Combinatorics.EnumerateReplacementsOfOrderedList(iterators);
					foreach (TaskNode[] way in ways)
					{
						yield return new(nonTerminal, isNonTerminal: true, children: way.ToList()) {
							Replacement = replacement
						};
					}
				}
			}
		}

		public override string ToString()
		{
			return string.Format ("[Grammar: Name={0}, Tier={1}]", Name, Tier);
		}
	}
}