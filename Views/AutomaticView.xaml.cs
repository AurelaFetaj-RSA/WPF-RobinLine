﻿using Newtonsoft.Json.Linq;
using Opc.UaFx.Client;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using static WPF_App.MainWindow;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for AutomaticView.xaml
    /// </summary>
    public partial class AutomaticView : UserControl, INotifyPropertyChanged
    {
        private readonly SemaphoreSlim _initializationLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource _disposalTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _tabTokenSource = new CancellationTokenSource();
        private bool _isDisposed;
        private bool _isTabInitialized;

        //private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private RobinConfiguration _currentConfig = new();

        private string _popupAction;
        private bool _ovenStatus;
        private readonly OpcUaClientService _opcUaClient = new OpcUaClientService();
        private DispatcherTimer _messageTimer = new DispatcherTimer();
        //private bool _previousReadyState = false;
        private int _previousState = -1;
        //private int _previousTemperature = int.MinValue;
        private bool _lastOven1TemperatureStatus = false;
        private bool _lastOven2TemperatureStatus = false;
        private bool _lastOvenReadyStatus = false;

        private DispatcherTimer _updateLightsTimer;
        private bool[] _lightsArray = new bool[12];
        private bool[] _previousLightsArray = new bool[21];

        private bool[] _previousOutputs;

        private IntegerUpDown? _oven1TempSetpointUpDown;
        private IntegerUpDown? _oven1FanPercentageUpDown;
        private IntegerUpDown? _oven1LampsPercentageUpDown;
        private IntegerUpDown? _oven2TempSetpointUpDown;
        private IntegerUpDown? _oven2FanPercentageUpDown;
        private IntegerUpDown? _oven2LampsPercentageUpDown;

        public AutomaticView()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize with configuration
            var config = new RobinLineOpcConfiguration();
            _opcUaClient = new OpcUaClientService();
            _messageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

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

            foreach (ComboBoxItem item in BeltSpeedComboBox.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == _currentConfig.BeltSpeed.ToString())
                {
                    BeltSpeedComboBox.SelectedItem = item;
                    break;
                }
            }

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
            try
            {
                // Update from UI
                _currentConfig.R1Inclusion = Robot1Toggle.IsChecked ?? false;
                _currentConfig.R2Inclusion = RobotToggle2.IsChecked ?? false;
                _currentConfig.Oven1Inclusion = Oven1Toggle.IsChecked ?? false;
                _currentConfig.Oven2Inclusion = Oven2Toggle.IsChecked ?? false;
                //_currentConfig.BeltSpeed = GetSelectedBeltSpeedValue();
                _currentConfig.BeltSpeed = (BeltSpeedComboBox.SelectedItem as ComboBoxItem)?.Tag as int? ?? 1;

                _currentConfig.Oven1TempSetpoint = Oven1TempSetpointUpDown.Value ?? 0;
                _currentConfig.Oven1FanPercentage = Oven1FanPercentageUpDown.Value ?? 0;
                _currentConfig.Oven1LampsPercentage = Oven1LampsPercentageUpDown.Value ?? 0;

                _currentConfig.Oven2TempSetpoint = Oven2TempSetpointUpDown.Value ?? 0;
                _currentConfig.Oven2FanPercentage = Oven2FanPercentageUpDown.Value ?? 0;
                _currentConfig.Oven2LampsPercentage = Oven2LampsPercentageUpDown.Value ?? 0;

                ConfigurationManager.SaveConfig(_currentConfig);
            }
            catch (Exception ex)
            {
                // Log to file since UI might not be available
                File.AppendAllText("error.log", $"{DateTime.Now}: Save failed - {ex}\n");
            }
        }

        private int GetSelectedBeltSpeedValue()
        {
            if (BeltSpeedComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return Convert.ToInt32(selectedItem.Tag);
            }
            return 2; // default value
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //await ViewLoadGuard.TabSwitchLock.WaitAsync();

                InitializeOvenControls();

                await _opcUaClient.InitializeAsync();
                await _opcUaClient.ConnectAsync("opc.tcp://172.31.40.130:48010");
                //await _opcUaClient.ConnectAsync("opc.tcp://192.31.30.40:48010");

                //await SubscribeToOutputSignals();

                RedLight.Loaded += (s, e) => Debug.WriteLine("RedLight loaded");
                GreenLight.Loaded += (s, e) => Debug.WriteLine("GreenLight loaded");
                OrangeLight.Loaded += (s, e) => Debug.WriteLine("OrangeLight loaded");

                // Verify datacontext
                Loaded += (s, e) => {
                    Debug.WriteLine($"RedLight DataContext: {RedLight.DataContext}");
                    Debug.WriteLine($"RedLight VisualParent: {VisualTreeHelper.GetParent(RedLight)}");
                };

                await _opcUaClient.SubscribeToNodesAsync();

                _opcUaClient.ValueUpdated += OnOpcValueChanged;
                StartMonitoringTasks();
            }
            catch (Exception ex)
            {
                //ShowMessage($"Initialization failed: {ex.Message}", MessageType.Error);
                ShowMessage($"Initialization failed", MessageType.Error);
            }
        }

        private void InitializeOvenControls()
        {
            _oven1TempSetpointUpDown = FindName("Oven1TempSetpointUpDown") as IntegerUpDown;
            _oven1FanPercentageUpDown = FindName("Oven1FanPercentageUpDown") as IntegerUpDown;
            _oven1LampsPercentageUpDown = FindName("Oven1LampsPercentageUpDown") as IntegerUpDown;

            _oven2TempSetpointUpDown = FindName("Oven2TempSetpointUpDown") as IntegerUpDown;
            _oven2FanPercentageUpDown = FindName("Oven2FanPercentageUpDown") as IntegerUpDown;
            _oven2LampsPercentageUpDown = FindName("Oven2LampsPercentageUpDown") as IntegerUpDown;
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
                            //bool isReady = (bool)value; // Cast to bool
                            ReadyLamp.Foreground = new SolidColorBrush((bool)value ? Colors.Green : Colors.Red);
                            //Console.WriteLine($"[UI] Robot1Ready set to: {isReady}");
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
                        case "RedLight":
                            //RedLight.Background = (bool)value ? Brushes.Red : Brushes.DarkGreen;
                            RedLight.Background = new SolidColorBrush((bool)value ? Colors.Red : Colors.DarkGreen);
                            break;
                        case "OutputPLC":
                            UpdateLights((bool[])value);
                            break;
                        case "Comunicazione":
                            //HandleHandshake((bool)value);
                            Dispatcher.InvokeAsync(async () =>
                            await HandleHandshake((bool)value));
                            break;
                        case "Restart":
                            HandleRestartRequest((bool)value); ;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error processing update for {nodeName}: {ex.Message}", MessageType.Error);
                }
            });
        }

        private async Task HandleHandshake(bool currentValue)
        {
            try
            {
                await _opcUaClient.WriteNodeAsync("Comunicazione", !currentValue);
                // Optional: add logging or status update
                // ShowMessage("Handshake processed", MessageType.Info); 
            }
            catch (Exception ex)
            {
                ShowMessage($"Handshake error: {ex.Message}", MessageType.Error);
                throw; // Re-throw if you want the outer handler to know about the failure
            }
        }

        private async Task HandleRestartRequest(bool restartRequested)
        {
            if (!restartRequested)
                return;

            try
            {
                await SendAllParametersToPLC();
                await Task.Delay(100); //small delay to ensure PLC processes the values
                await _opcUaClient.WriteNodeAsync("Restart", false);
                ShowMessage("System parameters sent after restart request", MessageType.Success);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to handle restart: {ex.Message}", MessageType.Error);
                // Consider whether to rethrow the exception here
            }
        }

        private async Task SendAllParametersToPLC()
        {
            try
            {
                // Send all configuration parameters
                var tasks = new List<Task>
                {
                    _opcUaClient.WriteNodeAsync("Robot1Inclusion", _currentConfig.R1Inclusion),
                    _opcUaClient.WriteNodeAsync("Robot2Inclusion", _currentConfig.R2Inclusion),
                    _opcUaClient.WriteNodeAsync("Oven1Inclusion", _currentConfig.Oven1Inclusion),
                    _opcUaClient.WriteNodeAsync("Oven2Inclusion", _currentConfig.Oven2Inclusion),
                    _opcUaClient.WriteNodeAsync("BeltSpeed", _currentConfig.BeltSpeed),
                    _opcUaClient.WriteNodeAsync("Oven1TempSetpoint", _currentConfig.Oven1TempSetpoint),
                    _opcUaClient.WriteNodeAsync("Oven1FanPercentage", _currentConfig.Oven1FanPercentage),
                    _opcUaClient.WriteNodeAsync("Oven1LampsPercentage", _currentConfig.Oven1LampsPercentage),
                    _opcUaClient.WriteNodeAsync("Oven2TempSetpoint", _currentConfig.Oven2TempSetpoint),
                    _opcUaClient.WriteNodeAsync("Oven2FanPercentage", _currentConfig.Oven2FanPercentage),
                    _opcUaClient.WriteNodeAsync("Oven2LampsPercentage", _currentConfig.Oven2LampsPercentage)
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to send some parameters: {ex.Message}", MessageType.Error);
                throw;
            }
        }

        private void UpdateTemperatureStatus(bool isReached, TextBlock textBlock, FontAwesome.Sharp.IconImage icon, bool isOven1)
        {
            ref bool lastStatus = ref (isOven1 ? ref _lastOven1TemperatureStatus : ref _lastOven2TemperatureStatus);

            // Always update if temperature is not reached, or if status changed
            if (!isReached || isReached != lastStatus)
            {
                //textBlock.Text = isReached ? "Reached" : "Not Reached";
                icon.Icon = isReached ? FontAwesome.Sharp.IconChar.Check : FontAwesome.Sharp.IconChar.Times;
                icon.Foreground = new SolidColorBrush(isReached ?
                    (Color)ColorConverter.ConvertFromString("#02a29a") : Colors.Red);

                // Only show message if status changed (avoid spamming)
                if (isReached != lastStatus)
                {
                    ShowMessage($"Temperature {(isReached ? "reached" : "not reached")}",
                        isReached ? MessageType.Success : MessageType.Warning);
                }

                lastStatus = isReached;
            }
        }

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
                ShowMessage($"Robot 1 {(isChecked ? "included" : "excluded")}", MessageType.Info);
                SaveCurrentConfiguration();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle robot: {ex.Message}", MessageType.Error);
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
                // Automatic Mode (Blue) 
                ellipse.Fill = new SolidColorBrush(Colors.Blue);
                textBlock.Text = "Automatic";
            }
            else
            {
                // Manual Mode (Orange)
                ellipse.Fill = new SolidColorBrush(Colors.Orange);
                textBlock.Text = "Manual";
            }

            // Only show message if status changed (optional optimization)
            if (isAutomatic != _lastOvenReadyStatus)
            {
                ShowMessage($"Oven mode switched to {(isAutomatic ? "automatic" : "manual")}",
                    isAutomatic ? MessageType.Info : MessageType.Warning);
                _lastOvenReadyStatus = isAutomatic;
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

        private async void Oven1LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true)
                return;

            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int lampsPercentage)
            {
                try
                {
                    await _opcUaClient.WriteNodeAsync("Oven1LampsPercentage", lampsPercentage);
                    SaveCurrentConfiguration();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to update lamps: {ex.Message}", MessageType.Error);
                }
            }
            else if (e.NewValue != null)
            {
                ShowMessage("Invalid value type for oven lamps percentage", MessageType.Error);
            }
        }

        private async void Oven1FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true)
                return;

            if (e.NewValue is int fanPercentage)
            {
                try
                {
                    await _opcUaClient.WriteNodeAsync("Oven1FanPercentage", fanPercentage);
                    SaveCurrentConfiguration();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to update fans: {ex.Message}", MessageType.Error);
                }
            }
            else if (e.NewValue != null)
            {
                ShowMessage("Invalid value type for oven fans percentage", MessageType.Error);
            }
        }

        private async void Oven1TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true)
                return;

            if (e.NewValue is int tempSetpoint)
            {
                try
                {
                    await _opcUaClient.WriteNodeAsync("Oven1TempSetpoint", tempSetpoint);
                    SaveCurrentConfiguration();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to update temperature: {ex.Message}", MessageType.Error);
                }
            }
            else if (e.NewValue != null)
            {
                ShowMessage("Invalid value type for oven temperature", MessageType.Error);
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

        private async void Oven2LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true)
                return;

            if (e.NewValue is int lampsPercentage)
            {
                try
                {
                    await _opcUaClient.WriteNodeAsync("Oven2LampsPercentage", lampsPercentage);
                    SaveCurrentConfiguration();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to update lamps: {ex.Message}", MessageType.Error);
                }
            }
            else if (e.NewValue != null)
            {
                ShowMessage("Invalid value type for oven lamps percentage", MessageType.Error);
            }
        }

        private async void Oven2FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true)
                return;

            if (e.NewValue is int fanPercentage)
            {
                try
                {
                    await _opcUaClient.WriteNodeAsync("Oven2FanPercentage", fanPercentage);
                    SaveCurrentConfiguration();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to update fans: {ex.Message}", MessageType.Error);
                }
            }
            else if (e.NewValue != null)
            {
                ShowMessage("Invalid value type for oven fans percentage", MessageType.Error);
            }
        }

        private async void Oven2TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_opcUaClient?.IsConnected != true)
                return;

            if (e.NewValue is int tempSetpoint)
            {
                try
                {
                    await _opcUaClient.WriteNodeAsync("Oven2TempSetpoint", tempSetpoint);
                    SaveCurrentConfiguration();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to update temperature: {ex.Message}", MessageType.Error);
                }
            }
            else if (e.NewValue != null)
            {
                ShowMessage("Invalid value type for oven temperature", MessageType.Error);
            }
        }

        private void UpdateSystemState(Object statusValue)
        {
            int status;
            try
            {
                status = Convert.ToInt32(statusValue);
            }
            catch
            {
                status = -1; //error
            }

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
            //if (lightArray.Length <= 11) return;
            Dispatcher.Invoke(() =>
            {
                if (lightArray.Length > 11)
                {
                    // Simple direct updates without any state tracking
                    Line.Text = lightArray[7] ? "Line started" : "Stop button pressed in";
                    Line.Foreground = new SolidColorBrush(
                    lightArray[7] ? (Color)ColorConverter.ConvertFromString("#3f80cb") : Colors.Red
                    );

                    OrangeLight.Background = lightArray[11] ? Brushes.Orange : Brushes.DarkGreen;
                    GreenLight.Background = lightArray[10] ? Brushes.LightGreen : Brushes.DarkGreen;
                    RedLight.Background = lightArray[9] ? Brushes.Red : Brushes.DarkGreen;
                }
            });

            //if (!_redLightShouldBlink)
            //{
            //    _redLightShouldBlink = true;
            //    var storyboard = (Storyboard)this.Resources["RedLightBlinkStoryboard"];
            //    Storyboard.SetTarget(storyboard, RedLight);
            //    storyboard.Begin(RedLight, true);
            //}
        }

        private void StartMonitoringTasks()
        {
            // Only need tasks for things not covered by subscriptions
            _ = MonitorSelectorStatus();
            _ = HandleInput();
        }

        private async Task MonitorSelectorStatus()
        {
            while (!_tabTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (_opcUaClient == null) return;

                    var inputArray = await _opcUaClient.ReadNodeAsync("InputPLC") as bool[];

                    if (inputArray?.Length > 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Selector.Text = inputArray[2] ? "Selector in Automatic" : "Selector in Manual";
                            Selector.Foreground = new SolidColorBrush(
                                inputArray[2] ? (Color)ColorConverter.ConvertFromString("#3f80cb") : Colors.Red
                            );
                        });
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Selector monitoring error: {ex.Message}", MessageType.Error);
                    await Task.Delay(5000, _tabTokenSource.Token); // Longer delay after error
                }

                await Task.Delay(1000, _tabTokenSource.Token);
            }
        }

        private async Task HandleInput()
        {
            while (!_tabTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (_opcUaClient == null) return;

                    var inputArray = await _opcUaClient.ReadNodeAsync("InputPLC") as bool[];

                    if (inputArray?.Length > 2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Selector.Text = inputArray[2] ? "Selector in Automatic" : "Selector in Manual";

                            if (inputArray[1])
                            {
                                Emergency.Text = "Emergency ok";
                                Emergency.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3f80cb"));
                            }
                            else
                            {
                                Emergency.Text = "Reset Emergency";
                                Emergency.Foreground = new SolidColorBrush(Colors.Red);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Selector monitoring error: {ex.Message}", MessageType.Error);
                    await Task.Delay(5000, _tabTokenSource.Token); // Longer delay after error
                }

                await Task.Delay(1000, _tabTokenSource.Token);
            }
        }

        private async void BeltSpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_opcUaClient?.IsConnected != true)
                return;

            if (BeltSpeedComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int speedValue = Convert.ToInt32(selectedItem.Tag); // Get Tag (1, 2, or 3)

                try
                {
                    // Write to OPC UA node
                    await _opcUaClient.WriteNodeAsync("BeltSpeed", speedValue);
                    ShowMessage($"Belt speed set to: {selectedItem.Content} (Value: {speedValue})", MessageType.Success);
                    SaveCurrentConfiguration();
                }
                catch (Exception ex)
                {
                    ShowMessage($"Failed to set belt speed: {ex.Message}", MessageType.Error);
                }
            }
        }

        public enum MessageType
        {
            Success,
            Error,
            Warning,
            Info
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

        // Close Button Event
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
            _tabTokenSource.Cancel();
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