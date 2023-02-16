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

        private ImmutableList<(string Name, string Topic, string Song)> _requests = 
            ImmutableList<(string, string, string)>.Empty;
        private ImmutableList<(string Name, string Song)> _usedList = 
            ImmutableList<(string, string)>.Empty;
        private ImmutableList<(string Name, string Topic, string Song)> _graveyard = 
            ImmutableList<(string, string, string)>.Empty;

        private int _idx = -1;
        private bool _menu;
        private PollState _state;
        private readonly int[] _y;
        private readonly int[] _head;
        private (string Name, string Song) _current;
        private List<(string Name, string Song)> _temp;
        private int _countUp;
        private bool _screenshot;

        private readonly Texture2D _menuPopup;
        private readonly Texture2D _addKey;
        private readonly Texture2D _shuffle;
        private readonly Texture2D _background;
        private readonly Texture2D _alert;
        private readonly Texture2D _result;

        public Chat(Board board)
        {
            _board = board;
            _state = PollState.Idle;
            _topics = _board.GetTopics();
            
            _menu = false;
            _y = new []{ 0, 0 };
            _head = new []{ 0, 0 };
            _current = (string.Empty, string.Empty);
            _temp = new List<(string, string)>();
            _countUp = 0;
            _screenshot = true;

            _menuPopup = LoadTexture("Resource/menu.png");
            _addKey = LoadTexture("Resource/add_key.png");
            _shuffle = LoadTexture("Resource/shuffle.png");
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
                _client.Send("NICK justinfan1234");
                _client.Send("JOIN #arpa__");
                _exitEvent.WaitOne();
            }
        }

        public void Draw(bool shutdownRequest)
        {
            DrawLists();

            if (_state == PollState.Active)
            {
                _countUp++;
                _current = _temp[new Random().Next(_temp.Count)];
                if (_countUp == 150) _state = PollState.Result;
            }
            
            if (!shutdownRequest && _state != PollState.Active && FindAllSongs(_idx).ToList().Count > 0) DrawPollButton();
            if (!shutdownRequest && _state != PollState.Active) DrawButtons();
            if (!shutdownRequest && _menu) DrawMenu();
            if (_state != PollState.Idle) DrawResult(_current);

            if (!_screenshot) return;
            SaveBoard();
            _screenshot = false;
        }

        public void Dispose()
        {
            _exitEvent.Set();
            UnloadTexture(_menuPopup);
            UnloadTexture(_shuffle);
            UnloadTexture(_addKey);
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
                var text = _topics[_idx].Replace("\n", " ");
                DrawRectangle(332, 192, 622, 62, _board.GetBoardColors()[_idx]);
                DrawTextEx(Ui.Galmuri48, text, new Vector2(344, 199), 48, 0, Color.BLACK);

                var group = Marquee(406, 30, FindAllSongs(_idx).ToList(), ref _y[0], ref _head[0]).ToArray();
                BeginScissorMode(332, 254, 622, 406);
                for (var i = 0; i < group.Length; i++)
                {
                    var pos = new Vector2(344, 260 + _y[0] + 30 * i);
                    DrawTextEx(Ui.Galmuri24, group[i].Song, pos, 24, 0, Color.BLACK);
                }
                EndScissorMode();
            }
            
            // Used List
            var usedList = Marquee(406, 30, _usedList, ref _y[1], ref _head[1]).ToArray();
            BeginScissorMode(966, 254, 622, 406);
            for (var i = 0; i < usedList.Length; i++)
            {
                var pos = new Vector2(978, 260 + _y[1] + 30 * i);
                DrawTextEx(Ui.Galmuri24, $"{usedList[i].Name} => {usedList[i].Song}", pos, 24, 0, Color.WHITE);
            }
            EndScissorMode();
        }

        private void DrawResult((string Name, string Song) request)
        {
            DrawTexture(_result, 332, 254, Color.WHITE);
            BeginScissorMode(332, 254, 622, 406);
            DrawTextEx(Ui.Galmuri48, request.Song, new Vector2(352, 314), 48, 0, Color.YELLOW);
            DrawTextEx(Ui.Galmuri24, request.Name, new Vector2(402, 380), 24, 0, Color.WHITE);
            EndScissorMode();
        }
        
        private void DrawButtons()
        {
            var blocks = _board.GetBoard();
            foreach (var block in blocks)
            {
                var button = new Rectangle(block.x + 2, block.y + 2, block.width - 4, block.height - 4);
                if (!CheckCollisionPointRec(GetMousePosition(), button)) continue;
                
                var idx = Array.IndexOf(blocks, block);
                if (IsMouseButtonPressed(0)) OnClick(idx);

                var text = idx is 0 or 13 || _board.GetGoldenKeys().Contains(idx) ? _topics[idx]
                    : FindAllSongs(idx).ToArray().Length.ToString();
                var textSize = MeasureTextEx(Ui.Galmuri48, text, 48, 0);
                var pos = new Vector2(button.x + (button.width - textSize.X) * 0.5f, 
                    button.y + (button.height - textSize.Y) * 0.5f);

                DrawRectangleRec(button, Fade(Color.WHITE, 0.7f));
                DrawTextEx(Ui.Galmuri48, text, pos, 48, 0, Color.BLACK);
            }
        }

        private void DrawMenu()
        {
            DrawTexture(_menuPopup, 640, 300, Color.WHITE);
            
            if (Ui.DrawButton(new Rectangle(680, 420, 240, 240), Color.LIME, 0.8f, _addKey))
            {
                var target = _board.AddKey();
                _topics = _board.GetTopics();
                _graveyard = _graveyard.AddRange(_requests.Where(x => x.Topic == target));
                _requests = _requests.RemoveAll(x => x.Topic == target);
                _idx = -1;
                _screenshot = true;
            }

            if (Ui.DrawButton(new Rectangle(1000, 420, 240, 240), Color.LIME, 0.8f, _shuffle))
            {
                _board.Shuffle();
                _topics = _board.GetTopics();
                _idx = -1;
                _screenshot = true;
            }

            if (Ui.DrawButton(new Rectangle(680, 330, 560, 60), Color.LIME, 0.8f,
                    Ui.Galmuri48, "리-셋", 48, Color.WHITE))
            {
                _board.Restore();
                _topics = _board.GetTopics();
                _requests = _requests.AddRange(_graveyard);
                _graveyard = _graveyard.Clear();
                _idx = -1;
                _screenshot = true;
            }
        }

        private void DrawPollButton()
        {
            var texts = new []{ "추첨", string.Empty, "다음" };
            
            if (!Ui.DrawButton(new Rectangle(332, 840, 160, 48), Color.GREEN, 0.7f,
                    Ui.Galmuri36, texts[(int)_state], 36, Color.BLACK)) return;
            if (_state == PollState.Idle) _temp = FindAllSongs(_idx).ToList();
            _state = (PollState)(((int)_state + 1) % 3);
            
            if (_state != PollState.Idle) return;
            _usedList = _usedList.Add(_current);
            _requests = _requests.RemoveAll(x => x.Name == _current.Name);
            _graveyard = _graveyard.RemoveAll(x => x.Name == _current.Name);
            _current = (string.Empty, string.Empty);
            _countUp = 0;
        }

        // Main Methods

        private void UpdateRequest(string msg)
        {
            var re = new Regex(@"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
            var objects = re.Match(msg).Groups;

            if (!IsValid(objects["content"].ToString())) return;
            
            var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
            if (_usedList.Select(x => x.Name).Contains(name)) return;
            
            var order = GetOrder(objects["content"].ToString());

            var idx = _topics[order.Item1];
            _requests = _requests.Add((name, idx, order.Item2));
            if (_requests.Count(x => x.Name == name) > 3)
                _requests = _requests.Remove(_requests.First(x => x.Name == name));
        }

        private bool IsValid(string text)
        {
            var content = text.Split(' ', 3);

            if (content.Length < 3) return false;
            if (!int.TryParse(content[1], out var idx)) return false;
            if (idx is < 1 or > 24) return false;
            return !_board.GetGoldenKeys().Contains(idx);
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

        private IEnumerable<(string Name, string Song)> FindAllSongs(int idx)
            => idx < 0 ? ImmutableList<(string, string)>.Empty 
                : _requests.FindAll(x => x.Topic == _topics[idx]).Select(x => (x.Name, x.Song));

        private void OnClick(int idx)
        {
            if (idx == 0) _menu = !_menu;
            else if (!_menu && FindAllSongs(idx).Any()) _idx = idx;
        }

        private static IReadOnlyCollection<T> Marquee<T>(float height, float unitHeight, IReadOnlyCollection<T> group,
            ref int y, ref int head)
        {
            var count = (int)Math.Ceiling(height / unitHeight);

            if (group.Count >= count)
            {
                y -= 2;
                if (y <= -30)
                {
                    head = (head + 1) % group.Count;
                    y = 0;
                }
            }
            else y = head = 0;

            var output = group.Skip(head).Take(count + 1).ToList();
            if (group.Count >= count && output.Count < count + 1)
                output.AddRange(group.Take(count + 1 - output.Count));
            return output.ToArray();
        }

        private static void SaveBoard()
        {
            var screenshot = LoadImageFromScreen();
            var chroma = LoadImage("Resource/board_chroma2.png");
            ImageDraw(ref screenshot, chroma, new Rectangle(0, 0, 1920, 1080), 
                new Rectangle(0, 0, 1920, 1080), Color.WHITE);
            ExportImage(screenshot, "board.png");
            UnloadImage(screenshot);
            UnloadImage(chroma);
        }
    }
}