using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Code for loading grammar files in a (thread) safe way
	/// </summary>
	public class GrammarLoader
	{
		private static readonly Regex grammarNameExtractor = new(@"^\s*grammar\s+name\s+(?<name>.*)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static readonly Regex grammarTierExtractor = new(@"^\s*grammar\s+tier\s+(?<tier>\w+)\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static readonly Regex grammarImportExtractor = new(@"^\s*(?<directive>(load)|(import))\s+(?<path>(\S+|(""([^""]|(\\""))+"")))(\s+as\s+(?<nt>\$[A-Za-z_][0-9A-Za-z_]+))?\s*", RegexOptions.Compiled);

		/// <summary>
		/// Path of the grammar file being loaded
		/// </summary>
		private string grammarFilepath;

		/// <summary>
		/// The grammar being loaded.
		/// </summary>
		private Grammar grammar;

		/// <summary>
		/// The list of text lines in the grammar text file
		/// </summary>
		private List<string> lines;

		/// <summary>
		/// Loads a grammar from a text file.
		/// </summary>
		/// <param name="filePath">The grammar file path.</param>
		/// <param name="requireMainNT">Specifies whether a Main rule is required to load the grammar.</param>
		/// <returns>The grammar represented within the provided file, or null
		/// if the grammar could not be loadder.</returns>
		public Grammar Load(string filePath, bool requireMainNT = true)
		{
			return LoadText(File.ReadAllLines(filePath), filePath, requireMainNT);
		}

		public Grammar LoadText(string[] lines, string filePath, bool requireMainNT = true)
		{
			grammarFilepath = filePath;
			grammar = new Grammar();
			this.lines = new List<string>(lines);
			StripCommentsAndEmptyLines();
			ParseProductionRules();
			if (requireMainNT && !grammar.ContainsRule("$Main"))
				return null;
			return grammar;
		}

		/// <summary>
		/// Loads another grammar file within the existing one
		/// </summary>
		/// <param name="directive">Specifies the import method used</param>
		/// <param name="nonTerminal">The non-terminal in which the grammar file will be imported</param>
		/// <param name="path">Path to the grammar file to be imported</param>
		/// be dumped</param>
		private void ImportGrammar(string directive, string path, string nonTerminal = null)
		{
			if (directive == "import")
				ImportSubGrammar(directive, path, nonTerminal);
			else if ((directive != "load") || !File.Exists(path))
				return;

			Grammar subGrammar = new GrammarLoader().Load(path, false);
			if (subGrammar == null)
				return;
			foreach (var rule in subGrammar.ProductionRules)
			{
				grammar.AddRule(rule);
			}

		}

		/// <summary>
		/// Imports production tules from another grammar file
		/// </summary>
		/// <param name="directive">Specifies the import method used</param>
		/// <param name="nonTerminal">The non-terminal in which the grammar file will be imported</param>
		/// <param name="path">Path to the grammar file to be imported</param>
		/// be dumped</param>
		private void ImportSubGrammar(string directive, string path, string nonTerminal = null)
		{
			if (directive != "import")
				return;
			if (!string.IsNullOrEmpty(nonTerminal))
			{
				ImportSubGrammarIntoNT(directive, path, nonTerminal);
				return;
			}
			if (!File.Exists(path))
				return;

			Grammar subGrammar = new GrammarLoader().Load(path, false);
			if (subGrammar == null)
				return;
			foreach (var rule in subGrammar.ProductionRules)
			{
				if (rule.NonTerminal == "$Main") continue;
				grammar.AddRule(rule);
			}
		}

		/// <summary>
		/// Imports production tules from another grammar file into a non-terminal
		/// </summary>
		/// <param name="directive">Specifies the import method used</param>
		/// <param name="nonTerminal">The non-terminal in which the grammar file will be imported</param>
		/// <param name="path">Path to the grammar file to be imported</param>
		/// be dumped</param>
		private void ImportSubGrammarIntoNT(string directive, string path, string nonTerminal)
		{
			if (string.IsNullOrEmpty(nonTerminal))
			{
				ImportSubGrammar(directive, path, nonTerminal);
				return;
			}
			if (directive != "import")
				return;
			string errMsg = "#ERROR! {void meta:{0}}";

			ProductionRule pr;
			if (!File.Exists(path))
			{
				errMsg = string.Format(errMsg, $"File {path} not found");
				pr = new ProductionRule(nonTerminal, new string[] { errMsg });
				grammar.AddRule(pr);
				return;
			}

			Grammar subGrammar = new GrammarLoader().Load(path, true);
			if (subGrammar == null)
			{
				errMsg = string.Format(errMsg, $"Cannot load grammar file {path}");
				pr = new ProductionRule(nonTerminal, new string[] { errMsg });
				grammar.AddRule(pr);
				return;
			}

			errMsg = string.Format(errMsg, "Not implemented. Sorry =(");
			pr = new ProductionRule(nonTerminal, new string[] { errMsg });
			grammar.AddRule(pr);
		}

		/// <summary>
		/// Removes a multi-line comment form a list of text lines 
		/// </summary>
		/// <param name="i">The index of the line in the list where the multi-line comment was found.</param>
		/// <param name="j">The index within the line of the first character to the
		/// right of the multi-line comment start symbol.</param>
		private void ParseMultiLineComment(int i, int j)
		{
			lines[i] = lines[i].Substring(0, j - 1);
			if (lines[i].Length < 1)
				lines.RemoveAt(i);

			while (i < lines.Count)
			{
				j = lines[i].IndexOf("*/");
				if (j != -1)
					break;
				lines.RemoveAt(i);
			}
			lines[i] = lines[i][(j + 2)..];
		}

		/// <summary>
		/// Parses the single line comment looking for the name and tier
		/// (difficulty degree) of the grammar
		/// </summary>
		/// <param name="grammar">The grammar file where the found data will be stored</param>
		/// <param name="line">The string that contains the comment</param>
		/// <param name="j">The position in the line where the comment starts</param>
		private void ParseSingleLineComment(string line, int j)
		{
			if (++j >= line.Length)
				return;

			Match m;
			line = line[j..];
			if (string.IsNullOrEmpty(grammar.Name))
			{
				m = grammarNameExtractor.Match(line);
				if (m.Success)
					grammar.Name = m.Result("${name}");
			}

			if ((grammar.Tier == DifficultyDegree.Unknown) && (m = grammarTierExtractor.Match(line)).Success)
			{
				try
				{
					grammar.Tier = (DifficultyDegree)Enum.Parse(typeof(DifficultyDegree), m.Result("${tier}"));
				}
				catch
				{
					return;
				}
			}

			m = grammarImportExtractor.Match(line);
			if (m.Success && grammarFilepath != null)
			{
				string path = Path.Combine(Path.GetDirectoryName(grammarFilepath), m.Result("${path}"));
				if (path != grammarFilepath)
					ImportGrammar(m.Result("${directive}"), path, m.Result("${nt}"));
			}
		}

		/// <summary>
		/// Parses a set of strings containing each a production rule, converting them into
		/// a non-terminal symbol addressable set of expanded production rules.
		/// </summary>
		private void ParseProductionRules()
		{
			ProductionRule pr;
			foreach (string line in lines)
			{
				pr = ProductionRule.FromString(line);
				if ((pr == null) || (pr.Replacements.Count < 1))
					continue;
				grammar.AddRule(pr);
			}
		}

		/// <summary>
		/// Strips comments from the content of a grammar text file
		/// </summary>
		/// <param name="i">The index of the current line being analized</param>
		private void StripComments(int i)
		{
			for (int j = 0; j < lines[i].Length; ++j)
			{
				// Double character commenting
				if ((lines[i][j] == '/') && ((j + 1) < lines[i].Length))
				{
					++j;
					if (lines[i][j] == '/')
					{
						ParseSingleLineComment(lines[i], j);
						lines[i] = lines[i].Substring(0, j - 1);
						break;
					}
					else if (lines[i][j] == '*')
					{
						ParseMultiLineComment(i, j);
						break;
					}
				}
				// Single character commenting
				else if ((lines[i][j] == '#') || (lines[i][j] == ';'))
				{
					ParseSingleLineComment(lines[i], j);
					lines[i] = lines[i].Substring(0, j);
					break;
				}
			}
		}

		/// <summary>
		/// Strips comments and empty lines from the content of a grammar text file
		/// </summary>
		private void StripCommentsAndEmptyLines()
		{
			for (int i = 0; i < lines.Count; ++i)
			{
				lines[i] = lines[i].Trim();
				StripComments(i);
				if (lines[i].Length < 1)
					lines.RemoveAt(i--);
			}
		}
	}
}

