using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Provides methods for data parsing
	/// </summary>
	public static class Scanner
	{
		/// <summary>
		/// Advances the read header to the next occurrence of the specified char
		/// </summary>
		/// <param name="c">The char to find</param>
		/// <param name="input">The input string</param>
		/// <param name="cc">Read header</param>
		/// <returns>true if provided character c is found before the end of the string, false otherwise</returns>
		public static bool AdvanceToChar(char c, string input, ref int cc)
		{
			while (cc < input.Length)
			{
				if (input[cc] == c)
					return true;
				++cc;
			}
			return false;
		}

		/// <summary>
		/// Advances the read header to the next occurrence of the specified chars
		/// </summary>
		/// <param name="c">The array of chars to find</param>
		/// <param name="input">The input string</param>
		/// <param name="cc">Read header</param>
		/// <returns>true if any of the provided characters in c is found before the end of the string, false otherwise</returns>
		public static bool AdvanceToChar(char[] c, string input, ref int cc)
		{
			if ((c == null) || (c.Length < 1))
                throw new ArgumentNullException(nameof(c));
			List<char> lc = new(c);

			while (cc < input.Length)
			{
				if (lc.Contains(input[cc]))
					return true;
				++cc;
			}
			return false;
		}

		/// <summary>
		/// Reads the char from input at cc index and compares to the provided char c. If match advances the read header cc
		/// </summary>
		/// <param name="c">The expected char to read</param>
		/// <param name="input">The input string</param>
		/// <param name="cc">Read header</param>
		/// <returns>true if provided character c matches the char at cc in input, false otherwise</returns>
		public static bool ReadChar(char c, string input, ref int cc)
		{
			if ((cc < input.Length) && (input[cc] == c))
			{
				++cc;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Advances the read header until no spaces are found
		/// </summary>
		/// <param name="s">Input string</param>
		/// <param name="cc">Read header</param>
		public static void SkipSpaces(string s, ref int cc)
		{
			if ((cc < 0) || (cc >= s.Length))
				return;
			while ((cc < s.Length) && Scanner.IsSpace(s[cc])) ++cc;
		}

		/// <summary>
		/// Extracts an unsigned 32 bit integer from the input string
		/// </summary>
		/// <param name="input">String from which the integer will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the id found in s if the extraction succeded, or 0 if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a valid id was found in s starting at cc, false otherwise</returns>
		public static bool XtractByte(string input, ref int cc, out byte value)
		{
			int bcc = cc;
			int length;
			string sValue;

			value = 0;
			while ((cc < input.Length) && IsAlNum(input[cc]))
				++cc;
			length = Math.Min(cc - bcc, input.Length - bcc);
			if ((length < 1) || (length > 10))
				return false;
			sValue = input.Substring(bcc, length);
			return Byte.TryParse(sValue, out value);
		}

		/// <summary>
		/// Extracts an 32 bit integer from the input string
		/// </summary>
		/// <param name="input">String from which the integer will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the id found in s if the extraction succeded, or 0 if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a valid id was found in s starting at cc, false otherwise</returns>
		public static bool XtractSByte(string input, ref int cc, out sbyte value)
		{
			int bcc = cc;
			int length;
			string sValue;

			value = 0;
			if ((cc < input.Length) && (input[cc] == '-'))
				++cc;
			while ((cc < input.Length) && IsNumeric(input[cc]))
				++cc;
			length = Math.Min(cc - bcc, input.Length - bcc);
			if ((length < 1) || (length > 4))
				return false;
			sValue = input.Substring(bcc, length);
			return SByte.TryParse(sValue, out value);
		}

		/// <summary>
		/// Extracts an unsigned 32 bit integer from the input string
		/// </summary>
		/// <param name="input">String from which the integer will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the id found in s if the extraction succeded, or 0 if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a valid id was found in s starting at cc, false otherwise</returns>
		public static bool XtractUInt16(string input, ref int cc, out ushort value)
		{
			int bcc = cc;
			int length;
			string sValue;

			value = 0;
			while ((cc < input.Length) && IsNumeric(input[cc]))
				++cc;
			length = Math.Min(cc - bcc, input.Length - bcc);
			if ((length < 1) || (length > 5))
				return false;
			sValue = input.Substring(bcc, length);
			return UInt16.TryParse(sValue, out value);
		}

		/// <summary>
		/// Extracts an 32 bit integer from the input string
		/// </summary>
		/// <param name="input">String from which the integer will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the id found in s if the extraction succeded, or 0 if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a valid id was found in s starting at cc, false otherwise</returns>
		public static bool XtractInt16(string input, ref int cc, out short value)
		{
			int bcc = cc;
			int length;
			string sValue;

			value = 0;
			if ((cc < input.Length) && (input[cc] == '-'))
				++cc;
			while ((cc < input.Length) && IsNumeric(input[cc]))
				++cc;
			length = Math.Min(cc - bcc, input.Length - bcc);
			if ((length < 1) || (length > 6))
				return false;
			sValue = input.Substring(bcc, length);
			return Int16.TryParse(sValue, out value);
		}

		/// <summary>
		/// Extracts an unsigned 32 bit integer from the input string
		/// </summary>
		/// <param name="input">String from which the integer will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the id found in s if the extraction succeded, or 0 if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a valid id was found in s starting at cc, false otherwise</returns>
		public static bool XtractUInt32(string input, ref int cc, out uint value)
		{
			int bcc = cc;
			int length;
			string sValue;

			value = 0;
			while ((cc < input.Length) && IsNumeric(input[cc]))
				++cc;
			length = Math.Min(cc - bcc, input.Length - bcc);
			if ((length < 1) || (length > 10))
				return false;
			sValue = input.Substring(bcc, length);
			return UInt32.TryParse(sValue, out value);
		}

		/// <summary>
		/// Extracts an 32 bit integer from the input string
		/// </summary>
		/// <param name="input">String from which the integer will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the id found in s if the extraction succeded, or 0 if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a valid id was found in s starting at cc, false otherwise</returns>
		public static bool XtractInt32(string input, ref int cc, out int value)
		{
			int bcc = cc;
			int length;
			string sValue;

			value = 0;
			if ((cc < input.Length) && (input[cc] == '-'))
				++cc;
			while ((cc < input.Length) && IsNumeric(input[cc]))
				++cc;
			length = Math.Min(cc - bcc, input.Length - bcc);
			if ((length < 1) || (length > 11))
				return false;
			sValue = input.Substring(bcc, length);
			return Int32.TryParse(sValue, out value);
		}

		/// <summary>
		/// Extracts a C-like valid identifier.
		/// </summary>
		/// <param name="input">String from which the identifier will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains a valid C-like identifier found in s if the extraction succeded, or null if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a C-like identifier was found in s starting at cc, false otherwise</returns>
		public static bool XtractIdentifier(string input, ref int cc, out string value)
		{
			value = null;
			if (cc >= input.Length)
				return false;

			int bcc = cc;
			if ((input[cc] != '_') && !IsAlpha(input[cc]))
				return false;

			do {
				++cc;
			} while ((cc < input.Length) && (IsAlNum(input[cc]) || (input[cc] == '_')));
			value = input[bcc..cc];
			return true;
		}

		/// <summary>
		/// Extracts a string delimited by double quotes, preserving escaped characters.
		/// </summary>
		/// <param name="input">String from which the quoted string will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the delimited string found in s if the extraction succeded, or null if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a string delimited by double quotes was found in s starting at cc, false otherwise</returns>
		public static bool XtractDoubleQuotedString(string input, ref int cc, out string value)
		{
			value = null;
			if ((cc >= input.Length) || !ReadChar('"', input, ref cc))
				return false;

			StringBuilder sb = new();
			while (cc < input.Length) {
				if (ReadChar ('"', input, ref cc))
					break;

				if (cc == '\\') {
					if ( ++cc >= input.Length )
						return false;
					if ((input [cc] != '"') && (input [cc] != '\\'))
						sb.Append ('\\');
				}
				sb.Append (input [cc++]);
			}
			value = sb.ToString ();
			return true;
		}

		/// <summary>
		/// Extracts a string delimited by single quotes, preserving escaped characters.
		/// </summary>
		/// <param name="input">String from which the quoted string will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the delimited string found in s if the extraction succeded, or null if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a string delimited by single quotes was found in s starting at cc, false otherwise</returns>
		public static bool XtractSingleQuotedString(string input, ref int cc, out string value)
		{
			value = null;
			if ((cc >= input.Length) || !ReadChar('\'', input, ref cc))
				return false;

			StringBuilder sb = new();
			while (cc < input.Length) {
				if (ReadChar ('\'', input, ref cc))
					break;

				if (cc == '\\') {
					if ( ++cc >= input.Length )
						return false;
					if ((input [cc] != '\'') && (input [cc] != '\\'))
						sb.Append ('\\');
				}
				sb.Append (input [cc++]);
			}
			value = sb.ToString ();
			return true;
		}

		/// <summary>
		/// Extracts the first double precision floating point number found inside a string
		/// </summary>
		/// <param name="input">String from which the double precision float will be extracted</param>
		/// <param name="cc">The search starting position</param>
		/// <param name="value">When this method returns contains the first double precision float found in s if the extraction succeded, or zero if the extraction failed.</param>
		/// <returns>True if the extraction succeded and a valid double precision float was found in s starting at cc, false otherwise</returns>
		public static bool XtractDouble(string input, ref int cc, out double value)
		{
			int bcc = cc;
			int length;
			string sValue;

			value = 0;
			// Sign
			if ((cc < input.Length) && (input[cc] == '-'))
				++cc;
			// Integer part
			while ((cc < input.Length) && IsNumeric(input[cc]))
				++cc;
			// Decimal part
			if ((cc < input.Length) && (input[cc] == '.'))
			{
				++cc;
				if ((cc >= input.Length) || !IsNumeric(input[cc]))
					return false;
				while ((cc < input.Length) && IsNumeric(input[cc]))
					++cc;
			}
			// Exponential part
			// The Exp e
			if ((cc < input.Length) && ((input[cc] == 'E') || (input[cc] == 'e')))
			{
				++cc;
				// The Exp sign (if any)
				if ((cc < input.Length) && ((input[cc] == '+') || (input[cc] == '-')))
					++cc;
				// First Exp digit
				if ((cc >= input.Length) || !IsNumeric(input[cc]))
					return false;
				++cc;
				// Second Exp digit (if any)
				if ((cc < input.Length) && IsNumeric(input[cc]))
				{
					++cc;
					// Third Exp digit (if any)
					if ((cc < input.Length) && IsNumeric(input[cc]))
						++cc;
				}

			}
			length = Math.Min(cc - bcc, input.Length - bcc);
			if ((length < 1) || (length > 11))
				return false;
			sValue = input.Substring(bcc, length);
			return Double.TryParse(sValue, out value);
		}

		/// <summary>
		/// Indicates whether a ANSI character is letter or digit.
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is a letter or digit; otherwise, false.</returns>
		public static bool IsAlNum(char c)
		{
			return ((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c <= 'z'));
		}

		/// <summary>
		/// Indicates whether a ANSI character is letter.
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is a letter; otherwise, false.</returns>
		public static bool IsAlpha(char c)
		{
			return ((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c <= 'z'));
		}

		/// <summary>
		/// Indicates whether a ANSI character is lower case letter.
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is an lower case letter; otherwise, false.</returns>
		public static bool IsLAlpha(char c)
		{
			return ((c >= 'a') && (c <= 'z'));
		}

		/// <summary>
		/// Indicates whether a ANSI character is lower case letter or digit.
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is an lower case letter or digit; otherwise, false.</returns>
		public static bool IsLAlNum(char c)
		{
			return ((c >= '0') && (c <= '9')) || ((c >= 'a') && (c <= 'z'));
		}

		/// <summary>
		/// Indicates whether a ANSI character is digit.
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is a digit; otherwise, false.</returns>
		public static bool IsNumeric(char c)
		{
			return ((c >= '0') && (c <= '9'));
		}

		/// <summary>
		/// Indicates whether a ANSI character is \f, \n, \r, \t, \v, or space
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is a space character; otherwise, false.</returns>
		public static bool IsSpace(char c)
		{
            return c switch
            {
                ' ' or '\f' or '\n' or '\r' or '\t' or '\v' => true,
                _ => false,
            };
        }

		/// <summary>
		/// Indicates whether a ANSI character is upper case letter.
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is an upper case letter; otherwise, false.</returns>
		public static bool IsUAlpha(char c)
		{
			return ((c >= 'A') && (c <= 'Z'));
		}

		/// <summary>
		/// Indicates whether a ANSI character is upper case letter or digit.
		/// </summary>
		/// <param name="c">A ASNI character</param>
		/// <returns>true if c is an upper case letter or digit; otherwise, false.</returns>
		public static bool IsUAlNum(char c)
		{
			return ((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'Z'));
		}

		/// <summary>
		/// Gets a value indicating if the provided string is an expandable production.
		/// An expandable production is a rule which can be split into two or more
		/// new production tules
		/// </summary>
		/// <param name="replacement">The replacement (right part) string of a production rule</param>
		/// <returns><c>true</c> if the provided string is an expandable production; otherwise, <c>false</c>.</returns>
		public static bool IsExpandable (string replacement)
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
		public static bool FindClosingPair(string s, ref int cc, char open, char close){
			int par = 1;
			while ((cc < s.Length) && (par > 0)) {
				if (s [cc] == '\\') {
					cc+= 2;
					continue;
				}
				if (s [cc] == open) ++par;
				else if (s [cc] == close) --par;
				++cc;
			}
			--cc;
			return par == 0;
		}

        public static bool IsValidParenthesisExpression(string sentence, char open, char close)
        {
            int count = 0;
            foreach (char c in sentence)
            {
                if (c == open)
                {
                    count += 1;
                }
                else if (c == close)
                {
                    if (count > 0)
                    {
                        count -= 1;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return count == 0;
        }

        public static string[] SplitRespectingParenthesis(string s, char opening, char closing, char splitChar)
        {
			var results = new List<string>();

			int cc = 0;
			int bcc = 0;
			while (cc < s.Length) {
                if (s[cc] == opening)
                {
					cc += 1;
                    if (!FindClosingPair(s, ref cc, opening, closing))
                    {
                        return null;
                    }
                }
                if (s[cc] == splitChar)
                {
                    results.Add(s[bcc..cc]);
					cc += 1;
					bcc = cc;
				}
				cc += 1;
			}

			if (cc - bcc > 0) {
				results.Add(s[bcc..cc]);
			}

			return results.ToArray();
		}

        public static List<(int Start, int End)> FindParenthesisRanges(string sentence, char opening, char closing)
        {
            var parts = new List<(int Start, int End)>();

            int cc = 0;
            int bcc = 0;
            while (cc < sentence.Length)
            {
                if (sentence[cc] != opening)
                {
                    cc += 1;
                    continue;
                }

                if (cc - bcc > 0)
                {
                    parts.Add((bcc, cc - 1));
                }

                bcc = cc;
                cc += 1;
                if (!FindClosingPair(sentence, ref cc, opening, closing))
                {
					return null;
                }
                parts.Add((bcc, cc));
                cc += 1;
                bcc = cc;
            }
            if (cc - bcc > 0)
            {
                parts.Add((bcc, cc - 1));
            }
            return parts;
        }

		private static readonly Regex nonTerminalIdentifierMatcher = new(@"(\$[0-9A-Za-z_]+)");

        /**
		* Splits a rule , i.e. the right hand side of what this object represents, 
		* into a list of strings, where each string either starts with $, starst with {, ends with } and has balance braces,
		* or is literal text.
		*/
        public static string[] SplitRule(string rule) {
			List<string> tokens = new();
			var literalText = "";
			for (int index = 0; index < rule.Length; index += 1)
			{
				if (rule[index] == '{') {
					if (literalText != "") {
						tokens.Add(literalText);
						literalText = "";
					}
					int endIndex = index + 1;
					if (FindClosingPair(rule, ref endIndex, '{', '}')) {
						tokens.Add(rule[index..(endIndex+1)]);
						index = endIndex;
						continue;
					} else {
						return null;
					}
				} else if (rule[index] == '$') {
					if (literalText != "") {
						tokens.Add(literalText);
						literalText = "";
					}
					int endIndex = index + 1;
					if (XtractIdentifier(rule, ref endIndex, out string value))
					{
						tokens.Add(rule[index..endIndex]);
						index = endIndex-1;
						continue;
					}
				} else {
					literalText += rule[index];
				}
			}
			if (literalText != "") {
				tokens.Add(literalText);
			}
			return tokens.ToArray();
		}
    }
}
