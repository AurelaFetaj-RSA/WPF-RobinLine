using Opc.Ua;
using Opc.Ua.Client;
using System.Net.Sockets;
using System;

namespace WPF_App.Services
{
    public class OpcUaConfigService
    {
        public string ServerAddress { get; set; } = "opc.tcp://127.0.0.1:4840"; // Default
    }

    public class OpcUaClientService : IDisposable
    {
        private ApplicationConfiguration _config;
        private Session _session;
        private Subscription _subscription;
        private bool _disposed;
        private readonly OpcConfiguration _configuration;

        public event Action<string> ConnectionStatusChanged;
        public event Action<string, object> ValueUpdated;

        public bool IsConnected => _session?.Connected == true;

        public OpcUaClientService()
        {
            _configuration = new RobinLineOpcConfiguration();
            if (_configuration == null)
                throw new InvalidOperationException("Failed to create default configuration");
        }

        public OpcUaClientService(OpcConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task InitializeAsync()
        {
            try
            {
                _config = new ApplicationConfiguration()
                {
                    ApplicationName = "WPF-RobinLine",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        AutoAcceptUntrustedCertificates = false, // Disable auto-accept (handle manually)
                        RejectSHA1SignedCertificates = true,
                        MinimumCertificateKeySize = 2048,
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = CertificateStoreType.X509Store,
                            StorePath = "CurrentUser\\My",
                            SubjectName = "CN=WPF-RobinLine"
                        }
                    },
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
                };

                // Set up certificate validation
                _config.CertificateValidator = new CertificateValidator();
                _config.CertificateValidator.CertificateValidation += HandleCertificateValidationFailed;

                await _config.Validate(ApplicationType.Client);
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke($"Initialization failed: {ex.Message}");
                throw;
            }
        }

        private void HandleCertificateValidationFailed(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            // Auto-accept untrusted certificates (for testing only - disable in production)
            e.Accept = true;

            // Log the certificate details
            ConnectionStatusChanged?.Invoke($"Certificate validation: Subject={e.Certificate.Subject}, Thumbprint={e.Certificate.Thumbprint}");

            // Example: Only accept certificates from trusted issuers
            // if (e.Certificate.Subject.Contains("MyTrustedCA"))
            // {
            //     e.Accept = true;
            // }
        }


        public async Task ConnectAsync(string serverUrl)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
                throw new ArgumentException("Server URL cannot be empty");

            // 1. First validate URL format
            if (!serverUrl.StartsWith("opc.tcp://"))
                serverUrl = "opc.tcp://" + serverUrl;

            var uri = new Uri(serverUrl);

            // 2. Run TCP connectivity test
            try
            {
                Console.WriteLine($"Testing connection to {uri.Host}:{uri.Port}...");

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(uri.Host, uri.Port).WaitAsync(TimeSpan.FromSeconds(5));

                Console.WriteLine("TCP connection successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP connection failed: {ex.Message}");
                throw new ServiceResultException(
                    StatusCodes.BadConnectionRejected,
                    $"Cannot reach OPC UA server at {serverUrl}. Check:\n" +
                    $"1. Server is running\n" +
                    $"2. Firewall allows port {uri.Port}\n" +
                    $"3. Network connectivity exists");
            }

