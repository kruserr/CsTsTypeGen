using System;
using System.IO;
using Xunit;
using CsTsTypeGen.Core;

namespace CsTsTypeGen.Tests.Tests
{
    public class CrossNamespaceReferencesTests : TestBase
    {
        [Fact]
        public void Test_CrossNamespaceReferences()
        {
            // Create a MetricsShared.Service class in one namespace
            File.WriteAllText(Path.Combine(_tempDir, "Models", "Service.cs"), @"
                namespace MetricsShared
                {
                    public class Service
                    {
                        public string Name { get; set; }
                        public string Status { get; set; }
                    }
                }
            ");

            // Create a class in a different namespace that references the Service class
            File.WriteAllText(Path.Combine(_tempDir, "Models", "MetricsSample.cs"), @"
                namespace Global
                {
                    using MetricsShared;

                    public class MetricsSample
                    {
                        public string Timestamp { get; set; }
                        public string Status { get; set; }
                        public string Uptime { get; set; }
                        public string CpuUsage { get; set; }
                        public string RamUsage { get; set; }
                        public string RamTotal { get; set; }
                        public DiskSample[] Disks { get; set; }
                        public MetricsShared.Service[] Services { get; set; }
                    }

                    public class DiskSample
                    {
                        public string Path { get; set; }
                        public string Usage { get; set; }
                        public string Total { get; set; }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify that the cross-namespace reference is properly qualified
            Assert.Contains("services: MetricsShared.Service[]", output);

            // Verify that the Service class is defined in the MetricsShared namespace
            Assert.Contains("declare namespace MetricsShared", output);
            Assert.Contains("export interface Service", output);
            
            // Verify that DiskSample in the same namespace is referenced without qualifier
            Assert.Contains("disks: DiskSample[]", output);
        }

        [Fact]
        public void Test_EnumAcrossNamespaces()
        {
            // Create an enum in one namespace
            File.WriteAllText(Path.Combine(_tempDir, "Enums", "Status.cs"), @"
                namespace MyApp.Enums
                {
                    public enum Status
                    {
                        Active,
                        Inactive,
                        Pending
                    }
                }
            ");

            // Create a class in a different namespace that references the enum
            File.WriteAllText(Path.Combine(_tempDir, "Models", "StatusReport.cs"), @"
                namespace MyApp.Models
                {
                    using MyApp.Enums;

                    public class StatusReport
                    {
                        public string Name { get; set; }
                        public Status CurrentStatus { get; set; }
                        public MyApp.Enums.Status PreviousStatus { get; set; }
                    }
                }
            ");

            // Run the processor
            int result = TypeGenProcessor.Run(_tempDir, _outputFile);
            
            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(_outputFile), "Expected output TypeScript file to be created.");

            string output = File.ReadAllText(_outputFile);

            // Verify that the enum is properly referenced without the namespace qualifier
            // This checks our special case for enums across namespaces
            Assert.Contains("currentStatus: Status", output);
            Assert.Contains("previousStatus: Status", output);

            // Verify that the Status enum is defined
            Assert.Contains("export type Status =", output);
        }
    }
}