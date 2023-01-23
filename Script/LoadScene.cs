using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class LoadScene
    {
        private static List<string> _logs;
        private static int _idx;

        public static void DrawLoad()
        {
            _logs = new (){"Default"};
            _logs.AddRange(Directory.GetFiles("Logs"));

            Control();

            DrawRectangle(12, 12, 480, 48, Color.WHITE);
            BeginScissorMode(12, 12, 480, 48);
            DrawTextEx(Program.MainFont, _logs[_idx], new Vector2(18, 18), 36, 0, Color.BLACK);
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