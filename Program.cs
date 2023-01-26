using GoldenKeyMK3.Script;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3
{
    public class Program
    {
        public static Font MainFont = LoadFont("Resource/Galmuri.fnt");
        private static bool _shutdownRequest;
        private static bool _shutdown;

        public static void Main()
        {
            SetConfigFlags(ConfigFlags.FLAG_WINDOW_MAXIMIZED | ConfigFlags.FLAG_WINDOW_UNDECORATED);
            InitWindow(1920, 1080, "황금열쇠 MK3");
            SetTargetFPS(60);

            SaveLoad.LoadSetting();

            while (!_shutdown)
            {
                if (WindowShouldClose())
                {
                    _shutdownRequest = !_shutdownRequest;
                    if (_shutdownRequest) Close.InitExit();
                }

                BeginDrawing();
                ClearBackground(Color.LIGHTGRAY);
                Scenes.DrawScene(_shutdownRequest);
                if (_shutdownRequest) Close.DrawExit(out _shutdownRequest, out _shutdown);
                else _shutdownRequest = Scenes.Buttons();
                EndDrawing();
            }

            Login.ExitEvent.Set();
            SaveLoad.SaveLog();
            UnloadFont(MainFont);
            UnloadTexture(Close.CancelIcon);
            UnloadTexture(Login.Background);
            CloseWindow();
        }
    }
}