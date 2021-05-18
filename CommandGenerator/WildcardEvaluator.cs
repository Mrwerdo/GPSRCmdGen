using System;
using System.Collections.Generic;
using System.Linq;
using RoboCup.AtHome.CommandGenerator.ReplaceableTypes;
using Object = RoboCup.AtHome.CommandGenerator.ReplaceableTypes.Object;

namespace RoboCup.AtHome.CommandGenerator
{
    public class WildcardEvaluator 
    {
        public Random Random;

		/// <summary>
		/// List of available categories
		/// </summary>
		public List<Category> Categories; 
		
		/// <summary>
		/// List of available gestures
		/// </summary>
		public List<Gesture> Gestures;
		
		/// <summary>
		/// List of available locations
		/// </summary>
		public List<Location> Locations;
		
		/// <summary>
		/// List of available names
		/// </summary>
		public List<PersonName> Names;
		
		/// <summary>
		/// List of available objects
		/// </summary>
		public List<Object> Objects;

		/// <summary>
		/// List of available objects
		/// </summary>
		public List<PredefinedQuestion> Questions;

        public WildcardEvaluator() { }

		public void Update(List<TextWildcard> wildcards) 
		{
            var wildcardMap = wildcards.GroupBy(t => t.Keycode).ToDictionary(t => t.Key, e => new Wildcard(e.ToList()));
			foreach (var pair in wildcardMap) {
				if (pair.Value.Replacement != null) continue;
				FindReplacement(pair.Value);
			}

			foreach (var pair in wildcardMap) {
				if (pair.Value.Replacement != null) continue;
				if (pair.Value.Name != "pron") continue;
				EvaluatePronoun(pair.Value);
			}
		}

		/// <summary>
		/// Evaluates a Wildcards and assignd a replacement to it.
		/// </summary>
		private void FindReplacement(Wildcard w)
		{
			switch (w.Name)
			{
				case "category":
					EvaluateCategory(w);
					break;

				case "gesture":
					EvaluateGesture(w);
					break;

				case "name":
				case "female":
				case "male":
					EvaluateName (w);
					break;

				case "beacon":
				case "placement":
				case "location":
				case "room":
					EvaluateLocation (w);
					break;

				case "object":
				case "aobject":
				case "kobject": 
				case "sobject":
					EvaluateObject (w);
					break;

				case "question":
					EvaluateQuestion(w);
					break;

				case "pron":
					break;

				case "void":
				default:
					w.Replacement = new HiddenTaskElement ();
					w.Obfuscated = new HiddenTaskElement ();
					break;

			}
		}

        private static void EvaluatePronoun(Wildcard w)
        {
			foreach (var child in w.TextWildcards) {
                if (child.Parent != null) 
                {
                    if (child.Parent.AggregateWildcard.TryGetTarget(out Wildcard p)) {
						var r = Pronoun.FromWildcard(w, p);
						w.Replacement = new NamedTaskElement(r);
					}
				}
			}
            // Wildcard prev = null;
            // string keycode;
            // string keyword;
            // for (int i = currentWildcardIx - 1; i >= 0; --i) {
            // 	keycode = textWildcards [i].Keycode;
            // 	keyword = wildcards [keycode].Keyword;
            // 	if ((keyword != null) && keyword.IsAnyOf ("name", "male", "female")) {
            // 		// prev = textWildcards [i];
            // 		prev = wildcards [keycode];
            // 		break;
            // 	}
            // }
            // for (int i = currentWildcardIx - 1; (prev == null) && (i >= 0); --i) {
            // 	keycode = textWildcards [i].Keycode;
            // 	keyword = wildcards [keycode].Keyword;
            // 	if ((keyword != null) && keyword.IsAnyOf ("void", "pron"))
            // 		continue;
            // 	// prev = textWildcards [i];
            // 	prev = wildcards [keycode];
            // 	break;
            // }
            // w.Replacement = new NamedTaskElement(Pronoun.FromWildcard(w, prev));
			
		}

