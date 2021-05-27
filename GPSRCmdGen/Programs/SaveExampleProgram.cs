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
            SaveGrammarFile("EGPSRGrammar", ".txt", Resources.GrammarHeader, Resources.FormatSpecification, Resources.EGPSRGrammar);
            SaveGrammarFile("GPSRGrammar", ".txt", Resources.GrammarHeader, Resources.FormatSpecification, Resources.GPSRGrammar);
            SaveGrammarFile("CommonRules", ".txt", Resources.GrammarHeader, Resources.FormatSpecification, Resources.CommonRules);
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

        private void SaveGrammarFile(string name, string ext, string header, string formatSpec, string content)
        {
            var path = GetPath(name + ext);
            if (!Overwrite(path))
                return;
            header = header.Replace("${GrammarName}", name.Capitalize());
            using StreamWriter writer = new(path); writer.WriteLine(header);
            writer.WriteLine(formatSpec);
            writer.WriteLine(content);
            writer.Close();
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

        /// <summary>
        /// Writes the default datafiles.
        /// </summary>
        private void WriteDatafiles()
        {
            string path = GetPath("Gestures.xml");
            if (Overwrite(path)) {
                File.WriteAllText(path, Resources.Gestures);
                Logger.Info($"Saved {path}");
            }
            path = GetPath("Locations.xml");
            if (Overwrite(path)) {
                File.WriteAllText(path, Resources.Locations);
                Logger.Info($"Saved {path}");
            }
            path = GetPath("Names.xml");
            if (Overwrite(path)) {
                File.WriteAllText(path, Resources.Names);
                Logger.Info($"Saved {path}");
            }
            path = GetPath("Objects.xml");
            if (Overwrite(path)) {
                File.WriteAllText(path, Resources.Objects);
                Logger.Info($"Saved {path}");
            }
            path = GetPath("Questions.xml");
            if (Overwrite(path)) {
                File.WriteAllText(path, Resources.Questions);
                Logger.Info($"Saved {path}");
            }
        }
    }
}