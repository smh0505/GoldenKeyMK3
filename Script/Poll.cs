﻿using System.Collections.Immutable;
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

        private float _yPos;
        private int _idx;

        private float _yPos2;
        private int _idx2;

        private float _xPos;
        private float _xPos2;

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

            _yPos = 0;
            _idx = 0;

            _yPos2 = 0;
            _idx2 = 0;

            _xPos = 0;
            _xPos2 = 0;
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
            if (FindAllSongs(Target).Any())
            {
                var color = _themePairs.TryGetValue(Target, out var value) ? Color.WHITE : value;
                DrawRectangle(332, 192, 622, 62, color);
                DrawTextEx(Ui.Galmuri48, Target.Replace("_", " "), new Vector2(344, 199), 48, 0, Color.BLACK);

                var requests = VertMarquee(FindAllSongs(Target).ToList(), 286, ref _yPos, ref _idx).ToArray();
                        
                BeginScissorMode(332, 254, 622, 286);
                for (var i = 0; i < requests.Length; i++)
                {
                    var pos = new Vector2(344, 260 + _yPos + 30 * i);
                    DrawTextEx(Ui.Galmuri24, requests[i].Item3, pos, 24, 0, Color.BLACK);
                }
                EndScissorMode();
            }
            else DrawTexture(_alert, 332, 192, Color.WHITE);
        }

        private void DrawResult()
        {
            DrawTexture(_result, 332, 254, Color.WHITE);
            BeginScissorMode(332, 254, 622, 286);
            if (State == PollState.Result) HorizonMarquee(_current.Song);
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

        public void DrawSequence()
        {
            var responses = Sequence.ToArray();
            
            BeginScissorMode(1080, 254, 200, 240);
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
            EndScissorMode();
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

        private static List<T> VertMarquee<T>(IReadOnlyCollection<T> requests, float height, ref float y, ref int head)
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
            
            var output = requests.Skip(head).Take(count + 1).ToList();
            if (requests.Count >= count && output.Count < count + 1)
                output.AddRange(requests.Take(count + 1 - output.Count));
            return output;
        }

        private void HorizonMarquee(string song)
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
    }
}