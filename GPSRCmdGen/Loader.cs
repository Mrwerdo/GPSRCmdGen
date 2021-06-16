using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using RoboCup.AtHome.CommandGenerator;
using RoboCup.AtHome.CommandGenerator.Containers;
using RoboCup.AtHome.CommandGenerator.ReplaceableTypes;

namespace RoboCup.AtHome.GPSRCmdGen
{
    public class Loader
    {
        public Logger Logger { get; set; }
        public Options Options { get; set; }

        public Loader() { }

        public Generator LoadGenerator()
        {
            var generator = new Generator(Options.Seed);
            try
            {
                generator.AllCategories = Load<CategoryContainer, Category>(Options.Objects, Resource.Objects);
                generator.AllNames = Load<NameContainer, PersonName>(Options.Names, Resource.Names);
                generator.AllRooms = Load<RoomContainer, Room>(Options.Locations, Resource.Locations);
                generator.AllGestures = Load<GestureContainer, Gesture>(Options.Gestures, Resource.Gestures);
                generator.AllQuestions = Load<QuestionsContainer, PredefinedQuestion>(Options.Questions, Resource.Questions);

                Logger.Info("Loading grammars...", ' ');
                generator.Grammar = LoadGrammars(Options.Grammars);
                if (Options.Verbose) Logger.Success("\tDone");
                return generator;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private static Grammar LoadDefaultGrammar()
        {
            var grammar = new Grammar();
            var loader = new Grammar.GrammarLoader();
            var g = loader.LoadText(Resource.CommonRules.Read().Split('\n'), null, false);
            foreach (var rule in g.ProductionRules)
            {
                grammar.AddRule(rule);
            }
            g = loader.LoadText(Resource.GPSRGrammar.Read().Split('\n'), null, false);
            foreach (var rule in g.ProductionRules)
            {
                grammar.AddRule(rule);
            }
            grammar.Tier = g.Tier;
            return g;
        }

        private static Grammar LoadGrammars(IEnumerable<string> Grammars)
        {
            if (!Grammars.Any()) return LoadDefaultGrammar();
            var grammar = new Grammar();
            var loader = new Grammar.GrammarLoader();
            foreach (var file in Grammars)
            {
                var g = loader.Load(file, false);
                foreach (var rule in g.ProductionRules)
                {
                    grammar.AddRule(rule);
                }
                if (g.Tier.CompareTo(grammar.Tier) > 0)
                {
                    grammar.Tier = g.Tier;
                }
            }
            return grammar;
        }

        private List<V> Load<P, V>(string path, Resource resource) where P : ILoadingContainer<V>
        {
            Logger.Info($"Loading {resource.Name.ToLower()}...", ' ');
            if (path != null)
            {
                P obj = LoadObject<P>(path);
                if (Options.Verbose) Logger.Success("\tDone");
                return obj.Results;
            }
            else
            {
                P obj = LoadXmlString<P>(resource.Read());
                if (Options.Verbose) Logger.Quiet($"\tDefault");
                return obj.Results;
            }
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
            using (StreamReader reader = new(filePath, Encoding.UTF8))
            {
                XmlSerializer serializer = new(typeof(T));
                serializer.UnknownAttribute += new XmlAttributeEventHandler(SerializeAttribute);
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
            using (MemoryStream ms = new(Encoding.UTF8.GetBytes(xml ?? string.Empty)))
            {
                XmlSerializer serializer = new(typeof(T));
                serializer.UnknownAttribute += new XmlAttributeEventHandler(SerializeAttribute);
                item = (T)serializer.Deserialize(ms);
                ms.Close();

            }
            return item;
        }

        private static void SerializeAttribute(object sender, XmlAttributeEventArgs e)
        {
            if (e.ObjectBeingDeserialized is IDescribable desc)
                desc.Properties.Add(e.Attr.Name, e.Attr.Value);
        }
    }
}