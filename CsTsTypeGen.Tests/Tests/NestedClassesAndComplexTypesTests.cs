using System;
using System.IO;
using Xunit;
using CsTsTypeGen.Core;

namespace CsTsTypeGen.Tests.Tests
{
    public class NestedClassesAndComplexTypesTests : TestBase
    {
        [Fact]
        public void Test_NestedClassesAndComplexTypes()
        {
            // Add a class with nested class definition
            File.WriteAllText(Path.Combine(_tempDir, "Models", "AdvancedTypes.cs"), @"
                using System;
                using System.Collections.Generic;

                namespace MyApp.Models
                {
                    /// <summary>
                    /// A class to test advanced C# type mappings to TypeScript
                    /// </summary>
                    public class AdvancedTypes
                    {
                        /// <summary>A nullable string</summary>
                        public string? OptionalString { get; set; }

                        /// <summary>A nullable integer</summary>
                        public int? OptionalInt { get; set; }

                        /// <summary>A dictionary mapping strings to integers</summary>
                        public Dictionary<string, int> KeyValues { get; set; }

                        /// <summary>A tuple of string and integer</summary>
                        public Tuple<string, int> Pair { get; set; }

                        /// <summary>A double array</summary>
                        public double[] Scores { get; set; }

                        /// <summary>A jagged list of strings</summary>
                        public List<List<string>> Matrix { get; set; }

                        /// <summary>A nested complex type</summary>
                        public NestedType Inner { get; set; }

                        /// <summary>
                        /// A nested class for testing
                        /// </summary>
                        public class NestedType
                        {
                            /// <summary>Description text</summary>
                            public string Description { get; set; }
                        }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify AdvancedTypes class output
            Assert.Contains("export interface AdvancedTypes", output);
            Assert.Contains("optionalString?: string", output);
            Assert.Contains("optionalInt?: number", output);
            Assert.Contains("keyValues: Record<string, number>", output);
            Assert.Contains("pair: [string, number]", output);
            Assert.Contains("scores: number[]", output);
            Assert.Contains("matrix: string[][]", output);
            Assert.Contains("inner: AdvancedTypes.NestedType", output);
            
            // Verify nested class output
            Assert.Contains("export interface NestedType", output);
            Assert.Contains("description: string", output);
        }
    }
}