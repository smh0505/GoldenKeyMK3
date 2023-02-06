using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Websocket.Client;

namespace GoldenKeyMK3.Script
{
    public class Chat
    {
        public static WebsocketClient Client;
        public static ManualResetEvent ExitEvent = new ManualResetEvent(false);

        private static Dictionary<int, bool> _board = new Dictionary<int, bool>
        {
            {1, false}, {2, true}, {3, false}, {4, false}, {5, true}, {6, false}, // bottom line
            {7, false}, {8, true}, {9, false}, {10, true}, {11, false}, // left line
            {12, false}, {13, true}, {14, false}, {15, false}, {16, true}, {17, false}, // top line
            {18, false}, {19, true}, {20, false}, {21, true}, {22, false} // right line
        };

        private static ConcurrentDictionary<string, Queue<(int, string)>> _requests 
            = new ConcurrentDictionary<string, Queue<(int, string)>>();
        private static bool _switch;

        public static void DrawChat()
        {

        }

        public static async void Connect()
        {
            using (Client = new WebsocketClient(new Uri("wss://irc-ws.chat.twitch.tv:443")))
            {
                Client.ReconnectTimeout = null;
                Client.MessageReceived.Subscribe(msg =>
                {
                    if (msg.ToString().StartsWith("PING")) Client.Send("PONG :tmi.twitch.tv");
                    if (msg.ToString().Contains("!픽")) UpdateRequest(msg.ToString());
                });
                await Client.Start();
                Client.Send("CAP REQ :twitch.tv/commands twitch.tv/tags");
                Client.Send("NICK justinfan1234");
                Client.Send("JOIN #arpa__");
                ExitEvent.WaitOne();
            }
        }

        private static void UpdateRequest(string msg)
        {
            var re = new Regex(@"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
            var objects = re.Match(msg).Groups;

            if (IsValid(objects["content"].ToString()))
            {
                var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
                _requests[name] ??= new Queue<(int, string)>();
                _requests[name].Enqueue(GetOrder(objects["content"].ToString()));
                if (_requests[name].Count > 3) _requests[name].Dequeue();
            }
        }

        private static bool IsValid(string text)
        {
            var content = text.Split(' ', 3);
            int idx = 0;
            if (content.Length <= 3) return false;
            if (!int.TryParse(content[1], out idx)) return false;
            if (idx is < 1 or > 22) return false;
            if (_board[idx]) return false;
            return true;
        }

        private static string GetUsername(string tags, string source)
        {
            var name = tags.Split(';')
                           .Select(x => x.Split('='))
                           .ToDictionary(x => x[0], x => x[1])["display-name"];
            if (string.IsNullOrEmpty(name))
            {
                var re = new Regex(@"^(?:(?<nick>[^\s]+?)!(?<user>[^\s]+?)@)?(?<host>[^\s]+)$");
                name = re.Match(source).Groups["nick"].ToString();
            }
            return name;
        }

        private static (int, string) GetOrder(string text)
        {
            var content = text.Split(' ', 3);
            var idx = Convert.ToInt32(content[1]);
            return (idx, content[2]);
        }

        private static List<(string, string)> FindAllSongs(int idx)
        {
            var dict = _requests.ToDictionary(x => x.Key, x => x.Value);
            var output = new List<(string, string)>();
            foreach (var pair in dict)
                foreach (var x in pair.Value)
                    if (x.Item1 == idx)
                        output.Add((pair.Key, x.Item2));
            return output;
        }
    }
}