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
		private Nullable<DifficultyDegree> _tier { get; set; }
        public Nullable<DifficultyDegree> Tier {
			get {
				if (_tier.HasValue) {
					return _tier.Value;
				} else {
					TaskNode p = null;
					if (Parent.TryGetTarget(out p)) {
						return p.Tier;
					} else {
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
		/// If null then this is a terminal node.
		/// </summary>
		public List<TaskNode> Children { get; private set; }
		public string StringValue { get; set; }
		public string Term { get; set; }

		public TaskNode(TaskNode parent, Nullable<DifficultyDegree> tier = null) {
			_tier = tier;
			Parent = new WeakReference<TaskNode>(parent);
			Children = new List<TaskNode>();
			StringValue = null;
		}

		public override string ToString() {
			string output = "";
			if (StringValue != null) {
				output += StringValue;
			} else if (Term != null) {
				output += Term;
			}
			foreach (TaskNode child in Children) {
				output += "(" + child.ToString() + ")";
			}
			return output;
		}
		
		public string Render() {
			string output = "";
			if (StringValue != null) {
				output += StringValue;
			} else if (Term != null) {
                output += String.Join(" ", Children.ConvertAll(t => t.Render()));
			}
			return output;
		}

		public string PrettyTree(int indent = 0) {
			string output = "";
			string idt = new string('.', indent * 2);
			if (StringValue != null) {
				output += idt + "-> " + StringValue;
			} else if (Term != null) {
				output += idt + "-> " + Term;
				if (Children.Count > 0) {
					output += "\n";
				}
				output += String.Join("\n", Children.ConvertAll(t => t.PrettyTree(indent + 1)));
			}
			return output;
		}

    }
}
