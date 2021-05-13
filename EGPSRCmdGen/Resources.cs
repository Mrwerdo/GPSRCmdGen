using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.IO;

namespace RoboCup.AtHome.EGPSRCmdGen {
    public class EGPSRCmdGenResources {
        public static string EGPSRGrammar {
            get {
                return CommandGenerator.Resources.Read("Resources.EGPSRGrammar.txt", Assembly.GetExecutingAssembly());
            }
        }
        public static string CommonRules {
            get {
                return CommandGenerator.Resources.Read("Resources.CommonRules.txt", Assembly.GetExecutingAssembly());
            }
        }
    }
}