using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace RoboCup.AtHome.CommandGenerator
{
    /// <summary>
    /// Takes a TaskNode tree and renders a command.
    /// </summary>
    public static class TaskNodeCommandRenderer
    {
        public static string? RenderCommand(this TaskNode root)
        {
            if (root.AlternativeExpression is not null) {
                string[] parts = Scanner.SplitRule(root.AlternativeExpression.Trim(), "$%", "$:");
                var result = "";
                foreach (string part in parts) {
                    if (part.StartsWith("$")) {
                        result += root.ParseNodePath(part).Render();
                    } else if (part.StartsWith("%")) {
                        var node = root.ParseNodePath("$" + part.TrimStart('%'));
                        result += RenderCommand(node) ?? throw new Exception($"Failed to render path: {part}");
                    } else if (part.StartsWith("{")) {
                        var p = part.TrimStart('{').TrimEnd('}').Split(' ');
                        if (p.Length == 2) {
                            var name = p[0];
                            var id = int.Parse(p[1]);
                            result += root.FindWildcard(name, id)?.ReplacementValue ?? throw new Exception($"Could not find wildcard named: {name}, with id: {id}");
                        } else if (p.Length == 1) {
                            result += root.FindWildcard(p[0])?.ReplacementValue ?? throw new Exception($"Could not find wildcard named: {p[0]}");
                        } else {
                            throw new Exception($"Invalid syntax: {{{part}}}");
                        }
                    } else {
                        result += part;
                    }
                }
                return result;
            } else {
                var nodes = root.Children.Where(t => t.AlternativeExpression is not null);
                if (nodes.Count() > 1) {
                    throw new Exception("Cannot render command. The alternative expression is ambiguous.");
                } else if (nodes.Count() == 1) {
                    return RenderCommand(nodes.First());
                } else if (root.Children.Count == 1) {
                    return RenderCommand(root.Children.First());
                } else {
                    throw new Exception($"Could not find expression to render node: {root.Value}");
                }
            }
        }

        private static TaskNode ParseNodePath(this TaskNode node, string path)
        {
            var parts = path.Split("$", StringSplitOptions.RemoveEmptyEntries);
            var current = node;
            foreach (var part in parts) {
                (string identifier, int? index) = GetIdentifierAndIndex(part);
                var children = current.Children.Where(t => t.Value == "$" + identifier);
                if (index is not null) {
                    if (children.Count() <= index) {
                        throw new Exception($"Out of bounds index. The node only has {children.Count()} children, but the path \"{path}\" attempted to access index {index}.");
                    } else {
                        current = children.ElementAt(index.Value);
                    }
                } else if (children.Count() > 1) {
                    throw new Exception($"Selecting the node {part} in the path {path} is ambiguous. Specify a child index to fix this (e.g. \"{part}:0\")");
                } else if (children.Count() == 1) {
                    current = children.First();
                } else {
                    throw new Exception($"\"{path}\" is attempting to access the children of leaf node {part}.");
                }
            }
            return current;
        }
        
        private static (string identifier, int? index) GetIdentifierAndIndex(string part) {
            var p = part.Split(":");
            if (p.Length == 2) {
                var index = int.Parse(p[1]);
                return (p[0], index);
            } else if (p.Length == 1) {
                return (p[0], null);
            } else {
                throw new Exception($"Invalid path syntax: \"{part}\"");
            }
        }

        public static TaskNode DeepestDecendent(this TaskNode node, Func<IEnumerable<TaskNode>, TaskNode> selector) {
            var child = node;
            while (true) {
                var c = selector(child.Children);
                if (c == null) break;
                child = c;
            }
            return child;
        }

        public static TextWildcard? Find(this TextWildcard wildcard, string name, int? id = null)
        {
            if (wildcard.Name == name) {
                if (id is not null) {
                    if (id == wildcard.Id) {
                        return wildcard;
                    } else {
                        return null;
                    }
                } else {
                    return wildcard;
                }
            }
            foreach (var child in wildcard.Children)
            {
                var result = child.Find(name);
                if (result != null) return result;
            }
            return null;
        }

        public static TextWildcard? FindWildcard(this TaskNode node, string name, int? id = null) 
        {
            var tw = node.TextWildcard?.Find(name, id);
            if (tw != null) return tw;
            foreach (var child in node.Children)
            {
                var result = FindWildcard(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}