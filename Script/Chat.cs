using System.Collections.Immutable;
using System.Numerics;
using System.Text.RegularExpressions;
using Websocket.Client;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public enum PollState
    {
        Idle = 0,
        Active,
        Result
    }

    public class Chat
    {
        public static WebsocketClient Client;
        public static ManualResetEvent ExitEvent = new ManualResetEvent(false);

        private static readonly List<int> Board = new List<int>{ 2, 5, 9, 11, 14, 17, 21, 23 };

        private static readonly Vector2[] Pos = new []
        {
            // Line 1
            new Vector2(868, 628), new Vector2(728, 628), new Vector2(588, 628),
            new Vector2(448, 628), new Vector2(308, 628), new Vector2(168, 628),

            new Vector2(28, 628), // DJMAX island

            // Line 2
            new Vector2(28, 528), new Vector2(28, 428), new Vector2(28, 328),
            new Vector2(28, 228), new Vector2(28, 128),

            // Line 3
            new Vector2(168, 28), new Vector2(308, 28), new Vector2(448, 28),
            new Vector2(588, 28), new Vector2(728, 28), new Vector2(868, 28),

            new Vector2(1008, 28), // EZ2ON island

            // Line 4
            new Vector2(1008, 128), new Vector2(1008, 228), new Vector2(1008, 328),
            new Vector2(1008, 428), new Vector2(1008, 528),
        };

        private static ImmutableList<(string Name, int Idx, string Song)> _requests =
            ImmutableList<(string, int, string)>.Empty;
        private static ImmutableList<(string Name, string Song)> _usedList =
            ImmutableList<(string, string)>.Empty;
        private static List<(string Name, string Song)> _dummy = new List<(string Name, string Song)>();

        private static readonly Texture2D BaseBoard = LoadTexture("Resource/baseboard.png");
        private static readonly Texture2D CenterBoard = LoadTexture("Resource/alert.png");
        private static bool _switch;
        private static int _idx = -1;
        private static int _target;
        private static int _frame;
        private static int _frameLimit;
        private static readonly Random Rnd = new Random();
        private static readonly int[] Head = { 0, 0 };
        private static readonly int[] YPos = { 0, 0 };
        private static PollState _state = PollState.Idle;

        public static void DrawChat(bool shutdownRequest)
        {
            DrawBoard(shutdownRequest);
            if (IsIdle(shutdownRequest)) DrawResetButton();
            if (IsReadyToSelect(shutdownRequest)) DrawPollButton();
            DrawSonglist();
            DrawUsedList();
            if (_state != PollState.Idle) DrawResult();
        }

        public static async void Connect()
        {
            using (Client = new WebsocketClient(new Uri("wss://irc-ws.chat.twitch.tv:443")))
            {
                Client.ReconnectTimeout = null;
                Client.MessageReceived.Subscribe(msg =>
                {
                    if (msg.ToString().StartsWith("PING")) Client.Send("PONG :tmi.twitch.tv");
                    if (_switch && msg.ToString().Contains("!픽")) UpdateRequest(msg.ToString());
                });
                await Client.Start();
                Client.Send("CAP REQ :twitch.tv/commands twitch.tv/tags");
                Client.Send("NICK justinfan1234");
                Client.Send("JOIN #arpa__");
                ExitEvent.WaitOne();
            }
        }

        public static void Dispose()
        {
            UnloadTexture(BaseBoard);
            UnloadTexture(CenterBoard);
        }

        // UIs

        private static void DrawBoard(bool shutdownRequest)
        {
            DrawTextureEx(BaseBoard, new Vector2(12, 12), 0, 2.0f, Color.WHITE);

            for (int i = 0; i < Pos.Length; i++)
            {
                // Slot
                var slot = new Rectangle(Pos[i].X + 2, Pos[i].Y + 2, 138, 98);
                var slotColor = Board.Contains(i + 1) ? Color.YELLOW : Color.WHITE;
                if (i == _idx) slotColor = Color.RED;

                if (!shutdownRequest && CheckCollisionPointRec(GetMousePosition(), slot))
                {
                    if (IsMouseButtonPressed(0) && _state == PollState.Idle) OnClick(i);
                    slotColor = Color.ORANGE;
                }

                DrawRectangleRec(slot, slotColor);

                // Text
                var count = Board.Contains(i + 1) ? "X" : FindAllSongs(i + 1).Count.ToString();
                var countPos = new Vector2(Pos[i].X + (140 - MeasureText(count, 72)) * 0.5f, Pos[i].Y + 14);

                DrawText(count, (int)countPos.X, (int)countPos.Y, 72, Color.BLACK);
            }
        }

        private static void DrawResetButton()
        {
            var button = new Rectangle(12, GetScreenHeight() - 72, 160, 60);
            var buttonColor = Fade(Color.SKYBLUE, 0.5f);
            var text = _switch ? "재설정" : "시작";
            var textLen = MeasureTextEx(Program.MainFont, text, 48, 0).X;

            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0))
                {
                    if (_switch)
                    {
                        _requests = _requests.Clear();
                        _idx = -1;
                    }
                    _switch = !_switch;
                }
                buttonColor = Color.SKYBLUE;
            }

            DrawRectangleRec(button, buttonColor);
            DrawTextEx(Program.MainFont, text, new Vector2(button.x + (button.width - textLen) * 0.5f, button.y + 6), 48, 0, Color.BLACK);
        }

        private static void DrawPollButton()
        {
            var button = new Rectangle(GetScreenWidth() - 172, GetScreenHeight() - 144, 160, 60);
            var buttonColor = Fade(Color.LIME, 0.5f);
            var text = _state == PollState.Result ? "다음" : "추첨";
            var textLen = MeasureTextEx(Program.MainFont, text, 48, 0).X;

            if (CheckCollisionPointRec(GetMousePosition(), button))
            {
                if (IsMouseButtonPressed(0))
                {
                    switch (_state)
                    {
                        case PollState.Idle:
                            _state = PollState.Active;
                            _target = Rnd.Next(FindAllSongs(_idx + 1).Count);
                            _frameLimit = Rnd.Next(61) + 120;
                            _dummy = FindAllSongs(_idx + 1);
                            break;
                        default:
                            _state = PollState.Idle;
                            var current = _dummy[_target];
                            _usedList = _usedList.Add(current);
                            _requests = _requests.RemoveAll(x => x.Name == current.Item1);
                            break;
                    }
                }
                buttonColor = Color.LIME;
            }

            DrawRectangleRec(button, buttonColor);
            DrawTextEx(Program.MainFont, text, new Vector2(button.x + (button.width - textLen) * 0.5f, button.y + 6), 48, 0, Color.BLACK);
        }

        private static void DrawSonglist()
        {
            var songlist = FindAllSongs(_idx + 1);
            var panels = VerticalMarquee(songlist, 36, 468, ref YPos[0], ref Head[0]);

            if (panels.Count > 0)
            {
                BeginScissorMode(184, 144, 808, 468);
                for (int i = 0; i < panels.Count; i++)
                {
                    var pos = new Vector2(184, 144 + YPos[0] + 42 * i);
                    DrawTextEx(Program.MainFont, panels[i].Item2, new Vector2(pos.X + 12, pos.Y + 6), 36, 0, Color.BLACK);
                }
                EndScissorMode();
            }
            else DrawTexture(CenterBoard, 184, 144, Color.WHITE);

            if (!_switch)
                DrawTextEx(Program.MainFont, "※ 준비중입니다. 잠시만 기다려주세요.", new Vector2(196, 564), 36, 0, Color.RED);
        }

        private static void DrawResult()
        {
            UpdateFrame();
            var current = _dummy[_target];

            DrawRectangle(184, 144, 808, 468, Color.DARKGRAY);
            BeginScissorMode(184, 144, 808, 468);
            DrawTextEx(Program.MainFont, "다음 곡은", new Vector2(196, 156), 36, 0, Color.WHITE);
            DrawTextEx(Program.MainFont, current.Item2, new Vector2(196, 204), 72, 0, Color.YELLOW);
            DrawTextEx(Program.MainFont, $"By {current.Item1}", new Vector2(196, 276), 48, 0, Color.WHITE);
            EndScissorMode();
        }

        private static void DrawUsedList()
        {
            var panels = VerticalMarquee(_usedList.ToList(), 24, 670, ref YPos[1], ref Head[1]);

            DrawRectangle(GetScreenWidth() - 744, 74, 732, 690, Color.DARKBLUE);
            if (panels.Count > 0)
            {
                BeginScissorMode(GetScreenWidth() - 744, 74, 732, 690);
                for (int i = 0; i < panels.Count; i++)
                {
                    var pos = new Vector2(GetScreenWidth() - 744, 74 + YPos[0] + 30 * i);
                    DrawTextEx(Program.MainFont, $"{panels[i].Item1} | {panels[i].Item2}", new Vector2(pos.X + 12, pos.Y + 6), 24, 0, Color.WHITE);
                }
                EndScissorMode();
            }
        }

        // Main Methods

        private static void UpdateRequest(string msg)
        {
            var re = new Regex(@"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
            var objects = re.Match(msg).Groups;

            if (IsValid(objects["content"].ToString()))
            {
                var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
                if (!_usedList.Select(x => x.Name).Contains(name))
                {
                    var order = GetOrder(objects["content"].ToString());
                    _requests = _requests.Add((name, order.Item1, order.Item2));
                    if (_requests.Count(x => x.Name == name) > 3)
                        _requests = _requests.Remove(_requests.First(x => x.Name == name));
                }
            }
        }

        private static bool IsValid(string text)
        {
            var content = text.Split(' ', 3);

            if (content.Length < 3) return false;
            if (!int.TryParse(content[1], out var idx)) return false;
            if (idx is < 1 or > 24) return false;
            if (Board.Contains(idx)) return false;

            return true;
        }

        private static string GetUsername(string tags, string source)
        {
            // display-name
            var name = tags.Split(';')
                           .Select(x => x.Split('='))
                           .ToDictionary(x => x[0], x => x[1])["display-name"];

            // username
            if (string.IsNullOrEmpty(name))
            {
                var re = new Regex(@"^(?:(?<nick>[^\s]+?)!(?<user>[^\s]+?)@)?(?<host>[^\s]+)$");
                name = re.Match(source).Groups["nick"].ToString();
            }

            return name;
        }

        private static (int, string) GetOrder(string text)
        {
            var content = text.Split(' ', 3);
            var idx = Convert.ToInt32(content[1]);
            return (idx, content[2]);
        }

        private static List<(string, string)> FindAllSongs(int idx)
            => _requests.FindAll(x => x.Idx == idx).Select(x => (x.Name, x.Song)).ToList();

        private static void OnClick(int i)
        {
            if (_switch)
            {
                if (!Board.Contains(i + 1)) _idx = i;
            }
            else
            {
                if (i != 6 && i != 18)
                {
                    if (Board.Contains(i + 1)) Board.Remove(i + 1);
                    else Board.Add(i + 1);
                }
                Board.Sort();
            }
        }

        private static List<(string, string)> VerticalMarquee(List<(string, string)> input, int textHeight, int height, ref int ypos, ref int head)
        {
            var count = (int)Math.Ceiling(height / (double)textHeight + 6);
            if (input.Count >= count)
            {
                ypos -= 2;
                if (ypos <= -(textHeight + 6))
                {
                    head = (head + 1) % input.Count;
                    ypos = 0;
                }
            }
            else ypos = head = 0;

            var output = input.Skip(head).Take(count + 1).ToList();
            if (input.Count >= count && output.Count < count + 1)
                output.AddRange(input.Take(count + 1 - output.Count));
            return output;
        }

        private static void UpdateFrame()
        {
            if (_state != PollState.Result)
            {
                _frame++;
                _target++;
                _target %= _dummy.Count;
                if (_frame >= _frameLimit)
                {
                    _state = PollState.Result;
                    _frame = 0;
                }
            }
        }

        // Conditions

        private static bool IsIdle(bool shutdownRequest)
            => !shutdownRequest && _state == PollState.Idle;

        private static bool IsReadyToSelect(bool shutdownRequest)
            => !shutdownRequest && _state != PollState.Active && _idx >= 0 && FindAllSongs(_idx + 1).Count > 0;
    }
}