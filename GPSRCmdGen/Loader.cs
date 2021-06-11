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

        public Generator LoadGenerator()
        {
            var generator = new Generator(Options.Seed);
            try
            {
                generator.AllCategories = Load<CategoryContainer, Category>("Objects", Options.Objects, Resources.Objects);
                generator.AllNames = Load<NameContainer, PersonName>("Names", Options.Names, Resources.Names);
                generator.AllRooms = Load<RoomContainer, Room>("Locations", Options.Locations, Resources.Locations);
                generator.AllGestures = Load<GestureContainer, Gesture>("Gestures", Options.Gestures, Resources.Gestures);
                generator.AllQuestions = Load<QuestionsContainer, PredefinedQuestion>("Questions", Options.Questions, Resources.Questions);

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
            Logger.Info($"Loading {name.ToLower()}...", ' ');
            if (path != null)
            {
                P obj = CommandGenerator.Loader.LoadObject<P>(path);
                if (Options.Verbose) Logger.Success("\tDone");
                return obj.Results;
            }
            else
            {
                P obj = CommandGenerator.Loader.LoadXmlString<P>(backup);
                if (Options.Verbose) Logger.Quiet($"\tDefault");
                return obj.Results;
            }
        }
    }
}