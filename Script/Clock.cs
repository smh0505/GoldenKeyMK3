using System.Diagnostics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Clock : IGameObject
    {
        public bool IsClockwise;
        private bool _isTicking;

        public int Idx;
        public TimeSpan Offset;
        public TimeSpan TimeSpan;

        private readonly string[] _laps;
        private readonly Stopwatch _stopwatch;
        private readonly Texture2D _timer;

        private readonly Rectangle[] _buttons;
        private readonly Color[] _buttonColors;
        private readonly bool[] _buttonHover;

        private readonly Rectangle[] _ui;

        public Clock()
        {
            IsClockwise = true;
            _isTicking = false;
            
            Idx = 0;
            Offset = TimeSpan.Zero;
            TimeSpan = TimeSpan.Zero;

            _laps = new[] { "1", "2", "3", "B" };
            _stopwatch = new Stopwatch();
            _timer = LoadTexture("Resource/timerButton.png");

            _buttons = new Rectangle[]
            {
                new(1086, 192, 30, 50),
                new(1166, 192, 30, 50),
                new(1200, 192, 120, 50),
                new(1322, 192, 40, 50),
                new(1364, 192, 180, 50),
                new(1546, 192, 40, 50)
            };
            _buttonColors = new[]
            {
                Color.GOLD, Color.GOLD, Color.LIME,
                Color.YELLOW, Color.RED, Color.YELLOW
            };
            _buttonHover = new[] { false, false, false, false, false, false };

            _ui = new Rectangle[]
            {
                new(1116, 192, 50, 50),
                new(1200, 192, 120, 50),
                new(1324, 192, 260, 50)
            };
        }

        public void Draw()
        {
            DrawRectangle(1080, 186, 510, 62, Color.WHITE);

            _buttonColors[4] = _isTicking ? Color.BLUE : Color.RED;
            for (var i = 0; i < 6; i++)
                DrawRectangleRec(_buttons[i], _buttonHover[i] ? _buttonColors[i] : Fade(_buttonColors[i], 0.7f));
            
            DrawTexture(_timer, 1086, 192, Color.BLACK);
            Ui.DrawTextCentered(_ui[0], Ui.Galmuri48, _laps[Idx], 48, Color.BLACK);
            Ui.DrawTextCentered(_ui[1], Ui.Galmuri48, IsClockwise ? "시계" : "반시계", 48, Color.BLACK);
            Ui.DrawTextCentered(_ui[2], Ui.Galmuri48,
                $"{TimeSpan.Hours:00}:{TimeSpan.Minutes:00}:{TimeSpan.Seconds:00}", 48, Color.BLACK);
        }

        public void Control(bool shutdownRequest)
        {
            if (_isTicking) TimeSpan = _stopwatch.Elapsed + Offset;
            
            for (var i = 0; i < 6; i++)
                _buttonHover[i] = Ui.IsHovering(_buttons[i], !shutdownRequest);

            if (_buttonHover[0] && IsMouseButtonPressed(0))
                Idx = Math.Clamp(--Idx, 0, 3);
            if (_buttonHover[1] && IsMouseButtonPressed(0))
                Idx = Math.Clamp(++Idx, 0, 3);
            if (_buttonHover[2] && IsMouseButtonPressed(0))
                IsClockwise = !IsClockwise;
            if (_buttonHover[3] && IsMouseButtonPressed(0))
                Offset += TimeSpan.FromMinutes(1);
            if (_buttonHover[4] && IsMouseButtonPressed(0))
            {
                _isTicking = !_isTicking;
                if (_isTicking) _stopwatch.Start();
                else _stopwatch.Stop();
            }
            if (_buttonHover[5] && IsMouseButtonPressed(0))
                Offset -= TimeSpan.FromSeconds(Math.Min(60, TimeSpan.TotalSeconds));
        }

        public void Dispose()
        {
            UnloadTexture(_timer);
            GC.SuppressFinalize(this);
        }
    }
}