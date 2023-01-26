using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    /*
     * Scripts for scenes
     * Intro.cs => Scene.Intro
     * Login.cs => Scene.Login
     * LoadScene.cs => Scene.Load
     * MainScene.cs => Scene.Main
     */

    public enum Scene
    {
        Intro = 0,
        Login,
        Load,
        Main
    }

    public class Scenes
    {
        private static Scene _currScene;

        public static void DrawScene(bool shutdownRequest)
        {
            switch (_currScene)
            {
                case Scene.Intro:
                    if (Intro.DrawIntro()) _currScene = Scene.Login;
                    break;
                case Scene.Login:
                    if (Login.DrawLogin(shutdownRequest))
                        _currScene = Directory.Exists("Logs") && Directory.GetFiles("Logs").Any()
                        ? Scene.Load : Scene.Main;
                    break;
                case Scene.Load:
                    if (LoadScene.DrawLoad(shutdownRequest))
                    {
                        Login.Connect();
                        _currScene = Scene.Main;
                    }
                    break;
                case Scene.Main:
                    Wheel.UpdateWheel(shutdownRequest);
                    break;
            }
        }

        public static bool Buttons()
        {
            var minimizeButton = new Rectangle(GetScreenWidth() - 124, 12, 50, 50);
            var closeButton = new Rectangle(GetScreenWidth() - 62, 12, 50, 50);
            var minimizeColor = Fade(Color.GREEN, 0.7f);
            var closeColor = Fade(Color.RED, 0.7f);

            if (CheckCollisionPointRec(GetMousePosition(), minimizeButton))
            {
                if (IsMouseButtonPressed(0)) MinimizeWindow();
                else minimizeColor = Color.GREEN;
            }
            DrawRectangleRec(minimizeButton, minimizeColor);

            var shutdownResponse = false;
            if (CheckCollisionPointRec(GetMousePosition(), closeButton))
            {
                if (IsMouseButtonPressed(0)) shutdownResponse = true;
                else closeColor = Color.RED;
            }
            DrawRectangleRec(closeButton, closeColor);
            return shutdownResponse;
        }
    }
}