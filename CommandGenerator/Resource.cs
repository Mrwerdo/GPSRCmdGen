using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace RoboCup.AtHome.CommandGenerator {
	public class Resource
    {
        public static Resource FormatSpecification { get; } = new Resource("FormatSpecification.txt");
        public static Resource GrammarHeader { get; } = new Resource("GrammarHeader.txt");
        public static Resource Gestures { get; } = new Resource("Gestures.xml");
        public static Resource Locations { get; } = new Resource("Locations.xml");
        public static Resource Names { get; } = new Resource("Names.xml");
        public static Resource Objects { get; } = new Resource("Objects.xml");
        public static Resource Questions { get; } = new Resource("Questions.xml");
        public static Resource EGPSRGrammar { get; } = new Resource("EGPSRGrammar.txt");
        public static Resource GPSRGrammar { get; } = new Resource("GPSRGrammar.txt");
        public static Resource CommonRules { get; } = new Resource("CommonRules.txt");

        public string FileName { get; private set; }
        public string Name => FileName.Split(".")[0];

        public Resource(string filename)
        {
            FileName = filename;
        }

        public string Read()
        {
            return Read(FileName);
        }

        private static string Read(string filename, Assembly assembly = null) {
            if (assembly == null) {
                assembly = Assembly.GetExecutingAssembly();
            }
            var embeddedProvider = new EmbeddedFileProvider(assembly);
            var o = embeddedProvider.GetFileInfo(filename);
            using var reader = o.CreateReadStream();
            using var r = new StreamReader(reader);
            return r.ReadToEnd();
        }
    }
}