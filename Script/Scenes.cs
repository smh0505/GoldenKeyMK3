using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum Scene
    {
        Intro = 0,
        Login,
        Load,
        Board
    }
    
    public class Scenes : IGameObject
    {
        private Scene _current;
        private readonly Close _close;
        private readonly Intro _intro;
        private readonly Login _login;
        private readonly Log _log;
        private readonly Board _board;

        private Setting _setting;
        private bool _isTransparent;

        public Scenes()
        {
            _current = Scene.Intro;
            _close = new Close();
            _intro = new Intro();
            _login = new Login();
            _log = new Log();
            _board = new Board();

            _setting = new Setting();
            _isTransparent = false;
        }

        // Public Methods
        
        public void Draw(bool shutdownRequest)
        {
            var backColor = _isTransparent ? Color.GREEN : Color.LIGHTGRAY;
            ClearBackground(backColor);
            
            if (_current > Scene.Load)
            {
                _board.Draw();
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
                            _current = Scene.Board;
                            _board.Connect(_login.Payload, _log.DefaultSet, _login.RecoverGame);
                        }
                    }
                    break;
                case Scene.Load:
                    _log.Draw(shutdownRequest);
                    if (_log.Panels.Any())
                    {
                        _current = Scene.Board;
                        _board.Connect(_login.Payload, _log.Panels, _login.RecoverGame);
                    }
                    break;
                case Scene.Board:
                    _board.Draw();
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

            /*
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
            }*/

            if (!shutdownRequest) switch (_current)
            {
                case Scene.Login:
                    _login.Control(shutdownRequest);
                    break;
                case Scene.Load:
                    _log.Control(shutdownRequest);
                    break;
                case Scene.Board:
                    _board.Control(shutdownRequest);
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
            _board.Dispose();
            Ui.Dispose();
            GC.SuppressFinalize(this);
        }
        
        // Private Methods

        private void LoadFiles()
        {
            if (File.Exists("default.yml")) _setting = SaveLoad.LoadSetting();
            if (!string.IsNullOrEmpty(_setting.Key)) _login.ReadKey(_setting.Key);
            if (_setting.Values != null && _setting.Values.Any()) _log.Generate(_setting.Values);
        }
    }
}