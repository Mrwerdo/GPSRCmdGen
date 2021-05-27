using CommandLine;

namespace RoboCup.AtHome.GPSRCmdGen
{
    [Verb("save-example")]
    public class SaveExampleOptions
    {
        [Option('d', "directory", Default = "./Example")]
        public string Directory { get; set; }

        [Option('v', "verbose", Default = true)]
        public bool Verbose { get; set; }

        [Option('o', "overwrite", Default = null)]
        public bool? Overwrite { get; set; }
    }
}