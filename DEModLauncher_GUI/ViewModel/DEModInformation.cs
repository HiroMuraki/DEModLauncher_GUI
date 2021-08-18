using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace DEModLauncher_GUI.ViewModel {
    public record DEModInformation : IModInformation {
        private static readonly Regex nameReg = new Regex("(?<=\"name\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex descriptionReg = new Regex("(?<=\"description\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex authorReg = new Regex("(?<=\"author\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex versionReg = new Regex("(?<=\"version\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex requiredVersion = new Regex("(?<=\"requiredVersion\":)[\\s\\S]+(?=[,]*)");

        private string _name;
        private string _description;
        private string _author;
        private string _version;
        private string _requiredVersion;

        public DEModInformation() {
            _name = "?";
            _description = "?";
            _author = "?";
            _version = "?";
            _requiredVersion = "?";
        }

        public string Name {
            get {
                return _name;
            }
            init {
                _name = value;
            }
        }
        public string Description {
            get {
                return _description;
            }
            init {
                _description = value;
            }
        }
        public string Author {
            get {
                return _author;
            }
            init {
                _author = value;
            }
        }
        public string Version {
            get {
                return _version;
            }
            init {
                _version = value;
            }
        }
        public string RequiredVersion {
            get {
                return _requiredVersion;
            }
            init {
                _requiredVersion = value;
            }
        }

        public override string ToString() {
            string output = $"名称：{_name}\n";
            output += $"作者：{_author}\n";
            output += $"版本：{_version}\n";
            output += $"游戏版本：{_requiredVersion}\n";
            output += $"描述：{ImproveReadability(_description)}";
            return output;
        }
        public DEModInformation GetDeepCopy() {
            DEModInformation copy = new DEModInformation();
            copy._name = _name;
            copy._description = _description;
            copy._author = _author;
            copy._version = _version;
            copy._requiredVersion = _requiredVersion;
            return copy;
        }

        public static DEModInformation Read(string path) {
            // 读取压缩包中的EternalMod.json
            ZipArchive zipArchive = ZipFile.Open(path, ZipArchiveMode.Read);
            ZipArchiveEntry entry = zipArchive.GetEntry("EternalMod.json");
            if (entry == null) {
                throw new FileNotFoundException("未找到EternalMod.json");
            }

            // 读取
            DEModInformation attribute = new DEModInformation();
            using (StreamReader reader = new StreamReader(entry.Open())) {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine().Trim();
                    if (line.StartsWith("\"name\"")) {
                        attribute._name = nameReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"description\"")) {
                        attribute._description = descriptionReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"author\"")) {
                        attribute._author = authorReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"version\"")) {
                        attribute._version = versionReg.Match(line).Value;
                    }
                    else if (line.StartsWith("\"requiredVersion\"")) {
                        attribute._requiredVersion = requiredVersion.Match(line).Value;
                    }
                }
            }
            // 读取数据并实例化，返回
            return attribute;
        }


        private static string ImproveReadability(string source) {
            StringBuilder output = new StringBuilder();
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