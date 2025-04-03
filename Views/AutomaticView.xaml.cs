using Opc.UaFx.Client;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPF_App.Services;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for AutomaticView.xaml
    /// </summary>
    public partial class AutomaticView : UserControl, INotifyPropertyChanged
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private string _playStopIcon = "Play";  // Default icon
        private string _playStopText = "Start";
        private string _confirmationMessage = "Are you sure you want to start the line?";
        private string _popupAction;
        private bool _ovenStatus;
        private readonly OpcUaClientService _opcUaClient = new OpcUaClientService();
        private DispatcherTimer _messageTimer = new DispatcherTimer();
        //private bool _previousReadyState = false;
        private int _previousState = -1;
        //private int _previousTemperature = int.MinValue;
        private bool _lastTemperatureStatus = false;
        private bool _lastOvenReadyStatus = false;

        public string PlayStopIcon
        {
            get => _playStopIcon;
            set
            {
                _playStopIcon = value;
                OnPropertyChanged(nameof(PlayStopIcon));
            }
        }

        public string PlayStopText
        {
            get => _playStopText;
            set
            {
                _playStopText = value;
                OnPropertyChanged(nameof(PlayStopText));
            }
        }

        public string ConfirmationMessage
        {
            get => _confirmationMessage;
            set
            {
                _confirmationMessage = value;
                OnPropertyChanged(nameof(ConfirmationMessage));
            }
        }

        public string PopupAction
        {
            get => _popupAction;
            set
            {
                _popupAction = value;
                OnPropertyChanged(nameof(PopupAction));
            }
        }

        public bool OvenStatus
        {
            get => _ovenStatus;
            set
            {
                _ovenStatus = value;
                OnPropertyChanged(nameof(OvenStatus));
            }
        }

        public AutomaticView()
        {
            //InitializeComponent();
            //DataContext = this;
            //OpcServerConnection();

            ////UpdateRobot1ReadyStatus();
            ////UpdateRobot2ReadyStatus();
            //Task.Run(UpdateRobot1ReadyStatus);
            //Task.Run(UpdateRobot2ReadyStatus);
            //Task.Run(ReadOven1Temperature);
            //Task.Run(UpdateOven1TemperatureStatus);
            //Task.Run(UpdateOven1State);
            //Task.Run(UpdateOven1ReadyStatus);
            //Task.Run(UpdateOven2State);
            //Task.Run(UpdateOven2ReadyStatus);
            //Task.Run(ReadOven2Temperature);
            //Task.Run(UpdateOven2TemperatureStatus);
            //Task.Run(StateButton);
            //Task.Run(UpdateGeneralLightsState);
            ////UpdateGeneralRedLightState();
            ////UpdateGeneralOrangeLightState();
            ////UpdateGeneralGreenLightState();
            //Task.Run(UpdateSelectorStatus);

            InitializeComponent();
            DataContext = this;

            // Initialize with configuration
            var config = new RobinLineOpcConfiguration();
            _opcUaClient = new OpcUaClientService();
            _messageTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        //private async void OpcServerConnection()
        //{
        //    await _opcUaClient.ConnectAsync("opc.tcp://172.31.20.101:48011");
        //}

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _opcUaClient.InitializeAsync();
                await _opcUaClient.ConnectAsync("opc.tcp://172.31.20.101:48011");
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

        private void OnOpcValueChanged(string nodeName, object value)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    switch (nodeName)
                    {
                        case "Robot1Ready":
                            bool isReady = (bool)value; // Cast to bool
                            ReadyLamp.Foreground = new SolidColorBrush(isReady ? Colors.Green : Colors.Red);
                            Console.WriteLine($"[UI] Robot1Ready set to: {isReady}");
                            break;
                        case "Robot2Ready":
                            ReadyLamp.Foreground = new SolidColorBrush((bool)value ? Colors.Green : Colors.Red);
                            break;
                        case "Oven1Temperature":
                            TemperatureTextBlock.Text = $"{value}°";
                            break;
                        case "Oven1TemperatureReached":
                            UpdateTemperatureStatus((bool)value, ReachedTextBlock, StatusIcon);
                            break;
                        case "Oven2Temperature":
                            Oven2TemperatureTextBlock.Text = $"{value}°";
                            break;
                        case "Oven2TemperatureReached":
                            UpdateTemperatureStatus((bool)value, Oven2ReachedTextBlock, Oven2StatusIcon);
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
                            UpdateSystemState((int)value);
                            break;
                        case "OutputPLC":
                            UpdateLights((bool[])value);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error processing update for {nodeName}: {ex.Message}", MessageType.Error);
                }
            });
        }

        private void UpdateTemperatureStatus(bool isReached, TextBlock textBlock, FontAwesome.Sharp.IconImage icon)
        {
            if (isReached != _lastTemperatureStatus)
            {
                textBlock.Text = isReached ? "Reached" : "Not Reached";
                icon.Icon = isReached ? FontAwesome.Sharp.IconChar.Check : FontAwesome.Sharp.IconChar.Times;
                icon.Foreground = new SolidColorBrush(isReached ?
                    (Color)ColorConverter.ConvertFromString("#02a29a") : Colors.Red);

                ShowMessage($"Temperature {(isReached ? "reached" : "not reached")}",
                    isReached ? MessageType.Success : MessageType.Warning);

                _lastTemperatureStatus = isReached;
            }
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayStopIcon == "Play")
            {
                ConfirmationMessage = "Are you sure you want to start the line?";
                PopupAction = "Start";
            }
            else
            {
                ConfirmationMessage = "Are you sure you want to stop the line?";
                PopupAction = "Stop";
            }

            ConfirmPopup.IsOpen = true;
        }

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            await _opcUaClient.WriteNodeAsync("Pause", true);
            ShowMessage("Line paused", MessageType.Info);
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Proceed with starting the line and changing the icon/text
            if (PopupAction == "Start")
            {
                // Handle Start logic
                await _opcUaClient.WriteNodeAsync("StartStop", true);
                PlayStopIcon = "Stop"; 
                PlayStopText = "Stop"; 

                ShowMessage("Line started", MessageType.Success);
            }
            else if (PopupAction == "Stop")
            {
                // Handle Stop logic
                await _opcUaClient.WriteNodeAsync("StartStop", false);
                PlayStopIcon = "Play";
                PlayStopText = "Start";
                ShowMessage("Line stopped", MessageType.Warning);
            }
            else if (PopupAction == "Reset")
            {
                // Handle Reset logic (you can add any reset logic here)
                await _opcUaClient.WriteNodeAsync("Reset", true);
                ShowMessage("Line reset", MessageType.Info);
            }

            // Close the popup after confirming
            ConfirmPopup.IsOpen = false;
        }

        //private async void StateButton()
        //{
        //    while (true)
        //    {
        //        //var status = await _opcUaClient.ReadIntegerAsync("ns=2;s=Tags.Eren_robin/pc_stato_macchina");

        //        //if (status != _previousState)
        //        //{
        //        //    Dispatcher.Invoke(() =>
        //        //    {
        //        //        switch (status)
        //        //        {
        //        //            case 0:
        //        //                StateText.Text = "Emergency";
        //        //                StateIcon.Fill = new SolidColorBrush(Colors.Red);
        //        //                ShowMessage("Emergency mode activated!", MessageType.Error);
        //        //                break;
        //        //            case 1:
        //        //                StateText.Text = "Automatic";
        //        //                StateIcon.Fill = new SolidColorBrush(Colors.Blue);
        //        //                ShowMessage("The system is running in automatic mode.", MessageType.Info);
        //        //                break;
        //        //            case 2:
        //        //                StateText.Text = "Manual";
        //        //                StateIcon.Fill = new SolidColorBrush(Colors.Orange);
        //        //                ShowMessage("Manual mode enabled. Operator control required.", MessageType.Warning);
        //        //                break;
        //        //            case 3:
        //        //                StateText.Text = "Cycle";
        //        //                StateIcon.Fill = new SolidColorBrush(Colors.Green);
        //        //                ShowMessage("The system is executing a cycle.", MessageType.Success);
        //        //                break;
        //        //            case 4:
        //        //                StateText.Text = "Alarm";
        //        //                StateIcon.Fill = new SolidColorBrush(Colors.DarkOrange);
        //        //                ShowMessage("An alarm has been triggered. Please check the system.", MessageType.Error);
        //        //                break;
        //        //            default:
        //        //                StateText.Text = "Status";
        //        //                StateIcon.Fill = new SolidColorBrush(Colors.White);
        //        //                ShowMessage("Unknown status received. Verify OPC UA connection.", MessageType.Warning);
        //        //                break;
        //        //        }
        //        //    });

        //        //    _previousState = status; // Update the stored state
        //        //}

        //        await Task.Delay(1000); // Check every second
        //    }
        //}

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the popup without making any changes
            ConfirmPopup.IsOpen = false;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationMessage = "Are you sure you want to reset the line?";
            PopupAction = "Reset";
            ConfirmPopup.IsOpen = true;
        }

        public async void Robot1Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Robot1Toggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Robot1Inclusion", isChecked);
                Robot1AvailabilityTextBlock.Text = isChecked ? "Included" : "Excluded";
                ShowMessage($"Robot 1 {(isChecked ? "included" : "excluded")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle robot: {ex.Message}", MessageType.Error);
            }
        }

        //private async void UpdateRobot1ReadyStatus()
        //{
        //    while (true)
        //    {
        //        //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_robot_robin_primer_ready");

        //        Dispatcher.Invoke(() =>
        //        {
        //            //if (isReady)
        //            //{
        //            //    ReadyLamp.Foreground = new SolidColorBrush(Colors.Green);

        //            //    if (!_previousReadyState) // Show message only if state changes
        //            //    {
        //            //        ShowMessage("Robot 1 is ready.", MessageType.Success);
        //            //        _previousReadyState = true;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);

        //            //    if (_previousReadyState) // Show message only if state changes
        //            //    {
        //            //        ShowMessage("Robot 1 is not ready.", MessageType.Warning);
        //            //        _previousReadyState = false;
        //            //    }
        //            //}
        //        });

        //        await Task.Delay(1000); // Check every second
        //    }
        //}

        public async void Robot2Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)RobotToggle2.IsChecked;
                await _opcUaClient.WriteNodeAsync("Robot2Inclusion", isChecked);
                Robot2AvailabilityTextBlock.Text = isChecked ? "Included" : "Excluded";
                ShowMessage($"Robot 1 {(isChecked ? "included" : "excluded")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle robot: {ex.Message}", MessageType.Error);
            }
        }

        //private async void UpdateRobot2ReadyStatus()
        //{
        //    while (true)
        //    {
        //        //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_robot_robin_colla_ready");

        //        // Update the UI on the main thread
        //        Dispatcher.Invoke(() =>
        //        {
        //            //if (isReady)
        //            //{
        //            //    //ReadyLamp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("green"));
        //            //    ReadyLamp.Foreground = new SolidColorBrush(Colors.Green);
        //            //    //ReadyRobot2TextBlock.Text = "Ready";
        //            //}
        //            //else
        //            //{
        //            //    ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);
        //            //    //ReadyRobot2TextBlock.Text = "Not Ready";
        //            //}
        //        });

        //        await Task.Delay(1000); // Check every second
        //    }
        //}

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
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle oven: {ex.Message}", MessageType.Error);
            }
        }

        private async void Oven1LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int lampsPercentage)
            {
                try
                {
                    //await _opcUaClient.WriteNodeAsync("Oven1LampsPercentage", lampsPercentage);
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
            if (e.NewValue is int fanPercentage)
            {
                try
                {
                    //await _opcUaClient.WriteNodeAsync("Oven1FanPercentage", fanPercentage);
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
            if (e.NewValue is int tempSetpoint)
            {
                try
                {
                    //await _opcUaClient.WriteNodeAsync("Oven1TempSetpoint", tempSetpoint);
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

        //private async void ReadOven1Temperature()
        //{
        //    // Read the temperature from the OPC UA server
        //    //var temperature = await _opcUaClient.ReadIntegerAsync("ns=2;s=Tags.Eren_robin/pc_temperatura_forno_primer");

        //    // Update the UI with the temperature value
        //    //TemperatureTextBlock.Text = $"{temperature}°";
        //}

        //private async void UpdateOven1TemperatureStatus()
        //{
        //    // Read the temperature reached status from the OPC UA server
        //    //var temperatureReached = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_primer_in_temperatura");

        //    // Update UI only if the status has changed
        //    //if (temperatureReached != _lastTemperatureStatus)
        //    //{
        //    //    if (temperatureReached)
        //    //    {
        //    //        // If the value is 1 (true), set "Reached" and green check icon
        //    //        ReachedTextBlock.Text = "Reached";
        //    //        StatusIcon.Icon = FontAwesome.Sharp.IconChar.Check;
        //    //        StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
        //    //        StatusBorder.Background = new SolidColorBrush(Colors.White);

        //    //        ShowMessage("Oven 1 has reached the set temperature.", MessageType.Success);
        //    //    }
        //    //    else
        //    //    {
        //    //        // If the value is 0 (false), set "Not Reached" and red X icon
        //    //        ReachedTextBlock.Text = "Not Reached";
        //    //        StatusIcon.Icon = FontAwesome.Sharp.IconChar.Times;
        //    //        StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
        //    //        StatusBorder.Background = new SolidColorBrush(Colors.White);

        //    //        ShowMessage("Oven 1 temperature has not been reached yet.", MessageType.Warning);
        //    //    }

        //    //    // Update the last status
        //    //    _lastTemperatureStatus = temperatureReached;
        //    //}
        //}

        //private async void UpdateOven1ReadyStatus()
        //{
        //    while (true)
        //    {
        //        //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Robot1.Ready");
        //        //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_primer_ready");

        //        //// Update the UI on the main thread
        //        //if (isReady != _lastOvenReadyStatus)
        //        //{
        //        //    Dispatcher.Invoke(() =>
        //        //    {
        //        //        if (isReady)
        //        //        {
        //        //            // Set green color for "Ready" state
        //        //            LampIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
        //        //            ReadyOven1TextBlock.Text = "Ready";

        //        //            ShowMessage("Oven 1 is ready for operation.", MessageType.Success);
        //        //        }
        //        //        else
        //        //        {
        //        //            // Set red color for "Not Ready" state
        //        //            LampIcon.Foreground = new SolidColorBrush(Colors.Red);
        //        //            ReadyOven1TextBlock.Text = "Not Ready";

        //        //            ShowMessage("Oven 1 is not ready yet.", MessageType.Warning);
        //        //        }
        //        //    });

        //        //    // Update the last status
        //        //    _lastOvenReadyStatus = isReady;
        //        //}

        //        await Task.Delay(1000); // Check every second
        //    }
        //}

        //private async void UpdateOven1State()
        //{
        //    // Read the oven mode from the OPC UA server
        //    //var isAutomatic = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_primer");

        //    // Update UI elements based on the oven mode
        //    //if (isAutomatic)
        //    //{
        //    //    // Automatic Mode (Blue)
        //    //    OvenStateText.Text = "Automatic";
        //    //    OvenStateIcon.Fill = new SolidColorBrush(Colors.Blue);
        //    //    ShowMessage("The oven 1 is running in automatic mode.", MessageType.Info);
        //    //}
        //    //else
        //    //{
        //    //    // Manual Mode (Orange)
        //    //    OvenStateText.Text = "Manual";
        //    //    OvenStateIcon.Fill = new SolidColorBrush(Colors.Orange);
        //    //    ShowMessage("The oven 1 is running in manual mode.", MessageType.Warning);
        //    //}
        //}

        public async void Oven2Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)Oven2Toggle.IsChecked;
                await _opcUaClient.WriteNodeAsync("Oven2Inclusion", isChecked);
                Availability2TextBlock.Text = isChecked ? "Included" : "Excluded";
                ShowMessage($"Oven 2 {(isChecked ? "included" : "excluded")}", MessageType.Info);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to toggle oven: {ex.Message}", MessageType.Error);
            }
        }

        //private async void UpdateOven2State()
        //{
        //    // Read the oven mode from the OPC UA server
        //    //var isAutomatic = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_colla");

        //    // Update UI elements based on the oven mode
        //    //if (isAutomatic)
        //    //{
        //    //    // Automatic Mode (Blue)
        //    //    Oven2StateText.Text = "Automatic";
        //    //    Oven2StateIcon.Fill = new SolidColorBrush(Colors.Blue);
        //    //}
        //    //else
        //    //{
        //    //    // Manual Mode (Orange)
        //    //    Oven2StateText.Text = "Manual";
        //    //    Oven2StateIcon.Fill = new SolidColorBrush(Colors.Orange);
        //    //}
        //}

        //private async void UpdateOven2ReadyStatus()
        //{
        //    while (true)
        //    {
        //        //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Robot1.Ready");
        //        //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_colla_ready");

        //        // Update the UI on the main thread
        //        //if (isReady != _lastOvenReadyStatus)
        //        //{
        //        //    Dispatcher.Invoke(() =>
        //        //    {
        //        //        if (isReady)
        //        //        {
        //        //            // Set green color for "Ready" state
        //        //            Oven2LampIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
        //        //            ReadyOven2TextBlock.Text = "Ready";

        //        //            ShowMessage("Oven 2 is ready for operation.", MessageType.Success);
        //        //        }
        //        //        else
        //        //        {
        //        //            // Set red color for "Not Ready" state
        //        //            Oven2LampIcon.Foreground = new SolidColorBrush(Colors.Red);
        //        //            ReadyOven2TextBlock.Text = "Not Ready";

        //        //            ShowMessage("Oven 2 is not ready yet.", MessageType.Warning);
        //        //        }
        //        //    });

        //        //    // Update the last status
        //        //    _lastOvenReadyStatus = isReady;
        //        //}

        //        await Task.Delay(1000); // Check every second
        //    }
        //}

        private async void Oven2LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is int lampsPercentage)
            {
                try
                {
                    //await _opcUaClient.WriteNodeAsync("Oven2LampsPercentage", lampsPercentage);
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
            if (e.NewValue is int fanPercentage)
            {
                try
                {
                    //await _opcUaClient.WriteNodeAsync("Oven2FanPercentage", fanPercentage);
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
            if (e.NewValue is int tempSetpoint)
            {
                try
                {
                    //await _opcUaClient.WriteNodeAsync("Oven2TempSetpoint", tempSetpoint);
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

        //private async void ReadOven2Temperature()
        //{
        //    // Read the temperature from the OPC UA server
        //    //var temperature = await _opcUaClient.ReadIntegerAsync("ns=2;s=Tags.Eren_robin/pc_temperatura_forno_colla");

        //    // Update the UI with the temperature value
        //    // Make sure to run this on the UI thread
        //    //Oven2TemperatureTextBlock.Text = $"{temperature}°";
        //}

        //private async void UpdateOven2TemperatureStatus()
        //{
        //    // Read the temperature reached status from the OPC UA server
        //    //var temperatureReached = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_colla_in_temperatura");

        //    // Update UI only if the status has changed
        //    //if (temperatureReached != _lastTemperatureStatus)
        //    //{
        //    //    if (temperatureReached)
        //    //    {
        //    //        // If the value is 1 (true), set "Reached" and green check icon
        //    //        Oven2ReachedTextBlock.Text = "Reached";
        //    //        Oven2StatusIcon.Icon = FontAwesome.Sharp.IconChar.Check;
        //    //        Oven2StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
        //    //        Oven2StatusBorder.Background = new SolidColorBrush(Colors.White);

        //    //        ShowMessage("Oven 2 has reached the set temperature.", MessageType.Success);
        //    //    }
        //    //    else
        //    //    {
        //    //        // If the value is 0 (false), set "Not Reached" and red X icon
        //    //        Oven2ReachedTextBlock.Text = "Not Reached";
        //    //        Oven2StatusIcon.Icon = FontAwesome.Sharp.IconChar.Times;
        //    //        Oven2StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
        //    //        Oven2StatusBorder.Background = new SolidColorBrush(Colors.White);

        //    //        ShowMessage("Oven 2 temperature has not been reached yet.", MessageType.Warning);
        //    //    }

        //    //    // Update the last status
        //    //    _lastTemperatureStatus = temperatureReached;
        //    //}
        //}

        //private async void UpdateGeneralLightsState()
        //{
        //    try
        //    {
        //        // Read the entire array from OPC UA
        //        //bool[] lightArray = await _opcUaClient.ReadArrayAsync<bool>("ns=2;s=Tags.Eren_robin/pc_output_plc_linea");

        //        // Define light mappings (Index -> UI Border Element + Color)
        //        var lights = new (int Index, System.Windows.Controls.Border Light, Color OnColor)[]
        //        {
        //            (9, RedLight, Colors.Red),
        //            (10, OrangeLight, Colors.Orange),
        //            (11, GreenLight, Colors.Green)
        //        };

        //        foreach (var (index, light, onColor) in lights)
        //        {
        //            //if (lightArray.Length > index)
        //            //{
        //            //    bool isLightOn = lightArray[index];

        //            //    light.Dispatcher.Invoke(() =>
        //            //    {
        //            //        light.Background = new SolidColorBrush(isLightOn ? onColor : Colors.DarkGreen);
        //            //        if (isLightOn)
        //            //        {
        //            //            ShowMessage("Something is wrong.", MessageType.Info);
        //            //        }
        //            //    });
        //            //}
        //            //else
        //            //{
        //            //    ShowMessage($"OPC UA array is smaller than expected (missing index {index})!", MessageType.Warning);
        //            //}
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowMessage($"Error reading OPC UA tag array: {ex.Message}", MessageType.Error);
        //    }
        //}

        //private async void UpdateGeneralOrangeLightState()
        //{
        //    var isOrangeLight = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_output_plc_linea[10]");

        //    //if (isOrangeLight)
        //    //{
        //    //    OrangeLight.Background = new SolidColorBrush(Colors.Orange);
        //    //}
        //    //else
        //    //{
        //    //    OrangeLight.Background = new SolidColorBrush(Colors.DarkGreen);
        //    //}

        //    OrangeLight.Dispatcher.Invoke(() =>
        //    {
        //        OrangeLight.Background = new SolidColorBrush(isOrangeLight ? Colors.Orange : Colors.DarkGreen);
        //        //if (isOrangeLight)
        //        //{
        //        //    ShowMessage("Something is wrong.", MessageType.Info);
        //        //}
        //    });

        //    try
        //    {
        //        // Read the entire array from OPC UA
        //        bool[] lightArray = await _opcUaClient.ReadArrayAsync<bool>("ns=2;s=Tags.Eren_robin/pc_output_plc_linea");

        //        if (redLightArray.Length > 9)
        //        {
        //            bool isRedLight = redLightArray[9];

        //            RedLight.Dispatcher.Invoke(() =>
        //            {
        //                RedLight.Background = new SolidColorBrush(isRedLight ? Colors.Red : Colors.DarkGreen);
        //                if (isRedLight)
        //                {
        //                    ShowMessage("Something is wrong.", MessageType.Info);
        //                }
        //            });
        //        }
        //        else
        //        {
        //            ShowMessage("OPC UA array is smaller than expected!", MessageType.Warning);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowMessage($"Error reading OPC UA tag array: {ex.Message}", MessageType.Error);
        //    }
        //}

        //private async void UpdateGeneralGreenLightState()
        //{
        //    var isGreenLight = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_output_plc_linea[11]");

        //    //if (isGreenLight)
        //    //{
        //    //    GreenLight.Background = new SolidColorBrush(Colors.Green);
        //    //}
        //    //else
        //    //{
        //    //    GreenLight.Background = new SolidColorBrush(Colors.DarkGreen);
        //    //}

        //    GreenLight.Dispatcher.Invoke(() =>
        //    {
        //        GreenLight.Background = new SolidColorBrush(isGreenLight ? Colors.Green : Colors.DarkGreen);
        //    });
        //}

        //private async void UpdateSelectorStatus()
        //{
        //    try
        //    {
        //        //bool[] isAutomatic = await _opcUaClient.ReadArrayAsync<bool>("ns=2;s=Tags.Eren_robin/pc_input_plc_linea");

        //        // Use the value from index 2 for the selector status
        //        //bool isSelectorAutomatic = isAutomatic[2];

        //        //Selector.Dispatcher.Invoke(() =>
        //        //{
        //        //    Selector.Text = isSelectorAutomatic ? "Selector in Automatic" : "Selector in Manual";
        //        //});
        //    }
        //    catch (Exception ex)
        //    {
        //        ShowMessage($"Error reading OPC UA tag: {ex.Message}", MessageType.Error);
        //    }
        //}

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
            if (lightArray.Length > 11)
            {
                RedLight.Background = new SolidColorBrush(lightArray[9] ? Colors.Red : Colors.DarkGreen);
                OrangeLight.Background = new SolidColorBrush(lightArray[10] ? Colors.Orange : Colors.DarkGreen);
                GreenLight.Background = new SolidColorBrush(lightArray[11] ? Colors.Green : Colors.DarkGreen);

                if (lightArray[9]) ShowMessage("Red light activated - check system", MessageType.Error);
            }
        }

        private void StartMonitoringTasks()
        {
            // Only need tasks for things not covered by subscriptions
            _ = MonitorSelectorStatus();
        }

        private async Task MonitorSelectorStatus()
        {
            while (!_cts.IsCancellationRequested)
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
                        });
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Selector monitoring error: {ex.Message}", MessageType.Error);
                    await Task.Delay(5000, _cts.Token); // Longer delay after error
                }

                await Task.Delay(1000, _cts.Token);
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