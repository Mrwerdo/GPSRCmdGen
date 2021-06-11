using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RoboCup.AtHome.CommandGenerator.ReplaceableTypes
{
	/// <summary>
	/// Represents a Question from the set of Predefined Questions
	/// according to the RoboCup@Home Rulebook 2015
	/// </summary>
	[Serializable]
	public class PredefinedQuestion : INameable, ITiered, IMetadatable
	{

		/// <summary>
		/// Gets or sets the question string
		/// </summary>
		[XmlElement("q")]
		public string Question { get; set; }

		/// <summary>
		/// Gets or sets the answer string
		/// </summary>
		[XmlElement("a")]
		public string Answer { get; set; }

		/// <summary>
		/// Gets or sets the  difficulty degree (tier) for UNDERSTANDING the question
		/// </summary>
		[XmlAttribute("difficulty"), DefaultValue(DifficultyDegree.Easy)]
		public DifficultyDegree Tier { get; set; }

		/// <summary>
		/// Initializes a new instance of this class.
		/// </summary>
		/// <remarks>Intended for serialization purposes</remarks>
		public PredefinedQuestion() { }

		/// <summary>
		/// Returns the <c>"question"</c> string
		/// </summary>
		[XmlIgnore]
		public string Name => "question";

		/// <summary>
		/// Returns a set of two strings, the firs containing the question,
		/// and the second one containing the answer
		/// </summary>
		[XmlIgnore]
        public List<string> Metadata
		{
			get
			{
                return new List<string>
                {
                    $"Q: {Question}",
                    $"A: {Answer}"
                };
			}
		}

		public override string ToString()
		{
			return $"{Question} ({Tier})";
		}
	}
}

