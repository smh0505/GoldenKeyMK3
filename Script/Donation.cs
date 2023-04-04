using System.Text.RegularExpressions;
using Websocket.Client;

namespace GoldenKeyMK3.Script
{
    public class Donation : IDisposable
    {
        private readonly ManualResetEvent _exitEvent;
        private WebsocketClient _client;

        private readonly Wheel _wheel;
        
        public Donation(Wheel wheel)
        {
            _exitEvent = new ManualResetEvent(false);
            _wheel = wheel;
        }

        public async void Connect(string payload)
        {
            using (_client = new WebsocketClient(new Uri("wss://toon.at:8071/" + payload)))
            {
                _client.MessageReceived.Subscribe(msg =>
                {
                    if (!msg.ToString().Contains("roulette")) return;
                    var roulette = Regex.Match(msg.ToString(), "\"message\":\"[^\"]* - [^\"]*\"").Value[10..];
                    var rValue = roulette.Split('-', 2)[1].Replace("\"", "")[1..];
                    if (rValue != "꽝") _wheel.WaitList = _wheel.WaitList.Add(rValue);
                });
                await _client.Start();
                _exitEvent.WaitOne();
            }
        }

        public void Dispose()
        {
            _exitEvent.Set();
            GC.SuppressFinalize(this);
        }
    }
}