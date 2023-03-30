using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace GoldenKeyMK3.Script
{
    public class Close : IDisposable
    {
        private int _count;
        private readonly Texture2D _scene;
        private readonly Texture2D _cancel;

        private bool _cancelHover;
        private readonly Rectangle _cancelButton;

        public Close()
        {
            _count = 0;
            _scene = LoadTexture("Resource/close.png");
            _cancel = LoadTexture("Resource/return.png");
            
            _cancelHover = false;
            _cancelButton = new Rectangle(12, 912, 100, 100);
        }

        public void Draw()
        {
            DrawTexture(_scene, 0, 0, Color.WHITE);
            DrawTextEx(Ui.Galmuri36, $"종료까지 앞으로 {5 - _count}회", new Vector2(12, 140), 36, 0, Color.WHITE);

            var cancelColor = _cancelHover ? Color.GREEN : Fade(Color.GREEN, 0.7f);
            DrawRectangleRec(_cancelButton, cancelColor);
            DrawTexture(_cancel, 12, 912, Color.WHITE);
        }

        public void Dispose()
        {
            UnloadTexture(_scene);
            UnloadTexture(_cancel);
            GC.SuppressFinalize(this);
        }

        public void Control(ref bool shutdownRequest, out bool shutdownResponse)
        {
            if (Ui.IsHovering(_cancelButton, shutdownRequest))
            {
                _cancelHover = true;
                if (IsMouseButtonPressed(0)) shutdownRequest = false;
            }
            else _cancelHover = false;
            
            if (IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                shutdownRequest = !shutdownRequest;
                _count = 0;
            }

            if (!shutdownRequest) shutdownResponse = false;
            else
            {
                if (IsKeyPressed(KeyboardKey.KEY_SPACE)) _count++;
                shutdownResponse = _count >= 5;
            }
        }
    }
}