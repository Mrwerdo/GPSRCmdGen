using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.IO;

namespace RoboCup.AtHome.GPSRCmdGen {
    public class GPSRCmdGenResources {
        public static string GPSRGrammar {
            get {
                return CommandGenerator.Resources.Read("Resources.GPSRGrammar.txt", Assembly.GetExecutingAssembly());
            }
        }
        public static string CommonRules {
            get {
                return CommandGenerator.Resources.Read("Resources.CommonRules.txt", Assembly.GetExecutingAssembly());
            }
        }
    }
}