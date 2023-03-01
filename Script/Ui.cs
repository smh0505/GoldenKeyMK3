using System.Numerics;
using Raylib_cs;
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

        public static bool DrawButton(Vector2 center, float radius, Color buttonColor, float hoverFade)
        {
            var isClicked = false;
            var hoverColor = Fade(buttonColor, hoverFade);
            if (CheckCollisionPointCircle(GetMousePosition(), center, radius))
            {
                if (IsMouseButtonPressed(0)) isClicked = true;
                hoverColor = buttonColor;
            }

            DrawCircleV(center, radius, hoverColor);
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

        public static void DrawCenteredTextMultiLine(Rectangle box, Font font, string[] texts,
            float fontSize, Color fontColor)
        {
            var textSizes = texts.Select(text => MeasureTextEx(font, text, fontSize, 0)).ToArray();
            var initHeight = (box.height - fontSize * textSizes.Length) * 0.5f;
            for (var i = 0; i < textSizes.Length; i++)
            {
                var pos = new Vector2(box.x + (box.width - textSizes[i].X) * 0.5f, box.y + initHeight + fontSize * i);
                DrawTextEx(font, texts[i], pos, fontSize, 0, fontColor);
            }
        }

        public static IReadOnlyCollection<T> FastMarquee<T>(float height, float unitHeight, IReadOnlyCollection<T> group,
            int speed, ref int y, ref int head)
        {
            var count = (int)Math.Ceiling(height / unitHeight);
            if (group.Count >= count)
            {
                y -= speed;
                if (y <= -unitHeight)
                {
                    head = (head + 1) % group.Count;
                    y = 0;
                }
            }
            else y = head = 0;

            var output = group.Skip(head).Take(count + 1).ToList();
            if (group.Count >= count && output.Count < count + 1)
                output.AddRange(group.Take(count + 1 - output.Count));
            return output.ToArray();
        }

        public static void HorizontalMarquee(float width, Font font, string text, float fontSize, int speed, ref int x)
        {
            var textSize = MeasureTextEx(font, text, fontSize, 0);
            
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