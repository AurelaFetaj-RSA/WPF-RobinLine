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

            _client.Security.AutoAcceptUntrustedCertificates = true;
            _client.CertificateValidationFailed += HandleCertificateValidationFailed;
        }

        private void HandleCertificateValidationFailed(object sender, OpcCertificateValidationFailedEventArgs e)
        {
            e.Accept = true;
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
                return;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"Connection Error: {ex.Message}");
                //_logger.LogError(ex, "Connection error.");
            }
        }

        //public async Task ConnectAsync(int retryCount = 3, int delayMs = 2000)
        //{
        //    if (_isConnected) return;

        //    for (int attempt = 1; attempt <= retryCount; attempt++)
        //    {
        //        try
        //        {
        //            _client.Connect();
        //            _isConnected = true; 
        //            Console.WriteLine("Connected to OPC UA Server");
        //            return;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Connection attempt {attempt} failed: {ex.Message}");
        //            if (attempt < retryCount) await Task.Delay(delayMs);
        //        }
        //    }

        //    _isConnected = false;
        //    Console.WriteLine("Failed to connect to OPC UA Server after multiple attempts.");
        //}


        public async Task<bool> ReadBooleanAsync(string nodeId)
        {
            await ConnectAsync();
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
            await ConnectAsync();
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
            await ConnectAsync();
            if (!_isConnected) return;

            try
            {
                var opcStatus = await Task.Run(() => _client.WriteNode(nodeId, value));

                if(opcStatus != OpcStatusCode.Good)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing boolean to {nodeId}: {ex.Message}");
                //_logger.LogError(ex, "Error writing boolean to {NodeId}", nodeId);
            }
        }

        public async Task WriteIntegerAsync(string nodeId, int value)
        {
            await ConnectAsync();
            if (!_isConnected) return;

            try
            {
                await Task.FromResult(() => _client.WriteNode(nodeId, value));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing integer to {nodeId}: {ex.Message}");
                //_logger.LogError(ex, "Error writing integer to {NodeId}", nodeId);
            }
        }

        public async Task<T[]> ReadArrayAsync<T>(string nodeId)
        {
            await ConnectAsync();
            if (!_isConnected) return Array.Empty<T>();

            try
            {
                return await Task.Run(() =>
                {
                    var result = _client.ReadNode(nodeId).Value;

                    if (result is T[] arrayResult)
                    {
                        return arrayResult;
                    }

                    if (result is IEnumerable<object> objectArray)
                    {
                        return objectArray.Select(item => (T)Convert.ChangeType(item, typeof(T))).ToArray();
                    }

                    throw new InvalidCastException($"Unable to convert OPC UA value to an array of {typeof(T).Name}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading array from {nodeId}: {ex.Message}");
                return Array.Empty<T>(); // Return an empty array in case of failure
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
