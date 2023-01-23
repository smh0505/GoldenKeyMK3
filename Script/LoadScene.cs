using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class LoadScene
    {
        private static List<string> _logs;
        private static int _idx;
        private static int _count;
        private static int _page;

        public static void DrawLoad(bool shutdownRequest)
        {
            _logs = new (){"Default"};
            _logs.AddRange(Directory.GetFiles("Logs"));

            if (!shutdownRequest) Control();

            DrawRectangle(40, 40, 480, 48, Color.WHITE);
            BeginScissorMode(40, 40, 480, 48);
            DrawTextEx(Program.MainFont, _logs[_idx], new Vector2(46, 46), 36, 0, Color.BLACK);
            EndScissorMode();

            _count = (int)Math.Floor((GetScreenHeight() - 154) / 48.0f);
            _page = 0;
            DrawRectangle(40, 114, 480, GetScreenHeight() - 154, Color.WHITE);
            BeginScissorMode(40, 114, 480, GetScreenHeight() - 154);
            for (int i = 0; i < _logs.Count; i++)
            {
                Vector2 pos = new (46, 120 + 48 * i);
                DrawTextEx(Program.MainFont, _logs[i], pos, 36, 0, Color.BLACK);
            }
            EndScissorMode();
        }

        private static void Control()
        {
            switch ((KeyboardKey)GetKeyPressed())
            {
                case KeyboardKey.KEY_UP:
                case KeyboardKey.KEY_W:
                case KeyboardKey.KEY_PAGE_UP:
                    _idx = _idx == 0 ? _logs.Count - 1 : _idx - 1;
                    break;
                case KeyboardKey.KEY_DOWN:
                case KeyboardKey.KEY_S:
                case KeyboardKey.KEY_PAGE_DOWN:
                    _idx = _idx == _logs.Count - 1 ? 0 : _idx + 1;
                    break;
            }
        }
    }
}