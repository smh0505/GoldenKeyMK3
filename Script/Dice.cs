namespace GoldenKeyMK3.Script
{
    public class Dice : IGameObject
    {
        public Dice()
        {
            
        }
        
        public void Draw()
        {
            // Waiting for Phase 2
        }

        public void Control()
        {
            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
