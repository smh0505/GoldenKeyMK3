using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class LoadScene
    {
        private static string[] _files;
        private static List<string> _logs;
        private static int _idx;
        private static int _count;
        private static int _pagenum;
        private static string[] _page;
        private static int _frames;

        public static bool DrawLoad(bool shutdownRequest)
        {
            _count = (int)Math.Floor((GetScreenHeight() - 80) / 48.0f);
            _files = Directory.GetFiles("Logs");
            _logs = _files.Select(o => File.GetCreationTime(o).ToString("g")).ToList();
            _logs.Insert(0, "기본 설정");

            DrawList();
            DrawTextEx(Program.MainFont, "설정 불러오기", new Vector2(560, 40), 72, 0, Color.BLACK);

            if (!shutdownRequest) return Control();
            return false;
        }

        private static bool Control()
        {
            if (IsKeyDown(KeyboardKey.KEY_UP) || IsKeyDown(KeyboardKey.KEY_W))
            {
                if (_frames == 0) _idx = _idx == 0 ? _logs.Count - 1 : _idx - 1;
                _frames = _frames == 5 ? 0 : _frames + 1;
            }
            if (IsKeyDown(KeyboardKey.KEY_DOWN) || IsKeyDown(KeyboardKey.KEY_S))
            {
                if (_frames == 0) _idx = _idx == _logs.Count - 1 ? 0 : _idx + 1;
                _frames = _frames == 5 ? 0 : _frames + 1;
            }
            if (IsKeyUp(KeyboardKey.KEY_UP) && IsKeyUp(KeyboardKey.KEY_W) 
                && IsKeyUp(KeyboardKey.KEY_DOWN) && IsKeyUp(KeyboardKey.KEY_S))
                _frames = 0;

            if (IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                if (_idx != 0)
                {
                    Wheel.Options = SaveLoad.LoadLog(_files[_idx - 1]);
                    Wheel.Waitlist.Clear();
                }
                return true;
            }
            return false;
        }

        private static void DrawList()
        {
            _pagenum = _idx / _count;
            _page = _logs.Skip(_pagenum * _count).Take(_count).ToArray();

            DrawRectangle(40, 40, 480, GetScreenHeight() - 80, Color.WHITE);
            BeginScissorMode(40, 40, 480, GetScreenHeight() - 80);
            for (int j = 0; j < _page.Length; j++)
            {
                Color textColor = Color.BLACK;
                if (j == _idx % _count)
                {
                    DrawRectangle(40, 40 + 48 * j, 480, 48, Color.DARKGRAY);
                    textColor = Color.WHITE;
                }

                Vector2 pos = new(46, 46 + 48 * j);
                DrawTextEx(Program.MainFont, _page[j], pos, 36, 0, textColor);
            }
            EndScissorMode();
        }
    }
}