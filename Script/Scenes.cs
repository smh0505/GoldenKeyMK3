using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    /*
     * Scripts for scenes
     * Intro.cs => Scene.Intro
     * Login.cs => Scene.Login
     * LoadScene.cs => Scene.Load
     * Wheel.cs => Scene.Main
     * Chat.cs => Scene.Board
     */

    public enum Scene
    {
        Intro = 0,
        Login,
        Load,
        Main,
        Board
    }

    public class Scenes
    {
        private static Scene _currScene;
        private static readonly Texture2D MinimizeIcon = LoadTexture("Resource/minus.png");
        private static readonly Texture2D CloseIcon = LoadTexture("Resource/power.png");

        public static void DrawScene(bool shutdownRequest)
        {
            switch (_currScene)
            {
                case Scene.Intro:
                    if (Intro.DrawIntro()) _currScene = Scene.Login;
                    break;
                case Scene.Login:
                    if (Login.DrawLogin(shutdownRequest))
                        if (Directory.Exists("Logs") && Directory.GetFiles("Logs").Any())
                            _currScene = Scene.Load;
                        else
                        {
                            Wheel.Options = SaveLoad.DefaultOptions;
                            Login.Connect();
                            Chat.Connect();
                            _currScene = Scene.Main;
                        }
                    break;
                case Scene.Load:
                    if (LoadScene.DrawLoad(shutdownRequest))
                    {
                        Login.Connect();
                        Chat.Connect();
                        _currScene = Scene.Main;
                    }
                    break;
                case Scene.Main:
                    Wheel.UpdateWheel(shutdownRequest);
                    break;
                case Scene.Board:
                    Chat.DrawChat(shutdownRequest);
                    break;
            }
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

            if (CheckCollisionPointRec(GetMousePosition(), minimizeButton))
            {
                if (IsMouseButtonPressed(0)) MinimizeWindow();
                else minimizeColor = Color.GREEN;
            }
            DrawRectangleRec(minimizeButton, minimizeColor);
            DrawTexture(MinimizeIcon, (int)minimizeButton.x, (int)minimizeButton.y, Color.BLACK);

            var shutdownResponse = false;
            if (CheckCollisionPointRec(GetMousePosition(), closeButton))
            {
                if (IsMouseButtonPressed(0)) shutdownResponse = true;
                else closeColor = Color.RED;
            }
            DrawRectangleRec(closeButton, closeColor);
            DrawTexture(CloseIcon, (int)closeButton.x, (int)closeButton.y, Color.BLACK);

            if ((int)_currScene > 2)
            {
                if (CheckCollisionPointRec(GetMousePosition(), switchButton))
                {
                    if (IsMouseButtonPressed(0))
                        switch (_currScene)
                        {
                            case Scene.Main:
                                _currScene = Scene.Board;
                                break;
                            case Scene.Board:
                                _currScene = Scene.Main;
                                break;
                        }
                    else switchColor = Color.YELLOW;
                }
                DrawRectangleRec(switchButton, switchColor);
            }

            return shutdownResponse;
        }

        public static void Dispose()
        {
            UnloadTexture(MinimizeIcon);
            UnloadTexture(CloseIcon);
        }
    }
}