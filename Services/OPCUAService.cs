//using OPC.FX.Advanced;
//using OPC.FX.Advanced.Client;
//using System.Net.Sockets;

//namespace WPF_App.Services
//{
//    public class OPCUAService
//    {
//        private UaClient _client;

//        public async Task ConnectAsync(string endpointUrl)
//        {
//            _client = new UaClient();
//            _client.EndpointUrl = endpointUrl;

//            try
//            {
//                await _client.ConnectAsync();
//                Console.WriteLine("Connected to OPC UA server.");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error connecting to OPC UA server: {ex.Message}");
//            }
//        }

//        public async Task DisconnectAsync()
//        {
//            if (_client != null)
//            {
//                await _client.DisconnectAsync();
//                Console.WriteLine("Disconnected from OPC UA server.");
//            }
//        }
//    }
//}
