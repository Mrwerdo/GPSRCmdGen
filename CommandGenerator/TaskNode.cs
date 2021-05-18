using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// A tree that expresses the path that was taken through the grammer to construct a sentence.
	/// </summary>
	public class TaskNode
	{
		private DifficultyDegree? _tier;
        public DifficultyDegree? Tier {
			get {
				if (_tier.HasValue) {
					return _tier.Value;
				} else {
                    if (Parent.TryGetTarget(out TaskNode p))
                    {
                        return p.Tier;
                    }
                    else
                    {
                        throw new ArgumentNullException("This node's Tier property is not set, nor are any of its parents Tier property set.");
                    }
                }
			}
			set {
				_tier = value;
			}
		}
		public WeakReference<TaskNode> Parent { get; private set; }
		/// <summary>
		/// If empty then this is a terminal node.
		/// </summary>
		public List<TaskNode> Children { get; private set; }

		public string Value { get; set; }
		public bool IsNonTerminal { get; set; }
		public ProductionRuleAttributes Attributes 
		{ 
			get {
				if (Rule == null) { return null; }
				return Rule.Attributes;
			}
		}
        public ProductionRule.Replacement Replacement { get; set; }
		public ProductionRule Rule 
		{
			get {
				if (Replacement == null) return null;
				return Replacement.Rule;
			}
		}
        public TextWildcard TextWildcard { get; set; }

		public TaskNode(string value, bool isNonTerminal) {
			Value = value;
			IsNonTerminal = isNonTerminal;
			Parent = new WeakReference<TaskNode>(null);
			Children = new List<TaskNode>();

			if (!IsNonTerminal) {
				int cc = 0;
                TextWildcard = TextWildcard.XtractWildcard(Value, ref cc);
			}
		}

        public bool IsLiteral
        {
            get
            {
                return !IsNonTerminal;
            }
			set 
			{
				IsNonTerminal = !value;
			}
        }

		public override string ToString() {
			return IsNonTerminal ? "$" + Value : Value;
		}
		
		public string Render() {
			string output = "";
			if (IsNonTerminal) {
                output += string.Join(" ", Children.ConvertAll(t => t.Render()));
			} else {
				output += Value;
			}
			return output;
		}

		public string PrettyTree(int indent = 0) {
			string output = "";
			string idt = new('.', indent * 2);
			if (IsNonTerminal) {
				output += idt + "-> " + Value;
				if (Replacement != null && Replacement.Rule.Attributes != null) {
					output += Replacement.Rule.Attributes.ToString().Indent(indent);
				}
				if (Children.Count > 0) {
					output += "\n";
				}
				output += string.Join("\n", Children.ConvertAll(t => t.PrettyTree(indent + 1)));
			} else {
				output += idt + "-> " + Value;
			}
			return output;
		}

		public List<Token> AsTokens() {
			var tokens = new List<Token>();
			if (IsNonTerminal && TextWildcard != null) {
				var obfuscated = TextWildcard.Obfuscated ? "?" : "";
				var nte = new NamedTaskElement($"{{{TextWildcard.Keycode}{obfuscated}}}");
				var token = new Token(Value, nte, null);
				tokens.Add(token);
			} else if (!IsNonTerminal) {
				var token = new Token(Value, null, null);
				tokens.Add(token);
			}
			foreach (var child in Children) {
				tokens.AddRange(child.AsTokens());
            }
			return tokens;
		}

		public List<TextWildcard> Wildcards {
			get {
				var l = new List<TextWildcard>();
				if (TextWildcard != null) {
					l.AddRange(TextWildcard.ToList());
				}
				foreach (var child in Children) {
					l.AddRange(child.Wildcards);
				}
				return l;
			}
		}
    }
}
