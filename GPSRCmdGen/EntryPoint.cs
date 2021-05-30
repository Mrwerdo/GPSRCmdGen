using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;

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
        public static int Main(string[] args)
        {
            var name = "GPSRCmdGen";
            var longName = "GPSR Command Generator 2021 modified by NUbots";

            var interactive = InteractiveCommand(options =>
            {
                var logger = new Logger(options.Verbose);
                logger.ProgramInfo(name, longName);

                var loader = new Loader()
                {
                    Logger = logger,
                    Options = options
                };

                var generator = loader.LoadGenerator();
                if (generator == null) return;
                var program = new InteractiveProgram()
                {
                    Logger = logger,
                    Options = options,
                    Generator = generator,
                    Name = name
                };
                program.Run();
            });

            var bulk = BulkCommand(options =>
            {
                var logger = new Logger(options.Verbose);
                logger.ProgramInfo(name, longName);

                var loader = new Loader()
                {
                    Logger = logger,
                    Options = options
                };

                var generator = loader.LoadGenerator();
                if (generator == null) return;
                var program = new BulkProgram()
                {
                    Logger = logger,
                    Options = options,
                    Generator = generator
                };
                program.Run();
            });

            var save = SaveCommand(options =>
            {
                var logger = new Logger(options.Verbose);
                var program = new SaveExampleProgram()
                {
                    Logger = logger,
                    Options = options
                };
                program.Run();
            });

            var root = RootCommand(options =>
            {
                Console.WriteLine("Help or version.");
                Console.WriteLine($"Verbose: {options.Verbose}");
            });
            root.Add(interactive);
            root.Add(bulk);
            root.Add(save);
            return root.Invoke(args);
        }

        private static Command RootCommand(Action<RootOptions> handler)
        {
            var root = new RootCommand
            {
                new Option<bool>("--verbose", getDefaultValue: () => false),
            };
            root.Handler = CommandHandler.Create(handler);
            return root;
        }

        private static Command InteractiveCommand(Action<InteractiveOptions> handler) {
            var interactive = new Command("interactive")
            {
                new Option<bool>("--verbose", getDefaultValue: () => true),
                new Option<int>("--seed", getDefaultValue: () => DateTime.Now.Millisecond),
                new Option<List<string>>("--grammar"),
                new Option<string>("--gestures"),
                new Option<string>("--locations"),
                new Option<string>("--names"),
                new Option<string>("--objects"),
                new Option<string>("--questions")
            };
            interactive.Handler = CommandHandler.Create(handler);
            return interactive;
        }

        private static Command BulkCommand(Action<BulkOptions> handler) {
            var bulk = new Command("bulk")
            {
                new Option<string>("--output"),
                new Option<int>("--count", getDefaultValue: () => 20),
                new Option<int>("--seed", getDefaultValue: () => DateTime.Now.Millisecond),
                new Option<List<string>>("--grammar"),
                new Option<string>("--gestures"),
                new Option<string>("--locations"),
                new Option<string>("--names"),
                new Option<string>("--objects"),
                new Option<string>("--questions")
            };
            bulk.Handler = CommandHandler.Create(handler);
            return bulk;
        }

        private static Command SaveCommand(Action<SaveExampleOptions> handler) {
            var save = new Command("save-example") {
                new Option<string>("--directory", getDefaultValue: () => "./Example"),
                new Option<bool?>("--overwrite", getDefaultValue: () => null)
            };
            save.Handler = CommandHandler.Create(handler);
            return save;
        }
    }
}
