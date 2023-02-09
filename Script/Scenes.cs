using System.Numerics;
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
        private static Scene _currScene;
        private static readonly Texture2D MinimizeIcon = LoadTexture("Resource/minus.png");
        private static readonly Texture2D CloseIcon = LoadTexture("Resource/power.png");
        private static string _switchText = "곡 추첨";

        public static void DrawScene(bool shutdownRequest)
        {
            switch (_currScene)
            {
                case Scene.Intro:
                    if (Intro.DrawIntro()) _currScene = Scene.Login;
                    break;
                case Scene.Login:
                    if (Login.DrawLogin(shutdownRequest)) PostLogin();
                    break;
                case Scene.Load:
                    if (LoadScene.DrawLoad(shutdownRequest)) PrepareGame();
                    break;
                case Scene.Main:
                    Wheel.UpdateWheel(shutdownRequest);
                    break;
                case Scene.Board:
                    Chat.DrawChat(shutdownRequest);
                    break;
            }
        }

        public static void Dispose()
        {
            UnloadTexture(MinimizeIcon);
            UnloadTexture(CloseIcon);
        }

        // UIs

        public static bool Buttons()
        {
            var minimizeButton = new Rectangle(GetScreenWidth() - 124, 12, 50, 50);
            var closeButton = new Rectangle(GetScreenWidth() - 62, 12, 50, 50);
            var switchButton = new Rectangle(GetScreenWidth() - 252, GetScreenHeight() - 72, 240, 60);

            var minimizeColor = Fade(Color.GREEN, 0.7f);
            var closeColor = Fade(Color.RED, 0.7f);
            var switchColor = Fade(Color.YELLOW, 0.7f);

            DrawMinimizeButton(minimizeButton, minimizeColor);
            if ((int)_currScene > 2) DrawSwitchButton(switchButton, switchColor);
            return DrawCloseButton(closeButton, closeColor);
        }

        private static void DrawMinimizeButton(Rectangle button, Color buttonColor)
        {
            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0)) MinimizeWindow();
                buttonColor = Color.GREEN;
            }
            DrawRectangleRec(button, buttonColor);
            DrawTexture(MinimizeIcon, (int)button.x, (int)button.y, Color.BLACK);
        }

        private static void DrawSwitchButton(Rectangle button, Color buttonColor)
        {
            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0)) OnClick();
                buttonColor = Color.YELLOW;
            }
            DrawRectangleRec(button, buttonColor);

            var switchSize = MeasureTextEx(Program.MainFont, _switchText, 48, 0);
            var switchPos = new Vector2(button.x + (button.width - switchSize.X) * 0.5f,
                button.y + (button.height - switchSize.Y) * 0.5f);
            DrawTextEx(Program.MainFont, _switchText, switchPos, 48, 0, Color.BLACK);
        }

        private static bool DrawCloseButton(Rectangle button, Color buttonColor)
        {
            var shutdownResponse = false;
            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0)) shutdownResponse = true;
                buttonColor = Color.RED;
            }
            DrawRectangleRec(button, buttonColor);
            DrawTexture(CloseIcon, (int)button.x, (int)button.y, Color.BLACK);
            return shutdownResponse;
        }

        // Controls

        private static void PrepareGame()
        {
            Login.Connect();
            Chat.Connect();
            _currScene = Scene.Main;
        }

        private static void PostLogin()
        {
            if (Directory.Exists("Logs") && Directory.GetFiles("Logs").Any())
                _currScene = Scene.Load;
            else
            {
                Wheel.Options = SaveLoad.DefaultOptions;
                PrepareGame();
            }
        }

        private static void OnClick()
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
            }
        }
    }
}