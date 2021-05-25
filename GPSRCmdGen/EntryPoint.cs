using System;
using System.IO;
using CommandLine;
using RoboCup.AtHome.CommandGenerator;

namespace RoboCup.AtHome.GPSRCmdGen
{
	/// <summary>
	/// Contains the program control logic
	/// </summary>
	public class EntryPoint
	{

		/// <summary>
		/// Checks if at least one of the required files are present. If not, initializes the
		/// directory with example files
		/// </summary>
		public static void InitializePath()
		{
			int xmlFilesCnt = Directory.GetFiles (Loader.ExePath, "*.xml", SearchOption.TopDirectoryOnly).Length;
			if ((xmlFilesCnt < 4) || !Directory.Exists (Loader.GetPath("gpsr_grammars")))
				ExampleFilesGenerator.GenerateExampleFiles ();
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main (string[] args)
		{
			InitializePath();
			Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options =>
            {
                var program = new Program(options)
				{
					Name = "GPSRCmdGen",
					LongName = "GPSR Command Generator 2021 modified by NUbots"
				};
				program.Setup();
				if (options.Bulk > 0) {
					program.RunBulk();
				} else {
					program.Run();
				}
			});
		}
	}
}
