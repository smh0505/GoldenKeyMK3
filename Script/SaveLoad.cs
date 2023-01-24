using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GoldenKeyMK3.Script
{
    public struct Setting
    {
        public string Key;
        public List<string> Values;
    }

    public class SaveLoad
    {
        private static Setting _setting;

        public static void LoadSetting()
        {
            if (File.Exists("default.yml"))
            {
                // Read Default File
                StreamReader r = new StreamReader("default.yml");
                var data = r.ReadToEnd();
                r.Close();

                // Deserialize Setting
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                _setting = deserializer.Deserialize<Setting>(data);

                // Insert Setting
                Login.Input = string.IsNullOrEmpty(_setting.Key) ? string.Empty : _setting.Key;
                foreach (var option in _setting.Values)
                    Wheel.Waitlist.Add(option);
            }
        }

        public static List<WheelPanel> LoadLog(string filename)
        {
            StreamReader r = new StreamReader(filename);
            var data = r.ReadToEnd();
            r.Close();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<List<WheelPanel>>(data);
        }

        public static void SaveLog()
        {
            if (Wheel.Options.Any())
            {
                // Create Log File
                Directory.CreateDirectory("Logs");
                var time = DateTime.Now.ToString("s").Replace(':', '-').Replace('T', '-');
                var filename = $"Logs/log-{time}.yml";

                // Serialize Log
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var optionList = serializer.Serialize(Wheel.Options);

                // Write Log File
                using FileStream file = File.Create(filename);
                StreamWriter w = new StreamWriter(file);
                w.Write(optionList);
                w.Close();
            }
        }
    }
}