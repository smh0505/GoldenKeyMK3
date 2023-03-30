using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Intro : IDisposable
    {
        private int _state;
        private int _frames;
        private int _letters;
        private int _logoLen;
        private float _alpha;
        private float _deltaTime;

        private readonly Color _logoColor;
        private readonly Texture2D _face;

        public Intro()
        {
            _state = _frames = _letters = 0;
            _logoLen = 16;
            _alpha = 1.0f;
            
            _logoColor = new Color(139, 71, 135, 255);
            _deltaTime = 0.0f;

            _face = LoadTexture("Resource/BloppyHB.png");
        }
        
        // Public Methods

        public bool Draw()
        {
            // DeltaTime Conversion: Monitor Refresh Rate to 60 fps
            _deltaTime += GetFrameTime();
            if (_deltaTime >= 1.0f / 60)
            {
                _deltaTime = 0;
                Calculate();
            }

            var text = "Developed by BloppyHB";
            var logoPos = new Vector2(832, 412);
            var facePos = new Vector2(960 - _face.width * 0.1f, 500 - _face.height * 0.1f);
            var textPos = new Vector2(960 - MeasureText(text, 36) * 0.5f, 508 + _face.height * 0.1f);

            switch (_state)
            {
                case 0: // blinking
                    if (_frames / 15 % 2 == 0)
                        DrawRectangle((int)logoPos.X, (int)logoPos.Y, 16, 16, _logoColor);
                    break;
                case 1: // top and left
                    DrawRectangle((int)logoPos.X, (int)logoPos.Y, _logoLen, 16, _logoColor);
                    DrawRectangle((int)logoPos.X, (int)logoPos.Y, 16, _logoLen, _logoColor);
                    break;
                case 2: // right and bottom
                    DrawRectangle((int)logoPos.X, (int)logoPos.Y, 256, 16, _logoColor);
                    DrawRectangle((int)logoPos.X, (int)logoPos.Y, 16, 256, _logoColor);
                    DrawRectangle((int)logoPos.X + 240, (int)logoPos.Y, 16, _logoLen, _logoColor);
                    DrawRectangle((int)logoPos.X, (int)logoPos.Y + 240, _logoLen, 16, _logoColor);
                    break;
                case 3: // typing
                    DrawRectangle((int)logoPos.X, (int)logoPos.Y, 256, 256, Fade(_logoColor, _alpha));
                    DrawRectangle((int)logoPos.X + 16, (int)logoPos.Y + 16, 224, 224, Color.LIGHTGRAY);

                    DrawText("made with".SubText(0, _letters), (int)logoPos.X, (int)logoPos.Y - 36, 36, Fade(_logoColor, _alpha));
                    DrawText("raylib".SubText(0, _letters), (int)logoPos.X + 84, (int)logoPos.Y + 156, 50, Fade(_logoColor, _alpha));
                    DrawText("cs".SubText(0, _letters), (int)logoPos.X + 84, (int)logoPos.Y + 186, 50, Fade(_logoColor, _alpha));
                    break;
                case 4: // developer
                    DrawTextureEx(_face, facePos, 0, 0.2f, Fade(Color.WHITE, _alpha));
                    DrawText(text, (int)textPos.X, (int)textPos.Y, 36, Fade(Color.BLACK, _alpha));
                    break;
                default: return true;
            }

            return false;
        }

        public void Dispose()
        {
            UnloadTexture(_face);
            GC.SuppressFinalize(this);
        }
        
        // Private Methods

        private void Calculate()
        {
            switch (_state)
            {
                case 0: // blinking
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
                case 4: // developer
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
    }
}