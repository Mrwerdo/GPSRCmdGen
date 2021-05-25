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
		/// Stores all known objects
		/// </summary>
		public ObjectManager AllObjects { get; set; }
		/// <summary>
		/// Stores all known questions
		/// </summary>
		public List<PredefinedQuestion> AllQuestions { get; set; }

		/// <summary>
		/// The grammar that is used for generating sentences.
		/// </summary>
		public Grammar Grammar { get; set; }

		public bool Quiet { get; set; }
		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of Generator
		/// </summary>
		public Generator (int seed)
		{
            Rnd = new Random(seed);
            AllObjects = ObjectManager.Instance;
            Quiet = false;
		}

        #endregion

        /// <summary>
        /// Randomly generates a task with the requested difficulty degree
        /// </summary>
        /// <param name="tier">The maximum difficulty degree allowed to produce the task</param>
        /// <returns></returns>
        public TaskNode GenerateTask()
		{
            TaskNode root = Grammar.GenerateSentence(Rnd);
            var locations = new List<Location>();
            locations.AddRange(AllRooms);
            locations.AddRange(AllRooms.SelectMany(t => t.Locations));
            locations.AddRange(AllObjects.Categories.Select(t => t.DefaultLocation).Where(t => !AllRooms.Any(r => r.Name == t.Name)));
            var evaluator = new WildcardEvaluator() 
			{
                Random = Rnd,
                Categories = AllObjects.Categories.ShuffleCopy(Rnd),
				Gestures = AllGestures.ShuffleCopy(Rnd),
                Locations = locations.ShuffleCopy(Rnd),
				Names = AllNames.ShuffleCopy(Rnd),
				Objects = AllObjects.Objects.ShuffleCopy(Rnd),
				Questions = AllQuestions.ShuffleCopy(Rnd)
			};
            evaluator.Update(root.Wildcards);
            return root;
        }
	}
}

