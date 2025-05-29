using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public static class TcpServerHelper
{
    private static readonly List<ClientInfo> _clients = new List<ClientInfo>();
    private static readonly object _clientsLock = new object();
    private static TcpListener _listener;
    private static CancellationTokenSource _cts;
    private static bool _isRunning;

    public static event Action<string> LogMessage = Console.WriteLine;

    public static async Task StartAsync(int port = 9999)
    {
        if (_isRunning) return;

        _isRunning = true;
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _listener.Start();

        LogMessage?.Invoke($"Server started on port {port}");

        try
        {
            while (_isRunning)
            {
                var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                var clientInfo = new ClientInfo(client);
                //AddClient(clientInfo);
                _ = HandleClientAsync(client, _cts.Token);
            }
        }
        catch (OperationCanceledException) { /* Shutdown */ }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"Server error: {ex}");
        }
    }

    public static bool SendCommandToRobot(IPAddress ipAddress, string command, int port)
    {
        lock (_clientsLock)
        {
            var client = _clients.Find(c =>
            {
                var endpoint = c.TcpClient.Client.RemoteEndPoint as IPEndPoint;
                return endpoint != null &&
                       endpoint.Address.Equals(ipAddress);
            });

            //if (client == null || !client.TcpClient.Connected)
            if (client == null)
            {
                LogMessage?.Invoke($"Robot {ipAddress}:{port} not found or disconnected");
                return false;
            }

            try
            {
                //if (client.LastSentCommand == command)
                //{
                //    LogMessage?.Invoke($"Command already sent to {ipAddress}:{port}");
                //    return true;
                //}

                var bytes = Encoding.ASCII.GetBytes(command);
                client.TcpClient.GetStream().Write(bytes, 0, bytes.Length);
                client.LastSentCommand = command;

                LogMessage?.Invoke($"Command sent to {ipAddress}:{port}: {command}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Failed to send to {ipAddress}:{port}: {ex.Message}");
                RemoveClient(client); // Auto-cleanup
                return false;
            }
        }
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        // Validate connection immediately
        if (client == null || !client.Connected)
        {
            LogMessage?.Invoke("Invalid client connection");
            client?.Dispose();
            return;
        }

        var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        LogMessage?.Invoke($"Handling client: {endpoint}");

        // Create and add client info
        var clientInfo = new ClientInfo(client);
        AddClient(clientInfo);  // Add to managed list
        //return;
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var buffer = new byte[4096];
                stream.ReadTimeout = 5000; // 5s timeout

                while (!ct.IsCancellationRequested && client.Connected)
                {
                    try
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                        if (bytesRead == 0) break; // Graceful disconnect

                        //clientInfo.LastActivity = DateTime.UtcNow; // Update activity
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        LogMessage?.Invoke($"Received from {endpoint}: {message}");

                        // Process message here
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException)
                    {
                        LogMessage?.Invoke($"Client {endpoint} disconnected abruptly: {ex.Message}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.Invoke($"Error with client {endpoint}: {ex.Message}");
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"Fatal error with client {endpoint}: {ex}");
        }
        finally
        {
            try
            {
                RemoveClient(clientInfo);  // Remove from managed list
                LogMessage?.Invoke($"Client cleanup complete: {endpoint}");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Error during client cleanup: {ex}");
            }
        }
    }

    private static void AddClient(ClientInfo client)
    {
        lock (_clientsLock) _clients.Add(client);
    }

    private static void RemoveClient(ClientInfo client)
    {
        lock (_clientsLock)
        {
            _clients.Remove(client);
            client.TcpClient.Dispose();
        }
    }

    public static void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cts?.Cancel();
        _listener?.Stop();

        lock (_clientsLock)
        {
            foreach (var client in _clients)
                client.TcpClient.Dispose();
            _clients.Clear();
        }
    }

    public static void Dispose() => Stop();

    private class ClientInfo
    {
        public TcpClient TcpClient { get; }
        public string LastSentCommand { get; set; }
        public bool IsAutoMode { get; set; }

        public ClientInfo(TcpClient client) => TcpClient = client;
    }
}