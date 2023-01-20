using System.Numerics;
using System.Text.RegularExpressions;
using Websocket.Client;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Login
    {
        public static ManualResetEvent ExitEvent = new ManualResetEvent(false);
        public static WebsocketClient Client;

        private static readonly List<string> Texts = new List<string>
        {
            "투네이션 통합 위젯 URL",
            "https://toon.at/widget/alertbox/",
            "연결에 실패했습니다. 다시 시도해주세요.",
            "리듬마블 황금열쇠 Mk.3 / Golden Key Mk.3 for Kim Pyun Jip's Rhythm Marble",
            "Developed by BloppyHB (https://github.com/smh0505/GoldenKeyMK3)",
            "Logo Image by 채팅_안치는사람 & cannabee",
            "Copyright © 2019-2022 Minseo Lee (itoupluk427@gmail.com)"
        };
        public static string Input = string.Empty;
        public static Texture2D Background = LoadTexture("Resource/Logo_RhythmMarble.png");
        public static string Payload = string.Empty;
        private static bool _isShowed = false;
        private static bool _isProcessing = false;
        private static bool _failed = false;

        public static bool DrawLogin(bool shutdownRequest)
        {
            // Background Image
            Vector2 picPos = new Vector2(GetScreenWidth() - Background.width - 20,
                GetScreenHeight() - Background.height - 20);
            DrawTextureEx(Background, picPos, 0, 1, Fade(Color.WHITE, 0.7f));

            // Texts
            Vector2 text1Pos = new Vector2((GetScreenWidth() - MeasureTextEx(Program.MainFont, Texts[0], 48, 0).X) / 2,
                GetScreenHeight() * 0.5f - 128);
            Vector2 text2Pos = new Vector2((GetScreenWidth() - MeasureTextEx(Program.MainFont, Texts[1], 48, 0).X) / 2,
                GetScreenHeight() * 0.5f - 80);

            // Input Box
            DrawTextEx(Program.MainFont, Texts[0], text1Pos, 48, 0, Color.BLACK);
            DrawTextEx(Program.MainFont, Texts[1], text2Pos, 48, 0, Color.BLACK);

            Rectangle textBox = new Rectangle(GetScreenWidth() * 0.25f, GetScreenHeight() * 0.5f - 28,
                GetScreenWidth() * 0.5f, 56);
            DrawRectangleRec(textBox, Color.WHITE);
            DrawRectangleLinesEx(textBox, 4, Color.BLACK);

            string inputText = (_isShowed ? Input : "".PadLeft(Input.Length, '*')) + "_";
            Rectangle inputRect = new Rectangle(textBox.x + 8, textBox.y + 4, textBox.width - 16, 48);
            float inputPos = MeasureTextEx(Program.MainFont, inputText, 48, 0).X >= textBox.width
                ? inputRect.x + inputRect.width - MeasureTextEx(Program.MainFont, inputText, 48, 0).X
                : inputRect.x;
            BeginScissorMode((int)inputRect.x, (int)inputRect.y, (int)inputRect.width, (int)inputRect.height);
            DrawTextEx(Program.MainFont, inputText, new Vector2(inputPos, inputRect.y), 48, 0, Color.BLACK);
            EndScissorMode();

            // More Texts
            Vector2 head = new Vector2(12, GetScreenHeight() - 132);
            DrawTextEx(Program.MainFont, Texts[3], head, 24, 0, Color.GRAY);
            DrawTextEx(Program.MainFont, Texts[4], head + new Vector2(0, 24), 24, 0, Color.GRAY);
            DrawTextEx(Program.MainFont, Texts[5], head + new Vector2(0, 48), 24, 0, Color.GRAY);
            DrawTextEx(Program.MainFont, Texts[6], head + new Vector2(0, 96), 24, 0, Color.GRAY);

            Vector2 alertPos = new Vector2((GetScreenWidth() - MeasureTextEx(Program.MainFont, Texts[2], 48, 0).X) * 0.5f,
                GetScreenHeight() * 0.5f + 32);
            if (_failed) DrawTextEx(Program.MainFont, Texts[2], alertPos, 48, 0, Color.RED);

            DrawButton(shutdownRequest);

            return GetInput(shutdownRequest).Result;
        }

        private static async Task<bool> GetInput(bool shutdownRequest)
        {
            if (!shutdownRequest) switch((KeyboardKey)GetKeyPressed())
            {
                case KeyboardKey.KEY_ENTER:
                    if (!_isProcessing) await LoadPayload();
                    if (!string.IsNullOrEmpty(Payload)) return true;
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

        private static void DrawButton(bool shutdownRequest)
        {
            Rectangle copyButton = new Rectangle(12, 12, 160, 80);
            Color copyColor = Fade(Color.GREEN, 0.7f);
            if (CheckCollisionPointRec(GetMousePosition(), copyButton) && !shutdownRequest)
            {
                if (IsMouseButtonPressed(0)) Input = GetClipboardText_();
                else copyColor = Color.GREEN;
            }
            DrawRectangleRec(copyButton, copyColor);

            Vector2 copyPos = new Vector2(92 - MeasureTextEx(Program.MainFont, "붙여넣기", 48, 0).X / 2, 28);
            DrawTextEx(Program.MainFont, "붙여넣기", copyPos, 48, 0, Color.BLACK);
        }

        private static async Task LoadPayload()
        {
            _isProcessing = true;
            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://toon.at/widget/alertbox/" + Input);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var line = Regex.Match(body, "\"payload\":\"[^\"]*\"").Value;
                Payload = Regex.Match(line, @"[\w]{8,}").Value;
            }
        }

        public static async void Connect()
        {
            using (Client = new WebsocketClient(new Uri("wss://toon.at:8071/" + Payload)))
            {
                Client.MessageReceived.Subscribe(msg =>
                {
                    if (msg.ToString().Contains("roulette"))
                    {
                        var roulette = Regex.Match(msg.ToString(), "\"message\":\"[^\"]* - [^\"]*\"").Value.Substring(10);
                        var rValue = roulette.Split('-')[1].Replace("\"", "").Substring(1);
                        if (rValue != "꽝") Wheel.Waitlist.Add(rValue);
                    }
                });
                await Client.Start();
                ExitEvent.WaitOne();
            }
        }
    }
}