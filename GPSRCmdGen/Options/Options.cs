using System;
using System.Collections.Generic;
using CommandLine;

namespace RoboCup.AtHome.GPSRCmdGen
{
    public class Options {

        [Option('v', "verbose", Default = false)]
        public bool Verbose { get; set; }
        
        [Option('s', "seed", Default = 0)]
        public int SeedOption { get; set; }

        [Option('g', "grammar")]
        public IEnumerable<string> Grammars { get; set; }

        [Option('g', "gestures")]
        public string Gestures { get; set; }

        [Option('l', "locations")]
        public string Locations { get; set; }

        [Option('n', "names")]
        public string Names { get; set; }

        [Option('o', "objects")]
        public string Objects { get; set; }

        [Option('q', "questions")]
        public string Questions { get; set; }

        public int Seed {
            get {
                if (SeedOption == 0) {
                    return DateTime.Now.Millisecond;
                } else {
                    return SeedOption;
                }
            }
        }
    }
}