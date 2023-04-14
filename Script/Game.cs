using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Game : IDisposable
    {
        private bool _shutdownRequest;
        private bool _shutdownResponse;
        private readonly Texture2D _cursor;
        private readonly Scenes _scenes;
        
        public Game()
        {
            _shutdownRequest = _shutdownResponse = false;

            SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            InitWindow(1920, 1081, "황금열쇠 MK3 Ver.2.1");
            SetTargetFPS(GetMonitorRefreshRate(GetCurrentMonitor()));
            HideCursor();
            
            _cursor = LoadTexture("Resource/cursor.png");
            _scenes = new Scenes();
        }
        
        // Public Methods

        public void Run()
        {
            while (!_shutdownResponse)
            {
                Update();
                Draw();
            }
            Dispose();
        }
        
        public void Dispose()
        {
            UnloadTexture(_cursor);
            _scenes.Dispose();
            CloseWindow();
            GC.SuppressFinalize(this);
        }
        
        // Private Methods

        private void Update() => _scenes.Control(ref _shutdownRequest, out _shutdownResponse);

        private void Draw()
        {
            BeginDrawing();
            ClearBackground(Color.LIGHTGRAY);
            _scenes.Draw(_shutdownRequest);
            if (IsCursorOnScreen())
                DrawTextureEx(_cursor, GetMousePosition(), 0, 0.03f, Color.WHITE);
            EndDrawing();
        }
    }
}