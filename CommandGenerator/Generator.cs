using System;
using System.Collections.Generic;
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
		/// Stores all known locations
		/// </summary>
		public LocationManager AllLocations { get; set; }
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
            AllLocations = LocationManager.Instance;
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
            var evaluator = new WildcardEvaluator() 
			{
                Random = Rnd,
                Categories = AllObjects.Categories.ShuffleCopy(Rnd),
				Gestures = AllGestures.ShuffleCopy(Rnd),
				Locations = new List<Location>(AllLocations).ShuffleCopy(Rnd),
				Names = AllNames.ShuffleCopy(Rnd),
				Objects = AllObjects.Objects.ShuffleCopy(Rnd),
				Questions = AllQuestions.ShuffleCopy(Rnd)
			};
            evaluator.Update(root.Wildcards);
            return root;
        }

        #region Load Methods

		/// <summary>
		/// Validates all default locations of categories are contained in the locations array.
		/// </summary>
		public void ValidateLocations()
		{
			foreach(Category c in AllObjects.Categories)
			{
				if (!AllLocations.Contains (c.DefaultLocation))
					AllLocations.Add (c.DefaultLocation);
			}
		}

		#endregion
	}
}

