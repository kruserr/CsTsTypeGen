using System;
using System.IO;
using Xunit;
using CsTsTypeGen.Core;
using System.Text.RegularExpressions;
using System.Linq;

namespace CsTsTypeGen.Tests.Tests
{
    public class CommentDocumentationTests : TestBase
    {
        [Fact]
        public void Test_CommentDocumentation()
        {
            // Add a class with documentation comments
            File.WriteAllText(Path.Combine(_tempDir, "Models", "DocumentedClass.cs"), @"
                namespace MyApp.Models
                {
                    /// <summary>
                    /// This is a well-documented class
                    /// with multi-line documentation
                    /// </summary>
                    public class DocumentedClass
                    {
                        /// <summary>
                        /// A property with detailed documentation
                        /// </summary>
                        /// <remarks>
                        /// Additional information about the property
                        /// </remarks>
                        public string DocumentedProperty { get; set; }

                        /// <summary>
                        /// Property with code example
                        /// </summary>
                        /// <code>
                        /// var example = new DocumentedClass();
                        /// example.PropertyWithExample = ""test"";
                        /// </code>
                        public string PropertyWithExample { get; set; }

                        // Simple comment on property
                        public int CommentedProperty { get; set; }

                        [Obsolete(""This property is deprecated, use NewProperty instead"")]
                        public string OldProperty { get; set; }

                        /// <summary>Replacement for OldProperty</summary>
                        public string NewProperty { get; set; }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify class documentation is transferred
            Assert.Contains("/**", output);
            Assert.Contains("* This is a well-documented class", output);
            Assert.Contains("* with multi-line documentation", output);

            // Verify property documentation
            Assert.Contains("* A property with detailed documentation", output);
            Assert.Contains("* Additional information about the property", output);
            
            // Verify code example
            Assert.Contains("* Property with code example", output);
            Assert.Contains("```csharp", output);
            Assert.Contains("var example = new DocumentedClass();", output);
            
            // Verify simple comment
            Assert.Contains("* Simple comment on property", output);
            
            // Verify obsolete annotation
            Assert.Contains("* @deprecated", output);
        }

        [Fact]
        public void Test_CodeBlockInRemarks_NotDuplicated()
        {
            // Add a class with a code block in remarks section
            File.WriteAllText(Path.Combine(_tempDir, "Models", "CodeBlockInRemarks.cs"), @"
                namespace MyApp.Models
                {
                    /// <summary>
                    /// Facade for the DBreeze embedded database.
                    /// </summary>
                    /// 
                    /// <remarks>
                    /// 
                    /// <code>
                    /// using (var db = new DBreezeFacade(@""dbr0""))
                    /// {
                    ///     db.Create(
                    ///         ""team"",
                    ///         new List<string> {
                    ///             ""John Doe"",
                    ///             ""Jane Smith"",
                    ///         }
                    ///     );
                    /// 
                    ///     var team = db.Read<string, List<string>>(""team"");
                    /// 
                    ///     db.Update(
                    ///         ""team"",
                    ///         new List<string> {
                    ///             ""Jane Smith"",
                    ///             ""John Doe"",
                    ///         }
                    ///     );
                    /// 
                    ///     db.Delete(""team"");
                    /// }
                    /// </code>
                    /// </remarks>
                    public class DBreezeFacade : IDisposable
                    {
                        // Some properties and methods
                        public void Dispose() { }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify the class documentation summary is present
            Assert.Contains("* Facade for the DBreeze embedded database.", output);
            
            // Verify the code block appears exactly once
            int codeBlockCount = CountStringOccurrences(output, "```csharp");
            Assert.Equal(1, codeBlockCount);
            
            // Verify the code block content is present
            Assert.Contains("using (var db = new DBreezeFacade", output);
            Assert.Contains("db.Create(", output);
            
            // Make sure the "Additional information about the property" text is not there
            // as it's the text that gets added for regular remarks without code blocks
            Assert.DoesNotContain("Additional information about the property", output);
        }

        [Fact]
        public void Test_CodeBlockInRemarks_ExactFormatting()
        {
            // Add a class with a code block in remarks section
            File.WriteAllText(Path.Combine(_tempDir, "Models", "CodeBlockInRemarksExactFormat.cs"), @"
                namespace MyApp.Models
                {
                    /// <summary>
                    /// Facade for the DBreeze embedded database.
                    /// </summary>
                    /// 
                    /// <remarks>
                    /// 
                    /// <code>
                    /// using (var db = new DBreezeFacade(@""dbr0""))
                    /// {
                    ///     db.Create(
                    ///         ""team"",
                    ///         new List<string> {
                    ///             ""John Doe"",
                    ///             ""Jane Smith"",
                    ///         }
                    ///     );
                    /// 
                    ///     var team = db.Read<string, List<string>>(""team"");
                    /// 
                    ///     db.Update(
                    ///         ""team"",
                    ///         new List<string> {
                    ///             ""Jane Smith"",
                    ///             ""John Doe"",
                    ///         }
                    ///     );
                    /// 
                    ///     db.Delete(""team"");
                    /// }
                    /// </code>
                    /// </remarks>
                    public class DBreezeFacade : IDisposable
                    {
                        // Some properties and methods
                        public void Dispose() { }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Extract the DBreezeFacade interface and its JSDoc comment
            string pattern = @"(\s+/\*\*[\s\S]*?\*/\s+export interface DBreezeFacade \{\s*\})";
            Match match = Regex.Match(output, pattern);
            Assert.True(match.Success, "Failed to find DBreezeFacade interface in output");
            
            string actualOutput = match.Groups[1].Value;
            
            // Define the expected output with exact formatting
            string expectedOutput = @"
    /**
     * Facade for the DBreeze embedded database.
     * 
     * ```csharp
     * using (var db = new DBreezeFacade(@""dbr0""))
     * {
     *     db.Create(
     *         ""team"",
     *         new List<string> {
     *             ""John Doe"",
     *             ""Jane Smith"",
     *         }
     *     );
     * 
     *     var team = db.Read<string, List<string>>(""team"");
     * 
     *     db.Update(
     *         ""team"",
     *         new List<string> {
     *             ""Jane Smith"",
     *             ""John Doe"",
     *         }
     *     );
     * 
     *     db.Delete(""team"");
     * }
     * 
     * 
     * ```
     */
    export interface DBreezeFacade {
    }";

            // Normalize line endings for comparison
            expectedOutput = NormalizeLineEndings(expectedOutput);
            actualOutput = NormalizeLineEndings(actualOutput);
            
            // Compare the actual output with the expected output
            Assert.Equal(expectedOutput, actualOutput);
        }
        
        private int CountStringOccurrences(string source, string searchString)
        {
            int count = 0;
            int position = 0;
            
            while ((position = source.IndexOf(searchString, position, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                count++;
                position += searchString.Length;
            }
            
            return count;
        }
        
        private string NormalizeLineEndings(string text)
        {
            // Replace all line endings with \n for consistent comparison
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}