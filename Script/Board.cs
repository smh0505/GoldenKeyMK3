using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Board
    {
        private static readonly Random Rnd = new ();
        private readonly Texture2D _frame;
        private readonly Texture2D _chroma;

        private readonly Dictionary<string, string[]> _topicPool;

        private readonly List<string> _baseTopics;
        private readonly string[] _topics = new string[26];
        private readonly List<int> _goldenKeys = new() { 1, 4, 8, 10, 13, 16, 20, 22 };

        private readonly Rectangle[] _board =
        {
            new Rectangle(1600, 900, 320, 180), // Start

            // Line 1
            new Rectangle(1387, 900, 213, 180),
            new Rectangle(1173, 900, 213, 180),
            new Rectangle(960, 900, 213, 180),
            new Rectangle(747, 900, 213, 180),
            new Rectangle(533, 900, 213, 180),
            new Rectangle(320, 900, 213, 180),

            new Rectangle(0, 900, 320, 180),    // DJMAX

            // Line 2
            new Rectangle(0, 756, 320, 144),
            new Rectangle(0, 612, 320, 144),
            new Rectangle(0, 468, 320, 144),
            new Rectangle(0, 324, 320, 144),
            new Rectangle(0, 180, 320, 144),

            new Rectangle(0, 0, 320, 180),      // Free
            
            // Line 3
            new Rectangle(320, 0, 213, 180),
            new Rectangle(533, 0, 213, 180),
            new Rectangle(747, 0, 213, 180),
            new Rectangle(960, 0, 213, 180),
            new Rectangle(1173, 0, 213, 180),
            new Rectangle(1387, 0, 213, 180),

            new Rectangle(1600, 0, 320, 180),   // EZ2ON

            // Line 4
            new Rectangle(1600, 180, 320, 144),
            new Rectangle(1600, 324, 320, 144),
            new Rectangle(1600, 468, 320, 144),
            new Rectangle(1600, 612, 320, 144),
            new Rectangle(1600, 756, 320, 144),
        };

        private readonly Color[] _boardColor = new Color[26];

        public Board()
        {
            _frame = LoadTexture("Resource/board_frame2.png");
            _chroma = LoadTexture("Resource/board_chroma2.png");

            _topicPool = SaveLoad.LoadTopics("board.yml");
            Generate();
        }

        public void Draw()
        {

        }

        public void Dispose()
        {
            UnloadTexture(_frame);
            UnloadTexture(_chroma);
        }

        // UIs

        private void DrawBoard()
        {
            for (int i = 0; i < 26; i++)
            {
                if (i is 0 or 7 or 13 or 20) continue;
                DrawRectangleRec(_board[i], _boardColor[i]);

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
                var topicTail = _topicPool[topicHead][tailIdx];

                var topic = string.Empty;
                if (topicHead != "기타") topic += topicHead;
                topic += "\n" + topicTail;
                _baseTopics.Add(topic);
            }

            Finish();
        }

        private void Shuffle()
        {
            var count = _goldenKeys.Count;
            _goldenKeys.Clear();

            var i = 0;
            while (i < count)
            {
                var newKey = Rnd.Next(26);
                if (newKey is 0 or 7 or 13 or 20) continue;
                if (_goldenKeys.Contains(newKey)) continue;
                _goldenKeys.Add(newKey);
                i++;
            }

            Finish();
        }

        private void AddKey()
        {
            _baseTopics.OrderBy(_ => Rnd.Next());
            _baseTopics.Remove(_baseTopics.Last());

            _goldenKeys.Add(-1);
            Shuffle();
        }

        private void Finish()
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

            for (var i = 0; i < _board.Length; i++)
                _boardColor[i] = _goldenKeys.Contains(i) ? Color.YELLOW
                    : ColorFromHSV(Rnd.NextSingle() * 360, 0.5f, 1);
        }

        private void SaveBoard()
        {
            DrawTexture(_chroma, 0, 0, Color.WHITE);
            TakeScreenshot("board.png");
        }
    }
}