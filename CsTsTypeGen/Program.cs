using System;
using System.IO;
using CsTsTypeGen.Core;

namespace CsTsTypeGen
{
    /// <summary>
    /// Entry point class for the CsTsTypeGen tool
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point of the application
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>0 on success, non-zero on error</returns>
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
}
