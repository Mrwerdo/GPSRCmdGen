using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public bool ValidatePaths() {
            if (Options.Grammars.Any(t => PathDoesNotExist("Grammar", t))) return false;
            // if (PathDoesNotExist("Objects", Options.Objects)) return false;
            // if (PathDoesNotExist("Names", Options.Names)) return false;
            // if (PathDoesNotExist("Locations", Options.Locations)) return false;
            // if (PathDoesNotExist("Gestures", Options.Gestures)) return false;
            // if (PathDoesNotExist("Questions", Options.Questions)) return false;
            return true;
		}

        private bool PathDoesNotExist(string name, string path) {
            if (!File.Exists(path)) {
                Logger.Error($"{name} path could not be found: {path}");
                return true;
            }
            return false;
        }

        public Generator LoadGenerator()
        {
            var generator = new Generator(Options.Seed);
            var container = Load<CategoryContainer, Category>("Objects", Options.Objects ?? "Objects.xml", Resources.Objects);
            if (container == null) throw new Exception("No objects found");
            foreach (var c in container)
            {
                generator.AllObjects.Add(c);
            }

            generator.AllNames = Load<NameContainer, PersonName>("Names", Options.Names ?? "Names.xml", Resources.Names);
            generator.AllRooms = Load<RoomContainer, Room>("Locations", Options.Locations ?? "Locations.xml", Resources.Locations);
            generator.AllGestures = Load<GestureContainer, Gesture>("Gestures", Options.Gestures ?? "Gestures.xml", Resources.Gestures);
            generator.AllQuestions = Load<QuestionsContainer, PredefinedQuestion>("Questions", Options.Questions ?? "Questions.xml", Resources.Questions);

            Logger.Info("Loading grammars...", ' ');
            generator.Grammar = LoadGrammars(Options.Grammars);
            if (Options.Verbose) Logger.Success("\tDone");
            return generator;
        }

        private static Grammar LoadGrammars(IEnumerable<string> Grammars)
        {
            var grammar = new Grammar();
            var loader = new Grammar.GrammarLoader();
            if (!Grammars.Any())
            {
                var g = loader.LoadText(Resources.CommonRules.Split('\n'), null, false);
				foreach (var rule in g.ProductionRules) 
				{
					grammar.AddRule(rule);
				}
                g = loader.LoadText(Resources.GPSRGrammar.Split('\n'), null, false);
				foreach (var rule in g.ProductionRules) 
				{
					grammar.AddRule(rule);
				}
				grammar.Tier = g.Tier;
			}
            foreach (var file in Grammars)
            {
                var g = loader.Load(file, false);
                foreach (var rule in g.ProductionRules)
                {
                    grammar.AddRule(rule);
                }
				if (g.Tier.CompareTo(grammar.Tier) > 0) {
					grammar.Tier = g.Tier;
				}
            }
            return grammar;
        }

        private List<V> Load<P, V>(string name, string path, string backup) where P : ILoadingContainer<V>
        {
            try
            {
                Logger.Info($"Loading {name.ToLower()}...", ' ');
                P obj = CommandGenerator.Loader.LoadObject<P>(CommandGenerator.Loader.GetPath(path));
                if (Options.Verbose) Logger.Success("\tDone");
                return obj.Results;
            }
            catch
            {
                P obj = CommandGenerator.Loader.LoadXmlString<P>(backup);
                Logger.Warning($"Default {name} loaded");
                Logger.Info("");
                return obj.Results;
            }
        }
    }
}