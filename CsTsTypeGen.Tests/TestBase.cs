using System;
using System.IO;

namespace CsTsTypeGen.Tests
{
    public abstract class TestBase : IDisposable
    {
        // Common setup for all tests
        protected readonly string _tempDir;
        protected readonly string _outputFile;

        protected TestBase()
        {
            // Common setup - create temp directory and output file path
            _tempDir = Path.Combine(Path.GetTempPath(), "CsTsTypeGenTest_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDir);
            
            // Create subfolders
            Directory.CreateDirectory(Path.Combine(_tempDir, "Models"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "Enums"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "Services"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "Common"));

            _outputFile = Path.Combine(_tempDir, "typedefs.d.ts");
        }

        public void Dispose()
        {
            // Cleanup after each test
            if (Directory.Exists(_tempDir))
            {
                try
                {
                    Directory.Delete(_tempDir, recursive: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to clean up test directory: {ex.Message}");
                }
            }
        }
    }
}