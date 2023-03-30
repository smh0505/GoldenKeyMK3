using GoldenKeyMK3.Script;

namespace GoldenKeyMK3
{
    public static class Program
    {
        private static readonly Game Game = new();
        
        public static void Main()
        {
            Console.CancelKeyPress += OnConsoleExit;
            Game.Run();
        }

        private static void OnConsoleExit(object sender, ConsoleCancelEventArgs e)
        {
            Game.Dispose();
            Environment.Exit(0);
        }
    }
}