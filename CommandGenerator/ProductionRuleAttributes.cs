using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace RoboCup.AtHome.CommandGenerator
{
	public class ProductionRuleAttributes
	{
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{{ Name = \"{Name}\" }}";
        }
    }
}
