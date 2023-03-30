using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public static class Ui
    {
        public static readonly Font Galmuri24 = LoadFont("Resource/Fonts/Galmuri24.fnt");
        public static readonly Font Galmuri36 = LoadFont("Resource/Fonts/Galmuri36.fnt");
        public static readonly Font Galmuri48 = LoadFont("Resource/Fonts/Galmuri48.fnt");
        public static readonly Font Galmuri60 = LoadFont("Resource/Fonts/Galmuri60.fnt");
        public static readonly Font Cafe36 = LoadFont("Resource/Fonts/Cafe36.fnt");

        public static Vector2 CenterPos(Rectangle rect, Texture2D texture)
            => new (rect.x + (rect.width - texture.width) * 0.5f, rect.y + (rect.height - texture.height) *0.5f);
        
        public static bool IsHovering(Rectangle button, bool isActive)
            => isActive && CheckCollisionPointRec(GetMousePosition(), button);

        public static bool IsHovering(Vector2 center, float radius, bool isActive)
            => isActive && CheckCollisionPointCircle(GetMousePosition(), center, radius);

        public static void DrawTextCentered(Rectangle box, Font font, string text, float fontSize, Color fontColor)
        {
            var textSize = MeasureTextEx(font, text, fontSize, 0);
            var textPos = new Vector2(box.x + (box.width - textSize.X) * 0.5f,
                box.y + (box.height - textSize.Y) * 0.5f);
            DrawTextEx(font, text, textPos, fontSize, 0, fontColor);
        }
        
        public static void DrawTextMultiLine(Rectangle box, Font font, string[] texts, float fontSize, Color fontColor)
        {
            var textSizes = texts.Select(text => MeasureTextEx(font, text, fontSize, 0)).ToArray();
            var initHeight = (box.height - fontSize * textSizes.Length) * 0.5f;
            for (var i = 0; i < textSizes.Length; i++)
            {
                var pos = new Vector2(box.x + (box.width - textSizes[i].X) * 0.5f, box.y + initHeight + fontSize * i);
                DrawTextEx(font, texts[i], pos, fontSize, 0, fontColor);
            }
        }

        public static void Dispose()
        {
            UnloadFont(Galmuri24);
            UnloadFont(Galmuri36);
            UnloadFont(Galmuri48);
            UnloadFont(Galmuri60);
            UnloadFont(Cafe36);
        }
    }
}