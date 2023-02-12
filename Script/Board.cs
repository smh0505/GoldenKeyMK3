using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Board
    {
        private static readonly Random rnd = new Random();
        private static List<(Rectangle block, Color blockColor)> _board = new List<(Rectangle, Color)>();

        private static void GenerateMap()
        {
            _board.Clear();
            var len = 1366.0f / 6;
            for (int i = 0; i < 6; i++)
            {
                var block = new Rectangle(277 + len * i, 0, len, 156);
                var blockColor = ColorFromHSV(rnd.NextSingle() * 360, 0.4f, 1);

                _board.Add((block, blockColor));

                block = new Rectangle(277 + len * i, 924, len, 156);
                blockColor = ColorFromHSV(rnd.NextSingle() * 360, 0.4f, 1);

                _board.Add((block, blockColor));
            }

            var height = 768.0f / 5;
            for (int i = 0; i < 5; i++)
            {
                var block = new Rectangle(0, 156 + height * i, 277, height);
                var blockColor = ColorFromHSV(rnd.NextSingle() * 360, 0.4f, 1);

                _board.Add((block, blockColor));

                block = new Rectangle(1643, 156 + height * i, 277, height);
                blockColor = ColorFromHSV(rnd.NextSingle() * 360, 0.4f, 1);

                _board.Add((block, blockColor));
            }
        }
    }
}