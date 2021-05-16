using System;
using System.Collections.Generic;
using CommandLine;

namespace RoboCup.AtHome.CommandGenerator
{
    public class Options {
        [Option('b', "bulk", Default = 0, HelpText = "Generate bulk sentences.")]
        public int Bulk { get; set; }

        [Option('o', "output")]
        public string Output { get; set; }

        [Option('v', "verbose", Default = true)]
        public bool Verbose { get; set; }
        
        [Option('s', "seed", Default = 0)]
        public int SeedOption { get; set; }

        [Value(0)]
        public IEnumerable<string> Files { get; set; }

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