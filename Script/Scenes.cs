using System.Diagnostics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum Scene
    {
        Intro = 0,
        Login,
        Load,
        Wheel,
        Poll,
        Raffle,
        Dice
    }
    
    public class Scenes : IDisposable
    {
        private Scene _current;
        private readonly Close _close;
        private readonly Intro _intro;
        private readonly Login _login;
        private readonly Log _log;
        private readonly Wheel _wheel;
        private readonly Donation _donation;
        private readonly Board _board;
        private readonly Poll _poll;
        private readonly Chat _chat;

        private readonly Dice _dice;

        private Setting _setting;
        private bool _isTransparent;

        private bool _isClockwise;
        private bool _isTicking;
        private int _idx;
        private readonly string[] _laps;
        private readonly Stopwatch _stopwatch;
        private TimeSpan _timeSpan;
        private double _offset;
        private readonly Texture2D _timer;
        
        public Scenes()
        {
            _current = Scene.Intro;
            _close = new Close();
            _intro = new Intro();
            _login = new Login();
            _log = new Log();
            _wheel = new Wheel();
            _donation = new Donation(_wheel);
            _board = new Board();
            _poll = new Poll(_board);
            _chat = new Chat(_poll, _board);

            _dice = new Dice();

            _setting = new Setting();
            _isTransparent = false;

            _isClockwise = true;
            _isTicking = false;
            _idx = 0;
            _laps = new[] { "1", "2", "3", "B" };
            _stopwatch = new Stopwatch();
            _timeSpan = TimeSpan.Zero;
            _offset = 0;
            _timer = LoadTexture("Resource/timerButton.png");
        }

        // Public Methods
        
        public void Draw(bool shutdownRequest)
        {
            var backColor = _isTransparent ? Color.GREEN : Color.LIGHTGRAY;
            ClearBackground(backColor);
            
            if (_current > Scene.Load)
            {
                _board.Draw();
                DrawTimer(shutdownRequest);
            }

            switch (_current)
            {
                case Scene.Intro:
                    if (_intro.Draw())
                    {
                        LoadFiles();
                        _current++;
                    }
                    break;
                case Scene.Login:
                    _login.Draw();
                    if (!string.IsNullOrEmpty(_login.Payload))
                    {
                        if (Directory.Exists("Logs") && Directory.GetFiles("Logs").Any())
                            _current = Scene.Load;
                        else
                        {
                            _current = Scene.Wheel;
                            _wheel.Panels = _log.DefaultSet;
                            _chat.Connect();
                            _donation.Connect(_login.Payload);
                        }
                    }
                    break;
                case Scene.Load:
                    _log.Draw(shutdownRequest);
                    if (_log.Panels.Any())
                    {
                        _wheel.Panels = _log.Panels;
                        _current = Scene.Wheel;
                        _chat.Connect();
                        _donation.Connect(_login.Payload);
                    }
                    break;
                case Scene.Wheel:
                    _wheel.Draw();
                    break;
                case Scene.Poll:
                    _poll.Draw(shutdownRequest);
                    break;
                case Scene.Dice:
                    _dice.Draw();
                    break;
            }
            
            if (shutdownRequest) _close.Draw();
        }

        public void Control(ref bool shutdownRequest, out bool shutdownResponse)
        {
            if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL)) switch ((KeyboardKey)GetKeyPressed())
            {
                case KeyboardKey.KEY_TAB:
                    SetWindowMonitor((GetCurrentMonitor() + 1) % GetMonitorCount());
                    SetTargetFPS(GetMonitorRefreshRate(GetCurrentMonitor()));
                    break;
                case KeyboardKey.KEY_P:
                    _isTransparent = !_isTransparent;
                    break;
            }

            if (!shutdownRequest && _current > Scene.Load)
            {
                ControlTimer(shutdownRequest);
                switch ((KeyboardKey)GetKeyPressed())
                {
                    case KeyboardKey.KEY_F1:
                        _current = Scene.Wheel;
                        break;
                    case KeyboardKey.KEY_F2:
                        _current = Scene.Poll;
                        break;
                    case KeyboardKey.KEY_F4:
                        _current = Scene.Dice;
                        break;
                    case KeyboardKey.KEY_F10:
                        _board.Shuffle(_isClockwise);
                        break;
                    case KeyboardKey.KEY_F11:
                        _board.AddKey(_isClockwise);
                        break;
                    case KeyboardKey.KEY_F12:
                        _board.Restore();
                        break;
                }
            }

            if (!shutdownRequest) switch (_current)
            {
                case Scene.Login:
                    _login.Control(shutdownRequest);
                    break;
                case Scene.Load:
                    _log.Control(shutdownRequest);
                    break;
                case Scene.Wheel:
                    _wheel.Control(shutdownRequest);
                    break;
                case Scene.Poll:
                    _poll.Control(shutdownRequest);
                    break;
            }
            
            _close.Control(ref shutdownRequest, out shutdownResponse);
        }

        public void Dispose()
        {
            _intro.Dispose();
            _close.Dispose();
            _login.Dispose();
            _log.Dispose();
            _wheel.Dispose();
            _donation.Dispose();
            _board.Dispose();
            _poll.Dispose();
            _chat.Dispose();
            Ui.Dispose();
            SaveLoad.SaveLog(_wheel);
            GC.SuppressFinalize(this);
        }
        
        // Private Methods

        private void LoadFiles()
        {
            if (File.Exists("default.yml")) _setting = SaveLoad.LoadSetting();
            if (!string.IsNullOrEmpty(_setting.Key)) _login.ReadKey(_setting.Key);
            if (_setting.Values != null && _setting.Values.Any()) _log.Generate(_setting.Values);
        }

        private void DrawTimer(bool shutdownRequest)
        {
            DrawRectangle(1080, 186, 510, 62, Color.WHITE);
            
            var downLap = new Rectangle(1086, 192, 30, 50);
            var upLap = new Rectangle(1166, 192, 30, 50);
            var clockwise = new Rectangle(1200, 192, 120, 50);
            var plus = new Rectangle(1322, 192, 40, 50);
            var pause = new Rectangle(1364, 192, 180, 50);
            var minus = new Rectangle(1546, 192, 40, 50);
            
            DrawRectangleRec(downLap, Ui.IsHovering(downLap, !shutdownRequest) ? Color.GOLD : Fade(Color.GOLD, 0.7f));
            DrawRectangleRec(upLap, Ui.IsHovering(upLap, !shutdownRequest) ? Color.GOLD : Fade(Color.GOLD, 0.7f));
            DrawRectangleRec(clockwise, Ui.IsHovering(clockwise, !shutdownRequest) ? Color.LIME : Fade(Color.LIME, 0.7f));
            DrawRectangleRec(plus, Ui.IsHovering(plus, !shutdownRequest) ? Color.YELLOW : Fade(Color.YELLOW, 0.7f));
            DrawRectangleRec(pause, Ui.IsHovering(pause, !shutdownRequest) 
                ? (_isTicking ? Color.GREEN : Color.RED) 
                : (_isTicking ? Fade(Color.GREEN, 0.7f) : Fade(Color.RED, 0.7f)));
            DrawRectangleRec(minus, Ui.IsHovering(minus, !shutdownRequest) ? Color.YELLOW : Fade(Color.YELLOW, 0.7f));
            
            DrawTexture(_timer, 1086, 192, Color.BLACK);
            Ui.DrawTextCentered(new Rectangle(1116, 192, 50, 50), Ui.Galmuri48, _laps[_idx], 48, Color.BLACK);
            Ui.DrawTextCentered(clockwise, Ui.Galmuri48, _isClockwise ? "시계" : "반시계", 48, Color.BLACK);
            Ui.DrawTextCentered(new Rectangle(1324, 192, 260, 50), Ui.Galmuri48, 
                $"{_timeSpan.Hours:00}:{_timeSpan.Minutes:00}:{_timeSpan.Seconds:00}", 48, Color.BLACK);
        }

        private void ControlTimer(bool shutdownRequest)
        {
            if (_isTicking) _timeSpan = _stopwatch.Elapsed + TimeSpan.FromSeconds(_offset);

            if (Ui.IsHovering(new Rectangle(1086, 192, 30, 50), !shutdownRequest) && IsMouseButtonPressed(0))
                _idx = Math.Clamp(--_idx, 0, 3);
            if (Ui.IsHovering(new Rectangle(1166, 192, 30, 50), !shutdownRequest) && IsMouseButtonPressed(0))
                _idx = Math.Clamp(++_idx, 0, 3);
            if (Ui.IsHovering(new Rectangle(1200, 192, 120, 50), !shutdownRequest) && IsMouseButtonPressed(0))
                _isClockwise = !_isClockwise;
            if (Ui.IsHovering(new Rectangle(1322, 192, 40, 50), !shutdownRequest) && IsMouseButtonPressed(0))
                _offset += 60.0f;
            if (Ui.IsHovering(new Rectangle(1364, 192, 180, 50), !shutdownRequest) && IsMouseButtonPressed(0))
            {
                _isTicking = !_isTicking;
                if (_isTicking) _stopwatch.Start();
                else _stopwatch.Stop();
            }
            if (Ui.IsHovering(new Rectangle(1546, 192, 40, 50), !shutdownRequest) && IsMouseButtonPressed(0))
                _offset -= Math.Min(60, _timeSpan.TotalSeconds);
        }
    }
}