		/// <summary>
		/// Evaluates a <c>location</c> wildcard, asigning a replacement.
		/// </summary>
		/// <param name="w">The wilcard to find a replacement for</param>
		public void EvaluateCategory(Wildcard w)
		{
			w.Replacement = !String.IsNullOrEmpty(w.Where) ? Categories.PopFirst(w.Where) : Categories.PopLast();
			w.Obfuscated = new Obfuscator("objects");
		}

		/// <summary>
		/// Evaluates a <c>location</c> wildcard, asigning a replacement.
		/// </summary>
		/// <param name="w">The wilcard to find a replacement for</param>
		public void EvaluateGesture(Wildcard w)
		{
			w.Replacement = !String.IsNullOrEmpty(w.Where) ? Gestures.PopFirst(w.Where) : Gestures.PopLast();
		}

		/// <summary>
		/// Evaluates a <c>location</c> wildcard, asigning a replacement.
		/// </summary>
		/// <param name="w">The wilcard to find a replacement for</param>
		public void EvaluateLocation(Wildcard w)
		{
			if ((w.Name == "location") && String.IsNullOrEmpty(w.Where))
				w.Keyword = w.Type ?? Random.RandomPick("beacon", "room", "placement");
			switch (w.Keyword)
			{
				case "beacon":
					w.Replacement = Locations.PopFirst(l => l.IsBeacon, w.Where);
					w.Obfuscated = ((SpecificLocation)w.Replacement).Room;
					break;

				case "room":
					w.Replacement = Locations.PopFirst(l => l is Room, w.Where);
					w.Obfuscated = new Obfuscator("apartment");
					break;

				case "placement":
					w.Replacement = Locations.PopFirst(l => l.IsPlacement, w.Where);
					w.Obfuscated = ((SpecificLocation)w.Replacement).Room;
					break;

				default:
					w.Replacement = !String.IsNullOrEmpty(w.Where) ? Locations.PopFirst(w.Where) : Locations.PopLast();
					w.Obfuscated = new Obfuscator("somewhere");
					break;
			}
		}

		/// <summary>
		/// Evaluates a <c>name</c> wildcard, asigning a replacement.
		/// </summary>
		/// <param name="w">The wilcard to find a replacement for</param>
		public void EvaluateName(Wildcard w)
		{
			if ((w.Name == "name") && String.IsNullOrEmpty(w.Where))
				w.Keyword = w.Type ?? Random.RandomPick ("male", "female");
            w.Replacement = w.Keyword switch
            {
                "male" => Names.PopFirst(n => n.Gender == Gender.Male, w.Where),
                "female" => Names.PopFirst(n => n.Gender == Gender.Female, w.Where),
                _ => !String.IsNullOrEmpty(w.Where) ? Names.PopFirst(w.Where) : Names.PopLast(),
            };
            w.Obfuscated = new Obfuscator("a person");
		}

		/// <summary>
		/// Evaluates an <c>object</c> wildcard, asigning a replacement.
		/// </summary>
		/// <param name="w">The wilcard to find a replacement for</param>
		public void EvaluateObject(Wildcard w)
		{
			if ((w.Name == "object") && String.IsNullOrEmpty(w.Where))
				w.Keyword = (w.Type == null) ? Random.RandomPick ("kobject", "aobject") : String.Format("{0}object", w.Type[0]);

            w.Replacement = w.Keyword switch
            {
                "aobject" => Objects.PopFirst(o => o.Type == ObjectType.Alike, w.Where),
                "kobject" => Objects.PopFirst(o => o.Type == ObjectType.Known, w.Where),
                "sobject" => Objects.PopFirst(o => o.Type == ObjectType.Special, w.Where),
                _ => !String.IsNullOrEmpty(w.Where) ? Objects.PopFirst(w.Where) : Objects.PopLast(),
            };
            w.Obfuscated = ((Object)w.Replacement).Category;
		}

		/// <summary>
		/// Evaluates a <c>location</c> wildcard, asigning a replacement.
		/// </summary>
		/// <param name="w">The wilcard to find a replacement for</param>
		public void EvaluateQuestion(Wildcard w)
		{
			w.Replacement = !String.IsNullOrEmpty(w.Where) ? Questions.PopFirst(w.Where) : Questions.PopLast ();
			w.Obfuscated = new Obfuscator("question");
		}
    }
}