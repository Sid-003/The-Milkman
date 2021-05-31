using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace The_Milkman.Services
{
    class VoiceRecognitionService
    {
        private const int MaxRetries = 10;

        private TcpClient _client;

        public event Func<string, Task> MessageReceived;

        private StreamReader _reader;

        private int _retries;

        public VoiceRecognitionService()
        {
        }

        public async Task ConnectAsync()
        {
            try
            {
                _client?.Dispose();
                _client = new TcpClient();
                await _client.ConnectAsync("localhost", 1337);
                _reader = new StreamReader(_client.GetStream());
            }
            catch (SocketException e)
            {
                if (_retries > MaxRetries)
                    throw new Exception("death");

                _retries++;

                await ConnectAsync();
            }
        }

        public async Task StartReceiveAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                string msg = await _reader.ReadLineAsync();

                if (string.IsNullOrEmpty(msg))
                    continue;

                await MessageReceived(msg);
            }
        }
    }
}