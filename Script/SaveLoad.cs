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

        public static void LoadData()
        {
            if (File.Exists("default.yml"))
            {
                StreamReader r = new StreamReader("default.yml");
                var data = r.ReadToEnd();
                r.Close();

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                _setting = deserializer.Deserialize<Setting>(data);
                Login.Input = _setting.Key;
                foreach (var option in _setting.Values)
                    Wheel.Waitlist.Add(option);
            }
        }
    }
}