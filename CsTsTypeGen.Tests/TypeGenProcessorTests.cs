using System;
using System.IO;
using Xunit;

namespace CsTsTypeGen.Tests
{
    public class TypeGenProcessorIntegrationTests
    {
        [Fact]
        public void GeneratesComprehensiveTypeScriptDefinitions()
        {
            // Arrange
            string tempDir = Path.Combine(Path.GetTempPath(), "CsTsTypeGenTest_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);
            string outputFile = Path.Combine(tempDir, "typedefs.d.ts");

            // Create subfolders
            Directory.CreateDirectory(Path.Combine(tempDir, "Models"));
            Directory.CreateDirectory(Path.Combine(tempDir, "Enums"));
            Directory.CreateDirectory(Path.Combine(tempDir, "Services"));

            // Enums/Status.cs
            File.WriteAllText(Path.Combine(tempDir, "Enums", "Status.cs"), @"
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

            // Models/User.cs
            File.WriteAllText(Path.Combine(tempDir, "Models", "User.cs"), @"
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

            // Models/Product.cs
            File.WriteAllText(Path.Combine(tempDir, "Models", "Product.cs"), @"
                namespace MyApp.Models
                {
                    /// <summary>Represents a product</summary>
                    public class Product
                    {
                        public string Title { get; set; }
                        public decimal Price { get; set; }
                        public Nullable<int> Stock { get; set; }
                        public ICollection<User> Users { get; set; }
                    }
                }
            ");

            // Services/OrderService.cs
            File.WriteAllText(Path.Combine(tempDir, "Services", "OrderService.cs"), @"
                namespace MyApp.Services
                {
                    using MyApp.Models;
                    using Microsoft.EntityFrameworkCore;

                    /// <summary>Service for handling orders</summary>
                    public class OrderService
                    {
                        public DbSet<Product> Products { get; set; }
                        public DbSet<User> Users { get; set; }
                    }
                }
            ");

            // Models/AdvancedTypes.cs
            File.WriteAllText(Path.Combine(tempDir, "Models", "AdvancedTypes.cs"), @"
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

            // Act
            int result = TypeGenProcessor.Run(tempDir, outputFile);

            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(outputFile);

            // Basic checks
            Assert.Contains("declare namespace MyApp", output);
            Assert.Contains("export interface User", output);
            Assert.Contains("name: string", output);
            Assert.Contains("status: Status", output);
            Assert.Contains("@deprecated", output);

            // Enum
            Assert.Contains("export type Status = 'Active' | 'Inactive' | 'Banned'", output);
            Assert.Contains("export enum StatusEnum", output);

            // Product
            Assert.Contains("export interface Product", output);
            Assert.Contains("stock?: number", output);
            Assert.Contains("users: User[]", output);

            // AdvancedTypes
            Assert.Contains("export interface AdvancedTypes", output);
            Assert.Contains("optionalString?: string", output);
            Assert.Contains("optionalInt?: number", output);
            Assert.Contains("keyValues: Record<string, number>", output); // if generator supports it
            Assert.Contains("pair: [string, number]", output);            // if generator supports it
            Assert.Contains("scores: number[]", output);
            Assert.Contains("matrix: string[][]", output);
            Assert.Contains("inner: AdvancedTypes.NestedType", output);
            Assert.Contains("export interface NestedType", output);

            // Cleanup
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
