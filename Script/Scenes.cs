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
        private Scene _currScene;
        private readonly Wheel _wheel;
        private readonly Login _login;
        private readonly LoadScene _load;
        public readonly Close CloseScene;
        private readonly Chat _chat;
        private readonly Board _board;
        
        private static string _switchText;

        private readonly Texture2D _minimizeIcon;
        private readonly Texture2D _closeIcon;

        public Scenes()
        {
            // Init itself
            _currScene = Scene.Intro;
            _switchText = "곡 추첨";

            _minimizeIcon = LoadTexture("Resource/minus.png");
            _closeIcon = LoadTexture("Resource/power.png");
            
            // Init scenes
            _wheel = new Wheel();
            _login = new Login(_wheel);
            _load = new LoadScene(_wheel);
            CloseScene = new Close();
            _board = new Board();
            _chat = new Chat(_board);

            if (File.Exists("default.yml")) SaveLoad.LoadSetting(_login);
        }

        public void Draw(bool shutdownRequest)
        {
            switch (_currScene)
            {
                case Scene.Intro:
                    if (Intro.Draw()) _currScene = Scene.Login;
                    break;
                case Scene.Login:
                    if (_login.Draw(shutdownRequest)) PostLogin();
                    break;
                case Scene.Load:
                    if (_load.Draw(shutdownRequest)) PrepareGame();
                    break;
                case Scene.Main:
                    _board.Draw();
                    _wheel.UpdateWheel(shutdownRequest);
                    break;
                case Scene.Board:
                    _board.Draw();
                    //_chat.Draw(shutdownRequest);
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            UnloadTexture(_minimizeIcon);
            UnloadTexture(_closeIcon);
            
            if (_wheel.Options.Any()) SaveLoad.SaveLog(_wheel);

            _login.Dispose();
            _load.Dispose();
            CloseScene.Dispose();
            _chat.Dispose();
            _wheel.Dispose();
            _board.Dispose();
        }

        // UIs

        public bool Buttons()
        {
            var minimizeButton = (int)_currScene > 2 ? new Rectangle(1476, 192, 50, 50)
                : new Rectangle(1796, 12, 50, 50);
            var closeButton = (int)_currScene > 2 ? new Rectangle(1538, 192, 50, 50)
                : new Rectangle(1858, 12, 50, 50);
            var switchButton = new Rectangle(1348, 828, 240, 60);
            
            if (DrawMinimizeButton(minimizeButton, Color.GREEN)) MinimizeWindow();
            if ((int)_currScene > 2 && DrawSwitchButton(switchButton, Color.YELLOW)) OnClick();
            return DrawCloseButton(closeButton, Color.RED);
        }

        private bool DrawMinimizeButton(Rectangle button, Color buttonColor)
            => Ui.DrawButton(button, buttonColor, 0.7f, _minimizeIcon);
        
        private bool DrawCloseButton(Rectangle button, Color buttonColor)
            => Ui.DrawButton(button, buttonColor, 0.7f, _closeIcon);

        private static bool DrawSwitchButton(Rectangle button, Color buttonColor)
            => Ui.DrawButton(button, buttonColor, 0.7f, Program.MainFont,
                _switchText, 48, Color.BLACK);

        // Controls

        private void PostLogin()
        {
            if (Directory.Exists("Logs") && Directory.GetFiles("Logs").Any())
                _currScene = Scene.Load;
            else
            {
                _wheel.Options = SaveLoad.DefaultOptions;
                PrepareGame();
            }
        }
        
        private void PrepareGame()
        {
            _currScene = Scene.Main;
            _login.Connect();
            _chat.Connect();
        }

        private void OnClick()
        {
            switch (_currScene)
            {
                case Scene.Main:
                    _currScene = Scene.Board;
                    _switchText = "황금열쇠";
                    break;
                case Scene.Board:
                    _currScene = Scene.Main;
                    _switchText = "곡 추첨";
                    break;
                case Scene.Intro:
                case Scene.Login:
                case Scene.Load:
                default:
                    break;
            }
        }
    }
}