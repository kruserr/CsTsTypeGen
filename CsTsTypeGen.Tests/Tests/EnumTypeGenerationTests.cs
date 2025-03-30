using System.IO;
using Xunit;

namespace CsTsTypeGen.Tests.Tests
{
    public class EnumTypeGenerationTests : TestBase
    {
        [Fact]
        public void Test_EnumTypeGeneration()
        {
            // Setup basic enums
            File.WriteAllText(Path.Combine(_tempDir, "Enums", "Status.cs"), @"
                namespace MyApp.Enums
                {
                    /// <summary>Status enum</summary>
                    public enum Status
                    {
                        Active,
                        Inactive,
                        Banned
                    }
                }
            ");

            // Add flags enum with custom base type
            File.WriteAllText(Path.Combine(_tempDir, "Enums", "Flags.cs"), @"
                namespace MyApp.Enums
                {
                    /// <summary>Flags enum using bitwise values</summary>
                    [Flags]
                    public enum Permissions : uint
                    {
                        None = 0,
                        Read = 1,
                        Write = 2,
                        Execute = 4,
                        All = Read | Write | Execute
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify Status enum output
            Assert.Contains("declare namespace MyApp", output);
            Assert.Contains("export type Status = 'Active' | 'Inactive' | 'Banned'", output);
            Assert.Contains("export enum StatusEnum", output);
            
            // Verify Permissions enum output
            Assert.Contains("export type Permissions = 'None' | 'Read' | 'Write' | 'Execute' | 'All'", output);
            Assert.Contains("export enum PermissionsEnum", output);
        }
    }
}