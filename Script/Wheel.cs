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

    public struct WheelPanel
    {
        public string Name;
        public int Count;
        public Color Color;

        public WheelPanel(string name, int count, Color color)
        {
            Name = name;
            Count = count;
            Color = color;
        }
    }

    public class Wheel
    {
        private readonly Chat _chat;
        
        public ImmutableList<string> WaitList;
        public List<WheelPanel> Options;
        
        private int Sum => Options.Sum(option => option.Count);
        private static WheelState _state = WheelState.Idle;
        private static float _startAngle;
        private static float _diffAngle = 50.0f;
        private static readonly Random Rnd = new ();
        
        private static readonly Dictionary<WheelState, string> ButtonPool = new ()
        {
            {WheelState.Idle, "돌리기"},
            {WheelState.Spinning, "멈추기"},
            {WheelState.Result, "다음"}
        };

        private readonly Texture2D _result;

        public Wheel(Chat chat)
        {
            _chat = chat;
            
            WaitList = ImmutableList<string>.Empty;
            Options = new List<WheelPanel>();

            _result = LoadTexture("Resource/next_key.png");
        }

        public void UpdateWheel(bool shutdownRequest)
        {
            if (!shutdownRequest && _chat.State != PollState.Active) _chat.DrawButtons();
            if (Sum > 0) DrawWheel();
            
            switch (_state)
            {
                case WheelState.Idle:
                    _diffAngle = 50.0f;
                    AddOption();
                    break;
                case WheelState.Spinning:
                    _startAngle += _diffAngle;
                    if (_startAngle >= 360.0f) _startAngle -= 360.0f;
                    break;
                case WheelState.Stopping:
                    _startAngle += _diffAngle;
                    if (_startAngle >= 360.0f) _startAngle -= 360.0f;
                    _diffAngle -= 1 / MathF.PI;
                    if (_diffAngle <= 0.0f) _state = WheelState.Result;
                    break;
                case WheelState.Result:
                    DrawResult();
                    break;
            }

            if (_state == WheelState.Stopping || Sum == 0 || shutdownRequest) return;
            if (DrawButton(new Vector2(400, 820), ButtonPool[_state]))
                OnClick();
            if (IsKeyPressed(KeyboardKey.KEY_SPACE)) OnClick();
        }

        public void Dispose()
        {
            UnloadTexture(_result);
        }

        // UIs

        private void DrawWheel()
        {
            var currAngle = _startAngle;
            var unitAngle = 360.0f / Sum;
            var center = new Vector2(680, 540);
            const float radius = 250.0f;

            // Circular sectors
            foreach (var option in Options)
            {
                DrawCircleSector(center, radius, currAngle, currAngle + unitAngle * option.Count, 0, option.Color);
                currAngle += unitAngle * option.Count;
            }

            // Labels
            currAngle = _startAngle;
            foreach (var option in Options)
            {
                var size = MeasureTextEx(Ui.Galmuri24, option.Name, 24, 0);
                var origin = new Vector2((radius + size.X) / 2, size.Y / 2);
                var theta = -90.0f - (currAngle + unitAngle * option.Count / 2);
                DrawTextPro(Ui.Galmuri24, option.Name, center, origin, theta, 24, 0, Color.BLACK);
                currAngle += unitAngle * option.Count;
            }

            // Triangular arrow
            Vector2[] vtx = { new (670, 280), new (680, 300), new (690, 280) };
            DrawTriangle(vtx[0], vtx[1], vtx[2], Color.BLACK);
        }

        private static bool DrawButton(Vector2 center, string buttonText)
            => Ui.DrawButton(center, 60.0f, Color.GREEN, 0.7f,
                Ui.Galmuri36, buttonText, 36, Color.BLACK);

        private void DrawResult()
        { 
            DrawTexture(_result, 330, 190, Color.WHITE);
            var namePos = new Vector2(354, 374);
            BeginScissorMode(330, 190, 700, 700);
            DrawTextEx(Ui.Galmuri60, Result().Name, namePos, 60, 0, Color.YELLOW);
            EndScissorMode();
        }

        // Controls

        private void AddOption()
        {
            var optionList = WaitList.ToList();
            WaitList = WaitList.Clear();

            foreach (var option in optionList)
            {
                var id = Options.FindIndex(x => x.Name == option);
                if (id != -1)
                {
                    var newOption = new WheelPanel(Options[id].Name, Options[id].Count + 1, Options[id].Color);
                    Options.RemoveAt(id);
                    Options.Insert(id, newOption);
                }
                else
                {
                    var newOption = new WheelPanel(option, 1, ColorFromHSV(Rnd.NextSingle() * 360.0f, 0.5f, 1));
                    Options.Add(newOption);
                }
            }
        }

        private void RemoveOption(WheelPanel option)
        {
            var id = Options.IndexOf(option);
            Options.Remove(option);
            if (option.Count <= 1) return;
            var newOption = new WheelPanel(option.Name, option.Count - 1, option.Color);
            Options.Insert(id, newOption);
        }

        private WheelPanel Result()
        {
            if (Options.Count <= 0) return new WheelPanel(string.Empty, 1, Color.WHITE);
            var theta = 540.0f - _startAngle;
            var id = (int)Math.Floor((theta >= 360.0f ? theta - 360 : theta) / (360.0f / Sum));

            var target = Options[0];
            var idCount = 0;
            foreach (var option in Options)
            {
                target = option;
                idCount += option.Count;
                if (idCount > id) break;
            }
            return target;
        }

        private void OnClick()
        {
            _state = (WheelState)(((int)_state + 1) % 4);
            if (_state == WheelState.Idle) RemoveOption(Result());
        }
    }
}