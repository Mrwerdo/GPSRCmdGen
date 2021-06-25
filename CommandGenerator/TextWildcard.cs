using System;
using System.Collections.Generic;
using RoboCup.AtHome.CommandGenerator.ReplaceableTypes;

namespace RoboCup.AtHome.CommandGenerator
{
    public class TextWildcard
	{
		public Wildcard AggregateWildcard { get; set; }
		public TextWildcard Parent { get; set; }
		public TaskNode Node { get; set; }

		public string Comment {
			get {
				var output = "";
                if (!string.IsNullOrEmpty(Metadata))
                {
                    if (AggregateWildcard is not null)
                    {
                        if (string.IsNullOrEmpty(AggregateWildcard.Replacement.Name))
                        {

                            output += "Remarks\n";
                        }
                        else
                        {
                            output += AggregateWildcard.Replacement.Name + "\n";
                        }
                        output += "\t" + RenderedMetadata() + "\n";
                    }
				}
				foreach (var child in Children) {
					var o = child.Comment;
					if (string.IsNullOrEmpty(o)) {
						continue;
					}
					output += o + "\n";
				}
				return output;
			}
		}

		public string ReplacementValue {
			get {
				if (AggregateWildcard is not null)
				{
					if (Obfuscated && AggregateWildcard.Obfuscated != null) {
						return AggregateWildcard.Obfuscated.Name;
					} else {
						return AggregateWildcard.Replacement?.Name ?? "";
					}
				} else {
					return "";
				}
			}
		}

        #region Variables

        /// <summary>
        /// Stores the Wildcard id
        /// </summary>
        public int Id { get; private set; }

		/// <summary>
		/// Stores the
		/// </summary>
		private int index;

		/// <summary>
		/// Stores the name of the wildcard
		/// </summary>
		private string name;

		/// <summary>
		/// Stores the next automatically calculated identifier.
		/// </summary>
		private static int nextAutoId = 1000;

		/// <summary>
		///Indicates if the wildcard is obfuscated
		/// </summary>
		public bool Obfuscated { get; private set; }

		/// <summary>
		/// Stores the type of the wildcard
		/// </summary>
		private string type;

		/// <summary>
		/// Gets the Wildcard metadata
		/// </summary>
		public string Metadata { get; set; } 

		private (int Start, int End) ParentRange { get; set; }

		/// <summary>
		/// Gets the original text from which the text wildcard was created
		/// </summary>
		public string Value { get; private set; }

        /// <summary>
        /// Gets the string of where clauses
        /// </summary>
        public string Where { get; set; }

		/// <summary>
		/// Heuristic for determining the word in the case of a gendered pronoun.
		/// </summary>
		public Gender? AggregateGender {
            get
            {
				if (Parent != null) return Parent.AggregateGender;
				return null;
			}
		}

		#endregion

		#region Constructors

		private TextWildcard() { 
			Children = new List<TextWildcard>();
			AggregateWildcard = null;
			ParentRange = (-1, -1);
		}

		#endregion

		#region Properties

		public List<TextWildcard> Children { get; set; }

		/// <summary>
		/// Gets the keycode associated to each wildcard group unique replacements
		/// </summary>
		public string Keycode
		{
			get{ return Name + Id.ToString().PadLeft(4, '0'); }
		}

		public void SetId(int value) {
            Id = (value < 0) ? nextAutoId++ : value;
		}

        /// <summary>
        /// Gets the name of the wildcard
        /// </summary>
        public string Name
        {
            get { return name; }
            protected set { name = string.IsNullOrEmpty(value) ? null : value.ToLower(); }
        }

        /// <summary>
        /// Gets the type of the wildcard
        /// </summary>
        public string Type
        {
            get { return type; }
            protected set { type = string.IsNullOrEmpty(value) ? null : value.ToLower(); }
        }

		#endregion

		#region Methods

		public override string ToString()
		{
			return Value ?? "";
		}

		public string RenderedMetadata() 
		{
			var offset = 0;
			var m = new string(Metadata);
			foreach (var child in Children) {
				if (child.ParentRange == (-1, -1)) continue;
				var n =  child.ReplacementValue;
				if (n == null) continue;
                m = m.Remove(child.ParentRange.Start - offset, child.ParentRange.End - child.ParentRange.Start)
					.Insert(child.ParentRange.Start - offset, n);
				offset += child.ParentRange.End - child.ParentRange.Start - n.Length;
			}
			return m.Capitalize();
		}

