using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.IO;

namespace RoboCup.AtHome.CommandGenerator {
    public class Resources {
        public static string Read(string filename, Assembly assembly = null) {
            if (assembly == null) {
                assembly = Assembly.GetExecutingAssembly();
            }
            var embeddedProvider = new EmbeddedFileProvider(assembly);
            var o = embeddedProvider.GetFileInfo(filename);
            using (var reader = o.CreateReadStream())
            {
                using (var r = new StreamReader(reader)) {
                    return r.ReadToEnd();
                }
            }
        }

        public static string FormatSpecification {
            get {
                return Read("FormatSpecification.txt");
            }
        }

        public static string GrammarHeader {
            get {
                return Read("GrammarHeader.txt");
            }
        }

        public static string Gestures {
            get {
                return Read("Gestures.xml");
            }
        }

        public static string Locations {
            get {
                return Read("Locations.xml");
            }
        }

        public static string Names {
            get {
                return Read("Names.xml");
            }
        }

        public static string Objects {
            get {
                return Read("Objects.xml");
            }
        }
        
        public static string Questions {
            get {
                return Read("Questions.xml");
            }
        }

    }
}