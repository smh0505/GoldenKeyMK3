using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Websocket.Client;

namespace GoldenKeyMK3.Script
{
    public struct SongRequest
    {
        public string Name;
        public Queue<(int Idx, string Title)> Songs;

        public SongRequest(string name)
        {
            Name = name;
            Songs = new Queue<(int Idx, string Title)>();
        }
    }

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

        private static ConcurrentBag<SongRequest> _requests = new ConcurrentBag<SongRequest>();
        private static bool _switch;

        public static async void Connect()
        {
            using (Client = new WebsocketClient(new Uri("wss://irc-ws.chat.twitch.tv:443")))
            {
                Client.ReconnectTimeout = null;
                Client.MessageReceived.Subscribe(msg =>
                {
                    if (msg.ToString().StartsWith("PING")) Client.Send("PONG :tmi.twitch.tv");
                    if (msg.ToString().Contains("!픽"))
                    {
                        var text = msg.ToString();
                        var objects = Regex.Split(text, @"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
                        var tags = objects[0].Split(';');
                    }
                });
                await Client.Start();
                Client.Send("CAP REQ :twitch.tv/commands twitch.tv/tags");
                Client.Send("NICK justinfan1234");
                Client.Send("JOIN #arpa__");
                ExitEvent.WaitOne();
            }
        }
=======
namespace GoldenKeyMK3.Script
{
    public class Chat
    {

>>>>>>> 5c437a6ef03ea540de6a76fdd6cb43578637faf3
    }
}