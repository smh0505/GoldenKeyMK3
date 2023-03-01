using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public struct Block
    {
        public Rectangle Box { get; }
        public string Topic { get; set; }
        public Color BoxColor { get; set; }

        public Block(Rectangle box, string topic, Color boxColor)
        {
            Box = box;
            Topic = topic;
            BoxColor = boxColor;
        }
    }
    
    public class Board
    {
        private static readonly Random Rnd = new ();
        private readonly Texture2D _frame;
        private readonly Texture2D _key;

        private readonly Dictionary<string, string[]> _topicPool;

        private List<string> _baseTopics = new ();
        private List<string> _backupBase = new ();
        private string[] _backupTopics = new string[26];
        public List<int> GoldenKeys;
        public readonly Block[] CurrBoard;

        public Board()
        {
            _frame = LoadTexture("Resource/board_frame3.png");
            _key = LoadTexture("Resource/keys.png");

            GoldenKeys = new List<int> { 2, 5, 9, 11, 15, 18, 22, 24 };
            CurrBoard = new Block[]
            {
                // Start
                new (new Rectangle(1600, 900, 320, 180), "출발", Color.BLANK),
                
                // Line 1
                new (new Rectangle(1387, 900, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(1173, 900, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(960, 900, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(747, 900, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(533, 900, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(320, 900, 213, 180), string.Empty, Color.BLANK),
                
                // DJMAX
                new (new Rectangle(0, 900, 320, 180), "디맥섬", Color.BLANK),
                
                // Line 2
                new (new Rectangle(0, 756, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(0, 612, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(0, 468, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(0, 324, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(0, 180, 320, 144), string.Empty, Color.BLANK),
                
                // Free
                new (new Rectangle(0, 0, 320, 180), "뱅하싶", Color.BLANK),
                
                // Line 3
                new (new Rectangle(320, 0, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(533, 0, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(747, 0, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(960, 0, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(1173, 0, 213, 180), string.Empty, Color.BLANK),
                new (new Rectangle(1387, 0, 213, 180), string.Empty, Color.BLANK),
                
                // EZ2ON
                new (new Rectangle(1600, 0, 320, 180), "투온섬", Color.BLANK),
                
                // Line 4
                new (new Rectangle(1600, 180, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(1600, 324, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(1600, 468, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(1600, 612, 320, 144), string.Empty, Color.BLANK),
                new (new Rectangle(1600, 756, 320, 144), string.Empty, Color.BLANK)
            };

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
                var block = CurrBoard[i].Box;
                var topic = CurrBoard[i].Topic;
                var blockColor = CurrBoard[i].BoxColor;
                
                DrawRectangleRec(block, blockColor);
                if (GoldenKeys.Contains(i))
                {
                    var pos = new Vector2(block.x + (block.width - _key.width) * 0.5f, block.y + (block.height - _key.height) * 0.5f);
                    DrawTexture(_key, (int)pos.X, (int)pos.Y, Color.WHITE);
                }
                else
                {
                    BeginScissorMode((int)block.x, (int)block.y, (int)block.width, (int)block.height);
                    Ui.DrawCenteredTextMultiLine(block, Ui.Cafe36, topic.Split("\n"), 36, Color.BLACK);
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
            Finish();
            _backupTopics = CurrBoard.Select(x => x.Topic).ToArray();
        }

        public void Shuffle()
        {
            var count = GoldenKeys.Count;
            GoldenKeys.Clear();

            GoldenKeys.Add(Rnd.Next(1, 7));
            GoldenKeys.Add(Rnd.Next(21, 26));

            while (GoldenKeys.Count < count)
            {
                var newKey = Rnd.Next(26);
                if (newKey is 0 or 7 or 13 or 20) continue;
                if (GoldenKeys.Contains(newKey)) continue;
                GoldenKeys.Add(newKey);
            }

            Finish();
        }

        public string AddKey()
        {
            _baseTopics = _baseTopics.OrderBy(_ => Rnd.Next()).ToList();
            var topic = _baseTopics.Last();
            _baseTopics.Remove(_baseTopics.Last());

            GoldenKeys.Add(-1);
            Shuffle();
            return topic;
        }

        private void Finish()
        {
            var temp = _baseTopics.OrderBy(_ => Rnd.Next()).ToList();
            for (var i = 0; i < CurrBoard.Length; i++)
            {
                if (i is 0 or 7 or 13 or 20) continue;
                if (GoldenKeys.Contains(i))
                {
                    CurrBoard[i].Topic = "황금열쇠";
                    CurrBoard[i].BoxColor = Color.YELLOW;
                }
                else
                {
                    CurrBoard[i].Topic = temp[0];
                    temp.RemoveAt(0);
                    CurrBoard[i].BoxColor = ColorFromHSV(Rnd.NextSingle() * 360, 0.5f, 1);
                }
            }
        }

        public void Restore()
        {
            _baseTopics = new List<string>(_backupBase);
            for (var i = 0; i < CurrBoard.Length; i++) CurrBoard[i].Topic = _backupTopics[i];
            GoldenKeys = new List<int> { 2, 5, 9, 11, 15, 18, 22, 24 };
            
            for (var i = 0; i < CurrBoard.Length; i++)
            {
                if (i is 0 or 7 or 13 or 20) continue;
                CurrBoard[i].BoxColor = GoldenKeys.Contains(i)
                    ? Color.YELLOW
                    : ColorFromHSV(Rnd.NextSingle() * 360, 0.5f, 1);
            }
        }
    }
}