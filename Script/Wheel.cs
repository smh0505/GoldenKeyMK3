using System.Collections.Immutable;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum WheelState
    {
        Idle = 0,
        Spinning,
        Stopping,
        Result
    }
    
    public class Wheel : IGameObject
    {
        public ImmutableList<string> WaitList;
        public List<Panel> Panels;
        private readonly Inventory _inventory;

        private WheelState _state;
        private float _startAngle;
        private float _theta;
        private readonly float _radius;
        private readonly Vector2 _center;

        private readonly Dictionary<WheelState, string> _buttonText;
        private readonly Texture2D _result;
        private bool _spinHover;

        public Wheel(Inventory inventory)
        {
            WaitList = ImmutableList<string>.Empty;
            Panels = new List<Panel>();
            _inventory = inventory;

            _state = WheelState.Idle;
            _startAngle = 180;
            _theta = 3000.0f;
            _radius = 250.0f;
            _center = new Vector2(680, 540);

            _buttonText = new Dictionary<WheelState, string>()
            {
                { WheelState.Idle, "돌리기" },
                { WheelState.Spinning, "멈추기" },
                { WheelState.Stopping, string.Empty },
                { WheelState.Result, "다음" },
            };

            _result = LoadTexture("Resource/next_key.png");
            _spinHover = false;
        }
        
        // Public Methods

        public void Draw()
        {
            if (Sum > 0)
            {
                DrawSectors();
                DrawLabels();

                Vector2[] vtx = { new(670, 280), new(680, 300), new(690, 280) };
                DrawTriangle(vtx[0], vtx[1], vtx[2], Color.BLACK);
            }
            if (_state == WheelState.Result) DrawResult();

            if (_state != WheelState.Stopping)
            {
                var spinColor = _spinHover ? Color.RED : Fade(Color.RED, 0.7f);
                DrawCircle(400, 820, 60.0f, spinColor);
                Ui.DrawTextCentered(new Rectangle(340, 760, 120, 120), Ui.Galmuri36, _buttonText[_state], 36, Color.BLACK);
            }
        }

        public void Control(bool shutdownRequest)
        {
            Update();

            _spinHover = Ui.IsHovering(new Vector2(400, 820), 60.0f, !shutdownRequest);
            if (_state != WheelState.Stopping && _spinHover && IsMouseButtonPressed(0))
            {
                _state = (WheelState)((int)(_state + 1) % 4);
                if (_state != WheelState.Idle) return;

                var target = Result();
                _inventory.AddItem(target.Name);
                RemovePanel(target);
            }
        }

        public void Dispose()
        {
            UnloadTexture(_result);
            GC.SuppressFinalize(this);
        }
        
        // Private Methods

        private int Sum => Panels.Sum(x => x.Count);
        private float Unit => 360.0f / Sum;

        private void Update()
        {
            switch (_state)
            {
                case WheelState.Idle:
                    _theta = 3000.0f;
                    AddPanel();
                    break;
                case WheelState.Spinning:
                case WheelState.Stopping:
                    _startAngle -= _theta * GetFrameTime();
                    if (_startAngle <= 0.0f) _startAngle += 360.0f;
                    if (_state == WheelState.Stopping)
                    {
                        _theta -= 60 / MathF.PI;
                        if (_theta <= 0.0f) _state = WheelState.Result;
                    }
                    break;
            }
        }

        private void AddPanel()
        {
            var rnd = new Random();
            var panels = WaitList.ToArray();
            WaitList = WaitList.Clear();

            foreach (var x in panels)
            {
                var id = Panels.FindIndex(y => y.Name == x);
                if (id != -1)
                    Panels[id] = new Panel(Panels[id].Name, Panels[id].Count + 1, Panels[id].Color);
                else Panels.Add(new Panel(x, 1, ColorFromHSV(rnd.NextSingle() * 360.0f, 0.5f, 1)));
            }
        }

        private void RemovePanel(Panel panel)
        {
            var id = Panels.IndexOf(panel);
            Panels[id] = new Panel(panel.Name, panel.Count - 1, panel.Color);
            if (Panels[id].Count == 0) Panels.RemoveAt(id);
        }

        private Panel Result()
        {
            if (Sum == 0) return new Panel(string.Empty, 1, Color.WHITE);

            var theta = 540.0f - _startAngle;
            var id = (int)Math.Floor((theta >= 360.0f ? theta - 360 : theta) / Unit);

            Panel target;
            var i = 0;
            var idCount = 0;
            
            do
            {
                target = Panels[i];
                idCount += Panels[i].Count;
                i++;
            } while (idCount <= id);

            return target;
        }

        // UI
        
        private void DrawSectors()
        {
            var current = _startAngle;
            
            foreach (var x in Panels)
            {
                DrawCircleSector(_center, _radius, current, current + Unit * x.Count, 0, x.Color);
                current += Unit * x.Count;
            }
        }

        private void DrawLabels()
        {
            var current = _startAngle;

            foreach (var x in Panels)
            {
                var size = MeasureTextEx(Ui.Galmuri24, x.Name, 24, 0);
                var origin = new Vector2((_radius + size.X) / 2, size.Y / 2);
                var theta = -90.0f - (current + Unit * x.Count / 2);
                DrawTextPro(Ui.Galmuri24, x.Name, _center, origin, theta, 24, 0, Color.BLACK);
                current += Unit * x.Count;
            }
        }

        private void DrawResult()
        {
            DrawTexture(_result, 330, 190, Color.WHITE);
            var pos = new Vector2(354, 374);
            BeginScissorMode(330, 190, 700, 700);
            DrawTextEx(Ui.Galmuri60, Result().Name, pos, 60, 0, Color.YELLOW);
            EndScissorMode();
        }
    }
}