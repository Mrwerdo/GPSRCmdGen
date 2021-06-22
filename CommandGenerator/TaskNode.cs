using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// A tree that expresses the path that was taken through the grammer to construct a sentence.
	/// </summary>
	public class TaskNode
	{
		public TaskNode Parent { get; set; }
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

		/// <summary>
		/// Finds the next node in the parse tree assuming an inorder traversal from right to left.
		/// Typically this property would be called after accquiring a leaf node to find its left peer.
		/// </summary>
		/// <returns>
		/// The next leaf node to the left or null if there is none.
		/// </returns>
		public TaskNode PreviousNodeInParseTree
		{
			get
			{
				var index = Parent?.Children.IndexOf(this) ?? -1;
				if (index > 0)
				{
					return Parent?.Children[index - 1].DeepestDecendent(d => d.LastOrDefault());
				}
				else if (index == -1)
				{
					return null;
				}
				else
				{
					return Parent?.PreviousNodeInParseTree?.DeepestDecendent(d => d.LastOrDefault());
				}
			}
		}

		public TaskNode(string value, bool isNonTerminal) {
			Value = value;
			IsNonTerminal = isNonTerminal;
			Parent = null;
			Children = new List<TaskNode>();

			if (!IsNonTerminal) {
				int cc = 0;
                TextWildcard = TextWildcard.XtractWildcard(Value, ref cc);
				if (TextWildcard != null) TextWildcard.Node = this;
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

		public string MetadataDescription(bool extra = true)
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
