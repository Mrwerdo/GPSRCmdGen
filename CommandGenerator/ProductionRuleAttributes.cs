using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboCup.AtHome.CommandGenerator
{
	public class ProductionRuleAttributes
	{
        public string Name { get; set; }
        public string SpeakTo { get; set; }
        public string Location { get; set; }

        public override string ToString()
        {
            return $"{{ Name = \"{Name}\" }}";
        }
    }
}
