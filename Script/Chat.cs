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
        idle = 0,
        active,
        result
    }

    public class Chat
    {
        public static WebsocketClient Client;
        public static ManualResetEvent ExitEvent = new ManualResetEvent(false);

        private static List<int> _board = new List<int>{ 2, 5, 9, 11, 14, 17, 21, 23 };

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
        
        private static (string Name, string Song) _currSong;

        private static readonly Texture2D BaseBoard = LoadTexture("Resource/baseboard.png");
        private static readonly Texture2D CenterBoard = LoadTexture("Resource/alert.png");
        private static bool _switch;
        private static int _idx = -1;
        private static int[] _head = { 0, 0 };
        private static int[] _ypos = { 0, 0 };

        public static void DrawChat(bool shutdownRequest)
        {
            DrawBoard(shutdownRequest);
            DrawButton(shutdownRequest);
            DrawSonglist();
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
                Client.Send("JOIN #mson2017");
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
                var slotColor = _board.Contains(i + 1) ? Color.YELLOW : Color.WHITE;
                if (i == _idx) slotColor = Color.RED;

                if (!shutdownRequest && CheckCollisionPointRec(GetMousePosition(), slot))
                {
                    if (IsMouseButtonPressed(0)) OnClick(i);
                    slotColor = Color.ORANGE;
                }

                DrawRectangleRec(slot, slotColor);

                // Text
                var count = _board.Contains(i + 1) ? "X" : FindAllSongs(i + 1).Count.ToString();
                var countPos = new Vector2(Pos[i].X + (140 - MeasureText(count, 72)) * 0.5f, Pos[i].Y + 14);

                DrawText(count, (int)countPos.X, (int)countPos.Y, 72, Color.BLACK);
            }
        }

        private static void DrawButton(bool shutdownRequest)
        {
            var button = new Rectangle(12, GetScreenHeight() - 72, 160, 60);
            var buttonColor = Fade(Color.SKYBLUE, 0.5f);
            var text = _switch ? "재설정" : "시작";
            var textLen = MeasureTextEx(Program.MainFont, text, 48, 0).X;

            if (!shutdownRequest && CheckCollisionPointRec(GetMousePosition(), button))
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
            DrawTextEx(Program.MainFont, text, new Vector2(12 + (160 - textLen) * 0.5f, GetScreenHeight() - 66), 48, 0,
                Color.BLACK);
        }

        private static void DrawSonglist()
        {
            var songlist = FindAllSongs(_idx + 1);
            var panels = VerticalMarquee(songlist, ref _ypos[0], ref _head[0]);

            if (panels.Count > 0)
            {
                BeginScissorMode(184, 144, 808, 468);
                for (int i = 0; i < panels.Count; i++)
                {
                    var pos = new Vector2(184, 144 + _ypos[0] + 42 * i);
                    DrawTextEx(Program.MainFont, panels[i].Item2, new Vector2(pos.X + 12, pos.Y + 6), 36, 0, Color.BLACK);
                }
                EndScissorMode();
            }
            else DrawTexture(CenterBoard, 184, 144, Color.WHITE);
            
            if (!_switch) 
                DrawTextEx(Program.MainFont, "※ 준비중입니다. 잠시만 기다려주세요.", new Vector2(196, 564), 36, 0, Color.RED);
        }

        // Main Methods

        private static void UpdateRequest(string msg)
        {
            var re = new Regex(@"^(?:@(?<tags>(?:.+?=.*?)(?:;.+?=.*?)*) )?(?::(?<source>[^ ]+?) )?(?<command>[0-9]{3}|[a-zA-Z]+)(?: (?<params>.+?))?(?: :(?<content>.*))?$");
            var objects = re.Match(msg).Groups;

            if (IsValid(objects["content"].ToString()))
            {
                var name = GetUsername(objects["tags"].ToString(), objects["source"].ToString());
                var order = GetOrder(objects["content"].ToString());
                _requests = _requests.Add((name, order.Item1, order.Item2));
                if (_requests.Count(x => x.Name == name) > 3)
                    _requests = _requests.Remove(_requests.First(x => x.Name == name));
            }
        }

        private static bool IsValid(string text)
        {
            var content = text.Split(' ', 3);

            if (content.Length < 3) return false;
            if (!int.TryParse(content[1], out var idx)) return false;
            if (idx is < 1 or > 24) return false;
            if (_board.Contains(idx)) return false;

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
                if (!_board.Contains(i + 1)) _idx = i;
            }
            else
            {
                if (i != 6 && i != 18)
                {
                    if (_board.Contains(i + 1)) _board.Remove(i + 1);
                    else _board.Add(i + 1);
                }
                _board.Sort();
            }
        }

        private static List<(string, string)> VerticalMarquee(List<(string, string)> input, ref int ypos, ref int head)
        {
            var count = (int)Math.Ceiling(468 / 42.0f);
            if (input.Count >= count)
            {
                ypos -= 2;
                if (ypos <= -42)
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
    }
}