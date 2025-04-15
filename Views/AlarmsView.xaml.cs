using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WPF_App.Services;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using static WPF_App.Views.AutomaticView;
using WPF_App.ViewModels;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for AlarmsView.xaml
    /// </summary>
    public partial class AlarmsView : UserControl
    {
        private readonly OpcUaClientService _opcUaClient = new OpcUaClientService();
        private DispatcherTimer _messageTimer = new DispatcherTimer();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Dictionary<Border, int> squareBitMapping;

        //private bool _robot1Alarm;
        //private bool _oven1Alarm;
        //private bool _robot2Alarm;
        //private bool _oven2Alarm;
        //private bool _inputBeltAlarm;
        //private bool _centralBeltAlarm;
        //private bool _robot1BeltAlarm;
        //private bool _robot2BeltAlarm;

        private bool _robot1ConnectedAlarm;
        private bool _oven1ConnectedAlarm;
        private bool _robot2ConnectedAlarm;
        private bool _oven2ConnectedAlarm;
        private bool _inputBeltAlarm;
        private bool _centralBeltAlarm;
        private bool _robot1BeltAlarm;
        private bool _robot2BeltAlarm;
        private bool _robot1NotReadyAlarm;
        private bool _robot2NotReadyAlarm;
        private bool _robot1TankLvlLowAlarm;
        private bool _robot1TankLvlEmptyAlarm;
        private bool _robot2TankLvlLowAlarm;
        private bool _robot2TankLvlEmptyAlarm;
        private bool _robot1ReadyAlarm;
        private bool _robot2ReadyAlarm;
        private bool _robot1AirPressureAlarm;
        private bool _robot2AirPressureAlarm;

        public bool Robot1ConnectedAlarm
        {
            get => _robot1ConnectedAlarm;
            set { _robot1ConnectedAlarm = value; OnPropertyChanged(nameof(Robot1ConnectedAlarm)); }
        }

        public bool Oven1ConnectedAlarm
        {
            get => _oven1ConnectedAlarm;
            set { _oven1ConnectedAlarm = value; OnPropertyChanged(nameof(Oven1ConnectedAlarm)); }
        }

        public bool Robot2ConnectedAlarm
        {
            get => _robot2ConnectedAlarm;
            set { _robot2ConnectedAlarm = value; OnPropertyChanged(nameof(Robot2ConnectedAlarm)); }
        }

        public bool Oven2ConnectedAlarm
        {
            get => _oven2ConnectedAlarm;
            set { _oven2ConnectedAlarm = value; OnPropertyChanged(nameof(Oven2ConnectedAlarm)); }
        }

        public bool InputBeltAlarm
        {
            get => _inputBeltAlarm;
            set { _inputBeltAlarm = value; OnPropertyChanged(nameof(InputBeltAlarm)); }
        }

        public bool CentralBeltAlarm
        {
            get => _centralBeltAlarm;
            set { _centralBeltAlarm = value; OnPropertyChanged(nameof(CentralBeltAlarm)); }
        }

        public bool Robot1BeltAlarm
        {
            get => _robot1BeltAlarm;
            set { _robot1BeltAlarm = value; OnPropertyChanged(nameof(Robot1BeltAlarm)); }
        }

        public bool Robot2BeltAlarm
        {
            get => _robot2BeltAlarm;
            set { _robot2BeltAlarm = value; OnPropertyChanged(nameof(Robot2BeltAlarm)); }
        }

        public bool Robot1NotReadyAlarm
        {
            get => _robot1NotReadyAlarm;
            set { _robot1NotReadyAlarm = value; OnPropertyChanged(nameof(Robot1NotReadyAlarm)); }
        }

        public bool Robot2NotReadyAlarm
        {
            get => _robot2NotReadyAlarm;
            set { _robot2NotReadyAlarm = value; OnPropertyChanged(nameof(Robot2NotReadyAlarm)); }
        }

        public bool Robot1TankLvlLowAlarm
        {
            get => _robot1TankLvlLowAlarm;
            set { _robot1TankLvlLowAlarm = value; OnPropertyChanged(nameof(Robot1TankLvlLowAlarm)); }
        }

        public bool Robot1TankLvlEmptyAlarm
        {
            get => _robot1TankLvlEmptyAlarm;
            set { _robot1TankLvlEmptyAlarm = value; OnPropertyChanged(nameof(Robot1TankLvlEmptyAlarm)); }
        }

        public bool Robot2TankLvlLowAlarm
        {
            get => _robot2TankLvlLowAlarm;
            set { _robot2TankLvlLowAlarm = value; OnPropertyChanged(nameof(Robot2TankLvlLowAlarm)); }
        }

        public bool Robot2TankLvlEmptyAlarm
        {
            get => _robot2TankLvlEmptyAlarm;
            set { _robot2TankLvlEmptyAlarm = value; OnPropertyChanged(nameof(Robot2TankLvlEmptyAlarm)); }
        }

        public bool Robot1ReadyAlarm
        {
            get => _robot1ReadyAlarm;
            set { _robot1ReadyAlarm = value; OnPropertyChanged(nameof(Robot1ReadyAlarm)); }
        }

        public bool Robot2ReadyAlarm
        {
            get => _robot2ReadyAlarm;
            set { _robot2ReadyAlarm = value; OnPropertyChanged(nameof(Robot2ReadyAlarm)); }
        }

        public bool Robot1AirPressureAlarm
        {
            get => _robot1AirPressureAlarm;
            set { _robot1AirPressureAlarm = value; OnPropertyChanged(nameof(Robot1AirPressureAlarm)); }
        }

        public bool Robot2AirPressureAlarm
        {
            get => _robot2AirPressureAlarm;
            set { _robot2AirPressureAlarm = value; OnPropertyChanged(nameof(Robot2AirPressureAlarm)); }
        }

        public AlarmsView()
        {
            InitializeComponent();
            InitializeSquareBitMapping();

            DataContext = this;

            _opcUaClient = new OpcUaClientService();
            _messageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (DataContext is MainViewModel vm)
                //    vm.IsLoading = true;

                await _opcUaClient.InitializeAsync();
                await _opcUaClient.ConnectAsync("opc.tcp://172.31.40.130:48010");
                //await _opcUaClient.ConnectAsync("opc.tcp://192.31.30.40:48010");

                //await _opcUaClient.SubscribeToNodesAsync();
                await _opcUaClient.SubscribeToNodeAsync("GeneralAlarms", OnOpcValueChanged);

                //_opcUaClient.ValueUpdated += OnOpcValueChanged;
                //StartMonitoringTasks();
            }
            catch (Exception ex)
            {
                //ShowMessage($"Initialization failed: {ex.Message}", MessageType.Error);
                ShowMessage($"Initialization failed", MessageType.Error);
            }
            finally
            {
                //if (DataContext is MainViewModel vm)
                //    vm.IsLoading = false;
            }
        }

        private void InitializeSquareBitMapping()
        {
            squareBitMapping = new Dictionary<Border, int>
            {
                // Word 1 (bits 0-15) - Position 1 in array
                { Robot1ConnectionSquare, 0 },   // A1
                { Oven1ConnectionSquare, 1 },    // A2
                { Robot2ConnectionSquare, 2 },   // A3
                { Oven2ConnectionSquare, 3 },    // A4
                { InputBeltSquare, 4 },          // A5
                { CentralBeltSquare, 5 },        // A6
                { Robot1BeltSquare, 6 },         // A7
                { Robot2BeltSquare, 7 },         // A8
                { Robot1TankLvlLowSquare, 8 },   // A9
                { Robot1TankLvlEmptySquare, 9 }, // A10
                { Robot2TankLvlLowSquare, 11 },  // A12
                { Robot2TankLvlEmptySquare, 12 },// A13
                { Robot1ReadySquare, 13 },       // A14
                { Robot2ReadySquare, 14 },       // A15
                { Robot1AirPressureSquare, 15 }, // A16
                
                // Word 2 (bits 16-31) - Position 2 in array
                { Robot2AirPressureSquare, 16 }, // A17
                { Robot1AutomaticSquare, 17 },   // A18
                { Robot2AutomaticSquare, 18 },   // A19
                { Oven1AutomaticSquare, 19 },    // A20
                { Oven2AutomaticSquare, 20 },    // A21
                // A22-A32 would go here if needed
                
                // Word 3 (bits 32-47) - Position 3 in array
                { Oven1MotorBeltSquare, 32 },    // A33
                { Oven1Fan1InverterSquare, 33 }, // A34
                { Oven1Fan2InverterSquare, 34 }, // A35
                { Oven1LampStopSquare, 35 },     // A36
                // A37-A47 would go here if needed
                
                // Word 4 (bits 48-63) - Position 4 in array
                { Oven2MotorBeltSquare, 48 },    // A49
                { Oven2Fan1InverterSquare, 49 }, // A50
                { Oven2Fan2InverterSquare, 50 }, // A51
                { Oven2LampStopSquare, 51 }      // A52
                // A53-A63 would go here if needed
            };
        }

        private void OnOpcValueChanged(string nodeId, object value)
        {
            if (nodeId != "GeneralAlarms") return;

            Dispatcher.Invoke(() =>
            {
                int[] testArray;

                if (value is ushort[] ushortArray)
                    testArray = ushortArray.Select(x => (int)x).ToArray();
                else if (value is int[] intArray)
                    testArray = intArray;
                else
                    return;

                if (testArray.Length >= 5)
                {
                    // Position 1 (A1-A16)
                    int word1 = testArray[1];
                    string binaryWord1 = Convert.ToString(word1, 2).PadLeft(16, '0');
                    UpdateSquaresFromBits(binaryWord1, 0);

                    // Position 2 (A17-A32)
                    int word2 = testArray[2];
                    string binaryWord2 = Convert.ToString(word2, 2).PadLeft(16, '0');
                    UpdateSquaresFromBits(binaryWord2, 16);

                    // Position 3 (A33-A48)
                    int word3 = testArray[3];
                    string binaryWord3 = Convert.ToString(word3, 2).PadLeft(16, '0');
                    UpdateSquaresFromBits(binaryWord3, 32);

                    // Position 4 (A49-A64)
                    int word4 = testArray[4];
                    string binaryWord4 = Convert.ToString(word4, 2).PadLeft(16, '0');
                    UpdateSquaresFromBits(binaryWord4, 48);
                }
            });
        }
        private void StartMonitoringTasks()
        {
            // Only need tasks for things not covered by subscriptions
            _ = MonitorGeneralAlarms();
        }

        //private async Task MonitorGeneralAlarms()
        //{
        //    while (!_cts.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            if (_opcUaClient == null) return;

        //            //var generalAlarmsArray = await _opcUaClient.ReadNodeAsync("GeneralAlarms") as int[];

        //            int[] testArray = { 0, 24576, 0, 8, 8 };


        //            if (testArray?.Length > 0)
        //            {

        //                    // Clear previous output
        //                    DebugOutputTextBlock.Text = "";
        //                    int word1 = testArray[1];

        //                    // Display each array item as byte
        //                    for (int i = 0; i < testArray.Length; i++)
        //                    {
        //                        int intValue = testArray[i];
        //                        byte byteValue = (byte)intValue;
        //                        //byte value = (byte)testArray[i];

        //                        string binary16 = Convert.ToString(intValue, 2).PadLeft(16, '0');
        //                        //DebugOutputTextBlock.Text += $"Array[{i}]: {Convert.ToString(value, 2).PadLeft(16, '0')} ({value})\n";
        //                        DebugOutputTextBlock.Text +=
        //                            $"Position {i}: " +
        //                            $"Int={intValue}, " +
        //                            $"Byte: {byteValue}, " +
        //                            $"16-bit Binary={binary16} \n";

        //                        foreach (var entry in squareBitMapping)
        //                        {
        //                            Border square = entry.Key;
        //                            int bitPosition = entry.Value;

        //                            char bit = binary16[15 - bitPosition]; // Convert to LSB-first

        //                            square.Background = (bit == '1')
        //                                ? new SolidColorBrush(Colors.Red)
        //                                : new SolidColorBrush(Colors.Blue);
        //                        }
        //                    }

        //                    //Robot1ConnectedAlarm = (word1 & (1 << 0)) != 0;  // A1 (bit 0)
        //                    //Oven1ConnectedAlarm = (word1 & (1 << 1)) != 0;    // A2 (bit 1)
        //                    //Robot2ConnectedAlarm = (word1 & (1 << 2)) != 0;    // A3 (bit 2)
        //                    //Oven2ConnectedAlarm = (word1 & (1 << 3)) != 0;     // A4 (bit 3)
        //                    //InputBeltAlarm = (word1 & (1 << 4)) != 0;         // A5 (bit 4)
        //                    //CentralBeltAlarm = (word1 & (1 << 5)) != 0;       // A6 (bit 5)
        //                    //Robot1BeltAlarm = (word1 & (1 << 6)) != 0;        // A7 (bit 6)
        //                    //Robot2BeltAlarm = (word1 & (1 << 7)) != 0;        // A8 (bit 7)
        //                    //Robot1NotReadyAlarm = (word1 & (1 << 13)) != 0;   // A14 (bit 13)
        //                    //Robot2NotReadyAlarm = (word1 & (1 << 14)) != 0;   // A15 (bit 14)

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ShowMessage($"Selector monitoring error: {ex.Message}", MessageType.Error);
        //            await Task.Delay(5000, _cts.Token); // Longer delay after error
        //        }

        //        await Task.Delay(1000, _cts.Token);
        //    }
        //}

        private async Task MonitorGeneralAlarms()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    if (_opcUaClient == null) return;

                    var generalArray = await _opcUaClient.ReadNodeAsync("GeneralAlarms");
                    //int[] testArray = { 0, 24576, 0, 8, 8 }; // Test data
                    //int[] testArray = { 0, 32768, 16, 4, 16 }; // Test data

                    if (generalArray is ushort[] ushortArray)
                    {
                        var testArray = ushortArray.Select(x => (int)x).ToArray();

                        if (testArray?.Length >= 5)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                //DebugOutputTextBlock.Text = "Alarm Status:\n";

                                // Process each position in the array
                                for (int i = 1; i < testArray.Length; i++) // Skip position 0
                                {
                                    int wordValue = testArray[i];
                                    string binary16 = Convert.ToString(wordValue, 2).PadLeft(16, '0');
                                    //DebugOutputTextBlock.Text += $"Position {i}: {binary16} ({wordValue})\n";
                                }

                                // Position 1 (A1-A16)
                                int word1 = testArray[1];
                                string binaryWord1 = Convert.ToString(word1, 2).PadLeft(16, '0');
                                UpdateSquaresFromBits(binaryWord1, 0); // Bits 0-15

                                // Position 2 (A17-A32)
                                int word2 = testArray[2];
                                string binaryWord2 = Convert.ToString(word2, 2).PadLeft(16, '0');
                                UpdateSquaresFromBits(binaryWord2, 16); // Bits 16-31

                                // Position 3 (A33-A48)
                                int word3 = testArray[3];
                                string binaryWord3 = Convert.ToString(word3, 2).PadLeft(16, '0');
                                UpdateSquaresFromBits(binaryWord3, 32); // Bits 32-47

                                // Position 4 (A49-A64)
                                int word4 = testArray[4];
                                string binaryWord4 = Convert.ToString(word4, 2).PadLeft(16, '0');
                                UpdateSquaresFromBits(binaryWord4, 48); // Bits 48-63
                            });
                        }
                    }
                    else if (generalArray is int[] intArray)
                    {
                        var testArray = intArray; // Already correct type
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Monitoring error: {ex.Message}", MessageType.Error);
                    //await Task.Delay(5000, _cts.Token);
                }

                //await Task.Delay(1000, _cts.Token);
            }
        }

        private void UpdateSquaresFromBits(string binaryWord, int bitOffset)
        {
            foreach (var entry in squareBitMapping)
            {
                Border square = entry.Key;
                int bitPosition = entry.Value;

                // Check if this bit is in the current word
                if (bitPosition >= bitOffset && bitPosition < bitOffset + 16)
                {
                    int relativeBitPos = bitPosition - bitOffset;
                    char bit = binaryWord[15 - relativeBitPos]; // MSB is index 0

                    Color blueColor = (Color)ColorConverter.ConvertFromString("#3f80cb");
                    SolidColorBrush blueBrush = new SolidColorBrush(blueColor);

                    square.Background = (bit == '1')
                        ? new SolidColorBrush(Colors.Red)
                        : blueBrush;
                }
            }
        }

        private void ShowMessage(string message, MessageType messageType)
        {
            MessageText.Text = message;

            // Set colors and icons based on message type
            switch (messageType)
            {
                case MessageType.Success:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DFF0D8")); // Light Green
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C763D")); // Dark Green
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C763D")); // Dark Green Border
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-success-100.png"));
                    break;

                case MessageType.Error:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2DEDE")); // Light Red
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A94442")); // Dark Red
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A94442")); // Dark Red Border
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-error-96.png"));
                    break;

                case MessageType.Warning:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCF8E3")); // Light Yellow
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A6D3B")); // Dark Yellow
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A6D3B")); // Dark Yellow Border
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-warning-96.png"));
                    break;

                case MessageType.Info:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9EDF7")); // Light Blue
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#31708F")); // Dark Blue
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#31708F")); // Dark Blue Border
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-info-104.png"));
                    break;
            }

            MessageBoxPanel.BorderThickness = new Thickness(2);
            MessageBoxPanel.Visibility = Visibility.Visible;

            // Auto-hide the message after 3 seconds
            _messageTimer.Interval = TimeSpan.FromSeconds(3);
            _messageTimer.Tick += (s, e) => HideMessage();
            _messageTimer.Start();
        }

        // Hide Message Function
        private void HideMessage()
        {
            MessageBoxPanel.Visibility = Visibility.Collapsed;
            _messageTimer.Stop();
        }

        private void CloseMessageBox(object sender, RoutedEventArgs e)
        {
            HideMessage();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_opcUaClient != null)
                _opcUaClient.ValueUpdated -= OnOpcValueChanged;

            Dispose();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _messageTimer.Stop();
            _opcUaClient?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
