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

        private readonly Texture2D _baseBoard;
        private readonly Texture2D _centerBoard;
        private readonly Texture2D _resultBoard;

        private bool _switch;
        private PollState _state;
        
        private ImmutableList<(string Name, int Block, string Song)> _requests =
            ImmutableList<(string, int, string)>.Empty;
        private ImmutableList<(string Name, string Song)> _usedList = 
            ImmutableList<(string Name, string Song)>.Empty;

        public Chat()
        {
            _baseBoard = LoadTexture("Resource/baseboard.png");
            _centerBoard = LoadTexture("Resource/alert.png");
            _resultBoard = LoadTexture("Resource/result.png");
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
            
            UnloadTexture(_baseBoard);
            UnloadTexture(_centerBoard);
            UnloadTexture(_resultBoard);
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

        private static bool IsValid(string text)
        {
            var content = text.Split(' ', 3);

            if (content.Length < 3) return false;
            if (!int.TryParse(content[1], out var idx)) return false;
            if (idx is < 1 or > 24) return false;
            if (Board.Contains(idx)) return false;

            return true;
        }

        private static string GetUsername(string tags, string source)
        {
            // display-name
            var name = tags.Split(';')
                           .Select(x => x.Split('='))
                           .ToDictionary(x => x[0], x => x[1])["display-name"];

            // username
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
            => _requests.FindAll(x => x.Idx == idx).Select(x => (x.Name, x.Song)).ToList();

        // Conditions

        private static bool IsIdle(bool shutdownRequest)
            => !shutdownRequest && _state == PollState.Idle;

        private static bool IsReadyToSelect(bool shutdownRequest)
            => !shutdownRequest && _state != PollState.Active && _idx >= 0 && FindAllSongs(_idx + 1).Count > 0;
    }
}