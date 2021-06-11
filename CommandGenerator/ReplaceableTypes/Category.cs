using System;
using System.Collections.Generic;
using System.Xml.Serialization;

#nullable enable

namespace RoboCup.AtHome.CommandGenerator.ReplaceableTypes
{
	/// <summary>
	/// Represents a real-wolrd object category
	/// according to the RoboCup@Home Rulebook 2015
	/// </summary>
	[Serializable]
	public class Category : INameable
	{

		/// <summary>
		/// Gets or sets the default location for objects in the category
		/// </summary>
		[XmlIgnore]
		public SpecificLocation? DefaultLocation { get; set; }

		/// <summary>
		/// Stores the list of objects in the category
		/// </summary>
		protected Dictionary<string, Object> objects = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="RoboCup.AtHome.CommandGenerator.Category"/> class.
		/// </summary>
		/// <remarks>Intended for serialization purposes</remarks>
		public Category() { }

		/// <summary>
		/// Gets the name of the Category
		/// </summary>
		[XmlAttribute("name")]
		public string? Name { get; set; }

		/// <summary>
		/// Gets or sets the name of the default location for objects in the category
		/// </summary>
		/// <remarks>Use for (de)serialization purposes only</remarks>
		[XmlAttribute("defaultLocation")]
		public string? LocationString {
			get { return DefaultLocation?.Name; }
			set {
				if (DefaultLocation == null)
					DefaultLocation = new SpecificLocation(value, true, false);
				else
					DefaultLocation.Name = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the default location's room for objects in the category
		/// </summary>
		/// <remarks>Use for (de)serialization purposes only</remarks>
		[XmlAttribute("room")]
		public string? RoomString
		{
			get { return DefaultLocation?.Room.Name; }
			set
			{
				if (DefaultLocation == null)
					DefaultLocation = new SpecificLocation("unknown", true, false);
				DefaultLocation.Room = new Room(value);
			}
		}

		/// <summary>
		/// Gets or sets the list of objects in the category.
		/// </summary>
		/// <remarks>Use for (de)serialization purposes only</remarks>
		[XmlElement("object")]
		public Object[] Objects
		{
			get { return new List<Object>(objects.Values).ToArray(); }
			set
			{
				foreach (Object o in value)
					AddObject(o);
			}
		}

		/// <summary>
		/// Adds an object to the category
		/// </summary>
		/// <param name="item">The ibject to add to the category</param>
		public void AddObject(Object item){
			if (item == null)
				return;
			if ((item.Category != null) && (item.Category != this))
				item.Category.RemoveObject(item);
			if (!objects.ContainsKey(item.Name))
				objects.Add(item.Name, item);
			if (item.Category != this)
				item.Category = this;
		}
		
		/// <summary>
		/// Removes the given GPSRObject from the Category
		/// </summary>
		/// <param name="item">The GPSRObject to remove</param>
		/// <returns>true if the GPSRObject was in the collection, false otherwise</returns>
		private bool RemoveObject(Object item)
		{
			return item != null && objects.Remove(item.Name);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Category"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="RoboCup.AtHome.CommandGenerator.Category"/>.</returns>
		public override string ToString ()
		{
			return string.Format("{0} [{2} Objects | {1} ]", Name, DefaultLocation?.Name ?? "Unknown Location", objects.Count);
		}
	}
}

