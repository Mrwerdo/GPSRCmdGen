using System;
using System.IO;
using RoboCup.AtHome.CommandGenerator;

namespace RoboCup.AtHome.GPSRCmdGen
{
    /// This is a program that saves examples, and **not** a program that is an example.
    class SaveExampleProgram
    {
        public SaveExampleOptions Options { get; set; }
        public Logger Logger { get; set; }

        private string SaveDirectory {
            get {
                return Options.Directory;
            }
        }

        /// <summary>
        /// Writes down a set of example data files with information gathered from the Factory
        /// </summary>
        public void Run()
        {
            CreateDirectoryIfNeeded();
            WriteDatafiles();
            SaveGrammarFile(Resource.EGPSRGrammar);
            SaveGrammarFile(Resource.GPSRGrammar);
            SaveGrammarFile(Resource.CommonRules);
        }

        /// <summary>
        /// Queries the user for file overwritten permission
        /// </summary>
        /// <param name="file">The name of the file which will be overwritten</param>
        /// <returns><c>true</c> if the user authorizes the overwrite, otherwise <c>false</c></returns>
        private bool Overwrite(string file)
        {
            if (Options.Overwrite != null) return Options.Overwrite.Value;
            FileInfo fi = new(file);
            if (!fi.Exists)
                return true;
            Console.Write("File {0} already exists. Overwrite? [yN]", fi.Name);
            string answer = Console.ReadLine().ToLower();
            if ((answer == "y") || (answer == "yes"))
            {
                fi.Delete();
                return true;
            }
            return false;
        }

        private void SaveGrammarFile(Resource file)
        {
            var path = GetPath(file.FileName);
            if (!Overwrite(path))
                return;
            var header = Resource.GrammarHeader.Read().Replace("${GrammarName}", file.Name.Capitalize());
            using (StreamWriter writer = new(path))
            {
                writer.WriteLine(header);
                writer.WriteLine(file.Read());
            }
            Logger.Info($"Saved {path}");
        }

        private void CreateDirectoryIfNeeded() {
            if (!Directory.Exists(SaveDirectory)) {
                Directory.CreateDirectory(SaveDirectory);
                Logger.Info($"Created directory: {SaveDirectory}");
            }
        }

        private string GetPath(string filename) {
            return SaveDirectory + "/" + filename;
        }

        private void WriteDatafiles()
        {
            var resources = new Resource[] {
                Resource.Gestures,
                Resource.Locations,
                Resource.Names,
                Resource.Objects,
                Resource.Questions,
                Resource.FormatSpecification
            };
            foreach (var resource in resources)
            {
                string path = GetPath(resource.FileName);
                if (Overwrite(path))
                {
                    File.WriteAllText(path, resource.Read());
                    Logger.Info($"Saved {path}");
                }
            }
        }
    }
}