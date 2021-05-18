using System.Collections.Generic;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Represents an object which contains a set of strings as metadata
	/// </summary>
	public interface IMetadatable : INameable
	{
		List<string> Metadata { get; }
	}
}
