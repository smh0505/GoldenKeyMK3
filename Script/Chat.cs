using System.Collections.Immutable;
using System.Numerics;
using System.Text.RegularExpressions;
using Websocket.Client;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Chat
    {
        public static WebsocketClient Client;
        public static ManualResetEvent ExitEvent = new ManualResetEvent(false);

        private static List<int> _board = new List<int>{ 2, 5, 8, 10, 13, 16, 19, 21 };

        private static readonly Vector2[] Pos = new []
        {
            new Vector2(GetScreenWidth() - 160, 382),
            new Vector2(GetScreenWidth() - 230, 382),
            new Vector2(GetScreenWidth() - 300, 382),
            new Vector2(GetScreenWidth() - 370, 382),
            new Vector2(GetScreenWidth() - 440, 382),
            new Vector2(GetScreenWidth() - 510, 382),

            new Vector2(GetScreenWidth() - 580, 332),
            new Vector2(GetScreenWidth() - 580, 282),
            new Vector2(GetScreenWidth() - 580, 232),
            new Vector2(GetScreenWidth() - 580, 182),
            new Vector2(GetScreenWidth() - 580, 132),

            new Vector2(GetScreenWidth() - 510, 82),
            new Vector2(GetScreenWidth() - 440, 82),
            new Vector2(GetScreenWidth() - 370, 82),
            new Vector2(GetScreenWidth() - 300, 82),
            new Vector2(GetScreenWidth() - 230, 82),
            new Vector2(GetScreenWidth() - 160, 82),

            new Vector2(GetScreenWidth() - 90, 132),
            new Vector2(GetScreenWidth() - 90, 182),
            new Vector2(GetScreenWidth() - 90, 232),
            new Vector2(GetScreenWidth() - 90, 282),
            new Vector2(GetScreenWidth() - 90, 332),
        };

        private static ImmutableList<(string Name, int Idx, string Song)> _requests =
            ImmutableList<(string Name, int Idx, string Song)>.Empty;
        private static bool _switch;
        private static readonly Texture2D BaseBoard = LoadTexture("Resource/baseboard.png");

        public static void DrawChat(bool shutdownRequest)
        {
            DrawBoard(shutdownRequest);
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
        }

        // UIs

        private static void DrawBoard(bool shutdownRequest)
        {
            var text = _switch ? "재설정" : "시작";

            DrawTexture(BaseBoard, GetScreenWidth() - 588, 74, Color.WHITE);

            for (int i = 0; i < Pos.Length; i++)
            {
                var button = new Rectangle(Pos[i].X, Pos[i].Y, 70, 50);
                var buttonColor = _board.Contains(i + 1) ? Color.YELLOW : Color.WHITE;
                if (CheckCollisionPointRec(GetMousePosition(), button) && !shutdownRequest)
                {
                    if (!_switch) ChangeState(i);
                    else text = _board.Contains(i + 1) ? "황금열쇠" : $"{FindAllSongs(i + 1).Count}곡";
                    buttonColor = Color.RED;
                }
                DrawRectangleRec(button, buttonColor);
            }

            var centerButton = new Rectangle(GetScreenWidth() - 498, 144, 396, 226);
            var centerColor = Color.LIGHTGRAY;
            var textColor = Color.BLACK;
            if (CheckCollisionPointRec(GetMousePosition(), centerButton) && !shutdownRequest)
            {
                if (IsMouseButtonPressed(0))
                {
                    if (_switch) _requests = _requests.Clear();
                    _switch = !_switch;
                }
                centerColor = Color.DARKGRAY;
                textColor = Color.WHITE;
            }
            DrawRectangleRec(centerButton, centerColor);

            var textSize = MeasureTextEx(Program.MainFont, text, 84, 0);
            var textPos = new Vector2(GetScreenWidth() - 300 - textSize.X * 0.5f,
                242 - textSize.Y * 0.5f);
            DrawTextEx(Program.MainFont, text, textPos, 84, 0, textColor);
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
            if (idx is < 1 or > 22) return false;
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

        private static void ChangeState(int i)
        {
            if (IsMouseButtonPressed(0))
            {
                if (_board.Contains(i + 1)) _board.Remove(i + 1);
                else _board.Add(i + 1);
                _board.Sort();
            }
        }
    }
}