using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DEModLauncher_GUI.ViewModel {
    public record DEModInformation {
        public string Name { get; init; } = "?";
        public string Description { get; init; } = "?";
        public string Author { get; init; } = "?";
        public string Version { get; init; } = "?";
        public string RequiredVersion { get; init; } = "?";

        public override string ToString() {
            string output = $"名称：{Name}\n";
            output += $"作者：{Author}\n";
            output += $"版本：{Version}\n";
            output += $"描述：{ImproveReadability(Description)}";
            return output;
        }
        public DEModInformation GetDeepCopy() {
            return new DEModInformation() {
                Name = Name,
                Description = Description,
                Author = Author,
                Version = Version,
                RequiredVersion = RequiredVersion
            };
        }

        public static DEModInformation Read(string path) {
            // 读取压缩包中的EternalMod.json
            var zipArchive = ZipFile.Open(path, ZipArchiveMode.Read);
            var entry = zipArchive.GetEntry("EternalMod.json");
            if (entry == null) {
                throw new FileNotFoundException("未找到EternalMod.json");
            }

            // 读取
            var attribute = new DEModInformation();
            using (var reader = new StreamReader(entry.Open())) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine().Trim();
                    if (line.StartsWith("\"name\"")) {
                        attribute.Name = nameReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"description\"")) {
                        attribute.Description = descriptionReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"author\"")) {
                        attribute.Author = authorReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"version\"")) {
                        attribute.Version = versionReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"requiredVersion\"")) {
                        attribute.RequiredVersion = requiredVersion.Match(line).Value;
                    }
                }
            }
            // 读取数据并实例化，返回
            return attribute;
        }

        private static readonly Regex nameReg = new Regex("(?<=\"name\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex descriptionReg = new Regex("(?<=\"description\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex authorReg = new Regex("(?<=\"author\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex versionReg = new Regex("(?<=\"version\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex requiredVersion = new Regex("(?<=\"requiredVersion\":)[\\s\\S]+(?=[,]*)");
        private static string ImproveReadability(string source) {
            var output = new StringBuilder();
            string[] words = source.Split(' ', '\t', '\n');
            for (int i = 0; i < words.Length; i++) {
                output.Append($"{words[i]} ");
                if (i % 10 == 9) {
                    output.Append('\n');
                    output.Append("          ");
                }
            }
            return output.ToString();
        }
    }
}