using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum BoardState
    {
        Poll = 0,
        Wheel,
        Raffle,
        Dice
    }
    
    public class Board : IGameObject
    {
        private BoardState _state;
        
        private readonly Wheel _wheel;
        private readonly Donation _donation;
        private readonly Poll _poll;
        private readonly Chat _chat;
        private readonly Dice _dice;
        private readonly Clock _clock;

        private readonly Random _rnd;
        private readonly Rectangle[] _blocks;
        private readonly Dictionary<string, string[]> _themePool;
        
        private readonly Texture2D _frame;
        private readonly Texture2D _keys;

        private List<int> _goldenKeys;
        private readonly Dictionary<string, Color> _themePairs;
        private readonly Dictionary<string, Texture2D> _islandPairs;
        private readonly Dictionary<string, Texture2D> _freePairs;

        private string[] _backup;
        private string[] _currBoard;
        private readonly bool[] _boardHover;

        public Board()
        {
            _state = BoardState.Poll;
            
            _wheel = new Wheel();
            _donation = new Donation(_wheel);
            _poll = new Poll();
            _chat = new Chat(_poll);
            _dice = new Dice();
            _clock = new Clock();
            
            _rnd = new Random();
            _blocks = new[]
            {
                // Start
                new Rectangle(1600, 900, 320, 180),
                
                // Line 1
                new Rectangle(1387, 900, 213, 180),
                new Rectangle(1173, 900, 213, 180),
                new Rectangle(960, 900, 213, 180),
                new Rectangle(747, 900, 213, 180),
                new Rectangle(533, 900, 213, 180),
                new Rectangle(320, 900, 213, 180),
                
                // Island 1
                new Rectangle(0, 900, 320, 180),
                
                // Line 2
                new Rectangle(0, 756, 320, 144),
                new Rectangle(0, 612, 320, 144),
                new Rectangle(0, 468, 320, 144),
                new Rectangle(0, 324, 320, 144),
                new Rectangle(0, 180, 320, 144),
                
                // Free
                new Rectangle(0, 0, 320, 180),
                
                // Line 3
                new Rectangle(320, 0, 213, 180),
                new Rectangle(533, 0, 213, 180),
                new Rectangle(747, 0, 213, 180),
                new Rectangle(960, 0, 213, 180),
                new Rectangle(1173, 0, 213, 180),
                new Rectangle(1387, 0, 213, 180),
                
                // Island 2
                new Rectangle(1600, 0, 320, 180),
                
                // Line 4
                new Rectangle(1600, 180, 320, 144),
                new Rectangle(1600, 324, 320, 144),
                new Rectangle(1600, 468, 320, 144),
                new Rectangle(1600, 612, 320, 144),
                new Rectangle(1600, 756, 320, 144),
            };
            _themePool = SaveLoad.LoadThemes();

            _frame = LoadTexture("Resource/frame.png");
            _keys = LoadTexture("Resource/keys.png");
            
            _goldenKeys = new List<int> { 2, 5, 9, 11, 15, 18, 22, 24 };
            _themePairs = new Dictionary<string, Color>();
            _islandPairs = new Dictionary<string, Texture2D>
            {
                { "디맥섬", LoadTexture("Resource/DJMAX.png") },
                { "투온섬", LoadTexture("Resource/EZ2ON.png") },
                { "CiRCLE", LoadTexture("Resource/circle.png") },
                { "화성", LoadTexture("Resource/mars.png") },
                { "이세카이 트럭", LoadTexture("Resource/truck.png") }
            };
            _freePairs = new Dictionary<string, Texture2D>
            {
                { "뱅하싶", LoadTexture("Resource/FREE.png") }
            };

            _backup = new string[26];
            _currBoard = new string[26];
            _boardHover = new[]
            {
                // Corner + Blocks
                false, false, false, false, false, false, false, // Line 1
                false, false, false, false, false, false, // Line 2
                false, false, false, false, false, false, false, // Line 3
                false, false, false, false, false, false, // Line 4
            };

            Generate();
        }

        public void Draw()
        {
            DrawBoard();
            DrawHover();
            _clock.Draw();

            switch (_state)
            {
                case BoardState.Wheel:
                    _wheel.Draw();
                    break;
                case BoardState.Poll:
                    _poll.Draw();
                    break;
            }
        }

        public void Control(bool shutdownRequest)
        {
            ControlHover(shutdownRequest, ref _poll.Target);
            _clock.Control(shutdownRequest);

            switch (_state)
            {
                case BoardState.Wheel:
                    _wheel.Control(shutdownRequest);
                    break;
                case BoardState.Poll:
                    _poll.Control(shutdownRequest);
                    break;
            }
        }

        public void Connect(string payload, List<Panel> panels)
        {
            _wheel.Panels = panels;
            _donation.Connect(payload);
            _chat.Connect();
        }

        public void Dispose()
        {
            _wheel.Dispose();
            _donation.Dispose();
            _poll.Dispose();
            _chat.Dispose();
            _clock.Dispose();
            SaveLoad.SaveLog(_wheel);
            
            UnloadTexture(_frame);
            UnloadTexture(_keys);
            foreach (var x in _islandPairs) UnloadTexture(x.Value);
            foreach (var x in _freePairs) UnloadTexture(x.Value);
            GC.SuppressFinalize(this);
        }
        
        // Controls

        private void Generate()
        {
            while (_themePairs.Count < 14)
            {
                var headIdx = _rnd.Next(_themePool.Keys.Count);
                var head = _themePool.Keys.ToArray()[headIdx];

                var tailIdx = _rnd.Next(_themePool[head].Length);
                var tail = _themePool[head][tailIdx];

                var theme = string.Empty;
                if (head != "기타") theme += head + ":_";
                theme += tail;
                if (_themePairs.All(x => x.Key != theme))
                    _themePairs[theme] = ColorFromHSV(_rnd.NextSingle() * 360.0f, 0.5f, 1);
            }

            Finish();
            _backup = _currBoard;
            _poll.Update(_themePairs);
        }
        
        private void Shuffle()
        {
            var count = _goldenKeys.Count;
            _goldenKeys.Clear();
            _goldenKeys.Add(_clock.IsClockwise ? _rnd.Next(21, 26) : _rnd.Next(1, 7));

            while (_goldenKeys.Count < count)
            {
                var newKey = _rnd.Next(26);
                if (newKey is 0 or 7 or 13 or 20) continue;
                if (_goldenKeys.Contains(newKey)) continue;
                _goldenKeys.Add(newKey);
            }

            Finish();
        }
        
        private void AddKey()
        {
            _goldenKeys.Add(-1);
            Shuffle();
        }
        
        private void Restore()
        {
            _currBoard = _backup;
            _goldenKeys = new List<int> { 2, 5, 9, 11, 15, 18, 22, 24 };
            _chat.Update(_goldenKeys.ToArray(), _currBoard);
        }

        private void Finish()
        {
            var themes = _themePairs.Select(x => x.Key).OrderBy(_ => _rnd.Next()).ToList();
            var islands = _islandPairs.Select(x => x.Key).OrderBy(_ => _rnd.Next()).Take(2).ToList();
            var freeParking = _freePairs.Select(x => x.Key).OrderBy(_ => _rnd.Next()).First();
            var board = new string[26];

            for (var i = 0; i < 26; i++)
            {
                board[i] = i switch
                {
                    0 => "출발",
                    7 => islands[0],
                    13 => freeParking,
                    20 => islands[1],
                    _ => _goldenKeys.Contains(i) ? "황금열쇠" : themes[0]
                };
                if (themes.Contains(board[i])) themes.Remove(board[i]);
            }

            _currBoard = board;
            _chat.Update(_goldenKeys.ToArray(), _currBoard);
        }
        
        private void ControlHover(bool shutdownRequest, ref string theme)
        {
            for (var i = 0; i < _blocks.Length; i++)
            {
                var button = new Rectangle(_blocks[i].x + 2, _blocks[i].y + 2, _blocks[i].width - 4, _blocks[i].height - 4);
                _boardHover[i] = Ui.IsHovering(button, !shutdownRequest);

                if (!_boardHover[i] || !IsMouseButtonPressed(0)) continue;
                _state = _goldenKeys.Contains(i) ? BoardState.Wheel : BoardState.Poll;
                if (_state == BoardState.Poll && _poll.State == PollState.Idle) 
                    theme = _currBoard[i];
            }
        }
        
        // UIs

        private void DrawBoard()
        {
            for (var i = 0; i < 26; i++)
            {
                switch (i)
                {
                    case 0:
                        break;
                    case 7 or 20:
                        DrawTexture(_islandPairs[_currBoard[i]], (int)_blocks[i].x, (int)_blocks[i].y, Color.WHITE);
                        break;
                    case 13:
                        DrawTexture(_freePairs[_currBoard[i]], (int)_blocks[i].x, (int)_blocks[i].y, Color.WHITE);
                        break;
                    default:
                        DrawRectangleRec(_blocks[i], _goldenKeys.Contains(i) ? Color.YELLOW : _themePairs[_currBoard[i]]);
                        if (_goldenKeys.Contains(i))
                        {
                            var pos = Ui.CenterPos(_blocks[i], _keys);
                            DrawTexture(_keys, (int)pos.X, (int)pos.Y, Color.WHITE);
                        }
                        else
                        {
                            BeginScissorMode((int)_blocks[i].x, (int)_blocks[i].y, (int)_blocks[i].width, (int)_blocks[i].height);
                            Ui.DrawTextMultiLine(_blocks[i], Ui.Cafe36, _currBoard[i].Split("_"), 36, Color.BLACK);
                            EndScissorMode();
                        }
                        break;
                }
            }
            
            DrawTexture(_frame, 0, 0, Color.WHITE);
        }

        private void DrawHover()
        {
            for (var i = 0; i < _blocks.Length; i++)
            {
                if (!_boardHover[i]) continue;
                var button = new Rectangle(_blocks[i].x + 2, _blocks[i].y + 2, _blocks[i].width - 4, _blocks[i].height - 4);
                DrawRectangleRec(button, Fade(Color.WHITE, 0.7f));
                var text = i is 0 or 13 || _goldenKeys.Contains(i)
                    ? _currBoard[i]
                    : _poll.FindAllSongs(_currBoard[i]).Count().ToString();
                Ui.DrawTextCentered(button, Ui.Galmuri36, text, 36, Color.BLACK);
            }
        }
    }
}