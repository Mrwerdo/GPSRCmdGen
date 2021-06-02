using System.Collections.Generic;

namespace RoboCup.AtHome.GPSRCmdGen
{
    public class RootOptions
    {
        public bool Version { get; set; }
        public bool Verbose { get; set; }
    }

    public class Options : RootOptions
    {
        public int Seed { get; set; }
        public List<string> Grammars { get; set; }
        public string Gestures { get; set; }
        public string Locations { get; set; }
        public string Names { get; set; }
        public string Objects { get; set; }
        public string Questions { get; set; }
    }

    public class InteractiveOptions : Options { }

    public class BulkOptions : Options
    {
        public string Output { get; set; }
        public int Count { get; set; }
    }

    public class SaveExampleOptions
    {
        public string Directory { get; set; }
        public bool Verbose { get; set; }
        public bool? Overwrite { get; set; }
    }
}