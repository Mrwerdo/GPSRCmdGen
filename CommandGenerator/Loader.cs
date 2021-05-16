using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using RoboCup.AtHome.CommandGenerator.Containers;
using RoboCup.AtHome.CommandGenerator.ReplaceableTypes;


namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Helper class for loading and storing Xml serialized data
	/// </summary>
	public static class Loader
	{
		#region Variables

		/// <summary>
		/// Stores the path of the executable file
		/// </summary>
		private static readonly string exePath;

		/// <summary>
		/// Stores the namespace strings for serialized objects
		/// </summary>
		private static readonly XmlSerializerNamespaces ns;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes the <see cref="RoboCup.AtHome.CommandGenerator.Loader"/> class.
		/// </summary>
		static Loader(){
			exePath = AppDomain.CurrentDomain.BaseDirectory;
			ns = new XmlSerializerNamespaces();
			ns.Add ("", "");
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the path of the executable file
		/// </summary>
		public static string ExePath{get {return Loader.exePath;} }

		#endregion

		#region Methods

		/// <summary>
		/// Gets a full path for the given filename using the executable
		/// file path as base path path.
		/// </summary>
		/// <param name="fileName">The name of the file to combine into a path</param>
		/// <returns>A full path for the given fileName.</returns>
		public static string GetPath(string fileName){
			return Path.Combine (exePath, fileName);
		}

		/// <summary>
		/// Gets a full path for the given filename using the executable
		/// file path as base path path and a subdirectory.
		/// </summary>
		/// <param name="subdir">The name of the subdirectory that will contain the file</param>
		/// <param name="fileName">The name of the file to combine into a path</param>
		/// <returns>A full path for the given fileName.</returns>
		public static string GetPath(string subdir, string fileName){
			return Path.Combine (Path.Combine(exePath, subdir), fileName);
		}

		/// <summary>
		/// Loads an object from a XML file.
		/// </summary>
		/// <param name="filePath">The path of the XML file</param>
		/// <typeparam name="T">The type of object to load.</typeparam>
		/// <returns>The object in the XML file</returns>
		private static T LoadObject<T>(string filePath)
		{
			T item;
			using (StreamReader reader = new(filePath, ASCIIEncoding.UTF8))
			{
				XmlSerializer serializer = new(typeof(T));
				serializer.UnknownAttribute+= new XmlAttributeEventHandler(Serializer_UnknownAttribute);
				item = (T)serializer.Deserialize(reader);
				reader.Close();

			}
			return item;
		}

		/// <summary>
		/// Loads an object from a XML string.
		/// </summary>
		/// <param name="xml">An XML encoded string</param>
		/// <typeparam name="T">The type of object to load.</typeparam>
		/// <returns>The object in the XML string</returns>
		private static T LoadXmlString<T>(string xml)
		{
			T item;
			using (MemoryStream ms = new(Encoding.UTF8.GetBytes(xml ?? String.Empty)))
			{
				XmlSerializer serializer = new(typeof(T));
				serializer.UnknownAttribute+= new XmlAttributeEventHandler(Serializer_UnknownAttribute);
				item = (T)serializer.Deserialize(ms);
				ms.Close();

			}
			return item;
		}

		private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e){
            if (e.ObjectBeingDeserialized is not IDescribable desc)
                return;
            desc.Properties.Add(e.Attr.Name, e.Attr.Value);
		}

		/// <summary>
		/// Stores an object into a XML file.
		/// </summary>
		/// <param name="filePath">The path of the XML file to store the objectt within.</param>
		/// <param name="o">The object to serialize and save.</param>
		public static void Save(string filePath, object o){
			XmlWriterSettings settings = new();
			settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
			settings.Indent = true;
			settings.OmitXmlDeclaration = false;
            using StreamWriter stream = new(filePath);
            using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
            {
                XmlSerializer serializer = new(o.GetType());
                serializer.Serialize(xmlWriter, o, ns);
                xmlWriter.Close();
            }
            stream.Close();
        }

        public static List<V> Load<P, V>(string name, string path, string backup) where P : ILoadingContainer<V>
		{
			try {
				P obj = LoadObject<P>(GetPath(path));
                Generator.Green("Done!");
				return obj.Results;
			} catch {
                P obj = LoadXmlString<P>(backup);
                Generator.Err($"Failed! Default {name} loaded");
				return obj.Results;
			}
		}

		/// <summary>
		/// Serializes the specified list of T objects into a string.
		/// </summary>
		/// <param name="list">The list to serialize.</param>
		/// <typeparam name="T">The type of serializable objects of the list.</typeparam>
		/// <returns>A string containing the XML representation of the list.</returns>
		public static string Serialize<T>(List<T> list)
		{
			string serialized;
			T[] array = list.ToArray ();
			XmlWriterSettings settings = new();
			settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
			settings.Indent = true;
			settings.OmitXmlDeclaration = false;
			using (StringWriter textWriter = new())
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
					XmlSerializer serializer = new(typeof(T[]));
					serializer.Serialize (xmlWriter, array, ns);
				}
				serialized = textWriter.ToString ();
			}
			return serialized;
		}
		#endregion
	}
}

