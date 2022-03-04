using System.Collections.Generic;

namespace DEModLauncher_GUI {
    public record ModPackConflictInfo {
        public int TotalCount { get; init; }
        public int ValidCount { get; init; }
        public int ConflictedCount { get; init; }
        public Dictionary<string, List<string>> ConflictedFiles { get; init; } = new Dictionary<string, List<string>>();
    }
}
