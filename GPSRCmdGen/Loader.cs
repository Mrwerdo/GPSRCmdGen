using System;
using System.Collections.Generic;
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
                P obj = CommandGenerator.Loader.LoadObject<P>(path);
                if (Options.Verbose) Logger.Success("\tDone");
                return obj.Results;
            }
            else
            {
                P obj = CommandGenerator.Loader.LoadXmlString<P>(resource.Read());
                if (Options.Verbose) Logger.Quiet($"\tDefault");
                return obj.Results;
            }
        }
    }
}