using System.Collections.Immutable;
using System.Numerics;
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

    public class Poll : IDisposable
    {
        private readonly Random _rnd;
        private readonly Inventory _inventory;
        public PollState State;

        public ImmutableList<(string Name, string Theme, string Song, double Time)> Requests;
        public ImmutableList<(string Name, string Theme, string Song, double Time)> IslandRequests;
        public ImmutableList<(string Name, string Song)> UsedList;
        public ImmutableList<(string Name, ChatState Response)> Sequence;
        
        public string Target;
        private Dictionary<string, Color> _themePairs;

        private readonly Texture2D _scene;
        private readonly Texture2D _alert;
        private readonly Texture2D _result;

        private List<(string Name, string Theme, string Song, double Time)> _temp;
        private (string Name, string Theme, string Song, double Time) _current;

        private readonly Rectangle[] _pollButton;
        private readonly bool[] _pollHover;

        private int _requestsIdx;
        private int _usedListIdx;
        private Vector2 _requestsPos;
        private Vector2 _usedListPos;
        private Vector2 _horizontalPos;

        public Poll(Inventory inventory)
        {
            _inventory = inventory;
            _rnd = new Random();
            
            Requests = ImmutableList<(string, string, string, double)>.Empty;
            IslandRequests = ImmutableList<(string, string, string, double)>.Empty;
            UsedList = ImmutableList<(string, string)>.Empty;
            Sequence = ImmutableList<(string, ChatState)>.Empty;
            
            _scene = LoadTexture("Resource/poll.png");
            _alert = LoadTexture("Resource/alert.png");
            _result = LoadTexture("Resource/result.png");

            _temp = new List<(string, string, string, double)>();
            State = PollState.Idle;
            Target = string.Empty;
            _themePairs = new Dictionary<string, Color>();
            _current = (string.Empty, string.Empty, string.Empty, 0);
            
            _pollHover = new[] { false, false, false };
            _pollButton = new Rectangle[]
            {
                new(332, 840, 160, 48),
                new(500, 840, 160, 48),
                new(668, 840, 160, 48)
            };

            _requestsIdx = _usedListIdx = 0;
            _requestsPos = new Vector2(340, 260);
            _usedListPos = new Vector2(340, 546);
            _horizontalPos = new Vector2(352, 314);
        }
        
        public void Draw()
        {
            DrawTexture(_scene, 320, 180, Color.WHITE);
            
            if (FindAllSongs(Target).Any()) DrawPoll();
            DrawRequests();
            DrawUsedList(); 
            if (State != PollState.Idle) DrawResult();
        }

        public void Control(bool shutdownRequest)
        {
            if (FindAllSongs(Target).Any()) ControlPoll(shutdownRequest);
            if (State == PollState.Active) _current = _temp[_rnd.Next(_temp.Count)];
        }

        public void Dispose()
        {
            UnloadTexture(_scene);
            UnloadTexture(_alert);
            UnloadTexture(_result);
            GC.SuppressFinalize(this);
        }
        
        // UIs

        private void DrawRequests()
        {
            var songList = FindAllSongs(Target).ToArray();
            if (songList.Any())
            {
                var color = _themePairs.TryGetValue(Target, out var value) ? value : Color.WHITE;
                DrawRectangle(332, 192, 622, 62, color);
                DrawTextEx(Ui.Galmuri48, Target.Replace("_", " "), new Vector2(344, 199), 48, 0, Color.BLACK);

                var refPoint = new Vector2(340, 260);
                var box = new Rectangle(332, 254, 622, 286);
                Ui.ScrollList(box, Ui.Galmuri24, songList.Select(x => x.Item3).ToArray(), 24, 30, ref _requestsIdx,
                    ref _requestsPos, in refPoint, Color.BLACK);
            }
            else DrawTexture(_alert, 332, 192, Color.WHITE);
        }

        private void DrawResult()
        {
            var refPoint = new Vector2(352, 314);
            
            DrawTexture(_result, 332, 254, Color.WHITE);
            BeginScissorMode(332, 254, 622, 286);
            
            if (State == PollState.Result) 
                Ui.ScrollText(Ui.Galmuri48, _current.Song, 48, 622, ref _horizontalPos, in refPoint, Color.YELLOW);
            else DrawTextEx(Ui.Galmuri48, _current.Song, refPoint, 48, 0, Color.WHITE);
            DrawTextEx(Ui.Galmuri24, _current.Name, new Vector2(402, 380), 24, 0, Color.WHITE);
            
            EndScissorMode();
        }
        
        private void DrawUsedList()
        {
            var refPoint = new Vector2(340, 546);
            var box = new Rectangle(332, 540, 622, 288);
            Ui.ScrollList(box, Ui.Galmuri24, UsedList.Select(x => $"{x.Name} => {x.Song}").ToArray(), 24, 30,
                ref _usedListIdx, ref _usedListPos, in refPoint, Color.WHITE);
        }

        public void DrawSequence()
        {
            var responses = Sequence.ToArray();
            
            for (var i = 0; i < responses.Length; i++)
            {
                var pos = new Vector2(1080, 254 + 24 * i);
                DrawRectangle((int)pos.X, (int)pos.Y, 200, 24, 
                    responses[i].Response switch
                    {
                        ChatState.Successful => Color.BLUE,
                        ChatState.Failed => Color.RED,
                        ChatState.Reconnecting => Color.PURPLE,
                        _ => Color.BLACK
                    });
                DrawTextEx(Ui.Galmuri24, responses[i].Name, pos, 24, 0, Color.WHITE);
            }
        }

        private void DrawPoll()
        {
            var color1 = _pollHover[0] ? Color.SKYBLUE : Fade(Color.SKYBLUE, 0.7f);
            var color2 = _pollHover[1] ? Color.SKYBLUE : Fade(Color.SKYBLUE, 0.7f);
            var color3 = _pollHover[2] ? Color.SKYBLUE : Fade(Color.SKYBLUE, 0.7f);
            
            var text1 = State switch
            {
                PollState.Idle => "추첨",
                PollState.Active => "멈추기",
                PollState.Result => "결정",
                _ => string.Empty
            };
            
            DrawRectangleRec(_pollButton[0], color1);
            Ui.DrawTextCentered(_pollButton[0], Ui.Galmuri36, text1, 36, Color.BLACK);

            if (State != PollState.Result) return;

            DrawRectangleRec(_pollButton[1], color2);
            Ui.DrawTextCentered(_pollButton[1], Ui.Galmuri36, "재추첨", 36, Color.BLACK);
            DrawRectangleRec(_pollButton[2], color3);
            Ui.DrawTextCentered(_pollButton[2], Ui.Galmuri36, "추첨 취소", 36, Color.BLACK);
        }

        // Controls

        public void Update(Dictionary<string, Color> themePairs)
            => _themePairs = new Dictionary<string, Color>(themePairs);

        public IEnumerable<(string, string, string, double)> FindAllSongs(string theme)
        {
            var output = new List<(string, string, string, double)>();
            output.AddRange(IslandRequests.FindAll(x => x.Theme == theme));
            output.AddRange(Requests.FindAll(x => x.Theme == theme));
            return output;
        }
        
        private void ControlPoll(bool shutdownRequest)
        {
            for (var i = 0; i < 3; i++)
                _pollHover[i] = Ui.IsHovering(_pollButton[i], !shutdownRequest);

            if (_pollHover[0] && IsMouseButtonPressed(0))
            {
                switch (State)
                {
                    case PollState.Idle:
                        State = PollState.Active;
                        _temp = new List<(string, string, string, double)>(FindAllSongs(Target));
                        break;
                    case PollState.Active:
                        State = PollState.Result;
                        break;
                    case PollState.Result:
                        State = PollState.Idle;
                        UsedList = UsedList.Add((_current.Name, _current.Song));
                        Requests = Requests.RemoveAll(x => x.Name == _current.Name);
                        IslandRequests = IslandRequests.RemoveAll(x => x.Name == _current.Name);
                        _current = (string.Empty, string.Empty, string.Empty, 0);
                        _inventory.RemoveItems();
                        break;
                }
            }

            if (State != PollState.Result) return;
            if (_pollHover[1] && IsMouseButtonPressed(0))
                State = PollState.Active;
            if (_pollHover[2] && IsMouseButtonPressed(0))
                State = PollState.Idle;
        }
    }
}