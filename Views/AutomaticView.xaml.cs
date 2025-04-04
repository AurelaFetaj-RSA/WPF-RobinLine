using Newtonsoft.Json.Linq;
using Opc.UaFx.Client;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPF_App.Services;
using WPF_RobinLine.Configurations;
using Xceed.Wpf.Toolkit;

namespace WPF_App.Views
{
    public partial class AutomaticView : UserControl, INotifyPropertyChanged, IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly OpcUaClientService _opcUaClient = new();
        private readonly DispatcherTimer _messageTimer = new() { Interval = TimeSpan.FromSeconds(3) };

        private RobinConfiguration _currentConfig = new();
        private int _previousState = -1;
        private bool _lastOven1TemperatureStatus;
        private bool _lastOven2TemperatureStatus;
        private bool _lastOvenReadyStatus;
        private bool _redLightShouldBlink;

        private string _playStopIcon = "Play";
        private string _playStopText = "Start";
        private string _confirmationMessage = "Are you sure you want to start the line?";
        private string _popupAction = string.Empty;
        private bool _ovenStatus;

        private IntegerUpDown? _oven1TempSetpointUpDown;
        private IntegerUpDown? _oven1FanPercentageUpDown;
        private IntegerUpDown? _oven1LampsPercentageUpDown;
        private IntegerUpDown? _oven2TempSetpointUpDown;
        private IntegerUpDown? _oven2FanPercentageUpDown;
        private IntegerUpDown? _oven2LampsPercentageUpDown;

        public string PlayStopIcon
        {
            get => _playStopIcon;
            set => SetProperty(ref _playStopIcon, value);
        }

        public string PlayStopText
        {
            get => _playStopText;
            set => SetProperty(ref _playStopText, value);
        }

        public string ConfirmationMessage
        {
            get => _confirmationMessage;
            set => SetProperty(ref _confirmationMessage, value);
        }

        public string PopupAction
        {
            get => _popupAction;
            set => SetProperty(ref _popupAction, value);
        }

        public bool OvenStatus
        {
            get => _ovenStatus;
            set => SetProperty(ref _ovenStatus, value);
        }

        public AutomaticView()
        {
            InitializeComponent();
            DataContext = this;

            // Load configuration
            _currentConfig = ConfigurationManager.LoadConfig();
            ApplyConfiguration();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void ApplyConfiguration()
        {
            // Apply to UI controls
            Robot1Toggle.IsChecked = _currentConfig.R1Inclusion;
            RobotToggle2.IsChecked = _currentConfig.R2Inclusion;
            Oven1Toggle.IsChecked = _currentConfig.Oven1Inclusion;
            Oven2Toggle.IsChecked = _currentConfig.Oven2Inclusion;

            // Oven settings
            Oven1TempSetpointUpDown.Value = _currentConfig.Oven1TempSetpoint;
            Oven1FanPercentageUpDown.Value = _currentConfig.Oven1FanPercentage;
            Oven1LampsPercentageUpDown.Value = _currentConfig.Oven1LampsPercentage;

            Oven2TempSetpointUpDown.Value = _currentConfig.Oven2TempSetpoint;
            Oven2FanPercentageUpDown.Value = _currentConfig.Oven2FanPercentage;
            Oven2LampsPercentageUpDown.Value = _currentConfig.Oven2LampsPercentage;
        }

        private void SaveCurrentConfiguration()
        {
            // Update from UI
            _currentConfig.R1Inclusion = Robot1Toggle.IsChecked ?? false;
            _currentConfig.R2Inclusion = RobotToggle2.IsChecked ?? false;
            _currentConfig.Oven1Inclusion = Oven1Toggle.IsChecked ?? false;
            _currentConfig.Oven2Inclusion = Oven2Toggle.IsChecked ?? false;

            _currentConfig.Oven1TempSetpoint = Oven1TempSetpointUpDown.Value ?? 0;
            _currentConfig.Oven1FanPercentage = Oven1FanPercentageUpDown.Value ?? 0;
            _currentConfig.Oven1LampsPercentage = Oven1LampsPercentageUpDown.Value ?? 0;

            _currentConfig.Oven2TempSetpoint = Oven2TempSetpointUpDown.Value ?? 0;
            _currentConfig.Oven2FanPercentage = Oven2FanPercentageUpDown.Value ?? 0;
            _currentConfig.Oven2LampsPercentage = Oven2LampsPercentageUpDown.Value ?? 0;

            ConfigurationManager.SaveConfig(_currentConfig);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _oven1TempSetpointUpDown = FindName("Oven1TempSetpointUpDown") as IntegerUpDown;
                _oven1FanPercentageUpDown = FindName("Oven1FanPercentageUpDown") as IntegerUpDown;
                _oven1LampsPercentageUpDown = FindName("Oven1LampsPercentageUpDown") as IntegerUpDown;

                _oven2TempSetpointUpDown = FindName("Oven2TempSetpointUpDown") as IntegerUpDown;
                _oven2FanPercentageUpDown = FindName("Oven2FanPercentageUpDown") as IntegerUpDown;
                _oven2LampsPercentageUpDown = FindName("Oven2LampsPercentageUpDown") as IntegerUpDown;

                await _opcUaClient.InitializeAsync();
                await _opcUaClient.ConnectAsync("opc.tcp://172.31.40.130:48010");
                await _opcUaClient.SubscribeToNodesAsync();

                _opcUaClient.ValueUpdated += OnOpcValueChanged;
                StartMonitoringTasks();
            }
            catch
            {
                ShowMessage("Initialization failed", MessageType.Error);
            }
        }

