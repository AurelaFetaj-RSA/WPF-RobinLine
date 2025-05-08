using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace WPF_RobinLine.Services
{
    public class TcpClientHelper : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _ipAddress;
        private readonly int _port;
        private CancellationTokenSource _cts;
        private bool _isConnected;

        public event EventHandler<bool> ConnectionStatusChanged;
        public event EventHandler<string> MessageReceived;

        public TcpClientHelper(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _cts = new CancellationTokenSource();
        }

        public async Task ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_ipAddress, _port).ConfigureAwait(false);
                _stream = _client.GetStream();
                _isConnected = true;

                // Raise event on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                    ConnectionStatusChanged?.Invoke(this, true));

                _ = Task.Run(() => ListenForMessagesAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Application.Current.Dispatcher.Invoke(() =>
                    ConnectionStatusChanged?.Invoke(this, false));
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken ct)
        {
            byte[] buffer = new byte[1024];
            while (!ct.IsCancellationRequested && _isConnected)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Application.Current.Dispatcher.Invoke(() =>
                            MessageReceived?.Invoke(this, message));
                    }
                }
                catch
                {
                    _isConnected = false;
                    Application.Current.Dispatcher.Invoke(() =>
                        ConnectionStatusChanged?.Invoke(this, false));
                    break;
                }
            }
        }

        public async Task SendCommandAsync(string command)
        {
            if (!_isConnected) return;
            byte[] data = Encoding.ASCII.GetBytes(command);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public void Disconnect()
        {
            _cts.Cancel();
            _stream?.Close();
            _client?.Close();
            _isConnected = false;
        }

        public void Dispose() => Disconnect();
    }
}
