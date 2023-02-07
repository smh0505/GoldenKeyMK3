using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class LoadScene
    {
        private static string[] _files;
        private static List<string> _logs;
        private static string[] _page;
        private static List<WheelPanel> _options;

        private static int _idx;
        private static int _count;
        private static int _pagenum;
        private static int _frames;

        private static int _idx2;
        private static int _count2;
        private static int _ypos;

        private static readonly string[] Texts =
        {
            "설정 불러오기",
            "현재 설정 : ",
            "디폴트 설정을 불러올 수 없습니다!",
        };

        public static bool DrawLoad(bool shutdownRequest)
        {
            _files = Directory.GetFiles("Logs");
            _logs = _files.Select(o => File.GetCreationTime(o).ToString("g")).ToList();
            if (SaveLoad.DefaultOptions.Any()) _logs.Insert(0, "기본 설정");

            DrawList();
            DrawTextEx(Program.MainFont, Texts[0], new Vector2(560, 40), 72, 0, Color.BLACK);
            DrawTextEx(Program.MainFont, Texts[1] + _logs[_idx], new Vector2(560, 132), 48, 0, Color.BLACK);
            if (!SaveLoad.DefaultOptions.Any())
                DrawTextEx(Program.MainFont, Texts[2],
                    new Vector2(584 + MeasureTextEx(Program.MainFont, Texts[0], 72, 0).X, 76),
                    36, 0, Color.RED);

            if (_options is not null) DrawLog();

            if (!shutdownRequest) return Control();
            return false;
        }

        // UIs

        private static void DrawList()
        {
            _count = (int)Math.Floor((GetScreenHeight() - 80) / 48.0f);
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

        private static void DrawLog()
        {
            var panels = Marquee();

            DrawRectangle(560, 200, GetScreenWidth() - 600, GetScreenHeight() - 240, Color.WHITE);
            BeginScissorMode(560, 200, GetScreenWidth() - 600, GetScreenHeight() - 240);
            for (int i = 0; i < panels.Count; i++)
            {
                var pos = new Vector2(560, 200 + _ypos + 48 * i);
                DrawRectangle((int)pos.X, (int)pos.Y, GetScreenWidth() - 600, 48, panels[i].Color);
                DrawTextEx(Program.MainFont, panels[i].Name + $" * {panels[i].Count}",
                    new Vector2(pos.X + 6, pos.Y + 6), 36, 0, Color.BLACK);
            }
            EndScissorMode();
        }

        // Controls

        private static bool Control()
        {
            if (IsKeyDown(KeyboardKey.KEY_UP))
            {
                if (_frames == 0) _idx = _idx == 0 ? _logs.Count - 1 : _idx - 1;
                _frames = _frames == 6 ? 0 : _frames + 1;
            }

            if (IsKeyDown(KeyboardKey.KEY_DOWN))
            {
                if (_frames == 0) _idx = _idx == _logs.Count - 1 ? 0 : _idx + 1;
                _frames = _frames == 6 ? 0 : _frames + 1;
            }

            if (IsKeyUp(KeyboardKey.KEY_UP) && IsKeyUp(KeyboardKey.KEY_DOWN))
            {
                _frames = 0;
                if (SaveLoad.DefaultOptions.Any())
                    _options = _idx == 0 ? SaveLoad.DefaultOptions : SaveLoad.LoadLog(_files[_idx - 1]);
                else _options = SaveLoad.LoadLog(_files[_idx]);
            }

            if (IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                Wheel.Options = _options;
                return true;
            }
            return false;
        }

        private static List<WheelPanel> Marquee()
        {
            // Translates position upward
            _count2 = (int)Math.Ceiling((GetScreenHeight() - 240) / 48.0f);
            if (_options.Count >= _count2)
            {
                _ypos -= 2;
                if (_ypos <= -48)
                {
                    _idx2 = (_idx2 + 1) % _options.Count;
                    _ypos = 0;
                }
            }
            else _ypos = _idx2 = 0;

            // Chooses items to show
            var panels = _options.Skip(_idx2).Take(_count2 + 1).ToList();
            if (_options.Count >= _count2 && panels.Count < _count2 + 1)
                panels.AddRange(_options.Take(_count2 + 1 - panels.Count));
            return panels;
        }
    }
}