using System;
using System.IO;
using Xunit;

namespace CsTsTypeGen.Tests.Tests
{
    public class StandardLibraryTypesTests : TestBase
    {
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
            Assert.Contains("jaggedArray: number[][]", output);

            // Verify delegate types
            Assert.Contains("funcValue: (p0: number) => string", output);
            Assert.Contains("actionValue: (p0: string) => void", output);
            Assert.Contains("predicateValue: (value: number) => boolean", output);

            // Verify dynamic and object types
            Assert.Contains("dynamicValue: any", output);
            Assert.Contains("objectValue: any", output);
        }
    }
}