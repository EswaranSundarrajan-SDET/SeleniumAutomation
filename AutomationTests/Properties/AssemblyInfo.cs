using NUnit.Framework;

// Run independent child tests with up to two workers by default.
[assembly: Parallelizable(ParallelScope.Children)]
[assembly: LevelOfParallelism(2)]
