using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{ 
    public class Log : IDisposable
    {
        public List<Panel> Panels;
        public readonly List<Panel> DefaultSet;
        private List<Panel> _loadedSet;

        private readonly List<string> _logs;
        private readonly int _count;
        private int _idx;
        private int _page;

        private float _yPos;
        private int _idx2;

        private readonly Texture2D _scene;

        public Log()
        {
            _scene = LoadTexture("Resource/select.png");
            
            Panels = new List<Panel>();
            DefaultSet = new List<Panel>();
            _loadedSet = new List<Panel>();

            _logs = Directory.Exists("Logs")
                ? Directory.GetFiles("Logs").ToList()
                : new List<string>();
            _idx = _page = 0;
            _count = (int)Math.Floor(1000 / 48.0f);

            _yPos = 0;
            _idx2 = 0;
        }

        public void Draw(bool shutdownRequest)
        {
            DrawTexture(_scene, 40, 40, Color.WHITE);
            DrawFiles(shutdownRequest);

            var text = _logs[_idx] == "기본 설정" ? "기본 설정" : File.GetCreationTime(_logs[_idx]).ToString("D");
            DrawTextEx(Ui.Galmuri48, text, new Vector2(757, 128), 48, 0, Color.BLACK);
            
            if (!DefaultSet.Any()) 
                DrawTextEx(Ui.Galmuri36, "기본 설정을 불러올 수 없습니다!", new Vector2(1053, 76), 36, 0, Color.RED);
            DrawLog();
        }

        public void Control(bool shutdownRequest)
        {
            SelectFile(shutdownRequest);
            switch ((KeyboardKey)GetKeyPressed())
            {
                case KeyboardKey.KEY_LEFT:
                    _page = _page == 0 ? _logs.Count / _count : _page - 1;
                    break;
                case KeyboardKey.KEY_RIGHT:
                    _page = _page == _logs.Count / _count ? 0 : _page + 1;
                    break;
                case KeyboardKey.KEY_ENTER:
                    Panels = _logs[_idx] == "기본 설정" ? DefaultSet : SaveLoad.LoadPanels(_logs[_idx]);
                    break;
            }
        }

        public void Dispose()
        {
            UnloadTexture(_scene);
            GC.SuppressFinalize(this);
        }

        public void Generate(List<string> defaultSet)
        {
            var rnd = new Random();
            
            foreach (var x in defaultSet)
            {
                var id = DefaultSet.FindIndex(y => y.Name == x);
                if (id != -1)
                    DefaultSet[id] = new Panel(DefaultSet[id].Name, DefaultSet[id].Count + 1, DefaultSet[id].Color);
                else DefaultSet.Add(new Panel(x, 1, ColorFromHSV(rnd.NextSingle() * 360.0f, 0.5f, 1)));
            }
            
            _logs.Insert(0, "기본 설정");
            _loadedSet = DefaultSet;
        }
        
        // Private Methods

        private void DrawFiles(bool shutdownRequest)
        {
            var page = _logs.Skip(_page * _count).Take(_count).ToArray();
            
            BeginScissorMode(40, 40, 480, 1000);
            for (var j = 0; j < page.Length; j++)
            {
                var button = new Rectangle(40, 40 + 48 * j, 480, 47);
                var pos = new Vector2(46, 46 + 48 * j);
                
                if (j == _idx % _count && _page == _idx / _count) DrawRectangleRec(button, Color.BLACK);
                if (Ui.IsHovering(button, !shutdownRequest)) DrawRectangleRec(button, Color.DARKGRAY);

                var textColor =
                    (j == _idx % _count && _page == _idx / _count) || Ui.IsHovering(button, !shutdownRequest)
                        ? Color.WHITE
                        : Color.BLACK;
                var text = page[j] == "기본 설정" ? page[j] : page[j][5..];
                DrawTextEx(Ui.Galmuri36, text, pos, 36, 0, textColor);
            }
            EndScissorMode();
            
            Ui.DrawTextCentered(new Rectangle(40, 1000, 480, 40), Ui.Galmuri24,
                $"{_page + 1} / {_logs.Count / _count + 1}", 24, Color.BLACK);
        }

        private void SelectFile(bool shutdownRequest)
        {
            var page = _logs.Skip(_page * _count).Take(_count).ToArray();

            for (var j = 0; j < page.Length; j++)
            {
                var button = new Rectangle(40, 40 + 48 * j, 480, 47);
                if (Ui.IsHovering(button, !shutdownRequest) && IsMouseButtonPressed(0))
                {
                    _idx = _page * _count + j;
                    _loadedSet = _logs[_idx] == "기본 설정" ? DefaultSet : SaveLoad.LoadPanels(_logs[_idx]);
                }
            }
        }

        private void DrawLog()
        {
            var panels = Marquee(_loadedSet);
            
            BeginScissorMode(560, 200, 1320, 840);
            for (var i = 0; i < panels.Count; i++)
            {
                var pos = new Vector2(560, 200 + _yPos + 48 * i);
                DrawRectangle((int)pos.X, (int)pos.Y, 1320, 48, panels[i].Color);
                DrawTextEx(Ui.Galmuri36, $"{panels[i].Name} * {panels[i].Count}", new Vector2(pos.X + 6, pos.Y + 6), 36,
                    0, Color.BLACK);
            }
            EndScissorMode();
        }

        private List<Panel> Marquee(IReadOnlyCollection<Panel> panels)
        {
            var count = (int)Math.Ceiling(840 / 48.0f);
            if (panels.Count >= count)
            {
                _yPos -= 120.0f / GetFPS();
                if (_yPos <= -48.0f)
                {
                    _idx2 = (_idx2 + 1) % panels.Count;
                    _yPos = 0;
                }
            }
            else
            {
                _yPos = 0;
                _idx2 = 0;
            }
            
            var output = panels.Skip(_idx2).Take(count + 1).ToList();
            if (panels.Count >= count && output.Count < count + 1)
                output.AddRange(panels.Take(count + 1 - output.Count));
            return output;
        }
    }
}