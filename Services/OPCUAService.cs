using Microsoft.Extensions.Logging;
using Opc.UaFx;
using Opc.UaFx.Client;
using WPF_RobinLine.Services;

namespace WPF_App.Services
{
    public class OpcUaClientService : IDisposable
    {
        private OpcClient _client; // Change OpcAdvancedClient to OpcClient
        private const string ServerUrl = "opc.tcp://172.31.20.101:48011";
        private bool _isConnected = false; // Track connection status manually
        //private readonly ILogger<OpcUaClientService> _logger;
        protected ILicenseInfo _opcLicenseInfo { get; set; } = null;

        public OpcUaClientService()
        {
            //_logger = logger;

            if (OpcClientKey.Key != string.Empty)
            {
                Opc.UaFx.Client.Licenser.LicenseKey = OpcClientKey.Key;
                _opcLicenseInfo = Opc.UaFx.Client.Licenser.LicenseInfo;
            }

            //if (OpcServerKey.Key != string.Empty)
            //{
            //    Opc.UaFx.Server.Licenser.LicenseKey = OpcServerKey.Key;
            //    _opcLicenseInfo = Opc.UaFx.Client.Licenser.LicenseInfo;
            //}

            // Initialize the OPC Client
            _client = new OpcClient(ServerUrl); 
        }

        public async Task ConnectAsync()
        {
            if (_isConnected) return;

            try
            {
                await Task.Run(() =>
                {
                    _client.Connect();
                    _isConnected = true;
                });

                Console.WriteLine("Connected to OPC UA Server");
                //_logger.LogInformation("Connected to OPC UA Server.");
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"Connection Error: {ex.Message}");
                //_logger.LogError(ex, "Connection error.");
            }
        }

        public async Task<bool> ReadBooleanAsync(string nodeId)
        {
            if (!_isConnected) return false;

            try
            {
                return await Task.Run(() =>
                {
                    var result = _client.ReadNode(nodeId).Value;
                    return Convert.ToBoolean(result);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading boolean from {nodeId}: {ex.Message}");
                //_logger.LogError(ex, "Error reading boolean from {NodeId}", nodeId);
                return false; // Return a default value in case of failure
            }
        }

        public async Task<int> ReadIntegerAsync(string nodeId)
        {
            if (!_isConnected) return 0;

            try
            {
                return await Task.Run(() =>
                {
                    var result = _client.ReadNode(nodeId).Value;
                    return Convert.ToInt32(result);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading integer from {nodeId}: {ex.Message}");
                //_logger.LogError(ex, "Error reading integer from {NodeId}", nodeId);
                return 0;
            }
        }

        public async Task WriteBooleanAsync(string nodeId, bool value)
        {
            if (!_isConnected) return;

            try
            {
                await Task.Run(() => _client.WriteNode(nodeId, value));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing boolean to {nodeId}: {ex.Message}");
                //_logger.LogError(ex, "Error writing boolean to {NodeId}", nodeId);
            }
        }

        public async Task WriteIntegerAsync(string nodeId, int value)
        {
            if (!_isConnected) return;

            try
            {
                await Task.Run(() => _client.WriteNode(nodeId, value));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing integer to {nodeId}: {ex.Message}");
                //_logger.LogError(ex, "Error writing integer to {NodeId}", nodeId);
            }
        }

        public void Disconnect()
        {
            if (_isConnected && _client != null)
            {
                _client.Disconnect();
                _isConnected = false;
                Console.WriteLine("Disconnected from OPC UA Server");
                //_logger.LogInformation("Disconnected from OPC UA Server.");
            }
        }


        public void Dispose()
        {
            if (_client != null)
            {
                _client.Disconnect();
                _client.Dispose();
            }
        }

    }
}
