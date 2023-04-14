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

    public struct CheckPointQueues
    {
        public (string Name, string Theme, string Song)[] Requests { get; set; }
        public (string Name, string Theme, string Song)[] IslandRequests { get; set; }
        public (string Name, string Song)[] UsedList { get; set; }
        public (string Name, int Count)[] Inventory { get; set; }
    }

    public struct CheckPointBoard
    {
        public string[] Board { get; set; }
        public string[] BackUpBoard { get; set; }
        public int[] GoldenKeys { get; set; }
        public Dictionary<string, Color> ThemePairs { get; set; }
        public bool IsClockwise { get; set; }
        public int Laps { get; set; }
        public TimeSpan Time { get; set; }
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

        public static CheckPointQueues LoadQueues()
        {
            // Read log file
            var r = new StreamReader("cp_queues.yml");
            var data = r.ReadToEnd();
            r.Close();
            
            // Deserialize
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var checkPointQueues = deserializer.Deserialize<CheckPointQueues>(data);

            return checkPointQueues;
        }
        
        public static CheckPointBoard LoadBoard()
        {
            // Read log file
            var r = new StreamReader("cp_board.yml");
            var data = r.ReadToEnd();
            r.Close();
            
            // Deserialize
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var checkPointBoard = deserializer.Deserialize<CheckPointBoard>(data);

            return checkPointBoard;
        }

        public static void SaveQueues(IEnumerable<(string Name, string Theme, string Song, double Time)> requests, 
            IEnumerable<(string Name, string Theme, string Song, double Time)> islandRequests,
            IEnumerable<(string, string)> usedList, IEnumerable<(string, int)> inventory)
        {
            // Create Log File
            var filename = "cp_queues.yml";
            var checkPointQueues = new CheckPointQueues()
            {
                Requests = requests.Select(x => (x.Name, x.Theme, x.Song)).ToArray(),
                IslandRequests = islandRequests.Select(x => (x.Name, x.Theme, x.Song)).ToArray(),
                UsedList = usedList.ToArray(),
                Inventory = inventory.ToArray()
            };

            // Serialize Log
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var output = serializer.Serialize(checkPointQueues);

            // Write Log File
            using var file = File.Create(filename);
            var w = new StreamWriter(file);
            w.Write(output);
            w.Close();
        }

        public static void SaveBoard(string[] board, string[] backUpBoard, int[] goldenKeys,
            Dictionary<string, Color> themePairs, bool isClockwise, int laps, TimeSpan time)
        {
            // Create Log File
            var filename = "cp_board.yml";
            var checkPointBoard = new CheckPointBoard()
            {
                Board = board,
                BackUpBoard = backUpBoard,
                GoldenKeys = goldenKeys,
                ThemePairs = themePairs,
                IsClockwise = isClockwise,
                Laps = laps,
                Time = time
            };
            
            // Serialize Log
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var output = serializer.Serialize(checkPointBoard);

            // Write Log File
            using var file = File.Create(filename);
            var w = new StreamWriter(file);
            w.Write(output);
            w.Close();
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