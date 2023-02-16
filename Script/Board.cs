using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Board
    {
        private static readonly Random Rnd = new ();
        private readonly Texture2D _frame;
        private readonly Texture2D _key;

        private readonly Dictionary<string, string[]> _topicPool;

        private List<string> _baseTopics = new ();
        private List<string> _backupBase = new ();
        private readonly string[] _topics = new string[26];
        private readonly string[] _backupTopics = new string[26];
        private List<int> _goldenKeys = new (){ 2, 5, 9, 11, 15, 18, 22, 24 };

        private readonly Rectangle[] _board =
        {
            new (1600, 900, 320, 180), // Start

            // Line 1
            new (1387, 900, 213, 180),
            new (1173, 900, 213, 180),
            new (960, 900, 213, 180),
            new (747, 900, 213, 180),
            new (533, 900, 213, 180),
            new (320, 900, 213, 180),

            new (0, 900, 320, 180),    // DJMAX

            // Line 2
            new (0, 756, 320, 144),
            new (0, 612, 320, 144),
            new (0, 468, 320, 144),
            new (0, 324, 320, 144),
            new (0, 180, 320, 144),

            new (0, 0, 320, 180),      // Free
            
            // Line 3
            new (320, 0, 213, 180),
            new (533, 0, 213, 180),
            new (747, 0, 213, 180),
            new (960, 0, 213, 180),
            new (1173, 0, 213, 180),
            new (1387, 0, 213, 180),

            new (1600, 0, 320, 180),   // EZ2ON

            // Line 4
            new (1600, 180, 320, 144),
            new (1600, 324, 320, 144),
            new (1600, 468, 320, 144),
            new (1600, 612, 320, 144),
            new (1600, 756, 320, 144),
        };

        private readonly Color[] _boardColor = new Color[26];

        public Board()
        {
            _frame = LoadTexture("Resource/board_frame2.png");
            _key = LoadTexture("Resource/keys.png");

            _topicPool = SaveLoad.LoadTopics("board.yml");
            Generate();
        }
        
        public void Dispose()
        {
            UnloadTexture(_frame);
            UnloadTexture(_key);
        }

        // UIs

        public void Draw()
        {
            for (var i = 0; i < 26; i++)
            {
                if (i is 0 or 7 or 13 or 20) continue;
                DrawRectangleRec(_board[i], _boardColor[i]);
                if (_goldenKeys.Contains(i))
                {
                    var pos = new Vector2(_board[i].x + (_board[i].width - _key.width) * 0.5f,
                        _board[i].y + (_board[i].height - _key.height) * 0.5f);
                    DrawTexture(_key, (int)pos.X, (int)pos.Y, Color.WHITE);
                }
                else
                {
                    var size = MeasureTextEx(Ui.Cafe36, _topics[i], 36, 0);
                    var pos = new Vector2(_board[i].x + (_board[i].width - size.X) * 0.5f,
                        _board[i].y + (_board[i].height - size.Y) * 0.5f);
                    
                    BeginScissorMode((int)_board[i].x, (int)_board[i].y, (int)_board[i].width, (int)_board[i].height);
                    DrawTextEx(Ui.Cafe36, _topics[i], pos, 36, 0, Color.BLACK);
                    EndScissorMode();
                }
            }
            DrawTexture(_frame, 0, 0, Color.WHITE);
        }

        // Controls

        private void Generate()
        {
            while (_baseTopics.Count < 14)
            {
                var headIdx = Rnd.Next(_topicPool.Keys.Count);
                var topicHead = _topicPool.Keys.ToArray()[headIdx];

                var tailIdx = Rnd.Next(_topicPool[topicHead].Length);
                var topicTail = _topicPool[topicHead][tailIdx].Replace("_", "\n");

                var topic = string.Empty;
                if (topicHead != "기타") topic += topicHead + ":\n";
                topic += topicTail;
                if (!_baseTopics.Contains(topic)) _baseTopics.Add(topic);
            }

            _backupBase = new List<string>(_baseTopics);
            Finish(true);
        }

        public void Shuffle()
        {
            var count = _goldenKeys.Count;
            _goldenKeys.Clear();

            _goldenKeys.Add(Rnd.Next(1, 7));
            _goldenKeys.Add(Rnd.Next(21, 26));

            while (_goldenKeys.Count < count)
            {
                var newKey = Rnd.Next(26);
                if (newKey is 0 or 7 or 13 or 20) continue;
                if (_goldenKeys.Contains(newKey)) continue;
                _goldenKeys.Add(newKey);
            }

            Finish(false);
        }

        public string AddKey()
        {
            _baseTopics = _baseTopics.OrderBy(_ => Rnd.Next()).ToList();
            var topic = _baseTopics.Last();
            _baseTopics.Remove(_baseTopics.Last());

            _goldenKeys.Add(-1);
            Shuffle();
            return topic;
        }

        private void Finish(bool save)
        {
            var temp = _baseTopics.OrderBy(_ => Rnd.Next()).ToList();
            for (var i = 0; i < _topics.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        _topics[i] = "출발";
                        break;
                    case 7:
                        _topics[i] = "디맥섬";
                        break;
                    case 13:
                        _topics[i] = "뱅하싶";
                        break;
                    case 20:
                        _topics[i] = "투온섬";
                        break;
                    default:
                        if (_goldenKeys.Contains(i)) _topics[i] = "황금열쇠";
                        else
                        {
                            _topics[i] = temp[0];
                            temp.RemoveAt(0);
                        }
                        break;
                }
            }

            if (save) _topics.CopyTo(_backupTopics, 0);

            for (var i = 0; i < _board.Length; i++)
                _boardColor[i] = _goldenKeys.Contains(i) ? Color.YELLOW
                    : ColorFromHSV(Rnd.NextSingle() * 360, 0.5f, 1);
        }

        public void Restore()
        {
            _baseTopics = new List<string>(_backupBase);
            _backupTopics.CopyTo(_topics, 0);
            _goldenKeys = new List<int> { 2, 5, 9, 11, 15, 18, 22, 24 };
            
            for (var i = 0; i < _board.Length; i++)
                _boardColor[i] = _goldenKeys.Contains(i) ? Color.YELLOW
                    : ColorFromHSV(Rnd.NextSingle() * 360, 0.5f, 1);
        }
        
        // Communication
        
        public List<int> GetGoldenKeys() => _goldenKeys;
        public string[] GetTopics() => _topics;
        public Rectangle[] GetBoard() => _board;
        public Color[] GetBoardColors() => _boardColor;
    }
}