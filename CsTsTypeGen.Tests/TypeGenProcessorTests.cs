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

            // Create sample files
            Directory.CreateDirectory(Path.Combine(tempDir, "Models"));
            Directory.CreateDirectory(Path.Combine(tempDir, "Enums"));
            Directory.CreateDirectory(Path.Combine(tempDir, "Services"));

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
                        public Status Status { get; set; }

                        [Obsolete]
                        public string LegacyField { get; set; }
                    }
                }
            ");

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

            // Act
            int result = TypeGenProcessor.Run(tempDir, outputFile);

            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(outputFile);

            // Validate some contents
            Assert.Contains("declare namespace MyApp", output);
            Assert.Contains("export interface User", output);
            Assert.Contains("name: string", output);
            Assert.Contains("age: number", output);
            Assert.Contains("createdAt: string", output);
            Assert.Contains("id: string", output);
            Assert.Contains("tags: string[]", output);
            Assert.Contains("@deprecated", output);
            Assert.Contains("export type Status = 'Active' | 'Inactive' | 'Banned';", output);
            Assert.Contains("export enum StatusEnum", output);
            Assert.Contains("stock?: number", output);
            Assert.Contains("users: User[]", output);
            Assert.Contains("products: DbSet<Product>", output);

            // Cleanup
            // Directory.Delete(tempDir, recursive: true);
        }
    }
}
