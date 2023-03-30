using Raylib_cs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GoldenKeyMK3.Script
{
    public struct Setting
    {
        public string Key { get; set; }
        public List<string> Values { get; set; }

        public Setting()
        {
            Key = string.Empty;
            Values = new List<string>();
        }

        public Setting(string key, List<string> values)
        {
            Key = key;
            Values = values;
        }
    }
    
    public struct Panel
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public Color Color { get; set; }

        public Panel(string name, int count, Color color)
        {
            Name = name;
            Count = count;
            Color = color;
        }
    }
    
    public static class SaveLoad
    {
        public static Setting LoadSetting()
        {
            // Read default.yml
            var r = new StreamReader("default.yml");
            var data = r.ReadToEnd();
            r.Close();
            
            // Deserialize
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var setting = deserializer.Deserialize<Setting>(data);

            setting.Values ??= new List<string>();
            return setting;
        }

        public static List<Panel> LoadPanels(string filename)
        {
            // Read log file
            var r = new StreamReader(filename);
            var data = r.ReadToEnd();
            r.Close();
            
            // Deserialize
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var panels = deserializer.Deserialize<List<Panel>>(data);

            return panels;
        }

        public static Dictionary<string, string[]> LoadThemes()
        {
            // Read log file
            var r = new StreamReader("board.yml");
            var data = r.ReadToEnd();
            r.Close();
            
            // Deserialize
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var themes = deserializer.Deserialize<Dictionary<string, string[]>>(data);

            return themes;
        }

        public static void SaveLog(Wheel wheel)
        {
            // Create Log File
            Directory.CreateDirectory("Logs");
            var time = DateTime.Now.ToString("s").Replace(':', '-').Replace('T', '-');
            var filename = $"Logs/log-{time}.yml";

            // Serialize Log
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var optionList = serializer.Serialize(wheel.Panels);

            // Write Log File
            using var file = File.Create(filename);
            var w = new StreamWriter(file);
            w.Write(optionList);
            w.Close();
        }
    }
}