using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
        private readonly OpcUaClientService _opcUaClient = new OpcUaClientService();
        private DispatcherTimer _messageTimer = new DispatcherTimer();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ManualView()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize with configuration
            var config = new RobinLineOpcConfiguration();
            _opcUaClient = new OpcUaClientService();
            _messageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _opcUaClient.InitializeAsync();
                await _opcUaClient.ConnectAsync("opc.tcp://172.31.40.130:48010");
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
            try
            {
                bool isChecked = (bool)Oven1LampsToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven1Lamps", isChecked);
                LampsTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Oven 1 {(isChecked ? "is on" : "is off")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 1 lamps: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven1FansToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven1FansToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven1Fans", isChecked);
                FansTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Oven 1 {(isChecked ? "is on" : "is off")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 1 fans: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven1BeltToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven1BeltToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven1Belt", isChecked);
                Oven1BeltTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Oven 1 {(isChecked ? "is on" : "is off")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 1 belt: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven2LampsToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven2LampsToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven2Lamps", isChecked);
                Oven2LampsTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Oven 2 {(isChecked ? "is on" : "is off")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 2 lamps: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven2FansToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven2FansToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven2Fans", isChecked);
                Oven2FansTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Oven 2 {(isChecked ? "is on" : "is off")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 2 fans: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven2BeltToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven2BeltToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven2Belt", isChecked);
                Oven2FansTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Oven 2 {(isChecked ? "is on" : "is off")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on oven 2 belt: {ex.Message}", MessageType.Error);
            }
        }

        public async void InputBeltToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)InputBeltToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("InputBelt", isChecked);
                InputBeltTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Input belt {(isChecked ? "is on" : "is off")}", MessageType.Info);

                Thread.Sleep(200);

                await _opcUaClient.WriteNodeAsync("InputBelt", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on input belt: {ex.Message}", MessageType.Error);
            }
        }

        public async void CentralBeltToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)CentralBeltToggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("CentralBelt", isChecked);
                InputBeltTextBlock.Text = isChecked ? "On" : "Off";
                ShowMessage($"Central belt {(isChecked ? "is on" : "is off")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to turn on central belt: {ex.Message}", MessageType.Error);
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