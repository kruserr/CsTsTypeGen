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
            Directory.CreateDirectory(Path.Combine(tempDir, "Common"));

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

            // Add more enums for testing with different base types
            File.WriteAllText(Path.Combine(tempDir, "Enums", "Flags.cs"), @"
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

            // Common/ExtendedTypes.cs - Add more standard library types
            File.WriteAllText(Path.Combine(tempDir, "Common", "ExtendedTypes.cs"), @"
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
                        public int[][] JaggedArray { get; set; }
                        
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
            Assert.Contains("export type Permissions", output); // New enum

            // Product
            Assert.Contains("export interface Product", output);
            Assert.Contains("stock?: number", output);
            Assert.Contains("users: User[]", output);

            // AdvancedTypes
            Assert.Contains("export interface AdvancedTypes", output);
            Assert.Contains("optionalString?: string", output);
            Assert.Contains("optionalInt?: number", output);
            Assert.Contains("keyValues: Record<string, number>", output);
            Assert.Contains("pair: [string, number]", output);
            Assert.Contains("scores: number[]", output);
            Assert.Contains("matrix: string[][]", output);
            Assert.Contains("inner: AdvancedTypes.NestedType", output);
            Assert.Contains("export interface NestedType", output);

            // ExtendedTypes - numeric types
            Assert.Contains("byteValue: number", output);
            Assert.Contains("intValue: number", output);
            Assert.Contains("decimalValue: number", output);

            // ExtendedTypes - date and time
            Assert.Contains("dateTimeValue: string", output);
            Assert.Contains("dateOnlyValue: string", output);

            // ExtendedTypes - collections
            Assert.Contains("genericDictionary: Record<string, any>", output);
            Assert.Contains("genericList: string[]", output);
            Assert.Contains("hashSetValue: number[]", output);

            // ExtendedTypes - tuples
            Assert.Contains("tripleTuple: [number, string, boolean]", output);
            Assert.Contains("namedValueTuple: [string, number]", output);

            // ExtendedTypes - nullable types
            Assert.Contains("nullableInt?: number", output);
            Assert.Contains("nullableGuid?: string", output);

            // Cleanup
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
