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

		#region Static Methods

		/// <summary>
		/// Writes the provided exception's Message to the console in RED text
		/// </summary>
		/// <param name="ex">Exception to be written.</param>
		public static void Err(Exception ex){
			Err (null, ex);
		}

		public static void Err(string format, params object[] args)
		{
			Err (String.Format (format, args));
		}

		/// <summary>
		/// Writes the provided message string to the console in RED text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		public static void Err(string message)
		{
			Err(message, (Exception)null);
		}

		/// <summary>
		/// Writes the provided message string and exception's Message to the console in RED text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		/// <param name="ex">Exception to be written.</param>
		public static void Err(string message, Exception ex)
		{
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			if(!String.IsNullOrEmpty(message))
				Console.WriteLine (message);
			if(ex != null)
				Console.WriteLine ("Exception {0}:", ex.Message);
			Console.ForegroundColor = pc;
		}

		/// <summary>
		/// Writes the provided exception's Message to the console in YELLOW text
		/// </summary>
		/// <param name="ex">Exception to be written.</param>
		public static void Warn(Exception ex)
		{
			Err (null, ex);
		}

		public static void Warn(string format, params object[] args)
		{
			Err (String.Format (format, args));
		}

		/// <summary>
		/// Writes the provided message string to the console in YELLOW text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		public static void Warn(string message)
		{
			Err(message, (Exception)null);
		}

		/// <summary>
		/// Writes the provided message string and exception's Message to the console in YELLOW text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		/// <param name="ex">Exception to be written.</param>
		public static void Warn(string message, Exception ex)
		{
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			if(!String.IsNullOrEmpty(message))
				Console.WriteLine (message);
			if(ex != null)
				Console.WriteLine ("Exception {0}:", ex.Message);
			Console.ForegroundColor = pc;
		}

		/// <summary>
		/// Writes the provided message string to the console in GREEN text
		/// </summary>
		/// <param name="message">The message to be written.</param>
		public static void Green(string message)
		{
			ConsoleColor pc = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine (message);
			Console.ForegroundColor = pc;
		}

		#endregion
	}
}

