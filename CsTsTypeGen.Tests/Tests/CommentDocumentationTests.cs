using System;
using System.IO;
using Xunit;

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
    }
}