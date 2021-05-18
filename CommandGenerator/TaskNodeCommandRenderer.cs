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
            if (root.IsLiteral) {
                return null;
            }
            if (root.Attributes == null) {
                foreach (var child in root.Children) {
                    var result = RenderCommand(child);
                    if (result != null) {
                        return result;
                    }
                }
                return null;
            }
            switch (root.Attributes.Name)
            {
                case "CountPeople":
                    var people = GetSymbolValue(root, "$peoplege");
                    var room = GetSymbolValue(root, "$room");
                    return $"CountPeople(type: {people}, location: {room})";
                default:
                    foreach (var child in root.Children)
                    {
                        var result = RenderCommand(child);
                        if (result != null) {
                            return result;
                        }
                    }
                    return null;
            }
        }

        private static string GetSymbolValue(TaskNode node, string nonTerminal) 
        {
            if (node.IsNonTerminal && node.Value == nonTerminal) {
                return node.Render();
            } else {
                foreach (var child in node.Children) {
                    var result = GetSymbolValue(child, nonTerminal);
                    if (result != null) {
                        return result;
                    }
                }
                return null;
            }
        }
    }
}