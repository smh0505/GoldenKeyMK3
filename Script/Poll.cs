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
    
    public struct PollRequest
    {
        public string Name { get; set; }
        public string Theme { get; set; }
        public string Song { get; set; }
        public double Timestamp { get; set; }
        
        public PollRequest(string name, string theme, string song, double timestamp)
        {
            Name = name;
            Theme = theme;
            Song = song;
            Timestamp = timestamp;
        }
    }

    public struct PollResponse
    {
        public string Name { get; set; }
        public string Song { get; set; }
        public bool IsSuccessful { get; set; }

        public PollResponse(string name, string song, bool isSuccessful)
        {
            Name = name;
            Song = song;
            IsSuccessful = isSuccessful;
        }
    }

    public class Poll : IDisposable
    {
        private readonly Board _board;
        private readonly Random _rnd;
        
        public ImmutableList<PollRequest> Requests;
        public ImmutableList<PollRequest> IslandRequests;
        public ImmutableList<PollRequest> UsedList;
        public ImmutableList<PollResponse> Sequence;

        private readonly Texture2D _scene;
        private readonly Texture2D _alert;
        private readonly Texture2D _result;

        private List<PollRequest> _temp;
        private PollState _state;
        private PollRequest _current;
        private int _target;

        private float _yPos;
        private int _idx;

        private float _yPos2;
        private int _idx2;

        private float _xPos;
        private float _xPos2;

        public Poll(Board board)
        {
            _rnd = new Random();
            _board = board;
            
            _scene = LoadTexture("Resource/poll.png");
            _alert = LoadTexture("Resource/alert.png");
            _result = LoadTexture("Resource/result.png");

            Requests = ImmutableList<PollRequest>.Empty;
            IslandRequests = ImmutableList<PollRequest>.Empty;
            UsedList = ImmutableList<PollRequest>.Empty;
            Sequence = ImmutableList<PollResponse>.Empty;

            _temp = new List<PollRequest>();
            _state = PollState.Idle;
            _target = 0;
            _current = new PollRequest(string.Empty, string.Empty, string.Empty, 0);

            _yPos = 0;
            _idx = 0;

            _yPos2 = 0;
            _idx2 = 0;

            _xPos = 0;
            _xPos2 = 0;
        }
        
        // Public Methods

        public void Draw(bool shutdownRequest)
        {
            DrawTexture(_scene, 320, 180, Color.WHITE);
            
            if (FindAllSongs(_target).Any()) DrawPoll(shutdownRequest);
            DrawRequests();
            DrawUsedList();
            DrawSequence();
            if (_state != PollState.Idle) DrawResult();
            
            DrawHover(shutdownRequest);
        }

        public void Control(bool shutdownRequest)
        {
            if (FindAllSongs(_target).Any()) ControlPoll(shutdownRequest);
            if (_state == PollState.Idle) ControlHover(shutdownRequest);
            if (_state == PollState.Active) _current = _temp[_rnd.Next(_temp.Count)];
        }

        public void Dispose()
        {
            UnloadTexture(_scene);
            UnloadTexture(_alert);
            UnloadTexture(_result);
            GC.SuppressFinalize(this);
        }
        
        // Private Methods

        private void DrawRequests()
        {
            if (FindAllSongs(_target).Any())
            {
                var theme = _board.CurrBoard[_target];
                var color = _target is 7 or 20 ? Color.WHITE : _board.ThemePairs[theme];
                DrawRectangle(332, 192, 622, 62, color);
                DrawTextEx(Ui.Galmuri48, theme.Replace("_", " "), new Vector2(344, 199), 48, 0, Color.BLACK);

                var requests = VertMarquee(FindAllSongs(_target).ToList(), 286, ref _yPos, ref _idx).ToArray();
                        
                BeginScissorMode(332, 254, 622, 286);
                for (var i = 0; i < requests.Length; i++)
                {
                    var pos = new Vector2(344, 260 + _yPos + 30 * i);
                    DrawTextEx(Ui.Galmuri24, requests[i].Song, pos, 24, 0, Color.BLACK);
                }
                EndScissorMode();
            }
            else DrawTexture(_alert, 332, 192, Color.WHITE);
        }

        private void DrawResult()
        {
            DrawTexture(_result, 332, 254, Color.WHITE);
            BeginScissorMode(332, 254, 622, 286);
            if (_state == PollState.Result) HoriMarquee(_current.Song);
            else DrawTextEx(Ui.Galmuri48, _current.Song, new Vector2(352, 314), 48, 0, Color.WHITE);
            DrawTextEx(Ui.Galmuri24, _current.Name, new Vector2(402, 380), 24, 0, Color.WHITE);
            EndScissorMode();
        }
        
        private void DrawUsedList()
        {
            var usedList = VertMarquee(UsedList, 288, ref _yPos2, ref _idx2).ToArray();
            
            BeginScissorMode(332, 540, 622, 288);
            for (var i = 0; i < usedList.Length; i++)
            {
                var pos = new Vector2(340, 546 + _yPos + 30 * i);
                DrawTextEx(Ui.Galmuri24, $"{usedList[i].Name} => {usedList[i].Song}", pos, 24, 0, Color.WHITE);
            }
            EndScissorMode();
        }

        private void DrawSequence()
        {
            var responses = Sequence.TakeLast(10).ToArray();
            
            BeginScissorMode(966, 254, 622, 240);
            for (var i = 0; i < responses.Length; i++)
            {
                var pos = new Vector2(966, 254 + 24 * i);
                DrawRectangle((int)pos.X, (int)pos.Y, 622, 24, responses[i].IsSuccessful ? Color.BLUE : Color.RED);
                DrawTextEx(Ui.Galmuri24, $"{responses[i].Name} => {responses[i].Song}", pos, 24, 0, Color.WHITE);
            }
            EndScissorMode();
        }

        private void DrawHover(bool shutdownRequest)
        {
            for (var i = 0; i < _board.Blocks.Length; i++)
            {
                var button = new Rectangle(_board.Blocks[i].x + 2, _board.Blocks[i].y + 2, _board.Blocks[i].width - 4,
                    _board.Blocks[i].height - 4);
                if (Ui.IsHovering(button, !shutdownRequest))
                {
                    DrawRectangleRec(button, Fade(Color.WHITE, 0.7f));
                    var text = i is 0 or 13 || _board.GoldenKeys.Contains(i)
                        ? _board.CurrBoard[i]
                        : FindAllSongs(i).ToArray().Length.ToString();
                    Ui.DrawTextCentered(button, Ui.Galmuri48, text, 48, Color.BLACK);
                }
            }
        }

        private void ControlHover(bool shutdownRequest)
        {
            for (var i = 0; i < _board.Blocks.Length; i++)
            {
                var button = new Rectangle(_board.Blocks[i].x + 2, _board.Blocks[i].y + 2, _board.Blocks[i].width - 4,
                    _board.Blocks[i].height - 4);
                if (Ui.IsHovering(button, !shutdownRequest) && IsMouseButtonPressed(0))
                    _target = i;
            }
        }

        private void DrawPoll(bool shutdownRequest)
        {
            var button1 = new Rectangle(332, 840, 160, 48);
            var button2 = new Rectangle(500, 840, 160, 48);
            var button3 = new Rectangle(668, 840, 160, 48);

            var color1 = Ui.IsHovering(button1, !shutdownRequest) ? Color.SKYBLUE : Fade(Color.SKYBLUE, 0.7f);
            var text1 = _state switch
            {
                PollState.Idle => "추첨",
                PollState.Active => "멈추기",
                PollState.Result => "결정",
                _ => string.Empty
            };
            
            DrawRectangleRec(button1, color1);
            Ui.DrawTextCentered(button1, Ui.Galmuri36, text1, 36, Color.BLACK);

            if (_state == PollState.Result)
            {
                var color2 = Ui.IsHovering(button2, !shutdownRequest) ? Color.SKYBLUE : Fade(Color.SKYBLUE, 0.7f);
                var color3 = Ui.IsHovering(button3, !shutdownRequest) ? Color.SKYBLUE : Fade(Color.SKYBLUE, 0.7f);
                
                DrawRectangleRec(button2, color2);
                Ui.DrawTextCentered(button2, Ui.Galmuri36, "재추첨", 36, Color.BLACK);
                DrawRectangleRec(button3, color3);
                Ui.DrawTextCentered(button3, Ui.Galmuri36, "추첨 취소", 36, Color.BLACK);
            }
        }

        private void ControlPoll(bool shutdownRequest)
        {
            var button1 = new Rectangle(332, 840, 160, 48);
            var button2 = new Rectangle(500, 840, 160, 48);
            var button3 = new Rectangle(668, 840, 160, 48);

            switch (_state)
            {
                case PollState.Idle:
                    if (Ui.IsHovering(button1, !shutdownRequest) && IsMouseButtonPressed(0))
                    {
                        _state = PollState.Active;
                        _temp = new List<PollRequest>(FindAllSongs(_target));
                    }
                    break;
                case PollState.Active:
                    if (Ui.IsHovering(button1, !shutdownRequest) && IsMouseButtonPressed(0))
                        _state = PollState.Result;
                    break;
                case PollState.Result:
                    if (Ui.IsHovering(button1, !shutdownRequest) && IsMouseButtonPressed(0))
                    {
                        _state = PollState.Idle;
                        UsedList = UsedList.Add(_current);
                        Requests = Requests.RemoveAll(x => x.Name == _current.Name);
                        IslandRequests = IslandRequests.RemoveAll(x => x.Name == _current.Name);
                        _current = new PollRequest();
                    }

                    if (Ui.IsHovering(button2, !shutdownRequest) && IsMouseButtonPressed(0))
                        _state = PollState.Active;

                    if (Ui.IsHovering(button3, !shutdownRequest) && IsMouseButtonPressed(0))
                        _state = PollState.Idle;

                    break;
            }
        }

        private List<PollRequest> VertMarquee(IReadOnlyCollection<PollRequest> requests, float height, ref float y, ref int head)
        {
            var count = (int)Math.Ceiling(height / 30.0f);
            if (requests.Count >= count)
            {
                y -= 120.0f / GetFPS();
                if (y <= -30.0f)
                {
                    head = (head + 1) % requests.Count;
                    y = 0;
                }
            }
            else y = head = 0;
            
            var output = requests.Skip(_idx).Take(count + 1).ToList();
            if (requests.Count >= count && output.Count < count + 1)
                output.AddRange(requests.Take(count + 1 - output.Count));
            return output;
        }

        private void HoriMarquee(string song)
        {
            var size = MeasureTextEx(Ui.Galmuri48, song, 48, 0).X;
            if (size > 622)
            {
                _xPos -= 120.0f / GetFPS();
                _xPos2 = _xPos + size + 36.0f;
                if (_xPos <= -size) _xPos = _xPos2;
            }
            else
            {
                _xPos = 0;
                _xPos2 = _xPos + size + 36.0f;
            }
            
            DrawTextEx(Ui.Galmuri48, song, new Vector2(352 + _xPos, 314), 48, 0, Color.YELLOW);
            if (size > 622) DrawTextEx(Ui.Galmuri48, song, new Vector2(352 + _xPos2, 314), 48, 0, Color.YELLOW);
        }

        private IEnumerable<PollRequest> FindAllSongs(int idx)
        {
            return idx switch
            {
                7 => IslandRequests.FindAll(x => x.Theme == _board.CurrBoard[7]),
                20 => IslandRequests.FindAll(x => x.Theme == _board.CurrBoard[20]),
                _ => idx is < 0 or > 25
                    ? ImmutableList<PollRequest>.Empty
                    : Requests.FindAll(x => x.Theme == _board.CurrBoard[idx])
            };
        }
    }
}