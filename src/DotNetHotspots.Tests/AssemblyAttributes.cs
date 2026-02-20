using Xunit;

// OutputServiceTests and ProgramTests both redirect Console.Out.
// Disable parallel test execution to prevent them from interfering with each other.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
