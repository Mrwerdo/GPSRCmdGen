using System;

namespace RoboCup.AtHome.CommandGenerator
{
    public partial class WhereParser{
		/// <summary>
		/// Encapsulates several conditions
		/// </summary>
		public class ConditionalStatement : IEvaluable
        {

			/// <summary>
			/// Gets the default value (left value in a binary statement)
			/// </summary>
			/// <value>A.</value>
			public IEvaluable A{ get; internal set; }

			/// <summary>
			/// Gets the operator for the conditional statement
			/// </summary>
			/// <value>The operator.</value>
			public string Operator{ get; internal set; }

			/// <summary>
			/// Gets the right value in a binary statement
			/// </summary>
			/// <value>A.</value>
			public IEvaluable B{ get; internal set; }

			/// <summary>
			/// Evaluate the conditional statement.
			/// If there is no operator returns A. If the operator is NOT returns !A.
			/// Otherwise returns the result of evaluating A op B.
			/// </summary>
			/// <param name="obj">A boolean result of evaluating the conditional statement</param>
            public bool Evaluate(object obj){
				if (string.IsNullOrEmpty(Operator))
					return A != null && A.Evaluate(obj);

                return Operator.ToLower() switch
                {
                    "and" => A.Evaluate(obj) && B.Evaluate(obj),
                    "or" => A.Evaluate(obj) || B.Evaluate(obj),
                    "xor" => A.Evaluate(obj) ^ B.Evaluate(obj),
                    "not" => !A.Evaluate(obj),
                    _ => throw new NotSupportedException($"Operator '{Operator}' is not supported"),
                };
            }

			public override string ToString()
			{
				if (string.IsNullOrEmpty(Operator))
					return A.ToString();
				else if(Operator.ToLower() == "not")
					return $"NOT({B})";
				return $"{A} {Operator} {B}";
			}
        }
    }
}

