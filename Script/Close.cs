using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Close
    {
        private static int _count;
        private readonly Texture2D _cancelIcon;
        private readonly Texture2D _closeScreen;

        public Close()
        {
            _cancelIcon = LoadTexture("Resource/return.png");
            _closeScreen = LoadTexture("Resource/close.png");
        }
        
        public void Reset() => _count = 0;

        public void Draw(out bool shutdownRequest, out bool shutdownResponse)
        {
            // UIs
            DrawTexture(_closeScreen, 0, 0, Color.WHITE);
            DrawTextEx(Ui.Galmuri36, $"종료까지 앞으로 {5 - _count}회", new Vector2(12, 140), 36, 0, Color.WHITE);

            shutdownRequest = !Ui.DrawButton(new Rectangle(12, 912, 100, 100), Color.GREEN, 0.7f);
            DrawTexture(_cancelIcon, 12, 912, Color.WHITE);

            // Exit Sequence
            if (IsKeyPressed(KeyboardKey.KEY_SPACE)) _count++;
            shutdownResponse = _count >= 5;
        }

        public void Dispose()
        {
            UnloadTexture(_cancelIcon);
            UnloadTexture(_closeScreen);
        }
    }
}