using System;
using System.IO;
using Xunit;

namespace CsTsTypeGen.Tests.Tests
{
    public class BasicClassGenerationTests : TestBase
    {
        [Fact]
        public void Test_BasicClassGeneration()
        {
            // Add a basic User class model
            File.WriteAllText(Path.Combine(_tempDir, "Models", "User.cs"), @"
                namespace MyApp.Models
                {
                    /// <summary>Represents a user</summary>
                    public class User
                    {
                        public string Name { get; set; }
                        public int Age { get; set; }
                        public bool IsActive { get; set; }
                        public DateTime CreatedAt { get; set; }
                        public Guid Id { get; set; }
                        public List<string> Tags { get; set; }
                        public MyApp.Enums.Status Status { get; set; }

                        [Obsolete]
                        public string LegacyField { get; set; }
                    }
                }
            ");

            // Add Status enum for reference
            File.WriteAllText(Path.Combine(_tempDir, "Enums", "Status.cs"), @"
                namespace MyApp.Enums
                {
                    public enum Status
                    {
                        Active,
                        Inactive,
                        Banned
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify User class output
            Assert.Contains("export interface User", output);
            Assert.Contains("name: string", output);
            Assert.Contains("age: number", output);
            Assert.Contains("isActive: boolean", output);
            Assert.Contains("createdAt: string", output);
            Assert.Contains("id: string", output);
            Assert.Contains("tags: string[]", output);
            Assert.Contains("status: Status", output);
            Assert.Contains("@deprecated", output);
            Assert.Contains("legacyField: string", output);
        }
    }
}