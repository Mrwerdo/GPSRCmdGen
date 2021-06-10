using System.Linq;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Takes a TaskNode tree and renders a command.
	/// </summary>
	public static class TaskNodeCommandRenderer
	{

        public static string RenderCommand(this TaskNode root)
        {
            if (!root.IsNonTerminal) {
                return null;
            }
            if (root.Attributes == null) {
                foreach (var child in root.Children) {
                    var result = RenderCommand(child);
                    if (result != null) {
                        return result;
                    }
                }
                return null;
            }
            string door;
            switch (root.Attributes.Name)
            {
                case "Close":
                    door = root.Replacement.Index switch {
                        0 => "entrance",
                        1 => "exit",
                        2 => "corridor",
                        _ => "unknown",
                    };
					return $"Close(door: {door})";
                case "Open":
                    door = root.Replacement.Index switch {
                        0 => "entrance",
                        1 => "exit",
                        2 => "corridor",
                        _ => "unknown",
                    };
					return $"Open(door: {door})";
                case "BringIt":
					var subject = root.FindWildcard("name")?.ReplacementValue ?? "me";
                    return $"BringIt(to: {subject})";
                case "CountPeople":
                    var people = root.Find("$peoplege");
                    var room = root.FindWildcard("room").ReplacementValue;
                    var gesture = people.FindWildcard("gesture").ReplacementValue;
                    return $"CountPeople(type: {people.DeepestDecendent().Value}, gesture: {gesture}, location: {room})";
                case "DescribePerson":
                    var posture = root.Find("$posture").DeepestDecendent().Value;
                    var beacon = root.FindWildcard("beacon").ReplacementValue;
                    var descper = root.Find("$descper");
                    var speakTo = descper.Attributes.SpeakTo == null ? "speaker" : $"person at {descper.Attributes.SpeakTo}";
                    var location = descper.Attributes.Location ?? beacon;
                    return $"DescribePerson(posture: {posture}, location: {location}, speakingTo: {speakTo})";
                default:
                    foreach (var child in root.Children)
                    {
                        var result = RenderCommand(child);
                        if (result != null) {
                            return result;
                        }
                    }
                    return null;
            }
        }

        public static TaskNode DeepestDecendent(this TaskNode node) {
            var child = node;
            while (true) { 
                var c = child.Children.FirstOrDefault();
                if (c == null) break;
                child = c;
            }
            return child;
        }

        public static TextWildcard Find(this TextWildcard wildcard, string name)
        {
            if (wildcard.Name == name) return wildcard;
            foreach (var child in wildcard.Children)
            {
                var result = child.Find(name);
                if (result != null) return result;
            }
            return null;
        }

        public static TextWildcard FindWildcard(this TaskNode node, string name) 
        {
            var tw = node.TextWildcard?.Find(name);
            if (tw != null) return tw;
            foreach (var child in node.Children)
            {
                var result = FindWildcard(child, name);
                if (result != null) return result;
            }
            return null;
        }

        public static TaskNode Find(this TaskNode node, string nonTerminal) 
        {
            if (node.IsNonTerminal && node.Value == nonTerminal) return node;
            foreach (var child in node.Children)
            {
                var result = Find(child, nonTerminal);
                if (result != null) return result;
            }
            return null;
        }
    }
}