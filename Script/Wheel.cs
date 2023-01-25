using System.Collections.Concurrent;
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
        public static ConcurrentBag<string> Waitlist = new ConcurrentBag<string>();
        public static List<WheelPanel> Options = new List<WheelPanel>();
        public static int Sum => Options.Sum(option => option.Count);

        private static WheelState _state = WheelState.Idle;
        private static float _startAngle;
        private static float _diffAngle = 50.0f;
        private static readonly Random Rnd = new Random();
        private static readonly Dictionary<WheelState, string> ButtonPool = new Dictionary<WheelState, string>
        {
            {WheelState.Idle, "돌리기"},
            {WheelState.Spinning, "멈추기"},
            {WheelState.Result, "다음"}
        };

        public static void UpdateWheel(bool shutdownRequest)
        {
            switch (_state)
            {
                case WheelState.Idle:
                    _diffAngle = 50.0f;
                    AddOption();
                    break;
                case WheelState.Spinning:
                case WheelState.Stopping:
                    _startAngle += _diffAngle;
                    if (_startAngle >= 360.0f) _startAngle -= 360.0f;
                    if (_state == WheelState.Stopping) _diffAngle -= 1 / MathF.PI;
                    if (_diffAngle <= 0.0f) _state = WheelState.Result;
                    break;
            }
            DrawWheel();
            if (_state == WheelState.Result) DrawResult();
            DrawButton(shutdownRequest);
        }

        private static void AddOption()
        {
            List<string> optionList = Waitlist.ToList();
            Waitlist.Clear();

            foreach (var option in optionList)
            {
                int id = Options.FindIndex(x => x.Name == option);
                if (id != -1)
                {
                    var newOption = new WheelPanel(Options[id].Name, Options[id].Count + 1, Options[id].Color);
                    Options.RemoveAt(id);
                    Options.Insert(id, newOption);
                }
                else
                {
                    var newOption = new WheelPanel(option, 1,
                        ColorFromHSV(Rnd.NextSingle() * 360.0f, Rnd.NextSingle(), Rnd.NextSingle() * 0.5f + 0.5f));
                    Options.Add(newOption);
                }
            }
        }

        private static void RemoveOption(WheelPanel option)
        {
            int id = Options.IndexOf(option);
            Options.Remove(option);
            if (option.Count > 1)
            {
                var newOption = new WheelPanel(option.Name, option.Count - 1, option.Color);
                Options.Insert(id, newOption);
            }
        }

        private static WheelPanel Result()
        {
            if (Options.Count > 0)
            {
                float theta = 540.0f - _startAngle;
                int id = (int)Math.Floor((theta >= 360.0f ? theta - 360 : theta) / (360.0f / Sum));

                WheelPanel target = Options[0];
                int idCount = 0;
                foreach (var option in Options)
                {
                    target = option;
                    idCount += option.Count;
                    if (idCount > id) break;
                }
                return target;
            }
            return new WheelPanel(string.Empty, 1, Color.WHITE);
        }

        private static void DrawWheel()
        {
            float currAngle = _startAngle;
            float unitAngle = 360.0f / Sum;
            Vector2 center = new Vector2(GetScreenWidth() * 0.3f, GetScreenHeight() * 0.5f);
            float radius = GetScreenHeight() * 0.375f;

            foreach (var option in Options)
            {
                DrawCircleSector(center, radius, currAngle, currAngle + unitAngle * option.Count, 0, option.Color);
                currAngle += unitAngle * option.Count;
            }

            currAngle = _startAngle;
            foreach (var option in Options)
            {
                Vector2 origin = new Vector2(radius / 2 + MeasureTextEx(Program.MainFont, option.Name, 24, 0).X / 2, 12);
                float theta = -90.0f - (currAngle + unitAngle * option.Count / 2);
                DrawTextPro(Program.MainFont, option.Name, center, origin, theta, 24, 0, Color.BLACK);
                currAngle += unitAngle * option.Count;
            }

            Vector2[] vtx =
            {
                new Vector2(GetScreenWidth() * 0.3f - 20, GetScreenHeight() * 0.125f - 20),
                new Vector2(GetScreenWidth() * 0.3f, GetScreenHeight() * 0.125f + 20),
                new Vector2(GetScreenWidth() * 0.3f + 20, GetScreenHeight() * 0.125f - 20),
            };
            DrawTriangle(vtx[0], vtx[1], vtx[2], Color.BLACK);
        }

        private static void DrawButton(bool shutdownRequest)
        {
            Vector2 center = new Vector2(80, GetScreenHeight() - 80.0f);

            if (_state != WheelState.Stopping && Options.Count > 0)
            {
                var buttonText = ButtonPool[_state];
                Color buttonColor = Fade(Color.GREEN, 0.7f);
                if (CheckCollisionPointCircle(GetMousePosition(), center, 60.0f) && !shutdownRequest)
                {
                    if (IsMouseButtonPressed(0))
                    {
                        _state = (WheelState)(((int)_state + 1) % 4);
                        if (_state == WheelState.Idle) RemoveOption(Result());
                    }
                    else buttonColor = Color.GREEN;
                }

                var buttonTextLen = MeasureTextEx(Program.MainFont, buttonText, 36, 0).X;
                var buttonTextPos = new Vector2(center.X - buttonTextLen / 2, center.Y - 18);
                DrawCircleV(center, 60.0f, buttonColor);
                DrawTextEx(Program.MainFont, buttonText, buttonTextPos, 36, 0, Color.BLACK);
            }
        }

        private static void DrawResult()
        {
            DrawRectangle(12, 12, (int)(GetScreenWidth() * 0.6f), GetScreenHeight() - 24, Fade(Color.BLACK, 0.5f));

            string text = "이번 황금열쇠는";
            Vector2 textPos = new Vector2(GetScreenWidth() * 0.3f - MeasureTextEx(Program.MainFont, text, 48, 0).X / 2,
                GetScreenHeight() * 0.4f - 48);
            DrawTextEx(Program.MainFont, text, textPos, 48, 0, Color.WHITE);

            Vector2 namePos =
                new Vector2(GetScreenWidth() * 0.3f - MeasureTextEx(Program.MainFont, Result().Name, 72, 0).X / 2,
                    GetScreenHeight() * 0.4f);
            BeginScissorMode(12, (int)(GetScreenHeight() * 0.4f), (int)(GetScreenWidth() * 0.6f), 72);
            DrawTextEx(Program.MainFont, Result().Name, namePos, 72, 0, Color.WHITE);
            EndScissorMode();
        }
    }
}