        private void OnOpcValueChanged(string nodeName, object value)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    switch (nodeName)
                    {
                        case "Robot1Ready":
                            ReadyLamp.Foreground = new SolidColorBrush((bool)value ? Colors.Green : Colors.Red);
                            break;
                        case "Robot2Ready":
                            ReadyLamp2.Foreground = new SolidColorBrush((bool)value ? Colors.Green : Colors.Red);
                            break;
                        case "Oven1Temperature":
                            TemperatureTextBlock.Text = $"{value}°";
                            break;
                        case "Oven1TemperatureReached":
                            UpdateTemperatureStatus((bool)value, ReachedTextBlock, StatusIcon, true);
                            break;
                        case "Oven2Temperature":
                            Oven2TemperatureTextBlock.Text = $"{value}°";
                            break;
                        case "Oven2TemperatureReached":
                            UpdateTemperatureStatus((bool)value, Oven2ReachedTextBlock, Oven2StatusIcon, false);
                            break;
                        case "Oven1Ready":
                            UpdateReadyStatus((bool)value, LampIcon, ReadyOven1TextBlock);
                            break;
                        case "Oven2Ready":
                            UpdateReadyStatus((bool)value, Oven2LampIcon, ReadyOven2TextBlock);
                            break;
                        case "Oven1Mode":
                            UpdateModeStatus((bool)value, OvenStateIcon, OvenStateText);
                            break;
                        case "Oven2Mode":
                            UpdateModeStatus((bool)value, Oven2StateIcon, Oven2StateText);
                            break;
                        case "SystemStatus":
                            UpdateSystemState(Convert.ToInt32(value));
                            break;
                        case "OutputPLC":
                            UpdateLights((bool[])value);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error processing {nodeName}: {ex.Message}", MessageType.Error);
                }
            });
        }

        private void UpdateTemperatureStatus(bool isReached, TextBlock textBlock, FontAwesome.Sharp.IconImage icon, bool isOven1)
        {
            ref bool lastStatus = ref (isOven1 ? ref _lastOven1TemperatureStatus : ref _lastOven2TemperatureStatus);
            if (isReached != lastStatus)
            {
                textBlock.Text = isReached ? "Reached" : "Not Reached";
                icon.Icon = isReached ? FontAwesome.Sharp.IconChar.Check : FontAwesome.Sharp.IconChar.Times;
                icon.Foreground = new SolidColorBrush(isReached ?
                    (Color)ColorConverter.ConvertFromString("#02a29a") : Colors.Red);

                ShowMessage($"Temperature {(isReached ? "reached" : "not reached")}",
                    isReached ? MessageType.Success : MessageType.Warning);

                lastStatus = isReached;
            }
        }

        private void UpdateReadyStatus(bool isReady, FontAwesome.Sharp.IconImage icon, TextBlock textBlock)
        {
            if (isReady != _lastOvenReadyStatus)
            {
                icon.Foreground = new SolidColorBrush(isReady ?
                    (Color)ColorConverter.ConvertFromString("#02a29a") : Colors.Red);
                textBlock.Text = isReady ? "Ready" : "Not Ready";

                ShowMessage($"Oven {(isReady ? "ready" : "not ready")}",
                    isReady ? MessageType.Success : MessageType.Warning);

                _lastOvenReadyStatus = isReady;
            }
        }

        private void UpdateModeStatus(bool isAutomatic, Ellipse ellipse, TextBlock textBlock)
        {
            if (isAutomatic)
            {
                ellipse.Fill = new SolidColorBrush(Colors.Blue);
                textBlock.Text = "Automatic";
            }
            else
            {
                ellipse.Fill = new SolidColorBrush(Colors.Orange);
                textBlock.Text = "Manual";
            }

            if (isAutomatic != _lastOvenReadyStatus)
            {
                ShowMessage($"Oven mode switched to {(isAutomatic ? "automatic" : "manual")}",
                    isAutomatic ? MessageType.Info : MessageType.Warning);
                _lastOvenReadyStatus = isAutomatic;
            }
        }

        private void UpdateSystemState(int status)
        {
            if (status != _previousState)
            {
                switch (status)
                {
                    case 0:
                        StateText.Text = "Emergency";
                        StateIcon.Fill = new SolidColorBrush(Colors.Red);
                        ShowMessage("Emergency mode activated!", MessageType.Error);
                        break;
                    case 1:
                        StateText.Text = "Automatic";
                        StateIcon.Fill = new SolidColorBrush(Colors.Blue);
                        ShowMessage("Automatic mode enabled", MessageType.Info);
                        break;
                    case 2:
                        StateText.Text = "Manual";
                        StateIcon.Fill = new SolidColorBrush(Colors.Orange);
                        ShowMessage("Manual mode enabled", MessageType.Warning);
                        break;
                    case 3:
                        StateText.Text = "Cycle";
                        StateIcon.Fill = new SolidColorBrush(Colors.Green);
                        ShowMessage("Cycle running", MessageType.Success);
                        break;
                    case 4:
                        StateText.Text = "Alarm";
                        StateIcon.Fill = new SolidColorBrush(Colors.DarkOrange);
                        ShowMessage("Alarm triggered", MessageType.Error);
                        break;
                }
                _previousState = status;
            }
        }

        private void UpdateLights(bool[] lightArray)
        {
            if (lightArray.Length <= 11) return;

            OrangeLight.Background = new SolidColorBrush(lightArray[11] ? Colors.Orange : Colors.DarkGreen);
            GreenLight.Background = new SolidColorBrush(lightArray[10] ? Colors.LightGreen : Colors.DarkGreen);

            bool isRedLightOn = lightArray[9];
            if (!_redLightShouldBlink && isRedLightOn)
            {
                _redLightShouldBlink = true;
                var storyboard = (Storyboard)Resources["RedLightBlinkStoryboard"];
                Storyboard.SetTarget(storyboard, RedLight);
                storyboard.Begin(RedLight, true);
            }
            else if (_redLightShouldBlink && !isRedLightOn)
            {
                _redLightShouldBlink = false;
                var storyboard = (Storyboard)Resources["RedLightBlinkStoryboard"];
                storyboard.Stop(RedLight);
                RedLight.Background = new SolidColorBrush(Colors.DarkGreen);
            }
        }

        private void StartMonitoringTasks()
        {
            _ = MonitorSelectorStatus();
        }

        private async Task MonitorSelectorStatus()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var inputArray = await _opcUaClient.ReadNodeAsync("InputPLC") as bool[];
                    if (inputArray?.Length > 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Selector.Text = inputArray[2] ? "Selector in Automatic" : "Selector in Manual";
                        });
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Selector error: {ex.Message}", MessageType.Error);
                    await Task.Delay(5000, _cts.Token);
                }
                await Task.Delay(1000, _cts.Token);
            }
        }

        //private void StartStopButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (PlayStopIcon == "Play")
        //    {
        //        ConfirmationMessage = "Are you sure you want to start the line?";
        //        PopupAction = "Start";
        //    }
        //    else
        //    {
        //        ConfirmationMessage = "Are you sure you want to stop the line?";
        //        PopupAction = "Stop";
        //    }
        //    ConfirmPopup.IsOpen = true;
        //}

        //private async void PauseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    await _opcUaClient.WriteNodeAsync("Pause", true);
        //    ShowMessage("Line paused", MessageType.Info);
        //}

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PopupAction == "Start")
                {
                    await _opcUaClient.WriteNodeAsync("StartStop", true);
                    PlayStopIcon = "Stop";
                    PlayStopText = "Stop";
                    ShowMessage("Line started", MessageType.Success);
                }
                else if (PopupAction == "Stop")
                {
                    await _opcUaClient.WriteNodeAsync("StartStop", false);
                    PlayStopIcon = "Play";
                    PlayStopText = "Start";
                    ShowMessage("Line stopped", MessageType.Warning);
                }
                else if (PopupAction == "Reset")
                {
                    await _opcUaClient.WriteNodeAsync("Reset", true);
                    ShowMessage("Line reset", MessageType.Info);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Operation failed: {ex.Message}", MessageType.Error);
            }
            ConfirmPopup.IsOpen = false;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmPopup.IsOpen = false;
        }

        //private void ResetButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ConfirmationMessage = "Are you sure you want to reset the line?";
        //    PopupAction = "Reset";
        //    ConfirmPopup.IsOpen = true;
        //}

        public async void Robot1Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Robot1Toggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Robot1Inclusion", isChecked);
                Robot1AvailabilityTextBlock.Text = isChecked ? "Included" : "Excluded";
                ShowMessage($"Robot 1 {(isChecked ? "included" : "excluded")}", MessageType.Info);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle robot: {ex.Message}", MessageType.Error);
            }
        }

        public async void Robot2Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)RobotToggle2.IsChecked;
                await _opcUaClient.WriteNodeAsync("Robot2Inclusion", isChecked);
                Robot2AvailabilityTextBlock.Text = isChecked ? "Included" : "Excluded";
                ShowMessage($"Robot 2 {(isChecked ? "included" : "excluded")}", MessageType.Info);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle robot: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven1Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven1Toggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven1Inclusion", isChecked);
                AvailabilityTextBlock.Text = isChecked ? "Included" : "Excluded";
                ShowMessage($"Oven 1 {(isChecked ? "included" : "excluded")}", MessageType.Info);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle oven: {ex.Message}", MessageType.Error);
            }
        }

        public async void Oven2Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven2Toggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven2Inclusion", isChecked);
                Availability2TextBlock.Text = isChecked ? "Included" : "Excluded";
                ShowMessage($"Oven 2 {(isChecked ? "included" : "excluded")}", MessageType.Info);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle oven: {ex.Message}", MessageType.Error);
            }
        }

        private async void Oven1LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true || e.NewValue is not int value) return;

            try
            {
                await _opcUaClient.WriteNodeAsync("Oven1LampsPercentage", value);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update lamps: {ex.Message}", MessageType.Error);
            }
        }

        private async void Oven1FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true || e.NewValue is not int value) return;

            try
            {
                await _opcUaClient.WriteNodeAsync("Oven1FanPercentage", value);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update fans: {ex.Message}", MessageType.Error);
            }
        }

        private async void Oven1TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true || e.NewValue is not int value) return;

            try
            {
                await _opcUaClient.WriteNodeAsync("Oven1TempSetpoint", value);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update temperature: {ex.Message}", MessageType.Error);
            }
        }

        private async void Oven2LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true || e.NewValue is not int value) return;

            try
            {
                await _opcUaClient.WriteNodeAsync("Oven2LampsPercentage", value);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update lamps: {ex.Message}", MessageType.Error);
            }
        }

        private async void Oven2FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true || e.NewValue is not int value) return;

            try
            {
                await _opcUaClient.WriteNodeAsync("Oven2FanPercentage", value);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update fans: {ex.Message}", MessageType.Error);
            }
        }

        private async void Oven2TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true || e.NewValue is not int value) return;

            try
            {
                await _opcUaClient.WriteNodeAsync("Oven2TempSetpoint", value);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update temperature: {ex.Message}", MessageType.Error);
            }
        }

        private async void BeltSpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BeltSpeedComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int speedValue = Convert.ToInt32(selectedItem.Tag); // Get Tag (1, 2, or 3)

                try
                {
                    // Write to OPC UA node
                    await _opcUaClient.WriteNodeAsync("BeltSpeed", speedValue);
                    ShowMessage($"Belt speed set to: {selectedItem.Content} (Value: {speedValue})", MessageType.Success);
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to set belt speed: {ex.Message}", MessageType.Error);
                }
            }
        }

        private void ShowMessage(string message, MessageType messageType)
        {
            MessageText.Text = message;

            switch (messageType)
            {
                case MessageType.Success:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DFF0D8"));
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C763D"));
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C763D"));
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-success-100.png"));
                    break;
                case MessageType.Error:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2DEDE"));
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A94442"));
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A94442"));
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-error-96.png"));
                    break;
                case MessageType.Warning:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCF8E3"));
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A6D3B"));
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A6D3B"));
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-warning-96.png"));
                    break;
                case MessageType.Info:
                    MessageBoxPanel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9EDF7"));
                    MessageText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#31708F"));
                    MessageBoxPanel.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#31708F"));
                    MessageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/icons8-info-104.png"));
                    break;
            }

            MessageBoxPanel.BorderThickness = new Thickness(2);
            MessageBoxPanel.Visibility = Visibility.Visible;

            _messageTimer.Interval = TimeSpan.FromSeconds(3);
            _messageTimer.Tick += (s, e) => HideMessage();
            _messageTimer.Start();
        }

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
            Dispose();
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    SaveCurrentConfiguration();
        //    base.OnClosed(e);
        //}

        public void Dispose()
        {
            _cts.Cancel();
            _messageTimer.Stop();
            _opcUaClient?.Dispose();
            GC.SuppressFinalize(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public enum MessageType
        {
            Success,
            Error,
            Warning,
            Info
        }
    }
}