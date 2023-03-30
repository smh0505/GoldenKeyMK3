using System.Numerics;
using System.Text.RegularExpressions;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Login : IDisposable
    {
        public string Payload;
        private string _input;
        private bool _isShowed;
        private bool _processing;
        private bool _failed;
        private float _frames;

        private bool _copyHover;
        private readonly Rectangle _copyButton;

        private readonly Texture2D _scene;

        public Login()
        {
            _scene = LoadTexture("Resource/login.png");
            Payload = _input = string.Empty;
            _isShowed = _processing = _failed = _copyHover = false;
            _frames = 0;
            _copyButton = new Rectangle(12, 12, 160, 80);
        }

        // Public Methods
        
        public void Draw()
        {
            var copyColor = _copyHover ? Color.GREEN : Fade(Color.GREEN, 0.7f);
            DrawRectangleRec(_copyButton, copyColor);
            Ui.DrawTextCentered(_copyButton, Ui.Galmuri48, "붙여넣기", 48, Color.BLACK);
            
            DrawTexture(_scene, 0, 0, Color.WHITE);
            if (_failed) DrawAlert();
            DrawTextBox();
        }

        public void Dispose()
        {
            UnloadTexture(_scene);
            GC.SuppressFinalize(this);
        }

        public async void Control(bool shutdownRequest)
        {
            if (Ui.IsHovering(_copyButton, !shutdownRequest))
            {
                _copyHover = true;
                if (IsMouseButtonPressed(0)) _input = GetClipboardText_();
            }
            else _copyHover = false;

            if (IsKeyDown(KeyboardKey.KEY_BACKSPACE) && _input.Length > 0)
            {
                _frames += GetFrameTime();
                if (_frames >= 1.0f / 20)
                {
                    _input = _input.Remove(_input.Length - 1);
                    _frames = 0;
                }
            }
            
            switch ((KeyboardKey)GetKeyPressed())
            {
                case KeyboardKey.KEY_ENTER:
                    if (!_processing) Payload = await LoadPayload();
                    if (string.IsNullOrEmpty(Payload)) _failed = true;
                    _processing = false;
                    break;
                case KeyboardKey.KEY_TAB:
                    _isShowed = !_isShowed;
                    break;
                default:
                    var x = GetCharPressed();
                    if (x is > 32 and < 127) _input += ((char)x).ToString();
                    break;
            }
        }

        public void ReadKey(string key) => _input = key;

        // UI

        private static void DrawAlert()
        {
            const string alert = "연결에 실패했습니다. 다시 시도해주세요.";
            var pos = new Vector2(960 - MeasureTextEx(Ui.Galmuri48, alert, 48, 0).X * 0.5f, 572);
            DrawTextEx(Ui.Galmuri48, alert, pos, 48, 0, Color.RED);
        }

        private void DrawTextBox()
        {
            var inputText = (_isShowed ? _input : "".PadLeft(_input.Length, '*')) + "_";
            var inputRect = new Rectangle(488, 516, 944, 48);
            var inputLen = MeasureTextEx(Ui.Galmuri48, inputText, 48, 0).X;
            var inputPos = inputLen >= inputRect.width ? inputRect.x + inputRect.width - inputLen : inputRect.x;
            
            BeginScissorMode((int)inputRect.x, (int)inputRect.y, (int)inputRect.width, (int)inputRect.height);
            DrawTextEx(Ui.Galmuri48, inputText, new Vector2(inputPos, inputRect.y), 48, 0, Color.BLACK);
            EndScissorMode();
        }

        // Private Methods
        
        private async Task<string> LoadPayload()
        {
            _processing = true;
            var client = new HttpClient();
            var response = await client.GetAsync("https://toon.at/widget/alertbox/" + _input);
            
            if (!response.IsSuccessStatusCode) return string.Empty;
            
            var body = await response.Content.ReadAsStringAsync();
            var line = Regex.Match(body, "\"payload\":\"[^\"]*\"").Value;
            return Regex.Match(line, @"[\w]{8,}").Value;
        }
    }
}