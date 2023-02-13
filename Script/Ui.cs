using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public static class Ui
    {
        public static bool DrawButton(Rectangle button, Color buttonColor, float hoverFade, Texture2D buttonImage)
        {
            var isClicked = false;
            var hoverColor = Fade(buttonColor, hoverFade);
            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0)) isClicked = true;
                hoverColor = buttonColor;
            }
            
            DrawRectangleRec(button, hoverColor);
            DrawTexture(buttonImage, (int)button.x, (int)button.y, Color.WHITE);
            return isClicked;
        }

        public static bool DrawButton(Rectangle button, Color buttonColor, float hoverFade, 
            Font font, string buttonText, float fontSize, Color fontColor)
        {
            var isClicked = false;
            var hoverColor = Fade(buttonColor, hoverFade);
            var textSize = MeasureTextEx(font, buttonText, fontSize, 0);
            var textPos = new Vector2(button.x + (button.width - textSize.X) * 0.5f,
                button.y + (button.height - textSize.Y) * 0.5f);
            
            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0)) isClicked = true;
                hoverColor = buttonColor;
            }
            
            DrawRectangleRec(button, hoverColor);
            DrawTextEx(font, buttonText, textPos, fontSize, 0, fontColor);
            return isClicked;
        }

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
    }
}