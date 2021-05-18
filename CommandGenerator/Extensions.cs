using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Helper class that contains extension methods
	/// </summary>
	public static class Extensions
	{

		/// <summary>
		/// Determines whether the specified String object has the same value of any of an array of other string objects.
		/// </summary>
		/// <returns><c>true</c> if the value of the source parameter is equal to the value of any of the strings contained in the others parameter; otherwise, <c>false</c>.</returns>
		/// <param name="source">The first string to compare, or null</param>
		/// <param name="others">The set of strings to compare, or null.</param>
		public static bool IsAnyOf(this string source, params string[] others){
			foreach (string o in others)
				if (String.Equals (source, o))
					return true;
			return false;
		}

		/// <summary>
		/// Determines whether the specified char has the same value of any of an array of other chars.
		/// </summary>
		/// <returns><c>true</c> if the value of the source parameter is equal to the value of any of the chars contained in the others parameter; otherwise, <c>false</c>.</returns>
		/// <param name="source">The first char to compare, or null</param>
		/// <param name="others">The set of chars to compare, or null.</param>
		public static bool IsAnyOf(this char source, params char[] others){
			foreach (char c in others)
				if (source == c)
					return true;
			return false;
		}

		/// <summary>
		/// Determines whether the specified String object has the same value of any of an array of other string objects. A parameter specifies the culture, case, and sort rules used in the comparison.
		/// </summary>
		/// <returns><c>true</c> if the value of the source parameter is equal to the value of any of the strings contained in the others parameter; otherwise, <c>false</c>.</returns>
		/// <param name="source">The first string to compare, or null</param>
		/// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison. </param>
		/// <param name="others">The set of strings to compare, or null.</param>
		public static bool IsAnyOf(this string source, StringComparison comparisonType, params string[] others){
			foreach (string o in others)
				if (String.Equals (source, o, comparisonType))
					return true;
			return false;
		}

		/// <summary>
		/// Returns and removes the first element in a sequence that satisfies a specified condition.
		/// </summary>
		/// <returns>The first element in the sequence that passes the test in the specified predicate function.</returns>
		/// <param name="source">An IList<T> to return and remove an element from.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		public static TSource PopFirst<TSource>(this IList<TSource> source, Func<TSource, bool> predicate)
		{
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
			if (source.Count < 1)
				throw new InvalidOperationException ("The source sequence is empty.");
			for (int i = 0; i < source.Count; ++i) {
				if (predicate (source [i])) {
					TSource item = source [i];
					source.RemoveAt(i);
					return item;
				}
			}
			throw new InvalidOperationException ("No element satisfies the condition in predicate");
		}

		/// <summary>
		/// Returns and removes the first element in a sequence that satisfies the condition contained in a query.
		/// WhereParser is used to evaluate the query.
		/// </summary>
		/// <returns>The first element in the sequence that passes the test in the specified predicate function.</returns>
		/// <param name="source">An IList<T> to return and remove an element from.</param>
		/// <param name="query">A query describing a function to test each element for a condition.</param>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		public static TSource PopFirst<TSource>(this List<TSource> source, string query){
			WhereParser.ConditionalStatement statement = WhereParser.Parse(query);
			if (statement == null)
				throw new InvalidOperationException("query is null or is not in an acceptable format");
			return source.PopFirst(o => statement.Evaluate(o));
		}

		/// <summary>
		/// Returns and removes the first element in a sequence that satisfies both,
		/// the condition of the given predicate and the one contained in a query.
		/// WhereParser is used to evaluate the query.
		/// </summary>
		/// <returns>The first element in the sequence that passes the test in the specified predicate function.</returns>
		/// <param name="source">An IList<T> to return and remove an element from.</param>
		/// <param name="predicate">A query describing a function to test each element for a condition (ignored when null).</param>
		/// <param name="query">A query describing a function to test each element for a condition (ignored when null).</param>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		public static TSource PopFirst<TSource>(this List<TSource> source,  Func<TSource, bool> predicate, string query){
			if (source == null)  throw new ArgumentNullException(nameof(source));
            if (predicate == null && String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(predicate), "At least predicate or query must be provided.");

			WhereParser.ConditionalStatement statement = WhereParser.Parse(query);
			if ((predicate == null) && (statement == null))
				throw new InvalidOperationException("No predicate was provided and query is null or is not in an acceptable format");
			else if(predicate == null)
				return source.PopFirst(o => statement.Evaluate(o));
			else if(statement == null)
				return source.PopFirst(predicate);
			else
				return source.PopFirst(o => predicate(o) && statement.Evaluate(o));
		}

		/// <summary>
		/// Retrieves and removes the last element in the list 
		/// </summary>
		/// <param name="list">The list from which the last element will be extracted</param>\
		/// <typeparam name="T">The data type of the list.</typeparam>
		public static TSource PopLast<TSource>(this IList<TSource> source)
		{
			if (source == null)
                throw new ArgumentNullException(nameof(source));
			if (source.Count < 1)
				throw new InvalidOperationException ("The source sequence is empty.");
			TSource item = source[source.Count - 1];
			source.RemoveAt (source.Count - 1);
			return item;
		}

		/// <summary>
		/// Shuffles the list using the Fisher-Yates shuffle
		/// </summary>
		/// <param name="list">The list to be shuffled</param>
		/// <param name="rnd">a random number generator</param>
		/// <typeparam name="T">The data type of the list.</typeparam>
		public static void Shuffle<T>(this IList<T> list, Random rnd)  
		{   
			int n = list.Count;  
			while (n > 1) {  
				n--;  
				int k = rnd.Next(n + 1);  
				T value = list[k];  
				list[k] = list[n];  
				list[n] = value;  
			}  
		}


		public static List<T> ShuffleCopy<T>(this IList<T> list, Random random) 
		{
            var copy = new List<T>(list);
            copy.Shuffle(random);
			return copy;
		}

        public static T SelectUniform<T>(this ICollection<T> collection, Random random)
        {
            return collection.ElementAt(random.Next(collection.Count));
        }

        public static T RandomPick<T>(this Random random, params T[] source)
        {
            return source.SelectUniform(random);
        }

		/// <summary>
		/// Context free grammar and where clause aware split. This function splits by non-terminal identifiers
		/// provided they are not inside of a where clause (i.e. curly braces).
		/// </summary>
		public static string[] SmartSplit(this string s, char c) {
			List<string> parts = new();
			int bcc = 0;
			int cc = 0;
			int paranthesisCounter = 0;
			while (cc < s.Length) {
				if (s[cc] == '{') {
					paranthesisCounter += 1;
				} else if (s[cc] == '}') {
					paranthesisCounter = Math.Max(paranthesisCounter - 1, 0);
				}

				if (s[cc] != c || paranthesisCounter > 0) {
					cc += 1;
					if (cc != s.Length) {
						continue;
					}
				}
				parts.Add(s[bcc..cc]);
				cc += 1;
				bcc = cc;
			}
			return parts.ToArray();
		}

		public static string Capitalize(this string s) {
			if (s.Length > 1) {
				int divider = 0;
                Scanner.SkipSpaces(s, ref divider);
				return s[0..(divider+1)].ToUpper() + s[(divider+1)..];
			} else {
				return s.ToUpper();
			}
		}

        public static string Indent(this string value, int size)
        {
			if (size == 0) return value;
			var indent = new string(' ', size);
			var output = "";
			foreach (var s in value.Split('\n')) {
				output += indent + s;
			}
            return output;
        }
	}
}

