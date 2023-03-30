using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Board : IDisposable
    {
        private readonly Random _rnd;
        public readonly Rectangle[] Blocks;
        private readonly Dictionary<string, string[]> _themePool;
        
        private readonly Texture2D _frame;
        private readonly Texture2D _keys;

        public List<int> GoldenKeys;
        public readonly Dictionary<string, Color> ThemePairs;
        private readonly Dictionary<string, Texture2D> _islandPairs;
        private readonly Dictionary<string, Texture2D> _freePairs;

        private string[] _backup;
        public string[] CurrBoard;

        public Board()
        {
            _rnd = new Random();
            
            Blocks = new[]
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

            _frame = LoadTexture("Resource/frame.png");
            _keys = LoadTexture("Resource/keys.png");

            _themePool = SaveLoad.LoadThemes();
            GoldenKeys = new List<int> { 2, 5, 9, 11, 15, 18, 22, 24 };

            ThemePairs = new Dictionary<string, Color>();
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
                { "뱅하싶", LoadTexture("Resource/FREE.png") },
                { "야-스", LoadTexture("Resource/yas.png") }
            };

            _backup = new string[26];
            CurrBoard = new string[26];
            
            Generate();
        }
        
        // Public Methods

        public void Draw()
        {
            for (var i = 0; i < 26; i++)
            {
                var block = Blocks[i];
                var theme = CurrBoard[i];

                switch (i)
                {
                    case 0:
                        break;
                    case 7 or 20:
                        DrawTexture(_islandPairs[theme], (int)block.x, (int)block.y, Color.WHITE);
                        break;
                    case 13:
                        DrawTexture(_freePairs[theme], (int)block.x, (int)block.y, Color.WHITE);
                        break;
                    default:
                        DrawRectangleRec(block, GoldenKeys.Contains(i) ? Color.YELLOW : ThemePairs[theme]);
                        if (GoldenKeys.Contains(i))
                        {
                            var pos = Ui.CenterPos(block, _keys);
                            DrawTexture(_keys, (int)pos.X, (int)pos.Y, Color.WHITE);
                        }
                        else
                        {
                            BeginScissorMode((int)block.x, (int)block.y, (int)block.width, (int)block.height);
                            Ui.DrawTextMultiLine(block, Ui.Cafe36, theme.Split("_"), 36, Color.BLACK);
                            EndScissorMode();
                        }
                        break;
                }
            }
            
            DrawTexture(_frame, 0, 0, Color.WHITE);
        }

        public void Dispose()
        {
            UnloadTexture(_frame);
            UnloadTexture(_keys);
            foreach (var x in _islandPairs) UnloadTexture(x.Value);
            foreach (var x in _freePairs) UnloadTexture(x.Value);
            GC.SuppressFinalize(this);
        }
        
        // Private Methods
        
        private void Generate()
        {
            while (ThemePairs.Count < 14)
            {
                var headIdx = _rnd.Next(_themePool.Keys.Count);
                var head = _themePool.Keys.ToArray()[headIdx];

                var tailIdx = _rnd.Next(_themePool[head].Length);
                var tail = _themePool[head][tailIdx];

                var theme = string.Empty;
                if (head != "기타") theme += head + ":_";
                theme += tail;
                if (ThemePairs.All(x => x.Key != theme))
                    ThemePairs[theme] = ColorFromHSV(_rnd.NextSingle() * 360.0f, 0.5f, 1);
            }

            CurrBoard = Finish();
            _backup = (string[])CurrBoard.Clone();
        }

        public void Shuffle(bool isClockwise)
        {
            var count = GoldenKeys.Count;
            GoldenKeys.Clear();
            GoldenKeys.Add(isClockwise ? _rnd.Next(21, 26) : _rnd.Next(1, 7));

            while (GoldenKeys.Count < count)
            {
                var newKey = _rnd.Next(26);
                if (newKey is 0 or 7 or 13 or 20) continue;
                if (GoldenKeys.Contains(newKey)) continue;
                GoldenKeys.Add(newKey);
            }

            CurrBoard = Finish();
        }

        public void AddKey(bool isClockwise)
        {
            GoldenKeys.Add(-1);
            Shuffle(isClockwise);
        }

        private string[] Finish()
        {
            var themes = ThemePairs.Select(x => x.Key).OrderBy(_ => _rnd.Next()).ToList();
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
                    _ => GoldenKeys.Contains(i) ? "황금열쇠" : themes[0]
                };
                if (themes.Contains(board[i])) themes.Remove(board[i]);
            }

            return board;
        }

        public void Restore()
        {
            CurrBoard = (string[])_backup.Clone();
            GoldenKeys = new List<int> { 2, 5, 9, 11, 15, 18, 22, 24 };
        }
    }
}