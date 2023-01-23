using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class LoadScene
    {
        private static List<string> _files;
        private static int _idx;
        private static int _count;
        private static int _frames;

        public static void DrawLoad(bool shutdownRequest)
        {
            _count = (int)Math.Floor((GetScreenHeight() - 80) / 48.0f);
            _files = new List<string>() { "default" };
            _files.AddRange(Directory.GetFiles("Logs"));

            if (!shutdownRequest) Control();

            DrawRectangle(40, 40, 480, GetScreenHeight() - 80, Color.WHITE);
            BeginScissorMode(40, 40, 480, GetScreenHeight() - 80);
            for (int j = 0; j < _files.Count; j++)
            {
                Color textColor = Color.BLACK;
                if (j == _idx) 
                {
                    DrawRectangle(40, 40 + 48 * j, 480, 48, Color.DARKGRAY);
                    textColor = Color.WHITE;
                }

                Vector2 pos = new(46, 46 + 48 * j);
                DrawTextEx(Program.MainFont, _files[j], pos, 36, 0, textColor);
            }
            EndScissorMode();
        }

        private static void Control()
        {
            if (IsKeyDown(KeyboardKey.KEY_UP) || IsKeyDown(KeyboardKey.KEY_W))
            {
                if (_frames == 0) _idx = _idx == 0 ? _files.Count - 1 : _idx - 1;
                _frames = _frames == 5 ? 0 : _frames + 1;
            }
            if (IsKeyDown(KeyboardKey.KEY_DOWN) || IsKeyDown(KeyboardKey.KEY_S))
            {
                if (_frames == 0) _idx = _idx == _files.Count - 1 ? 0 : _idx + 1;
                _frames = _frames == 5 ? 0 : _frames + 1;
            }
            if (IsKeyUp(KeyboardKey.KEY_UP) && IsKeyUp(KeyboardKey.KEY_W) 
                && IsKeyUp(KeyboardKey.KEY_DOWN) && IsKeyUp(KeyboardKey.KEY_S))
                _frames = 0;
        }
    }
}