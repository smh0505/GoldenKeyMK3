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
    }
}