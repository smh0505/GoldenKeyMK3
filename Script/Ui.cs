using System.Numerics;
using Raylib_cs;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public static class Ui
    {
        public static readonly Font Galmuri24 = LoadFont("Resource/Galmuri24.fnt");
        public static readonly Font Galmuri36 = LoadFont("Resource/Galmuri36.fnt");
        public static readonly Font Galmuri48 = LoadFont("Resource/Galmuri48.fnt");
        public static readonly Font Galmuri60 = LoadFont("Resource/Galmuri60.fnt");
        public static readonly Font Cafe36 = LoadFont("Resource/Cafe36.fnt");

        public static bool DrawButton(Vector2 center, float radius, Color buttonColor, float hoverFade,
            Font font, string buttonText, float fontSize, Color fontColor)
        {
            var isClicked = false;
            var hoverColor = Fade(buttonColor, hoverFade);
            var textSize = MeasureTextEx(font, buttonText, fontSize, 0);
            var textPos = new Vector2(center.X - textSize.X * 0.5f, center.Y - textSize.Y * 0.5f);

            if (CheckCollisionPointCircle(GetMousePosition(), center, radius))
            {
                if (IsMouseButtonPressed(0)) isClicked = true;
                hoverColor = buttonColor;
            }

            DrawCircleV(center, radius, hoverColor);
            DrawTextEx(font, buttonText, textPos, fontSize, 0, fontColor);
            return isClicked;
        }

        public static bool DrawButton(Rectangle button, Color buttonColor, float hoverFade)
        {
            var isClicked = false;
            var hoverColor = Fade(buttonColor, hoverFade);
            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0)) isClicked = true;
                hoverColor = buttonColor;
            }
            DrawRectangleRec(button, hoverColor);
            return isClicked;
        }

        public static void DrawCenteredText(Rectangle box, Font font, string text, float fontSize, Color fontColor)
        {
            var textSize = MeasureTextEx(font, text, fontSize, 0);
            var textPos = new Vector2(box.x + (box.width - textSize.X) * 0.5f,
                box.y + (box.height - textSize.Y) * 0.5f);
            DrawTextEx(font, text, textPos, fontSize, 0, fontColor);
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