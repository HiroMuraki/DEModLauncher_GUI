using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEModLauncher_GUI {
    public readonly struct ModPackConflictInformation {
        public readonly int TotalCount { get; }
        public readonly int ValidCount { get; }
        public readonly int ConflictedCount { get; }
        public readonly Dictionary<string, List<string>> ConflictedFiles { get; }

        public ModPackConflictInformation(int totalCount, int validCount, int conflictedCount, Dictionary<string, List<string>> confliectedFiles) {
            TotalCount = totalCount;
            ValidCount = validCount;
            ConflictedCount = conflictedCount;
            ConflictedFiles = confliectedFiles;
        }
    }
}
