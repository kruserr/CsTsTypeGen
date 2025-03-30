using System;
using System.IO;
using Xunit;

namespace CsTsTypeGen.Tests
{
    public class TypeGenProcessorIntegrationTests : IDisposable
    {
        // Common setup for all tests
        private readonly string _tempDir;
        private readonly string _outputFile;

        public TypeGenProcessorIntegrationTests()
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

        [Fact]
        public void Test_StandardLibraryTypes()
        {
            // Add a class with many standard library types
            File.WriteAllText(Path.Combine(_tempDir, "Common", "ExtendedTypes.cs"), @"
                using System;
                using System.Collections.Generic;
                using System.Collections.ObjectModel;
                using System.Collections.Concurrent;
                using System.Linq;
                using System.Threading.Tasks;

                namespace MyApp.Common
                {
                    /// <summary>
                    /// A class to test extended standard library types in C#
                    /// </summary>
                    public class ExtendedTypes
                    {
                        // Numeric Types
                        public byte ByteValue { get; set; }
                        public sbyte SByteValue { get; set; }
                        public short ShortValue { get; set; }
                        public ushort UShortValue { get; set; }
                        public int IntValue { get; set; }
                        public uint UIntValue { get; set; }
                        public long LongValue { get; set; }
                        public ulong ULongValue { get; set; }
                        public float FloatValue { get; set; }
                        public double DoubleValue { get; set; }
                        public decimal DecimalValue { get; set; }

                        // Character Types
                        public char CharValue { get; set; }

                        // Boolean Type
                        public bool BoolValue { get; set; }

                        // Date and Time Types
                        public DateTime DateTimeValue { get; set; }
                        public DateTimeOffset DateTimeOffsetValue { get; set; }
                        public TimeSpan TimeSpanValue { get; set; }
                        public DateOnly DateOnlyValue { get; set; }
                        public TimeOnly TimeOnlyValue { get; set; }

                        // Special Types
                        public Guid GuidValue { get; set; }
                        public Uri UriValue { get; set; }
                        public Version VersionValue { get; set; }

                        // Collection Types
                        public IDictionary<string, object> GenericDictionary { get; set; }
                        public IEnumerable<int> GenericEnumerable { get; set; }
                        public IList<string> GenericList { get; set; }
                        public HashSet<int> HashSetValue { get; set; }
                        public SortedSet<string> SortedSetValue { get; set; }
                        public Stack<int> StackValue { get; set; }
                        public Queue<string> QueueValue { get; set; }
                        public LinkedList<double> LinkedListValue { get; set; }
                        public ConcurrentBag<int> ConcurrentBagValue { get; set; }
                        public ConcurrentDictionary<int, string> ConcurrentDictionaryValue { get; set; }
                        public ReadOnlyCollection<int> ReadOnlyCollectionValue { get; set; }
                        public ReadOnlyDictionary<string, int> ReadOnlyDictionaryValue { get; set; }

                        // Tuple Types
                        public Tuple<int, string, bool> TripleTuple { get; set; }
                        public ValueTuple<int, string> ValueTuple { get; set; }
                        public (string Name, int Age) NamedValueTuple { get; set; }

                        // Task Types
                        public Task TaskValue { get; set; }
                        public Task<int> GenericTaskValue { get; set; }
                        public ValueTask<bool> ValueTaskValue { get; set; }

                        // Nullable Value Types
                        public int? NullableInt { get; set; }
                        public DateTime? NullableDateTime { get; set; }
                        public Nullable<Guid> NullableGuid { get; set; }

                        // Array Types
                        public int[] OneDimensionalArray { get; set; }
                        public int[,] TwoDimensionalArray { get; set; }
                        public int[][] JaggedArray { get; set; } // Explicitly declared jagged array
                        
                        // Func and Action Types
                        public Func<int, string> FuncValue { get; set; }
                        public Action<string> ActionValue { get; set; }
                        public Predicate<int> PredicateValue { get; set; }
                        
                        // Dynamic and Object Types
                        public dynamic DynamicValue { get; set; }
                        public object ObjectValue { get; set; }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);
            Console.WriteLine("Test Output: " + output);  // Debug print to see what's in the output

            // Verify numeric types
            Assert.Contains("byteValue: number", output);
            Assert.Contains("sByteValue: number", output);
            Assert.Contains("shortValue: number", output);
            Assert.Contains("uShortValue: number", output);
            Assert.Contains("intValue: number", output);
            Assert.Contains("uIntValue: number", output);
            Assert.Contains("longValue: number", output);
            Assert.Contains("uLongValue: number", output);
            Assert.Contains("floatValue: number", output);
            Assert.Contains("doubleValue: number", output);
            Assert.Contains("decimalValue: number", output);

            // Verify char and bool
            Assert.Contains("charValue: string", output);
            Assert.Contains("boolValue: boolean", output);

            // Verify date and time types
            Assert.Contains("dateTimeValue: string", output);
            Assert.Contains("dateTimeOffsetValue: string", output);
            Assert.Contains("timeSpanValue: string", output);
            Assert.Contains("dateOnlyValue: string", output);
            Assert.Contains("timeOnlyValue: string", output);
            
            // Verify special types
            Assert.Contains("guidValue: string", output);
            Assert.Contains("uriValue: string", output);
            Assert.Contains("versionValue: string", output);

            // Verify collection types
            Assert.Contains("genericDictionary: Record<string, any>", output);
            Assert.Contains("genericEnumerable: number[]", output);
            Assert.Contains("genericList: string[]", output);
            Assert.Contains("hashSetValue: number[]", output);
            Assert.Contains("sortedSetValue: string[]", output);
            Assert.Contains("stackValue: number[]", output);
            Assert.Contains("queueValue: string[]", output);
            Assert.Contains("linkedListValue: number[]", output);
            Assert.Contains("concurrentBagValue: number[]", output);
            Assert.Contains("concurrentDictionaryValue: Record<number, string>", output);
            Assert.Contains("readOnlyCollectionValue: number[]", output);
            Assert.Contains("readOnlyDictionaryValue: Record<string, number>", output);

            // Verify tuple types
            Assert.Contains("tripleTuple: [number, string, boolean]", output);
            Assert.Contains("valueTuple: [number, string]", output);
            Assert.Contains("namedValueTuple: [string, number]", output);

            // Verify task types
            Assert.Contains("taskValue: Promise<void>", output);
            Assert.Contains("genericTaskValue: Promise<number>", output);
            Assert.Contains("valueTaskValue: Promise<boolean>", output);

            // Verify nullable types
            Assert.Contains("nullableInt?: number", output);
            Assert.Contains("nullableDateTime?: string", output);
            Assert.Contains("nullableGuid?: string", output);

            // Verify array types
            Assert.Contains("oneDimensionalArray: number[]", output);
            Assert.Contains("twoDimensionalArray: number[][]", output);
            Assert.Contains("jaggedArray: number[][]", output); // Check for this exact string

            // Verify delegate types
            Assert.Contains("funcValue: (p0: number) => string", output);
            Assert.Contains("actionValue: (p0: string) => void", output);
            Assert.Contains("predicateValue: (value: number) => boolean", output);

            // Verify dynamic and object types
            Assert.Contains("dynamicValue: any", output);
            Assert.Contains("objectValue: any", output);
        }

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
