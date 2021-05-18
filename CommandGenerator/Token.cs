using System;
using System.Collections.Generic;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Represents a Token: an element of a Task
	/// A token is a substring extracted from a taks prototype string after analysis,
	/// which may correspond to either a literal string or a wildcard (value != null).
	/// </summary>
	public class Token : IMetadatable
	{
        #region Properties
		/// <summary>
		/// Stores he metadata contained in this Token, fetched from both,
		/// taks prototype string (metadata stored in grammar) and the
		/// metadata asociated to the Token's value.
		/// </summary>
		public List<string> Metadata { get; set; }


        /// <summary>
        /// Gets the original substring in the taks prototype string.
        /// </summary>
        public string Key { get; set; }

		/// <summary>
		/// Gets the replacement object for the wildcard represented by this
		/// Token in the taks prototype string, if the token is a wildcard.
		/// If the Token is a literal string, it returns null.
		/// </summary>
		public INameable Value { get; set; } 

		/// <summary>
		/// Gets the INameable.Name. of the Token.
		/// When Value property is not null, it returns the name of the Toklen's value.
		/// When Value property is null, it returns the Token's key, 
		/// </summary>
		public string Name { get { return Value == null ? Key : Value.Name; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.Token"/> class.
		/// </summary>
		/// <param name="key">The original substring in the taks prototype string.</param>
		public Token (string key) : this(key, null, null){}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.Token"/> class.
		/// </summary>
		/// <param name="key">The original substring in the taks prototype string.</param>
		/// <param name="value">The replacement object for the wildcard represented by this Token.</param>
		/// <param name="metadata">Additional metadata to add (e.g. from the grammar
		/// or the taks prototype string).</param>
		public Token (string key, INameable value, IEnumerable<string> metadata)
		{
			Key = key;
			Value = value;
			this.Metadata = new List<string>();
            if (value is IMetadatable imvalue)
                this.Metadata.AddRange(imvalue.Metadata);
            if (metadata != null)
				this.Metadata.AddRange(metadata);
		}

        #endregion

		#region Methods

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Token"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Token"/>.</returns>
		public override string ToString()
		{
            return Value == null ? Key : $"{Key} => {Value.Name}";
		}

		public string[] GetMetadata() {
			return Metadata.ToArray();
		}

		#endregion
	}
}

