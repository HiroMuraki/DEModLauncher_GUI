using System.Collections.Immutable;

namespace DEModLauncher_GUI
{
    public record class ModPackConflictInfo
    {
        public int TotalCount { get; init; }
        public int ValidCount { get; init; }
        public int ConflictedCount { get; init; }
        public ImmutableDictionary<string, string[]> ConflictedFiles { get; init; } = ImmutableDictionary<string, string[]>.Empty;
    }
}
