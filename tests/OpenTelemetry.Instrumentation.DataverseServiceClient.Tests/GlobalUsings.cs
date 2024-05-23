global using Xunit;
global using AutoBogus;
global using FluentAssertions;
global using NSubstitute;

// Needed because Sdk.CreateTracerProviderBuilder().AddInMemoryExporter(exportedItems) creates issues when reading from multiple threads
[assembly: CollectionBehavior(DisableTestParallelization = true)]
