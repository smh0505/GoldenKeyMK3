using Raylib_cs;
using static Raylib_cs.Raylib;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GoldenKeyMK3.Script
{
    public struct Setting
    {
        public string Key { get; }
        public List<string> Values { get; }

        public Setting(string key, List<string> values)
        {
            Key = key;
            Values = values;
        }
    }

    public static class SaveLoad
    {
        public static List<WheelPanel> DefaultOptions;
        private static Setting _setting;

        // Loading

        public static void LoadSetting(Login login)
        {
            // Read Default File
            var r = new StreamReader("default.yml");
            var data = r.ReadToEnd();
            r.Close();

            // Deserialize Setting
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _setting = deserializer.Deserialize<Setting>(data);

            // Insert Setting
            login.Input = string.IsNullOrEmpty(_setting.Key) ? string.Empty : _setting.Key;
            DefaultOptions = _setting.Values == null ? new List<WheelPanel>() : LoadPanels();
        }

        public static List<WheelPanel> LoadLog(string filename)
        {
            var r = new StreamReader(filename);
            var data = r.ReadToEnd();
            r.Close();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<List<WheelPanel>>(data);
        }

        private static List<WheelPanel> LoadPanels()
        {
            var panels = new List<WheelPanel>();
            var rnd = new Random();

            foreach (var option in _setting.Values)
            {
                var id = panels.FindIndex(x => x.Name == option);
                if (id != -1)
                {
                    var newOption = new WheelPanel(panels[id].Name, panels[id].Count + 1, panels[id].Color);
                    panels.RemoveAt(id);
                    panels.Insert(id, newOption);
                }
                else
                {
                    var newOption = new WheelPanel(option, 1, ColorFromHSV(rnd.NextSingle() * 360, 0.5f, 1));
                    panels.Add(newOption);
                }
            }
            return panels;
        }

        public static Dictionary<string, string[]> LoadTopics(string filename)
        {
            var r = new StreamReader(filename);
            var data = r.ReadToEnd();
            r.Close();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<Dictionary<string, string[]>>(data);
        }

        // Saving

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
            var optionList = serializer.Serialize(wheel.Options);

            // Write Log File
            using var file = File.Create(filename);
            var w = new StreamWriter(file);
            w.Write(optionList);
            w.Close();
        }
    }
}