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
            
        }
    }
}