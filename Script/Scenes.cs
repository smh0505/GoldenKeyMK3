using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum Scene
    {
        Intro = 0,  // Intro.cs
        Login,      // Login.cs
        Load,       // LoadScene.cs
        Main,       // Wheel.cs
        Board       // Chat.cs
    }

    public class Scenes
    {
        public Scene CurrScene;
        public readonly Close CloseScene;
        private readonly Wheel _wheel;
        private readonly Login _login;
        private readonly LoadScene _load;
        private readonly Chat _chat;
        private readonly Board _board;
        
        private readonly Texture2D _minimizeIcon;
        private readonly Texture2D _closeIcon;
        private readonly Texture2D _timer;

        private int _laps;
        private bool _isClockwise;
        private bool _isTicking;
        private double _beginTime;
        private double _currTime;
        private decimal _timePassed;

        public Scenes()
        {
            // Init itself
            CurrScene = Scene.Intro;

            _minimizeIcon = LoadTexture("Resource/minus.png");
            _closeIcon = LoadTexture("Resource/power.png");
            _timer = LoadTexture("Resource/timerButton.png");
            
            // Init scenes
            _board = new Board();
            _chat = new Chat(_board, this);
            _wheel = new Wheel(_chat);
            _login = new Login(_wheel);
            _load = new LoadScene(_wheel);
            CloseScene = new Close();

            if (File.Exists("default.yml")) SaveLoad.LoadSetting(_login);
            _laps = 1;
            _isClockwise = true;
            _isTicking = false;
            _beginTime = 0;
            _currTime = 0;
            _timePassed = 0;
        }

        public void Draw(bool shutdownRequest)
        {
            switch (CurrScene)
            {
                case Scene.Intro:
                    if (Intro.Draw()) CurrScene = Scene.Login;
                    break;
                case Scene.Login:
                    if (_login.Draw(shutdownRequest)) PostLogin();
                    break;
                case Scene.Load:
                    if (_load.Draw(shutdownRequest)) PrepareGame();
                    break;
                case Scene.Main:
                    _board.Draw();
                    Timer(shutdownRequest);
                    _wheel.UpdateWheel(shutdownRequest);
                    break;
                case Scene.Board:
                    _board.Draw();
                    Timer(shutdownRequest);
                    _chat.Draw(shutdownRequest);
                    break;
            }
        }

        public void Dispose()
        {
            UnloadTexture(_minimizeIcon);
            UnloadTexture(_closeIcon);
            UnloadTexture(_timer);
            
            if (_wheel.Options.Any()) SaveLoad.SaveLog(_wheel);

            _login.Dispose();
            _load.Dispose();
            CloseScene.Dispose();
            _chat.Dispose();
            _wheel.Dispose();
            _board.Dispose();
            Intro.Dispose();
            Ui.Dispose();
        }

        // UIs

        public bool Buttons()
        {
            var minimizeButton = (int)CurrScene > 2 ? new Rectangle(1476, 192, 50, 50)
                : new Rectangle(1796, 12, 50, 50);
            var closeButton = (int)CurrScene > 2 ? new Rectangle(1538, 192, 50, 50)
                : new Rectangle(1858, 12, 50, 50);
            
            if (Ui.DrawButton(minimizeButton, Color.GREEN, 0.7f)) MinimizeWindow();
            DrawTexture(_minimizeIcon, (int)minimizeButton.x, (int)minimizeButton.y, Color.WHITE);
            
            var output = Ui.DrawButton(closeButton, Color.RED, 0.7f);
            DrawTexture(_closeIcon, (int)closeButton.x, (int)closeButton.y, Color.WHITE);
            return output;
        }

        private void Timer(bool shutdownRequest)
        {
            if (!shutdownRequest && _isTicking) _timePassed = (decimal)(GetTime() - _beginTime + _currTime);
            
            if (!shutdownRequest && Ui.DrawButton(new Rectangle(966, 192, 30, 50), Color.GOLD, 0.7f))
                _laps = Math.Clamp(--_laps, 1, 3);
            if (!shutdownRequest && Ui.DrawButton(new Rectangle(1046, 192, 30, 50), Color.GOLD, 0.7f))
                _laps = Math.Clamp(++_laps, 1, 3);
            if (!shutdownRequest && Ui.DrawButton(new Rectangle(1080, 192, 120, 50), Color.LIME, 0.7f))
                _isClockwise = !_isClockwise;
            if (!shutdownRequest && Ui.DrawButton(new Rectangle(1202, 192, 40, 50), Color.YELLOW, 0.7f))
                _beginTime -= 60;
            if (!shutdownRequest && Ui.DrawButton(new Rectangle(1244, 192, 180, 50), _isTicking ? Color.GREEN : Color.RED, 0.7f))
            {
                _isTicking = !_isTicking;
                if (_isTicking) _beginTime = GetTime();
                else _currTime = GetTime() - _beginTime;
            }
            if (!shutdownRequest && Ui.DrawButton(new Rectangle(1426, 192, 40, 50), Color.YELLOW, 0.7f))
            {
                _beginTime += 60;
                if (_beginTime > GetTime()) _beginTime = GetTime();
            }

            DrawTexture(_timer, 966, 192, Color.BLACK);
            Ui.DrawCenteredText(new Rectangle(996, 192, 50, 50), Ui.Galmuri48, 
                _laps.ToString(), 48, Color.BLACK);
            Ui.DrawCenteredText(new Rectangle(1080, 192, 120, 50), Ui.Galmuri48, 
                _isClockwise ? "시계" : "반시계", 48, Color.BLACK);
            Ui.DrawCenteredText(new Rectangle(1204, 192, 260, 50), Ui.Galmuri48, 
                $"{_timePassed / 3600:00}:{(_timePassed / 60) % 60:00}:{_timePassed % 60:00}", 48, Color.BLACK);
        }

        // Controls

        private void PostLogin()
        {
            if (Directory.Exists("Logs") && Directory.GetFiles("Logs").Any())
                CurrScene = Scene.Load;
            else
            {
                _wheel.Options = SaveLoad.DefaultOptions;
                PrepareGame();
            }
        }
        
        private void PrepareGame()
        {
            CurrScene = Scene.Board;
            _login.Connect();
            _chat.Connect();
        }
    }
}