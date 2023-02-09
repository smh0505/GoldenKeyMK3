using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Close
    {
        private static int _count;
        private static readonly Texture2D CancelIcon = LoadTexture("Resource/return.png");
        private static readonly Texture2D CloseScreen = LoadTexture("Resource/close.png");
        
        public static void InitExit() => _count = 0;

        public static void DrawExit(out bool shutdownRequest, out bool shutdownResponse)
        {
            // UIs
            DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), Fade(Color.BLACK, 0.7f));
            DrawTexts();
            shutdownRequest = !DrawButton();

            // Exit Sequence
            if (IsKeyPressed(KeyboardKey.KEY_SPACE)) _count++;
            shutdownResponse = _count >= 5;
        }

        public static void Dispose()
        {
            UnloadTexture(CancelIcon);
            UnloadTexture(CloseScreen);
        }

        // UIs

        private static void DrawTexts()
        {
            DrawTexture(CloseScreen, 0, 0, Color.WHITE);
            DrawTextEx(Program.MainFont, $"종료까지 앞으로 {5 - _count}회", new Vector2(12, 140), 36, 0, Color.WHITE);
        }

        private static bool DrawButton()
        {
            var isClicked = false;
            var button = new Rectangle(12, GetScreenHeight() - 168, 100, 100);
            var buttonColor = Color.DARKGREEN;
            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0)) isClicked = true;
                buttonColor = Color.GREEN;
            }
            DrawRectangleRec(button, buttonColor);
            DrawTexture(CancelIcon, (int)button.x - 4, (int)button.y - 4, Color.YELLOW);
            return isClicked;
        }
    }
}