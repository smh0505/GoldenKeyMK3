using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public static class Intro
    {
        // Animation Variables
        private static int _state;
        private static int _frames;
        private static int _letters;
        private static int _logoLen = 16;
        private static float _alpha = 1.0f;

        // Raylib-cs logo color
        private static readonly Color LogoColor = new Color(139, 71, 135, 255);

        // Developer logo
        private static readonly Texture2D Logo = LoadTexture("Resource/BloppyHB.png");
        private static readonly Vector2 LogoPos =
            new Vector2((GetScreenWidth() - (float)Logo.width / 5) / 2,
                (GetScreenHeight() - (float)Logo.height / 5) / 2 - 40);

        // Developer logo text
        private const string Text = "Developed by BloppyHB";
        private static readonly float TextLen = MeasureText(Text, 36);
        private static readonly Vector2 TextPos =
            new ((GetScreenWidth() - TextLen) / 2, (GetScreenHeight() + Logo.height / 5) / 2 - 32);

        private static void Calculate()
        {
            switch (_state)
            {
                case 0: // Blinking
                    _frames++;
                    if (_frames == 120)
                    {
                        _frames = 0;
                        _state++;
                    }
                    break;
                case 1: // top and left
                    _logoLen += 4;
                    if (_logoLen == 256)
                    {
                        _logoLen = 16;
                        _state++;
                    }
                    break;
                case 2: // right and bottom
                    _logoLen += 4;
                    if (_logoLen == 256) _state++;
                    break;
                case 3: // typing
                    _frames++;
                    if (_frames % 12 == 0) _letters++;
                    if (_letters >= 10) _alpha -= 0.02f;
                    if (_alpha <= 0.0f)
                    {
                        _alpha = 0.0f;
                        _frames = 0;
                        _state++;
                    }
                    break;
                case 4: // Developer logo
                    if (_frames == 0) _alpha += 0.02f;
                    if (_alpha >= 1.0f)
                    {
                        _alpha = 1.0f;
                        _frames++;
                    }
                    if (_frames == 120) _alpha -= 0.02f;
                    if (_alpha <= 0.0f)
                    {
                        _alpha = 0.0f;
                        _frames = 0;
                        _state++;
                    }
                    break;
            }
        }

        public static bool Draw()
        {
            Calculate();
            var raylibPos = new Vector2(GetScreenWidth() * 0.5f - 128, GetScreenHeight() * 0.5f - 128);

            switch (_state)
            {
                case 0: // Blinking
                    if ((_frames / 15) % 2 == 0)
                        DrawRectangle((int)raylibPos.X, (int)raylibPos.Y, 16, 16, LogoColor);
                    break;
                case 1: // top and left
                    DrawRectangle((int)raylibPos.X, (int)raylibPos.Y, _logoLen, 16, LogoColor);
                    DrawRectangle((int)raylibPos.X, (int)raylibPos.Y, 16, _logoLen, LogoColor);
                    break;
                case 2: // right and bottom
                    DrawRectangle((int)raylibPos.X, (int)raylibPos.Y, 256, 16, LogoColor);
                    DrawRectangle((int)raylibPos.X, (int)raylibPos.Y, 16, 256, LogoColor);
                    DrawRectangle((int)raylibPos.X + 240, (int)raylibPos.Y, 16, _logoLen, LogoColor);
                    DrawRectangle((int)raylibPos.X, (int)raylibPos.Y + 240, _logoLen, 16, LogoColor);
                    break;
                case 3: // Typing
                    DrawRectangle((int)raylibPos.X, (int)raylibPos.Y, 256, 256, Fade(LogoColor, _alpha));
                    DrawRectangle((int)raylibPos.X + 16, (int)raylibPos.Y + 16, 224, 224, Color.LIGHTGRAY);

                    DrawText("made with".SubText(0, _letters), (int)raylibPos.X, (int)raylibPos.Y - 36, 36, Fade(LogoColor, _alpha));
                    DrawText("raylib".SubText(0, _letters), (int)raylibPos.X + 84, (int)raylibPos.Y + 156, 50, Fade(LogoColor, _alpha));
                    DrawText("cs".SubText(0, _letters), (int)raylibPos.X + 84, (int)raylibPos.Y + 186, 50, Fade(LogoColor, _alpha));
                    break;
                case 4: // Developer logo
                    DrawTextureEx(Logo, LogoPos, 0, 0.2f, Fade(Color.WHITE, _alpha));
                    DrawText(Text, (int)TextPos.X, (int)TextPos.Y, 36, Fade(Color.BLACK, _alpha));
                    break;
                default: return true;
            }
            return false;
        }
    }
}