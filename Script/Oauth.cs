using System.Diagnostics;

namespace GoldenKeyMK3.Script
{
    public class Oauth
    {
        public static string AccessToken;
        public static string RefreshToken;

        public static void Open()
        {
            var proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "https://www.google.com"
            };
            Process.Start(proc);
        }

        public static void RefreshAccessToken()
        {

        }

        public static void GetAccessToken()
        {

        }
    }
}