		/// <summary>
		/// Parses the where clauses and metadata in a text wildcard to extract nested wildcards (helper)
		/// </summary>
		/// <param name="w">The string containing nested TextWildcards to parse.</param>
		private void ParseNestedWildcardsHelper(string s){
			int cc = 0;
			s ??= string.Empty;

			do {
				while ((cc < s.Length) && (s[cc] != '{')) cc += 1;
				int bcc = cc;
				var inner = XtractWildcard (s, ref cc);
				if (inner == null)
					continue;
				inner.ParentRange = (bcc, cc);
				inner.Parent = this;
				Children.Add(inner);
				inner.ParseNestedWildcardsHelper(inner.Metadata);
				inner.ParseNestedWildcardsHelper(inner.Where);
			} while(cc < s.Length);
		}

		public List<TextWildcard> ToList() {
            var l = new List<TextWildcard>
            {
                this
            };
            foreach (var child in Children) {
				l.AddRange(child.ToList());
			}
			return l;
		}

		#endregion

		#region Static Methods

		public static TextWildcard XtractWildcard(string s, ref int cc)
		{
			if ((cc >= s.Length) || (s[cc] != '{'))
				return null;

			// Create Wildcard and set index
			TextWildcard wildcard = new();
			wildcard.index = cc;
			// Read wildcard name
			++cc;
			wildcard.Name = ReadWildcardName(s, ref cc);
			if (String.IsNullOrEmpty(wildcard.Name)) return null;

			// Read obfuscator
			wildcard.Obfuscated = Scanner.ReadChar('?', s, ref cc);

			// Read wildcard type
			wildcard.Type = ReadWildcardType(s, ref cc);

			// Read wildcard id
			wildcard.SetId(ReadWildcardId(s, ref cc));

			// Read wildcard where clauses (query)
            wildcard.Where = ReadWhereClauses(s, ref cc);

			// Read wildcard metadata
			wildcard.Metadata = ReadWildcardMetadata(s, ref cc);

			// Set wildcard value
			if (cc < s.Length) ++cc;
			wildcard.Value = s[wildcard.index..cc];
            wildcard.ParseNestedWildcardsHelper(wildcard.Metadata);
			wildcard.ParseNestedWildcardsHelper(wildcard.Where);
			return wildcard;
		}

		private static string ReadWildcardName(string s, ref int cc)
		{
			Scanner.SkipSpaces(s, ref cc);
			int bcc = cc;
			while ((cc < s.Length) && Scanner.IsLAlpha(s[cc])) ++cc;
			return s[bcc..cc];
		}

		private static string ReadWildcardType(string s, ref int cc)
		{
			Scanner.SkipSpaces(s, ref cc);
			int bcc = cc;
			while ((cc < s.Length) && Scanner.IsLAlpha(s[cc])) ++cc;
			string type = s[bcc..cc];
			if ((type != null) && type.IsAnyOf("meta", "where"))
			{
				cc -= type.Length;
				return String.Empty;
			}
			return type;
		}

		private static string ReadWhereClauses(string s, ref int cc)
		{
			Scanner.SkipSpaces(s, ref cc);
			int bcc = cc;
			// First, read the "where" literal string
			char[] where = new char[]{'w','h','e','r', 'e'};
			foreach(char c in where){
				if (Scanner.ReadChar(c, s, ref cc)) continue;
				cc = bcc;
				return null;
			}
			// After the "where" literal string, the where clauses come
			string clauses = WhereParser.Fetch(s, ref cc);
			return clauses;
		}

		private static string ReadWildcardMetadata(string s, ref int cc)
		{
			Scanner.SkipSpaces(s, ref cc);
			char[] meta = new char[]{'m','e','t','a'};
			foreach(char c in meta){
				if (Scanner.ReadChar(c, s, ref cc)) continue;
                Scanner.FindClosingPair(s, ref cc, '{', '}');
				return null;
			}
			Scanner.SkipSpaces(s, ref cc);
			if (!Scanner.ReadChar(':', s, ref cc))
			{
                Scanner.FindClosingPair(s, ref cc, '{', '}');
				return null;
			}
			int bcc = cc;
			Scanner.FindClosingPair(s, ref cc, '{', '}');
			return s[bcc..cc];
		}

		private static int ReadWildcardId(string s, ref int cc)
		{
            Scanner.SkipSpaces(s, ref cc);
            if (!Scanner.XtractUInt16(s, ref cc, out ushort usId)) return -1;
			return usId;
		}
		#endregion
	}
}