            // 3. Proceed with OPC UA endpoint discovery
            try
            {
                var endpoint = CoreClientUtils.SelectEndpoint(serverUrl, false, 10000);
                var endpointConfig = EndpointConfiguration.Create(_config);
                var endpointDescription = new ConfiguredEndpoint(null, endpoint, endpointConfig);

                _session = await Session.Create(
                    _config,
                    endpointDescription,
                    updateBeforeConnect: true,
                    sessionName: "WPF-RobinLine",
                    sessionTimeout: 60000,
                    identity: new UserIdentity(),
                    preferredLocales: null);

                ConnectionStatusChanged?.Invoke($"Connected to {serverUrl}");
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke($"Connection failed: {ex.Message}");
                throw;
            }
        }

        public async Task SubscribeToNodesAsync()
        {
            if (_session == null || !_session.Connected)
                throw new InvalidOperationException("Session not connected");

            if (_configuration?.Nodes == null || !_configuration.Nodes.Any())
            {
                ConnectionStatusChanged?.Invoke("No nodes configured for subscription");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    _subscription = new Subscription(_session.DefaultSubscription)
                    {
                        PublishingInterval = 1000,
                        Priority = 100
                    };

                    _session.AddSubscription(_subscription);
                    _subscription.Create();

                    foreach (var node in _configuration.Nodes)
                    {
                        if (node == null) continue;

                        var monitoredItem = new MonitoredItem(_subscription.DefaultItem)
                        {
                            StartNodeId = new NodeId(node.NodeId),
                            AttributeId = Attributes.Value,
                            SamplingInterval = 1000,
                            QueueSize = 10,
                            DiscardOldest = true
                        };

                        monitoredItem.Notification += (item, e) =>
                        {
                            try
                            {
                                var notification = e.NotificationValue as MonitoredItemNotification;
                                if (notification?.Value != null)
                                {
                                    ValueUpdated?.Invoke(node.Name, notification.Value.Value);
                                }
                            }
                            catch (Exception ex)
                            {
                                ConnectionStatusChanged?.Invoke($"Notification error for {node.Name}: {ex.Message}");
                            }
                        };

                        _subscription.AddItem(monitoredItem);
                    }

                    _subscription.ApplyChanges();
                }
                catch (Exception ex)
                {
                    ConnectionStatusChanged?.Invoke($"Subscription failed: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task SubscribeToNodeAsync(string nodeName, Action<string, object> onValueChanged)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                throw new ArgumentException("Node name cannot be empty");

            if (_configuration == null)
                throw new InvalidOperationException("Configuration not initialized");

            var nodeConfig = _configuration.GetNode(nodeName) ??
                throw new ArgumentException($"Node {nodeName} not found in configuration");

            if (_session == null || !_session.Connected)
                throw new InvalidOperationException("Session not connected");

            await Task.Run(() =>
            {
                try
                {
                    _subscription = new Subscription(_session.DefaultSubscription)
                    {
                        PublishingInterval = 1000,
                        Priority = 100
                    };

                    _session.AddSubscription(_subscription);
                    _subscription.Create();

                    var monitoredItem = new MonitoredItem(_subscription.DefaultItem)
                    {
                        StartNodeId = new NodeId(nodeConfig.NodeId),
                        AttributeId = Attributes.Value,
                        SamplingInterval = 1000,
                        QueueSize = 10,
                        DiscardOldest = true
                    };

                    monitoredItem.Notification += (item, e) =>
                    {
                        try
                        {
                            var notification = e.NotificationValue as MonitoredItemNotification;
                            if (notification?.Value != null)
                            {
                                onValueChanged?.Invoke(nodeName, notification.Value.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            ConnectionStatusChanged?.Invoke($"Notification error for {nodeName}: {ex.Message}");
                        }
                    };

                    _subscription.AddItem(monitoredItem);
                    _subscription.ApplyChanges();
                }
                catch (Exception ex)
                {
                    ConnectionStatusChanged?.Invoke($"Single node subscription failed: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task<object> ReadNodeAsync(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                throw new ArgumentException("Node name cannot be empty");

            if (_configuration == null)
                throw new InvalidOperationException("Configuration not initialized");

            var nodeConfig = _configuration.GetNode(nodeName) ??
                throw new ArgumentException($"Node {nodeName} not found in configuration");

            if (_session == null || !_session.Connected)
                throw new InvalidOperationException("Session not connected");

            return await Task.Run(() =>
            {
                try
                {
                    var nodeToRead = new ReadValueId
                    {
                        NodeId = new NodeId(nodeConfig.NodeId),
                        AttributeId = Attributes.Value
                    };

                    var response = _session.Read(
                        null,
                        0,
                        TimestampsToReturn.Both,
                        new ReadValueIdCollection { nodeToRead },
                        out var results,
                        out var diagnosticInfos);

                    if (StatusCode.IsBad(response.ServiceResult))
                        throw new ServiceResultException(response.ServiceResult);

                    if (results == null || results.Count == 0)
                        throw new ServiceResultException(StatusCodes.BadNoData);

                    return results[0].Value;
                }
                catch (Exception ex)
                {
                    ConnectionStatusChanged?.Invoke($"Read failed for {nodeName}: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task WriteNodeAsync(string nodeName, object value)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                throw new ArgumentException("Node name cannot be empty");

            if (_configuration == null)
                throw new InvalidOperationException("Configuration not initialized");

            var nodeConfig = _configuration.GetNode(nodeName) ??
                throw new ArgumentException($"Node '{nodeName}' not found in configuration");

            if (nodeConfig.IsReadOnly)
                throw new InvalidOperationException($"Node '{nodeName}' is read-only");

            if (_session == null || !_session.Connected)
                throw new InvalidOperationException("OPC UA session is not connected");

            await Task.Run(() =>
            {
                try
                {
                    object convertedValue = value;
                    if (value != null && value != DBNull.Value)
                    {
                        convertedValue = Convert.ChangeType(value, nodeConfig.DataType);
                    }

                    var nodeToWrite = new WriteValue
                    {
                        NodeId = new NodeId(nodeConfig.NodeId),
                        AttributeId = Attributes.Value,
                        Value = new DataValue(new Variant(convertedValue))
                    };

                    var response = _session.Write(
                        null,
                        new WriteValueCollection { nodeToWrite },
                        out var results,
                        out var diagnosticInfos);

                    if (StatusCode.IsBad(response.ServiceResult))
                        throw new ServiceResultException(response.ServiceResult);

                    if (results == null || results.Count == 0 || StatusCode.IsBad(results[0]))
                        throw new ServiceResultException(results?[0] ?? StatusCodes.BadUnknownResponse);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to write to node '{nodeName}': {ex.Message}", ex);
                }
            });
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _subscription?.Delete(false);
                _subscription?.Dispose();
                _subscription = null;

                _session?.Close();
                _session?.Dispose();
                _session = null;
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}