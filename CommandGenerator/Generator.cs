using System;
using System.Collections.Generic;
using System.Linq;
using RoboCup.AtHome.CommandGenerator.ReplaceableTypes;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Generates Random Sentences for the GPSR test
	/// </summary>
	public class Generator
	{
		#region Properties

		/// <summary>
		/// Random numbers generator
		/// </summary>
		public Random Rnd { get; set; }
		/// <summary>
		/// Stores all known gestures
		/// </summary>
		public List<Gesture> AllGestures { get; set; }
		/// <summary>
		/// Stores all known rooms.
		/// </summary>
		public List<Room> AllRooms { get; set; }
		/// <summary>
		/// Stores all known names
		/// </summary>
		public List<PersonName> AllNames { get; set; }
		/// <summary>
		/// Stores all known categories and objects.
		/// </summary>
		public List<Category> AllCategories { get; set; }
		/// <summary>
		/// Stores all known questions
		/// </summary>
		public List<PredefinedQuestion> AllQuestions { get; set; }

		/// <summary>
		/// The grammar that is used for generating sentences.
		/// </summary>
		public Grammar Grammar { get; set; }

		public bool Quiet { get; set; }

		public List<Location> AllLocations {
			get {
				var locations = new List<Location>();
				locations.AddRange(AllRooms);
				locations.AddRange(AllRooms.SelectMany(t => t.Locations));
				locations.AddRange(AllCategories.Select(t => t.DefaultLocation).Where(t => !AllRooms.Any(r => r.Name == t.Name)));
                return locations;
			}
		}
		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of Generator
		/// </summary>
		public Generator (int seed)
		{
            Rnd = new Random(seed);
            Quiet = false;
		}

        #endregion

		private void EvaluateWildcards(TaskNode root)
		{
            var evaluator = new WildcardEvaluator() 
			{
                Random = Rnd,
                Categories = AllCategories.ShuffleCopy(Rnd),
				Gestures = AllGestures.ShuffleCopy(Rnd),
                Locations = AllLocations.ShuffleCopy(Rnd),
				Names = AllNames.ShuffleCopy(Rnd),
				Objects = AllCategories.SelectMany(c => c.Objects).ToList().ShuffleCopy(Rnd),
				Questions = AllQuestions.ShuffleCopy(Rnd)
			};
            evaluator.Update(root);
		}

        public TaskNode GenerateTask(string nonTerminal = "$Main")
		{
            TaskNode root = Grammar.GenerateSentence(nonTerminal, Rnd);
            EvaluateWildcards(root);
            return root;
        }

		public TaskNode GuidedGenerateTask(List<(string token, int? index)> tokens) 
		{
            TaskNode root = Grammar.GenerateSentence("$" + tokens.First().token, Rnd, tokens, stackCounter: 0);
			EvaluateWildcards(root);
            return root;
		}
	}
}

