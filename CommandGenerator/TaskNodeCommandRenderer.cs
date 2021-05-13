using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboCup.AtHome.CommandGenerator
{
	/// <summary>
	/// Takes a TaskNode tree and renders a command.
	/// </summary>
	public static class TaskNodeCommandRenderer
	{

        public static string RenderCommand(this TaskNode root)
        {
            
            // Detect the node, then apply it's corresponding function.
            return "";
        }

        private static string Open(TaskNode node) {
            return "";
        }


        // If true then the node does not participate in rendering, but it's children do.
        private static bool IsHidden(TaskNode node) {
            return "$Main".Equals(node.Term);
        }

        private static string Name(TaskNode node) {
            if ("open the".Equals(node.StringValue)) {
                return "Open";
            } else if ("close the".Equals(node.StringValue)) {
                return "Close";
            } else {
                return "Unknown";
            }
        }
    }
}