using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Reflection;

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

            var createDataset = CreateDatasetCommand(options => {
                var logger = new Logger(options.Verbose);

                var loader = new Loader()
                {
                    Logger = logger,
                    Options = options
                };

                var generator = loader.LoadGenerator();
                if (generator == null) return;
                var program = new CreateDatasetProgram()
                {
                    Logger = logger,
                    Options = options,
                    Generator = generator
                };
                program.Run();
            });

            var root = new RootCommand
            {
                new Option<bool>("--verbose", getDefaultValue: () => false, description: "Show additional output."),
                new Option<bool>("--version", description: "Show version information")
            };

            // Maybe when dotnet's System.CommandLine library improves this won't need to be here.
            root.Handler = CommandHandler.Create<RootOptions>(options =>
            {
                if (options.Version && options.Verbose)
                {
                    Console.WriteLine($"{name} {GetAssemblyVersion()}");
                    Console.WriteLine(longName);
                }
                else if (options.Version)
                {
                    Console.WriteLine($"{name} {GetAssemblyVersion()}");
                } else {
                    var helpBuilder = new HelpBuilder(new System.CommandLine.IO.SystemConsole(), Console.BufferWidth);
                    helpBuilder.Write(root);
                }
            });

            root.Add(interactive);
            root.Add(bulk);
            root.Add(save);
            root.Add(createDataset);

            return root.Invoke(args);
        }

        private static Command InteractiveCommand(Action<InteractiveOptions> handler) {
            var interactive = new Command("interactive")
            {
                new Option<bool>("--verbose", getDefaultValue: () => true),
                new Option<int>("--seed", getDefaultValue: () => DateTime.Now.Millisecond),
                new Option<List<string>>("--grammars"),
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
                new Option<List<string>>("--grammars"),
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

        private static Command CreateDatasetCommand(Action<CreateDatasetOptions> handler) {
            var createDataset = new Command("create-dataset") {
                new Option<string>("--path", getDefaultValue: () => "dataset.csv"),
                new Option<bool?>("--overwrite", getDefaultValue: () => null),
                new Option<int?>("--limit", getDefaultValue: () => null),
                new Option<int>("--seed", getDefaultValue: () => DateTime.Now.Millisecond),
                new Option<List<string>>("--grammars"),
                new Option<string>("--gestures"),
                new Option<string>("--locations"),
                new Option<string>("--names"),
                new Option<string>("--objects"),
                new Option<string>("--questions")
            };
            createDataset.Handler = CommandHandler.Create(handler);
            return createDataset;
        }

        private static string GetAssemblyVersion()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return assemblyVersionAttribute?.InformationalVersion ?? assembly.GetName().Version.ToString();
        }
    }
}
