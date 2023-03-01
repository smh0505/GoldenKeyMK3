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

    public struct PollRequest
    {
        public string Name { get; }
        public string Topic { get; }
        public string Song { get; }
        public double Timestamp { get; }

        public PollRequest()
        {
            Name = string.Empty;
            Topic = string.Empty;
            Song = string.Empty;
            Timestamp = 0;
        }
        
        public PollRequest(string name, string topic, string song, double timestamp)
        {
            Name = name;
            Topic = topic;
            Song = song;
            Timestamp = timestamp;
        }
    }

    public struct PollResponse
    {
        public string Name { get; }
        public string Song { get; }
        public bool IsSuccessful { get; }

        public PollResponse(string name, string song, bool isSuccessful)
        {
            Name = name;
            Song = song;
            IsSuccessful = isSuccessful;
        }
    }

    public class Chat
    {
        private readonly ManualResetEvent _exitEvent = new (false);
        private WebsocketClient _client;

        private readonly Board _board;
        private readonly Scenes _scenes;

        private ImmutableList<PollRequest> _requests = ImmutableList<PollRequest>.Empty;
        private ImmutableList<PollRequest> _usedList = ImmutableList<PollRequest>.Empty;
        private ImmutableList<PollRequest> _graveyard = ImmutableList<PollRequest>.Empty;
        private ImmutableList<PollResponse> _sequence = ImmutableList<PollResponse>.Empty;

        public PollState State;
        private int _idx = -1;
        private bool _menu;
        private readonly int[] _y;
        private readonly int[] _head;
        
        private PollRequest _current;
        private List<PollRequest> _temp;
        private double _timestamp;
        private readonly Random _rnd;

        private readonly Texture2D _menuPopup;
        private readonly Texture2D _menuButton;
        private readonly Texture2D _background;
        private readonly Texture2D _alert;
        private readonly Texture2D _result;

        public Chat(Board board, Scenes scenes)
        {
            _board = board;
            _scenes = scenes;
            State = PollState.Idle;
            
            _menu = false;
            _y = new []{ 0, 0 };
            _head = new []{ 0, 0 };
            _current = new PollRequest();
            _temp = new List<PollRequest>();
            _timestamp = 0;
            _rnd = new Random();

            _menuPopup = LoadTexture("Resource/menu.png");
            _menuButton = LoadTexture("Resource/menuButton.png");
            _background = LoadTexture("Resource/list_background.png");
            _alert = LoadTexture("Resource/alert.png");
            _result = LoadTexture("Resource/result.png");
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

        public void Draw(bool shutdownRequest)
        {
            _timestamp = GetTime();
            DrawLists();

            if (State == PollState.Active) _current = _temp[_rnd.Next(_temp.Count)];

            if (!shutdownRequest && FindAllSongs(_idx).ToList().Count > 0) DrawPollButton();
            if (!shutdownRequest && State == PollState.Idle) DrawButtons();
            if (!shutdownRequest && _menu) DrawMenu();
            if (State != PollState.Idle) DrawResult(_current);
        }

        public void Dispose()
        {
            _exitEvent.Set();
            UnloadTexture(_menuPopup);
            UnloadTexture(_menuButton);
            UnloadTexture(_background);
            UnloadTexture(_alert);
            UnloadTexture(_result);
        }

        // UIs

        private void DrawLists()
        {
            DrawTexture(_background, 320, 180, Color.WHITE);
            
            // Current Lists
            if (!FindAllSongs(_idx).Any()) DrawTexture(_alert, 332, 192, Color.WHITE);
            else
            {
                var block = _board.CurrBoard[_idx];
                var text = block.Topic.Replace("\n", " ");
                DrawRectangle(332, 192, 622, 62, block.BoxColor);
                DrawTextEx(Ui.Galmuri48, text, new Vector2(344, 199), 48, 0, Color.BLACK);

                var group = Ui.FastMarquee(286, 30, FindAllSongs(_idx).ToList(), 2, ref _y[0], ref _head[0]).ToArray();
                BeginScissorMode(332, 254, 622, 286);
                for (var i = 0; i < group.Length; i++)
                {
                    var pos = new Vector2(344, 260 + _y[0] + 30 * i);
                    DrawTextEx(Ui.Galmuri24, group[i].Song, pos, 24, 0, Color.BLACK);
                }
                EndScissorMode();
            }
            
            // Used List
            var usedList = Ui.FastMarquee(288, 30, _usedList, 2, ref _y[1], ref _head[1]).ToArray();
            BeginScissorMode(332, 540, 622, 288);
            for (var i = 0; i < usedList.Length; i++)
            {
                var pos = new Vector2(340, 546 + _y[1] + 30 * i);
                DrawTextEx(Ui.Galmuri24, $"{usedList[i].Name} => {usedList[i].Song}", pos, 24, 0, Color.WHITE);
            }
            EndScissorMode();

            var responses = _sequence.TakeLast(10).ToArray();
            BeginScissorMode(966, 254, 622, 240);
            for (var i = 0; i < responses.Length; i++)
            {
                var pos = new Vector2(966, 254 + 24 * i);
                DrawRectangle((int)pos.X, (int)pos.Y, 622, 24, responses[i].IsSuccessful ? Color.BLUE : Color.RED);
                DrawTextEx(Ui.Galmuri24, $"{responses[i].Name} => {responses[i].Song}", pos, 24, 0, Color.WHITE);
            }
            EndScissorMode();
        }

        private void DrawResult(PollRequest request)
        {
            DrawTexture(_result, 332, 254, Color.WHITE);
            BeginScissorMode(332, 254, 622, 286);
            DrawTextEx(Ui.Galmuri48, request.Song, new Vector2(352, 314), 48, 0, Color.YELLOW);
            DrawTextEx(Ui.Galmuri24, request.Name, new Vector2(402, 380), 24, 0, Color.WHITE);
            EndScissorMode();
        }
        
        public void DrawButtons()
        {
            var blocks = _board.CurrBoard;
            foreach (var block in blocks)
            {
                var box = block.Box;
                var button = new Rectangle(box.x + 2, box.y + 2, box.width - 4, box.height - 4);
                if (!CheckCollisionPointRec(GetMousePosition(), button)) continue;
                
                var idx = Array.IndexOf(blocks, block);
                if (IsMouseButtonPressed(0)) OnClick(idx);

                var text = idx is 0 or 13 || _board.GoldenKeys.Contains(idx) 
                    ? _board.CurrBoard[idx].Topic
                    : FindAllSongs(idx).ToArray().Length.ToString();
                DrawRectangleRec(button, Fade(Color.WHITE, 0.7f));
                Ui.DrawCenteredText(button, Ui.Galmuri48, text, 48, Color.BLACK);
            }
        }

        private void DrawMenu()
        {
            DrawTexture(_menuPopup, 640, 300, Color.WHITE);
            
            if (Ui.DrawButton(new Rectangle(680, 420, 240, 240), Color.LIME, 0.8f))
            {
                var target = _board.AddKey();
                _graveyard = _graveyard.AddRange(_requests.Where(x => x.Topic == target));
                _requests = _requests.RemoveAll(x => x.Topic == target);
                _idx = -1;
            }

            if (Ui.DrawButton(new Rectangle(1000, 420, 240, 240), Color.LIME, 0.8f))
            {
                _board.Shuffle();
                _idx = -1;
            }

            if (Ui.DrawButton(new Rectangle(680, 330, 560, 60), Color.LIME, 0.8f))
            {
                _board.Restore();
                _requests = _requests.AddRange(_graveyard);
                _graveyard = _graveyard.Clear();
                _idx = -1;
            }
            
            DrawTexture(_menuButton, 640, 300, Color.WHITE);
        }

        private void DrawPollButton()
        {
            var button1 = new Rectangle(332, 840, 160, 48);
            
            switch (State)
            {
                case PollState.Idle:
                    if (Ui.DrawButton(button1, Color.GREEN, 0.7f))
                    {
                        State = PollState.Active;
                        _temp = FindAllSongs(_idx).ToList();
                    }
                    Ui.DrawCenteredText(button1, Ui.Galmuri36, "추첨", 36, Color.BLACK);
                    break;
                case PollState.Active:
                    if (Ui.DrawButton(button1, Color.GREEN, 0.7f)) State = PollState.Result;
                    Ui.DrawCenteredText(button1, Ui.Galmuri36, "멈추기", 36, Color.BLACK);
                    break;
                case PollState.Result:
                    if (Ui.DrawButton(button1, Color.GREEN, 0.7f))
                    {
                        State = PollState.Idle;
                        _usedList = _usedList.Add(_current);
                        _requests = _requests.RemoveAll(x => x.Name == _current.Name);
                        _graveyard = _graveyard.RemoveAll(x => x.Name == _current.Name);
                        _current = new PollRequest();
                    }
                    Ui.DrawCenteredText(button1, Ui.Galmuri36, "결정", 36, Color.BLACK);

                    var button2 = new Rectangle(500, 840, 160, 48);
                    if (Ui.DrawButton(button2, Color.GREEN, 0.7f))
                    {
                        State = _temp.Any() ? PollState.Active : PollState.Idle;
                        _current = new PollRequest();
                    }
                    Ui.DrawCenteredText(button2, Ui.Galmuri36, "재추첨", 36, Color.BLACK);
                    
                    var button3 = new Rectangle(668, 840, 160, 48);
                    if (Ui.DrawButton(button3, Color.GREEN, 0.7f))
                    {
                        State = PollState.Idle;
                        _current = new PollRequest();
                    }
                    Ui.DrawCenteredText(button3, Ui.Galmuri36, "추첨 취소", 36, Color.BLACK);
                    break;
            }
        }

        // Main Methods

        private void UpdateRequest(string msg)
        {
            var re = new Regex(@"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
            var objects = re.Match(msg).Groups;

            var cp1 = IsValid(objects["content"].ToString(), out var topic, out var song);

            var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
            var cp2 = !_usedList.Select(x => x.Name).Contains(name);

            var cp3 = !(GetTime() - _requests.FindLast(x => x.Name == name).Timestamp < 30);

            var isSuccessful = cp1 && cp2 && cp3;
            _sequence = _sequence.Add(new PollResponse(name, song, isSuccessful));

            if (!isSuccessful) return;
            _requests = _requests.Add(new PollRequest(name, topic, song, _timestamp));
            if (_requests.Count(x => x.Name == name) + _graveyard.Count(x => x.Name == name) <= 3) return;
            if (_graveyard.FindLastIndex(x => x.Name == name) != -1)
                _graveyard = _graveyard.Remove(_graveyard.First(x => x.Name == name));
            else _requests = _requests.Remove(_requests.First(x => x.Name == name));
        }

        private bool IsValid(string text, out string topic, out string song)
        {
            var content = text.Split(" ", 3);

            topic = song = string.Empty;
            if (content.Length < 3) return false;
            if (!int.TryParse(content[1], out var idx)) return false;
            if (idx is <= 0 or 13 or > 25) return false;

            topic = _board.CurrBoard[idx].Topic;
            song = content[2];
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

        private IEnumerable<PollRequest> FindAllSongs(int idx)
            => idx is < 0 or >= 26
                ? ImmutableList<PollRequest>.Empty
                : _requests.FindAll(x => x.Topic == _board.CurrBoard[idx].Topic);

        private void OnClick(int idx)
        {
            switch (idx)
            {
                case 0:
                    _menu = !_menu;
                    break;
                default:
                    if (_menu) break;
                    if (_board.GoldenKeys.Contains(idx))
                    {
                        _scenes.CurrScene = _scenes.CurrScene switch
                        {
                            Scene.Board => Scene.Main,
                            Scene.Main => Scene.Board,
                            _ => _scenes.CurrScene
                        };
                        _idx = -1;
                    }
                    else _idx = idx;
                    break;
            }
        }
    }
}