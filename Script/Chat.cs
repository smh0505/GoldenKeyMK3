using System.Collections.Immutable;
using System.Numerics;
using System.Text.RegularExpressions;
using Websocket.Client;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum PollState
    {
        Idle = 0,
        Active,
        Result
    }

    public class Chat
    {
        private readonly ManualResetEvent _exitEvent = new (false);
        private WebsocketClient _client;

        private readonly Board _board;
        private string[] _topics;

        private bool _switch;
        private PollState _state;
        
        private ImmutableList<(string Name, int Block, string Song)> _requests =
            ImmutableList<(string, int, string)>.Empty;
        private ImmutableList<(string Name, string Song)> _usedList = 
            ImmutableList<(string, string)>.Empty;

        public Chat(Board board)
        {
            _board = board;
            _switch = false;
            _state = PollState.Idle;
            _topics = _board.GetTopics();
        }

        public async void Connect()
        {
            using (_client = new WebsocketClient(new Uri("wss://irc-ws.chat.twitch.tv:443")))
            {
                _client.ReconnectTimeout = null;
                _client.MessageReceived.Subscribe(msg =>
                {
                    if (msg.ToString().StartsWith("PING")) _client.Send("PONG :tmi.twitch.tv");
                    if (_switch && _state == PollState.Idle && msg.ToString().Contains("!픽"))
                        UpdateRequest(msg.ToString());
                });
                await _client.Start();
                _client.Send("CAP REQ :twitch.tv/commands twitch.tv/tags");
                _client.Send("NICK justinfan1234");
                _client.Send("JOIN #arpa__");
                _exitEvent.WaitOne();
            }
        }

        public void Dispose()
        {
            _exitEvent.Set();
        }

        // UIs

        

        // Main Methods

        private void UpdateRequest(string msg)
        {
            var re = new Regex(@"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
            var objects = re.Match(msg).Groups;

            if (!IsValid(objects["content"].ToString())) return;
            
            var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
            if (_usedList.Select(x => x.Name).Contains(name)) return;
            
            var order = GetOrder(objects["content"].ToString());
            
            _requests = _requests.Add((name, order.Item1, order.Item2));
            if (_requests.Count(x => x.Name == name) > 3)
                _requests = _requests.Remove(_requests.First(x => x.Name == name));
        }

        private bool IsValid(string text)
        {
            var content = text.Split(' ', 3);

            if (content.Length < 3) return false;
            if (!int.TryParse(content[1], out var idx)) return false;
            if (idx is < 1 or 13 or > 25) return false;
            return !_board.GetGoldenKeys().Contains(idx - 1);
        }

        private static string GetUsername(string tags, string source)
        {
            // display-name
            var name = tags.Split(';')
                .Select(x => x.Split('='))
                .ToDictionary(x => x[0], x => x[1])["display-name"];
            if (!string.IsNullOrEmpty(name)) return name;
            
            // username
            var re = new Regex(@"^(?:(?<nick>[^\s]+?)!(?<user>[^\s]+?)@)?(?<host>[^\s]+)$");
            name = re.Match(source).Groups["nick"].ToString();
            return name;
        }

        private static (int, string) GetOrder(string text)
        {
            var content = text.Split(' ', 3);
            var idx = Convert.ToInt32(content[1]);
            return idx >= 13 ? (idx + 1, content[2]) : (idx, content[2]);
        }

        private IEnumerable<(string, string)> FindAllSongs(int idx)
            => _requests.FindAll(x => x.Block == idx).Select(x => (x.Name, x.Song)).ToList();

        private void Modify()
        {
            var temp = _board.GetTopics();
            var newRequests = new List<(string, int, string)>();
            for (var i = 0; i < _topics.Length; i++)
            {
                if (i is < 0 or 7 or 13 or 20 or > 25) continue;
                if (_topics[i] == "황금열쇠") continue;
                
                var idx = Array.IndexOf(temp, _topics[i]);
                var group = _requests.FindAll(x => x.Block == i).ToList();
                _requests = _requests.RemoveAll(x => group.Contains(x));
                if (idx != -1)
                    newRequests.AddRange(group.Select(x => (x.Name, idx, x.Song)));
            }

            _requests = _requests.AddRange(newRequests);
            _topics = temp;
        }

        // Conditions

        private bool IsIdle(bool shutdownRequest)
            => !shutdownRequest && _state == PollState.Idle;

        private bool IsReadyToSelect(bool shutdownRequest, int idx)
            => !shutdownRequest && _state != PollState.Active && FindAllSongs(idx + 1).Any();
    }
}