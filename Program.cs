using GoldenKeyMK3.Script;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3
{
    public static class Program
    {
        private static bool _shutdownRequest;
        private static bool _shutdown;

        public static void Main()
        {
            //TODO Borderless Fullscreen
            SetConfigFlags(/*ConfigFlags.FLAG_WINDOW_MAXIMIZED | */ConfigFlags.FLAG_WINDOW_UNDECORATED);
            InitWindow(1920, 1080, "황금열쇠 MK3");
            SetTargetFPS(60);

            var scenes = new Scenes();

            while (!_shutdown)
            {
                if (WindowShouldClose())
                {
                    _shutdownRequest = !_shutdownRequest;
                    if (_shutdownRequest) scenes.CloseScene.Reset();
                }

                BeginDrawing();
                ClearBackground(Color.LIGHTGRAY);
                scenes.Draw(_shutdownRequest);
                if (_shutdownRequest) scenes.CloseScene.Draw(out _shutdownRequest, out _shutdown);
                else _shutdownRequest = scenes.Buttons();
                EndDrawing();
            }

            scenes.Dispose();
            CloseWindow();
        }
    }
}