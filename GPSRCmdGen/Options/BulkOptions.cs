using CommandLine;

namespace RoboCup.AtHome.GPSRCmdGen
{
    [Verb("bulk")]
    public class BulkOptions: Options {
        [Option('o', "output", HelpText = "A output path to a file where the results will be stored. If not provided results are printed to stdout.")]
        public string Output { get; set; }
        [Option('t', "total", Default = 20, HelpText = "The total number of sentences to generate.")]
        public int Bulk { get; set; }
    }
}