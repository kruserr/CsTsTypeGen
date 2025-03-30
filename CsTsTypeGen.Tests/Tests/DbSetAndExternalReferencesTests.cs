using System;
using System.IO;
using Xunit;
using CsTsTypeGen.Core;

namespace CsTsTypeGen.Tests.Tests
{
    public class DbSetAndExternalReferencesTests : TestBase
    {
        [Fact]
        public void Test_DbSetAndExternalReferences()
        {
            // Add a service with DbSet references
            File.WriteAllText(Path.Combine(_tempDir, "Services", "OrderService.cs"), @"
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

            // Add referenced models
            File.WriteAllText(Path.Combine(_tempDir, "Models", "Product.cs"), @"
                namespace MyApp.Models
                {
                    public class Product
                    {
                        public string Title { get; set; }
                    }
                }
            ");

            File.WriteAllText(Path.Combine(_tempDir, "Models", "User.cs"), @"
                namespace MyApp.Models
                {
                    public class User
                    {
                        public string Name { get; set; }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify OrderService class output
            Assert.Contains("export interface OrderService", output);
            Assert.Contains("products: DbSet<Product>", output);
            Assert.Contains("users: DbSet<User>", output);
            
            // Verify DbSet interface is defined
            Assert.Contains("interface DbSet<T> extends Array<T>", output);
        }
    }
}