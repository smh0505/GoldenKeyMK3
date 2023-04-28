using System.Collections.Immutable;
using System.Numerics;
using System.Text.RegularExpressions;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Inventory : IGameObject
    {
        public ImmutableList<(string Name, int Count)> ItemList;

        private readonly Rectangle _inventoryBox;
        private readonly Texture2D _inventoryButton;

        private (string Name, int Count)[] _page;
        private int _pageId;
        private readonly Rectangle[] _pageButton;
        private readonly bool[] _pageHover;

        private readonly Rectangle[] _plusButton;
        private readonly bool[] _plusHover;
        private readonly Rectangle[] _minusButton;
        private readonly bool[] _minusHover;

        private readonly Rectangle[] _counter;
        private readonly Vector2[] _pos;

        public Inventory()
        {
            ItemList = ImmutableList<(string, int)>.Empty;
            _inventoryBox = new Rectangle(1280, 254, 310, 240);
            _inventoryButton = LoadTexture("Resource/inventory.png");

            _page = Array.Empty<(string, int)>();
            _pageId = 0;
            _pageButton = new Rectangle[]
            {
                new(1280, 444, 50, 50),
                new(1540, 444, 50, 50)
            };
            _pageHover = new[] { false, false };
            
            _plusButton = new Rectangle[]
            {
                new(1494, 262, 40, 36),
                new(1494, 306, 40, 36),
                new(1494, 350, 40, 36),
                new(1494, 394, 40, 36)
            };
            _plusHover = new[] { false, false, false, false };
            
            _minusButton = new Rectangle[]
            {
                new(1542, 262, 40, 36),
                new(1542, 306, 40, 36),
                new(1542, 350, 40, 36),
                new(1542, 394, 40, 36)
            };
            _minusHover = new[] { false, false, false, false };

            _counter = new Rectangle[]
            {
                new(1288, 258, 40, 44),
                new(1288, 302, 40, 44),
                new(1288, 346, 40, 44),
                new(1288, 390, 40, 44)
            };
            _pos = new Vector2[]
            {
                new(1288, 268),
                new(1288, 312),
                new(1288, 356),
                new(1288, 400)
            };
        }

        public void Draw()
        {
            DrawRectangleRec(_inventoryBox, Color.PURPLE);
            
            if (!ItemList.Any())
                Ui.DrawTextCentered(_inventoryBox, Ui.Galmuri60, "비었음!", 60, Color.WHITE);
            else DrawItem();
        }

        public void Control(bool shutdownRequest)
        {
            if (ItemList.Any()) _page = ItemList.Skip(_pageId * 4).Take(4).ToArray();
            for (var i = 0; i < 2; i++)
                _pageHover[i] = Ui.IsHovering(_pageButton[i], !shutdownRequest);

            if (_pageHover[0] && IsMouseButtonPressed(0))
                _pageId = _pageId == 0 ? (int)Math.Ceiling(ItemList.Count / 4.0f) - 1 : _pageId - 1;
            if (_pageHover[1] && IsMouseButtonPressed(0))
                _pageId = _pageId == (int)Math.Ceiling(ItemList.Count / 4.0f) - 1 ? 0 : _pageId + 1;

            for (var i = 0; i < _page.Length; i++)
            {
                _plusHover[i] = !_page[i].Name.Contains("[1회]") && Ui.IsHovering(_plusButton[i], !shutdownRequest);
                _minusHover[i] = Ui.IsHovering(_minusButton[i], !shutdownRequest);
                
                if (_plusHover[i] && IsMouseButtonPressed(0)) Plus(_page[i].Name);
                if (_minusHover[i] && IsMouseButtonPressed(0)) Minus(_page[i].Name);
            }
        }

        public void Dispose()
        {
            UnloadTexture(_inventoryButton);
            GC.SuppressFinalize(this);
        }
        
        // Controls

        public void AddItem(string item)
        {
            if (Regex.IsMatch(item, @"(?<=3턴간\s).*$"))
            {
                var newItem = Regex.Match(item, @"(?<=3턴간\s).*$").Value;
                var i = ItemList.FindIndex(x => x.Name == newItem);
                if (i != -1)
                {
                    var temp = ItemList[i];
                    ItemList = ItemList.RemoveAt(i);
                    ItemList = ItemList.Insert(i, (temp.Name, temp.Count + 3));
                }
                else ItemList = ItemList.Add((newItem, 3));
            }
            if (Regex.IsMatch(item, @"^.*(?=\(1회\))"))
            {
                var newItem = "[1회] " + Regex.Match(item, @"^.*(?=\(1회\))").Value;
                var i = ItemList.FindIndex(x => x.Name == newItem);
                if (i == -1) ItemList = ItemList.Add((newItem, 1));
            }
        }

        public void RemoveItems()
        {
            ItemList = ItemList.RemoveAll(x => !x.Name.Contains("[1회]") & x.Count == 1);
            var items = ItemList.FindAll(x => x.Count > 1);
            foreach (var x in items)
            {
                var i = ItemList.IndexOf(x);
                ItemList = ItemList.Remove(x);
                ItemList = ItemList.Insert(i, (x.Name, x.Count - 1));
            }

            _pageId = Math.Clamp(_pageId, 0, (int)Math.Ceiling(ItemList.Count / 4.0f) - 1);
        }

        private void Plus(string item)
        {
            var x = ItemList.FindIndex(x => x.Name == item);
            var y = ItemList[x];

            ItemList = ItemList.Remove(y);
            ItemList = ItemList.Insert(x, (y.Name, y.Count + 1));
        }
        
        private void Minus(string item)
        {
            var x = ItemList.FindIndex(x => x.Name == item);
            var y = ItemList[x];

            ItemList = ItemList.Remove(y);
            if (y.Count > 1) ItemList = ItemList.Insert(x, (y.Name, y.Count - 1));
            _pageId = Math.Clamp(_pageId, 0, (int)Math.Ceiling(ItemList.Count / 4.0f) - 1);
        }

        // UIs

        private void DrawItem()
        {
            for (var i = 0; i < 2; i++)
                if (_pageHover[i]) DrawRectangleRec(_pageButton[i], Color.DARKPURPLE);
            DrawTexture(_inventoryButton, 1280, 254, Color.WHITE);
            
            for (var i = 0; i < _page.Length; i++)
            {
                var refPoint = new Vector2(1288, 268 + 44 * i);
                if (_page[i].Name.Contains("[1회]"))
                {
                    BeginScissorMode(1288, 258 + 44 * i, 198, 44);
                    Ui.ScrollText(Ui.Galmuri24, _page[i].Name, 24, 206, ref _pos[i], in refPoint, Color.WHITE);
                    EndScissorMode();
                }
                else
                {
                    refPoint.X += 40;
                    Ui.DrawTextCentered(_counter[i], Ui.Galmuri36, _page[i].Count.ToString(), 36, Color.WHITE);
                    BeginScissorMode(1328, 258 + 44 * i, 158, 44);
                    Ui.ScrollText(Ui.Galmuri24, _page[i].Name, 24, 158, ref _pos[i], in refPoint, Color.WHITE);
                    EndScissorMode();
                }
                
                if (_plusHover[i]) DrawRectangleRec(_plusButton[i], Color.DARKPURPLE);
                if (_minusHover[i]) DrawRectangleRec(_minusButton[i], Color.DARKPURPLE);
                if (!_page[i].Name.Contains("[1회]")) 
                    Ui.DrawTextCentered(_plusButton[i], Ui.Galmuri24, "+1", 24, Color.WHITE);
                Ui.DrawTextCentered(_minusButton[i], Ui.Galmuri24, "-1", 24, Color.WHITE);
            }
            
            Ui.DrawTextCentered(new Rectangle(1330, 444, 210, 50), Ui.Galmuri36,
                $"페이지 {_pageId + 1} / {(int)Math.Ceiling(ItemList.Count / 4.0f)}", 36, Color.WHITE);
        }
    }
}