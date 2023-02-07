using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Close
    {
        private static readonly Texture2D CancelIcon = LoadTexture("Resource/return.png");
        private static int _count;
        private static readonly List<string> Texts = new List<string>
        {
            "프로그램을 종료하시겠습니까?",
            "종료를 위해서는 스페이스바를 연속으로 5번 누르세요.",
            "또는 ESC를 눌러 돌아가기"
        };
        private static readonly Rectangle CancelButton = new Rectangle(12, GetScreenHeight() - 156, 100, 100);

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
        }

        // UIs

        private static void DrawTexts()
        {
            DrawTextEx(Program.MainFont, Texts[0], new Vector2(12, 12), 48, 0, Color.WHITE);
            DrawTextEx(Program.MainFont, Texts[1], new Vector2(12, 60), 36, 0, Color.WHITE);
            DrawTextEx(Program.MainFont, $"종료까지 앞으로 {5 - _count}회", new Vector2(12, 92), 36, 0, Color.WHITE);
            DrawTextEx(Program.MainFont, Texts[2], new Vector2(12, GetScreenHeight() - 44), 36, 0, Color.WHITE);
        }

        private static bool DrawButton()
        {
            bool isClicked = false;
            Color cancelColor = Color.DARKGREEN;
            if (CheckCollisionPointRec(GetMousePosition(), CancelButton))
            {
                if (IsMouseButtonPressed(0)) isClicked = true;
                else cancelColor = Color.GREEN;
            }
            DrawRectangleRec(CancelButton, cancelColor);
            DrawTexture(CancelIcon, (int)CancelButton.x - 4, (int)CancelButton.y - 4, Color.YELLOW);
            return isClicked;
        }
    }
}