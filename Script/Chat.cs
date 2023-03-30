using System.Text.RegularExpressions;
using Websocket.Client;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Chat : IDisposable
    {
        private readonly Poll _poll;
        private readonly Board _board;
        
        private readonly ManualResetEvent _exitEvent;
        private WebsocketClient _client;
        
        public Chat(Poll poll, Board board)
        {
            _poll = poll;
            _board = board;
            _exitEvent = new ManualResetEvent(false);
        }
        
        public async void Connect()
        {
            using (_client = new WebsocketClient(new Uri("wss://irc-ws.chat.twitch.tv:443")))
            {
                _client.ReconnectTimeout = null;
                _client.MessageReceived.Subscribe(msg =>
                {
                    if (msg.ToString().StartsWith("PING")) _client.Send("PONG :tmi.twitch.tv");
                    if (msg.ToString().Contains("!픽")) UpdateRequest(msg.ToString());
                });
                
                await _client.Start();
                _client.Send("CAP REQ :twitch.tv/commands twitch.tv/tags");
                _client.Send("NICK justinfan5678");
                _client.Send("JOIN #arpa__");
                _exitEvent.WaitOne();
            }
        }

        public void Dispose()
        {
            _exitEvent.Set();
            GC.SuppressFinalize(this);
        }
        
        // Private Methods

        private void UpdateRequest(string msg)
        {
            var re = new Regex(@"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
            var objects = re.Match(msg).Groups;

            var cp1 = IsValid(objects["content"].ToString(), out var theme, out var song, out var island);

            var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
            var cp2 = !_poll.UsedList.Select(x => x.Name).Contains(name);

            var cp3 = island 
                ? !(GetTime() - _poll.Requests.FindLast(x => x.Name == name).Timestamp < 30)
                : !(GetTime() - _poll.IslandRequests.FindLast(x => x.Name == name).Timestamp < 30);

            var isSuccessful = cp1 && cp2 && cp3;
            _poll.Sequence = _poll.Sequence.Add(new PollResponse(name, song, isSuccessful));
            if (_poll.Sequence.Count > 50) _poll.Sequence = _poll.Sequence.RemoveAt(0);

            if (!isSuccessful) return;
            if (!island)
            {
                _poll.Requests = _poll.Requests.Add(new PollRequest(name, theme, song, GetTime()));
                if (_poll.Requests.Count(x => x.Name == name) > 3) 
                    _poll.Requests = _poll.Requests.Remove(_poll.Requests.First(x => x.Name == name));
            }
            else
            {
                _poll.IslandRequests = _poll.IslandRequests.Add(new PollRequest(name, theme, song, GetTime()));
                if (_poll.IslandRequests.Count(x => x.Name == name) > 1)
                    _poll.IslandRequests = _poll.IslandRequests.Remove(_poll.IslandRequests.First(x => x.Name == name));
            }
        }
        
        private bool IsValid(string text, out string theme, out string song, out bool island)
        {
            var content = text.Split(" ", 3);

            theme = song = string.Empty;
            island = false;
            if (content.Length < 3) return false;
            if (!int.TryParse(content[1], out var idx)) return false;
            if (idx is <= 0 or 13 or > 25) return false;

            theme = _board.CurrBoard[idx];
            song = content[2][..^1];
            if (idx is 7 or 20) island = true;
            return !_board.GoldenKeys.Contains(idx);
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
    }
}