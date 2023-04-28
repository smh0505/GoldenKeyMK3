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
            var initHeight = (box.height - fontSize * texts.Length) * 0.5f;
            for (var i = 0; i < texts.Length; i++)
            {
                var size = MeasureTextEx(font, texts[i], fontSize, 0).X;
                var pos = new Vector2(box.x + (box.width - size) * 0.5f, box.y + (initHeight + fontSize * i));
                DrawTextEx(font, texts[i], pos, fontSize, 0, fontColor);
            }
        }

        public static void ScrollText(Font font, string text, float fontSize, float width, ref Vector2 pos,
            in Vector2 refPoint, Color color)
        {
            var size = MeasureTextEx(font, text, fontSize, 0).X;
            pos = size >= width ? pos with { X = pos.X - 60.0f / GetFPS() } : refPoint;
            if (pos.X < refPoint.X - size) pos.X += size + 36.0f;
            DrawTextEx(font, text, pos, fontSize, 0, color);
            if (size >= width) DrawTextEx(font, text, pos with { X = pos.X + size + 36.0f }, fontSize, 0, color);
        }

        public static void ScrollList(Rectangle box, Font font, string[] texts, float fontSize,
            float unitHeight, ref int idx, ref Vector2 startPt, in Vector2 refPt, Color color)
        {
            var count = (int)Math.Ceiling(box.height / unitHeight);
            startPt = texts.Length >= count ? startPt with { Y = startPt.Y - 60.0f / GetFPS() } : refPt;
            if (texts.Length >= count)
            {
                if (startPt.Y <= refPt.Y - unitHeight)
                {
                    idx = (idx + 1) % texts.Length;
                    startPt.Y += unitHeight;
                }
            }
            else idx = 0;
            
            var items = texts.Skip(idx).Take(count + 1).ToList();
            if (texts.Length >= count && items.Count < count + 1)
                items.AddRange(texts.Take(count + 1 - items.Count));
            
            BeginScissorMode((int)box.x, (int)box.y, (int)box.width, (int)box.height);
            for (var i = 0; i < items.Count; i++)
            {
                var pos = startPt with { Y = startPt.Y + unitHeight * i };
                DrawTextEx(font, items[i], pos, fontSize, 0, color);
            }
            EndScissorMode();
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