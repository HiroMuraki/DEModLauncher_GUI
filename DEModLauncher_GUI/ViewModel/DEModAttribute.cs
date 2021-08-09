using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DEModLauncher_GUI.ViewModel {
    public record DEModAttribute {
        private static readonly Regex nameReg = new Regex("(?<=\"name\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex descriptionReg = new Regex("(?<=\"description\":\")[\\s\\S]+(?=\"[,]*)");
        private static readonly Regex authorReg = new Regex("(?<=\"author\":\")[\\s\\S]+(?=\"[,]*)");

        private string _name;
        private string _description;
        private string _author;

        public DEModAttribute() {
            _name = "???";
            _description = "???";
            _author = "???";
        }

        public string Name {
            get {
                return _name;
            }
            private set {
                _name = value;
            }
        }
        public string Description {
            get {
                return _description;
            }
            private set {
                _description = value;
            }
        }
        public string Author {
            get {
                return _author;
            }

            private set {
                _author = value;
            }
        }

        public override string ToString() {
            string output = $"名称：{Name}\n";
            output += $"作者：{Author}";
            output += $"描述：{Description}\n";
            return output;
        }

        public static DEModAttribute Read(string path) {
            // 读取压缩包中的EternalMod.json
            ZipArchive zipArchive = ZipFile.Open(path, ZipArchiveMode.Read);
            ZipArchiveEntry entry = zipArchive.GetEntry("EternalMod.json");
            if (entry == null) {
                throw new FileNotFoundException("未找到EternalMod.json");
            }

            // 读取
            DEModAttribute attribute = new DEModAttribute();
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
                }
            }
            // 读取数据并实例化，返回
            return attribute;
        }
    }
}