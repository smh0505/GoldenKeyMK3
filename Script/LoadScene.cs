using System.Collections;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class LoadScene
    {
        private readonly Wheel _wheel;
        private readonly Texture2D _select;
        private static List<WheelPanel> _options;

        private static int _idx;
        private static int _frames;

        private static int _idx2;
        private static int _y;

        private const string AlertText = "디폴트 설정을 불러올 수 없습니다!";
        private static readonly string[] Texts =
        {
            "설정 불러오기",
            "현재 설정 : ",
            "디폴트 설정을 불러올 수 없습니다!",
        };

        public LoadScene(Wheel wheel)
        {
            _wheel = wheel;
            _select = LoadTexture("Resource/select.png");
        }
        
        public bool Draw(bool shutdownRequest)
        {
            var files = Directory.GetFiles("Logs");
            var logs = files.Select(o => File.GetCreationTime(o).ToString("g")).ToList();
            if (SaveLoad.DefaultOptions.Any()) logs.Insert(0, "기본 설정");

            DrawList(logs);
            DrawTextEx(Program.MainFont, logs[_idx], new Vector2(757, 135), 48, 0, Color.BLACK);
            if (!SaveLoad.DefaultOptions.Any())
                DrawTextEx(Program.MainFont, AlertText, new Vector2(1053, 76), 36, 0, Color.RED);
            
            if (_options is not null) DrawLog();
            
            return !shutdownRequest && Control(files, logs);
        }

        public void Dispose()
        {
            UnloadTexture(_select);
        }

        // UIs

        private static void DrawList(IEnumerable<string> logs)
        {
            var count = (int)Math.Floor((GetScreenHeight() - 80) / 48.0f);
            var pageIdx = _idx / count;
            var page = logs.Skip(pageIdx * count).Take(count).ToArray();

            DrawRectangle(40, 40, 480, GetScreenHeight() - 80, Color.WHITE);
            BeginScissorMode(40, 40, 480, GetScreenHeight() - 80);
            for (var j = 0; j < page.Length; j++)
            {
                var textColor = Color.BLACK;
                if (j == _idx % count)
                {
                    DrawRectangle(40, 40 + 48 * j, 480, 48, Color.DARKGRAY);
                    textColor = Color.WHITE;
                }

                Vector2 pos = new(46, 46 + 48 * j);
                DrawTextEx(Program.MainFont, page[j], pos, 36, 0, textColor);
            }
            EndScissorMode();
        }

        private static void DrawLog()
        {
            var panels = Marquee(_options);

            DrawRectangle(560, 200, GetScreenWidth() - 600, GetScreenHeight() - 240, Color.WHITE);
            BeginScissorMode(560, 200, GetScreenWidth() - 600, GetScreenHeight() - 240);
            for (var i = 0; i < panels.Count; i++)
            {
                var pos = new Vector2(560, 200 + _y + 48 * i);
                DrawRectangle((int)pos.X, (int)pos.Y, GetScreenWidth() - 600, 48, panels[i].Color);
                DrawTextEx(Program.MainFont, panels[i].Name + $" * {panels[i].Count}",
                    new Vector2(pos.X + 6, pos.Y + 6), 36, 0, Color.BLACK);
            }
            EndScissorMode();
        }

        // Controls

        private bool Control(IReadOnlyList<string> files, ICollection logs)
        {
            if (IsKeyDown(KeyboardKey.KEY_UP))
            {
                if (_frames == 0) _idx = _idx == 0 ? logs.Count - 1 : _idx - 1;
                _frames = _frames == 6 ? 0 : _frames + 1;
            }

            if (IsKeyDown(KeyboardKey.KEY_DOWN))
            {
                if (_frames == 0) _idx = _idx == logs.Count - 1 ? 0 : _idx + 1;
                _frames = _frames == 6 ? 0 : _frames + 1;
            }

            if (IsKeyUp(KeyboardKey.KEY_UP) && IsKeyUp(KeyboardKey.KEY_DOWN))
            {
                _frames = 0;
                if (SaveLoad.DefaultOptions.Any())
                    _options = _idx == 0 ? SaveLoad.DefaultOptions : SaveLoad.LoadLog(files[_idx - 1]);
                else _options = SaveLoad.LoadLog(files[_idx]);
            }

            if (!IsKeyPressed(KeyboardKey.KEY_ENTER)) return false;
            _wheel.Options = _options;
            return true;
        }

        private static List<WheelPanel> Marquee(IReadOnlyCollection<WheelPanel> options)
        {
            // Translates position upward
            var count = (int)Math.Ceiling((GetScreenHeight() - 240) / 48.0f);
            if (options.Count >= count)
            {
                _y -= 2;
                if (_y <= -48)
                {
                    _idx2 = (_idx2 + 1) % options.Count;
                    _y = 0;
                }
            }
            else _y = _idx2 = 0;

            // Chooses items to show
            var panels = options.Skip(_idx2).Take(count + 1).ToList();
            if (options.Count >= count && panels.Count < count + 1)
                panels.AddRange(options.Take(count + 1 - panels.Count));
            return panels;
        }
    }
}