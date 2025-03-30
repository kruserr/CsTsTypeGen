using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsTsTypeGen
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("CsTsTypeGen - C# to TypeScript Definition Generator");
            Console.WriteLine("====================================================");

            string currentDirectory = Environment.CurrentDirectory;
            string inputDir = Path.Combine(currentDirectory, "..");
            string outputPath = Path.Combine(currentDirectory, "typedefs.d.ts");

            string csSourceDir = Environment.GetEnvironmentVariable("CsTsTypeGen_SourceDirectory");
            string tsDefinitionsPath = Environment.GetEnvironmentVariable("CsTsTypeGen_DefinitionsPath");
            string generateDefinitions = Environment.GetEnvironmentVariable("CsTsTypeGen_GenerateDefinitions");

            if (!string.IsNullOrEmpty(generateDefinitions) && generateDefinitions.ToLower() == "false")
            {
                Console.WriteLine("TypeScript definition generation is disabled. Skipping...");
                return 0;
            }

            if (args.Length > 0)
            {
                inputDir = args[0];
            }
            else if (!string.IsNullOrEmpty(csSourceDir))
            {
                inputDir = csSourceDir;
            }

            if (args.Length > 1)
            {
                outputPath = args[1];
            }
            else if (!string.IsNullOrEmpty(tsDefinitionsPath))
            {
                outputPath = tsDefinitionsPath;
            }

            if (!Directory.Exists(inputDir))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Input directory not found: " + inputDir);
                Console.ResetColor();
                Console.WriteLine("Usage: cstsgen <input-directory> [output-file]");
                Console.WriteLine("  input-directory: Directory containing C# files to scan");
                Console.WriteLine("  output-file:     Path for TypeScript definitions output (default: ./typedefs.d.ts)");
                Console.WriteLine();
                Console.WriteLine("Default configuration:");
                Console.WriteLine("  <GenerateTypeScriptDefinitions>true</GenerateTypeScriptDefinitions>");
                Console.WriteLine("  <TypeScriptDefinitionsPath>$(MSBuildProjectDirectory)/typedefs.d.ts</TypeScriptDefinitionsPath>");
                Console.WriteLine("  <CsTsSourceDirectory>$(MSBuildProjectDirectory)/..</CsTsSourceDirectory>");
                return 1;
            }

            try
            {
                return TypeGenProcessor.Run(inputDir, outputPath);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
                return 1;
            }
        }
    }

    public static class TypeGenProcessor
    {
        public static int Run(string inputDir, string outputPath)
        {
            Console.WriteLine("Scanning C# files in: " + inputDir);

            string[] csFiles = Directory.GetFiles(inputDir, "*.cs", SearchOption.AllDirectories);
            Dictionary<string, List<ClassDeclarationSyntax>> namespaceGroups = new Dictionary<string, List<ClassDeclarationSyntax>>();
            Dictionary<string, List<EnumDeclarationSyntax>> enumGroups = new Dictionary<string, List<EnumDeclarationSyntax>>();

            Console.WriteLine("Found " + csFiles.Length + " C# files");

            foreach (string file in csFiles)
            {
                try
                {
                    string code = File.ReadAllText(file);
                    SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                    CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                    string ns = "Global";

                    foreach (MemberDeclarationSyntax member in root.Members)
                    {
                        if (member is BaseNamespaceDeclarationSyntax nsDecl)
                        {
                            ns = nsDecl.Name.ToString();
                            break;
                        }
                    }

                    List<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                    List<EnumDeclarationSyntax> enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>().ToList();

                    if (!namespaceGroups.ContainsKey(ns))
                        namespaceGroups[ns] = new List<ClassDeclarationSyntax>();
                    namespaceGroups[ns].AddRange(classes);

                    if (!enumGroups.ContainsKey(ns))
                        enumGroups[ns] = new List<EnumDeclarationSyntax>();
                    enumGroups[ns].AddRange(enums);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: Error processing file " + file + ": " + ex.Message);
                    Console.ResetColor();
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/**");
            sb.AppendLine(" * TypeScript definitions generated by CsTsTypeGen (https://github.com/kruserr/CsTsTypeGen)");
            sb.AppendLine(" * This file is automatically generated, if you modify this file manually, it will be overwritten.");
            sb.AppendLine(" */");
            sb.AppendLine();

            sb.AppendLine("interface DbSet<T> extends Array<T> {");
            sb.AppendLine("  add(entity: T): void;");
            sb.AppendLine("  remove(entity: T): void;");
            sb.AppendLine("  find(id: any): T | undefined;");
            sb.AppendLine("}\n");

            foreach (KeyValuePair<string, List<ClassDeclarationSyntax>> nsGroup in namespaceGroups)
            {
                string[] nsParts = nsGroup.Key.Split('.');
                for (int i = 0; i < nsParts.Length; i++)
                {
                    string indent = new string(' ', i * 2);
                    sb.AppendLine(indent + "declare namespace " + nsParts[i] + " {");
                }

                string nsIndent = new string(' ', nsParts.Length * 2);

                if (enumGroups.ContainsKey(nsGroup.Key))
                {
                    foreach (EnumDeclarationSyntax enumNode in enumGroups[nsGroup.Key])
                    {
                        string enumName = enumNode.Identifier.Text;
                        string comment = GetCommentBlock(enumNode);
                        if (!string.IsNullOrEmpty(comment))
                        {
                            sb.AppendLine(nsIndent + "/**");
                            foreach (string line in comment.Split('\n'))
                                sb.AppendLine(nsIndent + " * " + line);
                            sb.AppendLine(nsIndent + " */");
                        }
                        List<string> enumMembers = new List<string>();
                        foreach (var member in enumNode.Members)
                            enumMembers.Add("'" + member.Identifier.Text + "'");

                        sb.AppendLine(nsIndent + "export type " + enumName + " = " + string.Join(" | ", enumMembers) + ";");

                        sb.AppendLine(nsIndent + "export enum " + enumName + "Enum {");
                        foreach (var member in enumNode.Members)
                        {
                            sb.AppendLine(nsIndent + "  " + member.Identifier.Text + ",");
                        }
                        sb.AppendLine(nsIndent + "}\n");
                    }
                }

                foreach (ClassDeclarationSyntax classNode in nsGroup.Value)
                {
                    string className = classNode.Identifier.Text;
                    string comment = GetCommentBlock(classNode);
                    if (!string.IsNullOrEmpty(comment))
                    {
                        sb.AppendLine(nsIndent + "/**");
                        foreach (string line in comment.Split('\n'))
                            sb.AppendLine(nsIndent + " * " + line);
                        sb.AppendLine(nsIndent + " */");
                    }
                    sb.AppendLine(nsIndent + "export interface " + className + " {");
                    foreach (PropertyDeclarationSyntax prop in classNode.Members.OfType<PropertyDeclarationSyntax>())
                    {
                        string propName = ToCamelCase(prop.Identifier.Text);
                        string tsType = MapType(prop.Type.ToString());
                        bool isNullable = IsNullableProperty(prop);
                        bool isObsolete = HasObsoleteAttribute(prop);
                        string propComment = GetCommentBlock(prop);

                        if (isObsolete || !string.IsNullOrEmpty(propComment))
                        {
                            sb.AppendLine(nsIndent + "  /**");
                            if (isObsolete)
                                sb.AppendLine(nsIndent + "   * @deprecated This property is marked as obsolete");
                            if (!string.IsNullOrEmpty(propComment))
                            {
                                foreach (string line in propComment.Split('\n'))
                                    sb.AppendLine(nsIndent + "   * " + line);
                            }
                            sb.AppendLine(nsIndent + "   */");
                        }

                        string optionalSuffix = isNullable ? "?" : "";
                        sb.AppendLine(nsIndent + "  " + propName + optionalSuffix + ": " + tsType + ";");
                    }
                    sb.AppendLine(nsIndent + "}\n");
                }

                for (int i = nsParts.Length - 1; i >= 0; i--)
                {
                    string indent = new string(' ', i * 2);
                    sb.AppendLine(indent + "}");
                }
                sb.AppendLine();
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));
            File.WriteAllText(outputPath, sb.ToString());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ TypeScript definitions written to: " + outputPath);
            Console.ResetColor();
            return 0;
        }

        static string MapType(string csharpType)
        {
            bool isNullable = csharpType.EndsWith("?");
            if (isNullable)
                csharpType = csharpType.Substring(0, csharpType.Length - 1);

            if (csharpType.StartsWith("Nullable<") && csharpType.EndsWith(">"))
            {
                csharpType = csharpType.Substring(9, csharpType.Length - 10);
            }
            if (csharpType.StartsWith("ICollection<") && csharpType.EndsWith(">"))
            {
                string inner = csharpType.Substring(12, csharpType.Length - 13);
                return MapType(inner) + "[]";
            }
            if (csharpType.StartsWith("DbSet<") && csharpType.EndsWith(">"))
            {
                string inner = csharpType.Substring(6, csharpType.Length - 7);
                return "DbSet<" + MapType(inner) + ">";
            }
            if (csharpType.StartsWith("List<") || csharpType.StartsWith("IList<"))
            {
                int lt = csharpType.IndexOf("<");
                int gt = csharpType.IndexOf(">");
                string inner = csharpType.Substring(lt + 1, gt - lt - 1);
                return MapType(inner) + "[]";
            }

            switch (csharpType)
            {
                case "string": return "string";
                case "int": case "long": case "decimal": case "double": case "float": return "number";
                case "bool": return "boolean";
                case "DateTime": case "DateTimeOffset": return "string";
                case "Guid": return "string";
                default: return csharpType;
            }
        }

        static string ToCamelCase(string input)
        {
            return string.IsNullOrEmpty(input) ? input : char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        static bool IsNullableProperty(PropertyDeclarationSyntax prop)
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

        static bool HasObsoleteAttribute(PropertyDeclarationSyntax prop)
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

        static string GetCommentBlock(SyntaxNode node)
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
                    foreach (var codeBlock in doc.DescendantNodes().OfType<XmlElementSyntax>().Where(e => e.StartTag.Name.LocalName.Text == "code"))
                    {
                        docLines.Add("");
                        docLines.Add("```csharp");
                        docLines.AddRange(ProcessCodeBlock(codeBlock));
                        docLines.Add("```");
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

        static string ExtractTextFromXmlNode(XmlElementSyntax element)
        {
            string text = element.Content.ToFullString();
            text = Regex.Replace(text, @"^\s*///\s*", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"<.*?>", "");
            return text.Trim();
        }

        static List<string> ProcessCodeBlock(XmlElementSyntax codeBlock)
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
    }
}
