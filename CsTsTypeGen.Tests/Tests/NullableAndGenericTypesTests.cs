using System.IO;
using Xunit;

namespace CsTsTypeGen.Tests.Tests
{
    public class NullableAndGenericTypesTests : TestBase
    {
        [Fact]
        public void Test_NullableAndGenericTypes()
        {
            // Add a Product class with nullable and generic types
            File.WriteAllText(Path.Combine(_tempDir, "Models", "Product.cs"), @"
                namespace MyApp.Models
                {
                    /// <summary>Represents a product</summary>
                    public class Product
                    {
                        public string Title { get; set; }
                        public decimal Price { get; set; }
                        public Nullable<int> Stock { get; set; }
                        public int? Rating { get; set; }
                        public ICollection<string> Categories { get; set; }
                        public List<User> RelatedUsers { get; set; }
                    }
                }
            ");

            // Add a basic User class reference
            File.WriteAllText(Path.Combine(_tempDir, "Models", "User.cs"), @"
                namespace MyApp.Models
                {
                    public class User
                    {
                        public string Name { get; set; }
                        public int Age { get; set; }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify Product class output
            Assert.Contains("export interface Product", output);
            Assert.Contains("title: string", output);
            Assert.Contains("price: number", output);
            Assert.Contains("stock?: number", output);  // Nullable<int>
            Assert.Contains("rating?: number", output); // int?
            Assert.Contains("categories: string[]", output);
            Assert.Contains("relatedUsers: User[]", output);
        }
    }
}