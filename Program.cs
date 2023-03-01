using GoldenKeyMK3.Script;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3
{
    public static class Program
    {
        private static bool _shutdownRequest;
        private static bool _shutdown;
        private static readonly Texture2D MousePointer = LoadTexture("Resource/cursor.png");

        public static void Main()
        {
            SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            InitWindow(1920, 1081, "황금열쇠 MK3");
            SetTargetFPS(60);
            HideCursor();

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
                DrawTextureEx(MousePointer, GetMousePosition(), 0, 0.03f, Color.WHITE);
                EndDrawing();
            }

            UnloadTexture(MousePointer);
            scenes.Dispose();
            CloseWindow();
        }
    }
}