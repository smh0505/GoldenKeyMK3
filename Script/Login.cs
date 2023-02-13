using System.Numerics;
using System.Text.RegularExpressions;
using Websocket.Client;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Login
    {
        private readonly ManualResetEvent _exitEvent = new (false);
        private WebsocketClient _client;

        public string Input;
        private string _payload;
        private static bool _isShowed;
        private static bool _isProcessing;
        private static bool _failed;
        
        private readonly Texture2D _background;
        private readonly Texture2D _login;

        private const string AlertText = "연결에 실패했습니다. 다시 시도해주세요.";

        public Login()
        {
            _background = LoadTexture("Resource/Logo_RhythmMarble.png");
            _login = LoadTexture("Resource/login.png");
            Input = string.Empty;
            _payload = string.Empty;
        }

        public bool Draw(bool shutdownRequest)
        {
            DrawTexture(_login, 0, 0, Color.WHITE);
            if (_failed) DrawAlert();
            DrawTextBox();
            if (shutdownRequest) return false;
            if (DrawButton(new Rectangle(12, 12, 160, 80), Color.GREEN))
                Input = GetClipboardText_();
            return GetInput().Result;
        }

        public async void Connect(Wheel wheel)
        {
            using (_client = new WebsocketClient(new Uri("wss://toon.at:8071/" + _payload)))
            {
                _client.MessageReceived.Subscribe(msg =>
                {
                    if (!msg.ToString().Contains("roulette")) return;
                    var re = new Regex(@".message.:.+? - (?<rValue>.+?).");
                    wheel.WaitList = wheel.WaitList.Add(re.Match(msg.ToString()).Groups["rValue"].ToString());

                    //var roulette = Regex.Match(msg.ToString(), "\"message\":\"[^\"]* - [^\"]*\"").Value.Substring(10);
                    //var rValue = roulette.Split('-')[1].Replace("\"", "").Substring(1);
                    //if (rValue != "꽝") Wheel.Waitlist.Add(rValue);
                });
                await _client.Start();
                _exitEvent.WaitOne();
            }
        }

        public void Dispose()
        {
            _exitEvent.Set();
            UnloadTexture(_background);
            UnloadTexture(_login);
        }

        // UIs

        private static void DrawAlert()
        {
            var alertPos = new Vector2((GetScreenWidth() - MeasureTextEx(Program.MainFont, AlertText, 48, 0).X) * 0.5f,
                GetScreenHeight() * 0.5f + 32);
            DrawTextEx(Program.MainFont, AlertText, alertPos, 48, 0, Color.RED);
        }

        private void DrawTextBox()
        {
            var inputText = (_isShowed ? Input : "".PadLeft(Input.Length, '*')) + "_";
            var inputRect = new Rectangle(GetScreenWidth() * 0.25f + 8, GetScreenHeight() * 0.5f -24, 
                GetScreenWidth() * 0.5f - 16, 48);
            var inputLen = MeasureTextEx(Program.MainFont, inputText, 48, 0).X;
            var inputPos = inputLen >= inputRect.width ? inputRect.x + inputRect.width - inputLen : inputRect.x;

            BeginScissorMode((int)inputRect.x, (int)inputRect.y, (int)inputRect.width, (int)inputRect.height);
            DrawTextEx(Program.MainFont, inputText, new Vector2(inputPos, inputRect.y), 48, 0, Color.BLACK);
            EndScissorMode();
        }

        private static bool DrawButton(Rectangle button, Color buttonColor)
            => Ui.DrawButton(button, buttonColor, 0.7f, Program.MainFont, "붙여넣기", 48, Color.BLACK);
        
        // Controls

        private async Task<bool> GetInput()
        {
            switch((KeyboardKey)GetKeyPressed())
            {
                case KeyboardKey.KEY_ENTER:
                    if (!_isProcessing) await LoadPayload();
                    if (!string.IsNullOrEmpty(_payload)) return true;
                    else
                    {
                        _isProcessing = false;
                        _failed = true;
                    }
                    break;
                case KeyboardKey.KEY_TAB:
                    _isShowed = !_isShowed;
                    break;
                case KeyboardKey.KEY_BACKSPACE:
                    if (Input.Length > 0) Input = Input.Remove(Input.Length - 1);
                    break;
                default:
                    var x = Raylib.GetCharPressed();
                    if (x is > 32 and < 127) Input += ((char)x).ToString();
                    break;
            }
            return false;
        }

        private async Task LoadPayload()
        {
            _isProcessing = true;
            var client = new HttpClient();
            var response = await client.GetAsync("https://toon.at/widget/alertbox/" + Input);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var line = Regex.Match(body, "\"payload\":\"[^\"]*\"").Value;
                _payload = Regex.Match(line, @"[\w]{8,}").Value;
            }
        }
    }
}