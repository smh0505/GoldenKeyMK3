using System.Text.RegularExpressions;
using Websocket.Client;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum ChatState
    {
        Successful = 0,
        Failed,
        Reconnecting
    }
    
    public class Chat : IDisposable
    {
        private readonly Poll _poll;
        private int[] _goldenKeys;
        private string[] _board; 
        
        private readonly ManualResetEvent _exitEvent;
        private WebsocketClient _client;
        
        public Chat(Poll poll)
        {
            _poll = poll;
            _exitEvent = new ManualResetEvent(false);
        }
        
        public async void Connect()
        {
            using (_client = new WebsocketClient(new Uri("wss://irc-ws.chat.twitch.tv:443")))
            {
                _client.ReconnectTimeout = TimeSpan.FromMinutes(15);
                _client.MessageReceived.Subscribe(msg =>
                {
                    if (msg.ToString().StartsWith("PING")) _client.Send("PONG :tmi.twitch.tv");
                    if (msg.ToString().Contains("!픽")) UpdateRequest(msg.ToString());
                });
                _client.ReconnectionHappened.Subscribe(info =>
                {
                    Console.WriteLine($"Reconnection Happened: {info.Type}");
                    _poll.Sequence = _poll.Sequence.Add(("연결중...", ChatState.Reconnecting));
                    _client.Send("CAP REQ :twitch.tv/commands twitch.tv/tags");
                    _client.Send("NICK justinfan3649");
                    _client.Send("JOIN #arpa__");
                });
                
                await _client.Start();
                _exitEvent.WaitOne();
            }
        }

        public void Update(int[] goldenKeys, string[] board)
        {
            _goldenKeys = goldenKeys;
            _board = board;
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

            // CheckPoint 1: if the format is valid and the number is in range
            // Format: !픽 [int: 1 to 25, excluding 13] [string]
            var cp1 = IsValid(objects["content"].ToString(), out var theme, out var song, out var island);

            // CheckPoint 2: if the name is not in _poll.UsedList
            var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
            var cp2 = !_poll.UsedList.Select(x => x.Name).Contains(name);

            // CheckPoint 3: if the timestamp from the last request has the difference > 30 sec
            var cp3 = island
                ? GetTime() - _poll.IslandRequests.FindLast(x => x.Name == name).Time > 30
                : GetTime() - _poll.Requests.FindLast(x => x.Name == name).Time > 30;

            // Check if all checkpoints are true
            var isSuccessful = cp1 && cp2 && cp3;
            _poll.Sequence = _poll.Sequence.Add((name, isSuccessful ? ChatState.Successful : ChatState.Failed));
            if (_poll.Sequence.Count > 10) _poll.Sequence = _poll.Sequence.RemoveAt(0);

            // If all true, then insert the song into _poll.(Island)Requests
            if (!isSuccessful) return;
            if (!island)
            {
                _poll.Requests = _poll.Requests.Add((name, theme, song, GetTime()));
                if (_poll.Requests.Count(x => x.Name == name) > 3) 
                    _poll.Requests = _poll.Requests.Remove(_poll.Requests.First(x => x.Name == name));
            }
            else
            {
                _poll.IslandRequests = _poll.IslandRequests.Add((name, theme, song, GetTime()));
                if (_poll.IslandRequests.Count(x => x.Name == name) > 1)
                    _poll.IslandRequests = _poll.IslandRequests.Remove(_poll.IslandRequests.First(x => x.Name == name));
            }
        }
        
        private bool IsValid(string text, out string theme, out string song, out bool island)
        {
            var content = text.Split(" ", 3);

            theme = song = string.Empty;
            island = false;
            if (content.Length < 3) return false;   // Format incorrect
            if (!int.TryParse(content[1], out var idx)) return false;   // Index not int
            if (idx is <= 0 or 13 or > 25) return false;    // Index out of range

            theme = _board[idx];
            song = content[2][..^1];
            if (idx is 7 or 20) island = true;
            return !_goldenKeys.Contains(idx);
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