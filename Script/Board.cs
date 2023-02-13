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

        private readonly List<string> _baseTopics;
        private readonly string[] _topics = new string[24];
        private readonly List<int> _goldenKeys = new() { 1, 4, 8, 10, 13, 16, 20, 22 };

        private readonly Dictionary<string, string[]> _topicPool;

        public Board()
        {
            _frame = LoadTexture("Resource/board_frame.png");
            _chroma = LoadTexture("Resource/board_chroma.png");

            _topicPool = SaveLoad.LoadTopics("board.yml");
            GenerateMap();
        }

        public void Dispose()
        {
            UnloadTexture(_frame);
            UnloadTexture(_chroma);
        }

        private void GenerateMap()
        {
            while (_baseTopics.Count < 14)
            {
                var headIdx = Rnd.Next(_topicPool.Keys.Count);
                var topicHead = _topicPool.Keys.ToArray()[headIdx];

                var tailIdx = Rnd.Next(_topicPool[topicHead].Length);
                var topicTail = _topicPool[topicHead][tailIdx];

                var topic = string.Empty;
                if (topicHead != "기타") topic += topicHead;
                topic += " - " + topicTail;
                _baseTopics.Add(topic);
            }

            Finish();
        }

        private void Modify()
        {
            
        }

        private void Finish()
        {
            var temp = _baseTopics.OrderBy(_ => Rnd.Next()).ToList();
            for (var i = 0; i < _topics.Length; i++)
            {
                switch (i)
                {
                    case 6:
                        _topics[i] = "디맥섬";
                        break;
                    case 18:
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
        }
    }
}