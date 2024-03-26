using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DEModLauncher_GUI.ViewModel;

internal record DEModInfoViewModel : IDeepCloneable<DEModInfoViewModel>
{
    [DataContract]
    private class Info
    {
        [DataMember(Name = "name")]
        public string Name { get; init; } = "";
        [DataMember(Name = "description")]
        public string Description { get; init; } = "";
        [DataMember(Name = "version")]
        public string Version { get; init; } = "";
        [DataMember(Name = "author")]
        public string Author { get; init; } = "";
        [DataMember(Name = "requiredVersion")]
        public string RequiredVersion { get; init; } = "";

        public static Info Load(Stream stream)
        {
            return _serializer.ReadObject(stream) as Info ?? new Info();
        }

        public static Info Load(string fileName)
        {
            using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return Load(reader);
            }
        }


        #region NonPublic
        private static readonly DataContractJsonSerializer _serializer = new(typeof(Info));
        #endregion
    }

    public string Name { get; init; } = "?";

    public string Description { get; init; } = "?";

    public string Author { get; init; } = "?";

    public string Version { get; init; } = "?";

    public string RequiredVersion { get; init; } = "?";

    public override string ToString()
    {
        string output = $"名称：{Name}\n";
        output += $"作者：{Author}\n";
        output += $"版本：{Version}\n";
        output += $"描述：{ImproveReadability(Description)}";
        return output;
    }

    public DEModInfoViewModel GetDeepClone()
    {
        return new DEModInfoViewModel()
        {
            Name = Name,
            Description = Description,
            Author = Author,
            Version = Version,
            RequiredVersion = RequiredVersion
        };
    }

    public static DEModInfoViewModel Read(string path)
    {
        // 读取压缩包中的EternalMod.json
        ZipArchive zipArchive = ZipFile.Open(path, ZipArchiveMode.Read);
        ZipArchiveEntry? entry = zipArchive.GetEntry("EternalMod.json")
            ?? throw new FileNotFoundException("未找到EternalMod.json");

        // 读取
        var t = Info.Load(entry.Open());
        // 读取数据并实例化，返回
        return new DEModInfoViewModel()
        {
            Name = t.Name,
            Description = t.Description,
            Author = t.Author,
            Version = t.Version,
            RequiredVersion = t.RequiredVersion
        };
    }

    #region NonPublic
    private static string ImproveReadability(string source)
    {
        var output = new StringBuilder();
        string[] words = source.Split(' ', '\t', '\n');
        for (int i = 0; i < words.Length; i++)
        {
            output.Append($"{words[i]} ");
            if (i % 10 == 9)
            {
                output.Append('\n');
                output.Append("          ");
            }
        }
        return output.ToString();
    }
    #endregion
}