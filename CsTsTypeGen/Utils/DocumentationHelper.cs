using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsTsTypeGen.Utils
{
    /// <summary>
    /// Helper class for extracting documentation from C# syntax nodes
    /// </summary>
    public static class DocumentationHelper
    {
        /// <summary>
        /// Extracts documentation comments from a syntax node
        /// </summary>
        /// <param name="node">The syntax node to extract documentation from</param>
        /// <returns>The extracted documentation comment or null if none found</returns>
        public static string GetCommentBlock(SyntaxNode node)
        {
            foreach (var trivia in node.GetLeadingTrivia())
            {
                var structure = trivia.GetStructure();
                if (structure is DocumentationCommentTriviaSyntax doc)
                {
                    var docLines = new List<string>();
                    var summary = doc.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault(e => e.StartTag.Name.LocalName.Text == "summary");
                    if (summary != null)
                    {
                        string summaryText = ExtractTextFromXmlNode(summary);
                        if (!string.IsNullOrWhiteSpace(summaryText))
                            docLines.Add(summaryText);
                    }
                    
                    // Process code blocks (including those inside remarks)
                    var codeBlocks = doc.DescendantNodes().OfType<XmlElementSyntax>().Where(e => e.StartTag.Name.LocalName.Text == "code").ToList();
                    foreach (var codeBlock in codeBlocks)
                    {
                        docLines.Add("");
                        docLines.Add("```csharp");
                        docLines.AddRange(ProcessCodeBlock(codeBlock));
                        docLines.Add("```");
                    }
                    
                    // Process remarks section content only if it doesn't contain code blocks
                    var remarks = doc.ChildNodes().OfType<XmlElementSyntax>().FirstOrDefault(e => e.StartTag.Name.LocalName.Text == "remarks");
                    if (remarks != null && !remarks.DescendantNodes().OfType<XmlElementSyntax>().Any(e => e.StartTag.Name.LocalName.Text == "code"))
                    {
                        string remarksText = ExtractTextFromXmlNode(remarks);
                        if (!string.IsNullOrWhiteSpace(remarksText))
                        {
                            docLines.Add("");
                            docLines.Add("Additional information about the property");
                            docLines.AddRange(remarksText.Split('\n').Select(line => line.Trim()));
                        }
                    }
                    
                    string result = string.Join("\n", docLines);
                    result = Regex.Replace(result, @"</?code>|</?remarks>", "");
                    return result.Trim();
                }
            }
            var regularComments = node.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia)).Select(t => t.ToString().TrimStart('/').Trim());
            if (regularComments.Any())
                return string.Join("\n", regularComments);
            return null;
        }

        /// <summary>
        /// Extracts text content from an XML element
        /// </summary>
        private static string ExtractTextFromXmlNode(XmlElementSyntax element)
        {
            string text = element.Content.ToFullString();
            text = Regex.Replace(text, @"^\s*///\s*", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"<.*?>", "");
            return text.Trim();
        }

        /// <summary>
        /// Processes a code block within documentation comments
        /// </summary>
        private static List<string> ProcessCodeBlock(XmlElementSyntax codeBlock)
        {
            var result = new List<string>();
            string codeContent = codeBlock.Content.ToFullString();
            string[] lines = codeContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                string cleaned = Regex.Replace(line, @"^\s*///\s?", "");
                if (string.IsNullOrWhiteSpace(cleaned) && (result.Count == 0 || line == lines[lines.Length - 1]))
                    continue;
                result.Add(cleaned);
            }
            return result;
        }

        /// <summary>
        /// Checks if a property is nullable based on its type and attributes
        /// </summary>
        public static bool IsNullableProperty(PropertyDeclarationSyntax prop)
        {
            string typeStr = prop.Type.ToString();
            if (typeStr.EndsWith("?") || typeStr.StartsWith("Nullable<")) return true;
            foreach (var attrList in prop.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name.ToString() == "AllowNull") return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a property has the Obsolete attribute
        /// </summary>
        public static bool HasObsoleteAttribute(PropertyDeclarationSyntax prop)
        {
            foreach (var attrList in prop.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    string name = attr.Name.ToString();
                    if (name == "Obsolete" || name == "ObsoleteAttribute") return true;
                }
            }
            return false;
        }
    }
}