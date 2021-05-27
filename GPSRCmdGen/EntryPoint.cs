using CommandLine;

namespace RoboCup.AtHome.GPSRCmdGen
{
    /// <summary>
    /// Contains the program control logic
    /// </summary>
    public class EntryPoint
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            var name = "GPSRCmdGen";
            var longName = "GPSR Command Generator 2021 modified by NUbots";
            Parser.Default.ParseArguments<InteractiveOptions, BulkOptions, SaveExampleOptions>(args)
            .WithParsed<InteractiveOptions>(options =>
            {
                options.Verbose = true;
                var logger = new Logger(options.Verbose);
                logger.ProgramInfo(name, longName);

                var loader = new Loader()
                {
                    Logger = logger,
                    Options = options
                };

                if (!loader.ValidatePaths()) return;

                var generator = loader.LoadGenerator();
                var program = new InteractiveProgram()
                {
                    Logger = logger,
                    Options = options,
                    Generator = generator,
                    Name = name
                };
                program.Run();
            })
            .WithParsed<BulkOptions>(options =>
            {
                var logger = new Logger(options.Verbose);
                logger.ProgramInfo(name, longName);

                var loader = new Loader()
                {
                    Logger = logger,
                    Options = options
                };

				if (!loader.ValidatePaths()) return;
				
                var generator = loader.LoadGenerator();
                var program = new BulkProgram()
                {
                    Logger = logger,
                    Options = options,
                    Generator = generator
                };
                program.Run();
            })
			.WithParsed<SaveExampleOptions>(options => 
			{
                var logger = new Logger(options.Verbose);
				var program = new SaveExampleProgram() {
					Logger = logger,
					Options = options
				};
				program.Run();
			});
        }
    }
}
