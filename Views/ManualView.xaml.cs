using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPF_App.Services;
using Xceed.Wpf.Toolkit;
using static WPF_App.Views.AutomaticView;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for ManualView.xaml
    /// </summary>
    public partial class ManualView : UserControl
    {
        private readonly OpcUaClientService _opcUaClient;
        private DispatcherTimer _messageTimer = new DispatcherTimer();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _allowToggle = true;
        private readonly OpcUaConfigService _config;

        public ManualView()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize with configuration
            var config = new RobinLineOpcConfiguration();
            _opcUaClient = new OpcUaClientService();
            _messageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

            _config = App.ServiceProvider.GetService<OpcUaConfigService>();
            _opcUaClient = App.ServiceProvider.GetService<OpcUaClientService>();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _opcUaClient.InitializeAsync();
                await _opcUaClient.ConnectAsync(_config.ServerAddress); // Use the shared address
                //await _opcUaClient.ConnectAsync("opc.tcp://172.31.40.130:48010");
                //await _opcUaClient.ConnectAsync("opc.tcp://192.31.30.40:48010");
                await _opcUaClient.SubscribeToNodesAsync();

                //_opcUaClient.ValueUpdated += OnOpcValueChanged;
                //StartMonitoringTasks();
            }
            catch (Exception ex)
            {
                //ShowMessage($"Initialization failed: {ex.Message}", MessageType.Error);
                ShowMessage($"Initialization failed", MessageType.Error);
            }
        }

        public async void Oven1LampsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                Oven1LampsToggle.IsChecked = !Oven1LampsToggle.IsChecked;
                return;
            }
            try
            {
                bool isChecked = (bool)Oven1LampsToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartOven1Lamps", isChecked);
                    LampsTextBlock.Text = "On";
                    ShowMessage($"Oven 1 lamps are on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartOven1Lamps", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopOven1Lamps", true);
                    LampsTextBlock.Text = "Off";
                    ShowMessage($"Oven 1 lamps are off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopOven1Lamps", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 1 lamps: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven1FansToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                Oven1FansToggle.IsChecked = !Oven1FansToggle.IsChecked;
                return;
            }
            try
            {
                bool isChecked = (bool)Oven1FansToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartOven1Fans", isChecked);
                    FansTextBlock.Text = "On";
                    ShowMessage($"Oven 1 fans are on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartOven1Fans", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopOven1Fans", true);
                    FansTextBlock.Text = "Off";
                    ShowMessage($"Oven 1 fans are off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopOven1Fans", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 1 fans: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven1BeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                Oven1BeltToggle.IsChecked = !Oven1BeltToggle.IsChecked;
                return;
            }
            try
            {
                bool isChecked = (bool)Oven1BeltToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartOven1Belt", isChecked);
                    Oven1BeltTextBlock.Text = "On";
                    ShowMessage($"Oven 1 belt is on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartOven1Belt", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopOven1Belt", true);
                    Oven1BeltTextBlock.Text = "Off";
                    ShowMessage($"Oven 1 belt is off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopOven1Belt", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 1 belt: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven2LampsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                Oven2LampsToggle.IsChecked = !Oven2LampsToggle.IsChecked;
                return;
            }
            try
            {
                bool isChecked = (bool)Oven2LampsToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartOven2Lamps", isChecked);
                    Oven2LampsTextBlock.Text = "On";
                    ShowMessage($"Oven 2 lamps are on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartOven2Lamps", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopOven2Lamps", true);
                    Oven2LampsTextBlock.Text = "Off";
                    ShowMessage($"Oven 2 lamps are off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopOven2Lamps", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 2 lamps: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven2FansToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                Oven2FansToggle.IsChecked = !Oven2FansToggle.IsChecked;
                return;
            }
            try
            {
                bool isChecked = (bool)Oven2FansToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartOven2Fans", isChecked);
                    Oven2FansTextBlock.Text = "On";
                    ShowMessage($"Oven 2 fans are on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartOven2Fans", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopOven2Fans", true);
                    Oven2FansTextBlock.Text = "Off";
                    ShowMessage($"Oven 2 fans are off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopOven2Fans", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 2 fans: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven2BeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                Oven2BeltToggle.IsChecked = !Oven2BeltToggle.IsChecked;
                return;
            }
            try
            {
                bool isChecked = (bool)Oven2BeltToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartOven2Belt", isChecked);
                    Oven2BeltTextBlock.Text = "On";
                    ShowMessage($"Oven 2 belt is on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartOven2Belt", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopOven2Belt", true);
                    Oven2BeltTextBlock.Text = "Off";
                    ShowMessage($"Oven 2 belt is off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopOven2Belt", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 2 belt: {ex.Message}", MessageType.Error);
            }
        }

        public async void InputBeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                InputBeltToggle.IsChecked = !InputBeltToggle.IsChecked;
                return;
            }
            try
            {

                bool isChecked = (bool)InputBeltToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartInputBelt", isChecked);
                    InputBeltTextBlock.Text = "On";
                    ShowMessage("Input belt is on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartInputBelt", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopInputBelt", true);
                    InputBeltTextBlock.Text = "Off";
                    ShowMessage("Input belt is off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopInputBelt", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on input belt: {ex.Message}", MessageType.Error);
            }
        }

        public async void CentralBeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_allowToggle)
            {
                CentralBeltToggle.IsChecked = !CentralBeltToggle.IsChecked;
                return;
            }
            try
            {
                bool isChecked = (bool)CentralBeltToggle.IsChecked;
                if (isChecked)
                {
                    await _opcUaClient.WriteNodeAsync("StartCentralBelt", isChecked);
                    CentralBeltTextBlock.Text = "On";
                    ShowMessage("Central belt is on", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StartCentralBelt", false);
                }
                else
                {
                    await _opcUaClient.WriteNodeAsync("StopCentralBelt", true);
                    CentralBeltTextBlock.Text = "Off";
                    ShowMessage("Central belt is off", MessageType.Info);

                    Thread.Sleep(200);

                    await _opcUaClient.WriteNodeAsync("StopCentralBelt", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on central belt: {ex.Message}", MessageType.Error);
            }
        }

        public async void InputBeltToggle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var statusObject = await _opcUaClient.ReadNodeAsync("SystemStatus");
                int status = Convert.ToInt32(statusObject);

                _allowToggle = (status == 2);

                if (!_allowToggle)
                {
                    e.Handled = true; // Block the toggle action
                    ShowMessage("Cannot toggle input belt. Machine is not in manual mode.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                _allowToggle = false;
                ShowMessage($"Error checking system status: {ex.Message}", MessageType.Error);
                e.Handled = true; // Block on error
            }
        }

        public async void CentralBeltToggle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var statusObject = await _opcUaClient.ReadNodeAsync("SystemStatus");
                int status = Convert.ToInt32(statusObject);

                _allowToggle = (status == 2);

                if (!_allowToggle)
                {
                    e.Handled = true; // Block the toggle action
                    ShowMessage("Cannot toggle central belt. Machine is not in manual mode.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                _allowToggle = false;
                ShowMessage($"Error checking system status: {ex.Message}", MessageType.Error);
                e.Handled = true; // Block on error
            }
        }

        public async void Oven1Toggle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var isAutomaticStatus = await _opcUaClient.ReadNodeAsync("Oven1Mode");
                var isAutomaticSelector = await _opcUaClient.ReadNodeAsync("InputPLC") as bool[];

                _allowToggle = (isAutomaticStatus is false && isAutomaticSelector[2] is false);

                if (!_allowToggle)
                {
                    e.Handled = true; // Block the toggle action
                    ShowMessage("Cannot toggle oven 1. Machine is not in manual mode.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                _allowToggle = false;
                ShowMessage($"Error checking system status: {ex.Message}", MessageType.Error);
                e.Handled = true; // Block on error
            }
        }

        public async void Oven2Toggle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var isAutomaticStatus = await _opcUaClient.ReadNodeAsync("Oven2Mode");
                var isAutomaticSelector = await _opcUaClient.ReadNodeAsync("InputPLC") as bool[];

                _allowToggle = (isAutomaticStatus is false && isAutomaticSelector[2] is false);

                if (!_allowToggle)
                {
                    e.Handled = true; // Block the toggle action
                    ShowMessage("Cannot toggle oven 2. Machine is not in manual mode.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                _allowToggle = false;
                ShowMessage($"Error checking system status: {ex.Message}", MessageType.Error);
                e.Handled = true; // Block on error
            }
        }

        private void ShowMessage(string message, MessageType messageType)
        {
            ManualMessageText.Text = message;

            // Set colors and icons based on message type
            switch (messageType)
            {
                case MessageType.Success:
                    ManualMessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DFF0D8")); // Light Green
                    ManualMessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C763D")); // Dark Green
                    ManualMessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C763D")); // Dark Green Border
                    ManualMessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-success-100.png"));
                    break;

                case MessageType.Error:
                    ManualMessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2DEDE")); // Light Red
                    ManualMessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A94442")); // Dark Red
                    ManualMessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A94442")); // Dark Green Border
                    ManualMessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-error-96.png"));
                    break;

                case MessageType.Warning:
                    ManualMessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCF8E3")); // Light Yellow
                    ManualMessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A6D3B")); // Dark Yellow
                    ManualMessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A6D3B")); // Dark Yellow Border
                    ManualMessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-warning-96.png"));
                    break;

                case MessageType.Info:
                    ManualMessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9EDF7")); // Light Blue
                    ManualMessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#31708F")); // Dark Blue
                    ManualMessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#31708F")); // Dark Blue Border
                    ManualMessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-info-104.png"));
                    break;
            }

            ManualMessageBoxPanel.Visibility = Visibility.Visible;

            // Auto-hide the message after 3 seconds
            _messageTimer.Interval = TimeSpan.FromSeconds(3);
            _messageTimer.Tick += (s, e) => HideMessage();
            _messageTimer.Start();
        }

        private void HideMessage()
        {
            ManualMessageBoxPanel.Visibility = Visibility.Collapsed;
            _messageTimer.Stop();
        }

        private void CloseMessageBox(object sender, RoutedEventArgs e)
        {
            HideMessage();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
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