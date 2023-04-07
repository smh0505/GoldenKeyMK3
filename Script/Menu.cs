using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Menu : IGameObject
    {
        private readonly Board _board;
        
        private readonly Texture2D _menuBack;
        private readonly Texture2D _menuButtons;

        private readonly Rectangle[] _menu;
        private readonly bool[] _menuHover;
        
        public Menu(Board board)
        {
            _board = board;
            
            _menuBack = LoadTexture("Resource/menuback.png");
            _menuButtons = LoadTexture("Resource/menu.png");

            _menu = new Rectangle[]
            {
                new(664, 324, 284, 128),
                new(972, 324, 284, 128),
                new(664, 476, 284, 128),
                new(972, 476, 284, 128)
            };
            _menuHover = new[] { false, false, false, false };
        }

        public void Draw()
        {
            DrawTexture(_menuBack, 640, 300, Color.WHITE);
            for (var i = 0; i < 4; i++)
                if (_menuHover[i]) DrawRectangleRec(_menu[i], Color.BLUE);
            DrawTexture(_menuButtons, 640, 300, Color.WHITE);
        }

        public void Control(bool shutdownRequest)
        {
            for (var i = 0; i < 4; i++)
                _menuHover[i] = Ui.IsHovering(_menu[i], !shutdownRequest);
            
            if (_menuHover[0] && IsMouseButtonPressed(0))
            {
                _board.Shuffle();
                _board.MenuOpen = false;
            }
            if (_menuHover[1] && IsMouseButtonPressed(0))
            {
                _board.AddKey();
                _board.MenuOpen = false;
            }
            if (_menuHover[2] && IsMouseButtonPressed(0))
            {
                _board.Restore();
                _board.MenuOpen = false;
            }
            if (_menuHover[3] && IsMouseButtonPressed(0))
            {
                _board.State = BoardState.Dice;
                _board.MenuOpen = false;
            }
        }

        public void Dispose()
        {
            UnloadTexture(_menuBack);
            UnloadTexture(_menuButtons);
            GC.SuppressFinalize(this);
        }
    }
}