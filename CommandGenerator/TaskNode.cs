using System;
using System.Collections.Generic;

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

		public override string ToString() {
			return IsNonTerminal ? "$" + Value : Value;
		}
		
		public string Render() {
			string s = RenderPrivate();
			while(s.Contains("  "))
				s = s.Replace ("  ", " ");
			s = s.Replace (" ,", ",");
			s = s.Replace (" ;", ";");
			s = s.Replace (" .", ".");
			s = s.Replace (" :", ":");
			s = s.Replace (" ?", "?");
			return s;
		}

		private string RenderPrivate() {
			string output = "";
			if (IsNonTerminal) {
                output += string.Join(" ", Children.ConvertAll(t => t.RenderPrivate()));
			} else if (TextWildcard != null) {
				output += TextWildcard.ReplacementValue;
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

		public void Print() 
		{
			string sTask = Render();
			if (sTask.Length < 1)
				return;

			// switch Console color to white, backuping the previous one
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();
			// Prints a === line
			string pad = string.Empty.PadRight(Console.BufferWidth - 1, '=');
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
			} while (!string.IsNullOrEmpty(sTask));
			Console.WriteLine();
            Console.WriteLine(PrintTaskMetadata());
			Console.WriteLine();
			// Prints another line
			Console.WriteLine(pad);
			// Restores Console color
			Console.ForegroundColor = pc;
			Console.WriteLine();
		}

		public string PrintTaskMetadata(bool extra = true)
		{
			var output = "";
			EnumerateTree(t => {
                output += t.TextWildcard?.Comment ?? "";
			});
			if (extra) {
				output += "\nParse tree:\n";
				output += PrettyTree() + "\n\n";
				output += "Command:\n";
				output += this.RenderCommand();
			}
			return output + "\n";
		}

		public void EnumerateTree(Action<TaskNode> callback) {
			callback(this);
			foreach (var child in Children) {
				child.EnumerateTree(callback);
			}
		}
    }
}
