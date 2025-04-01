using Opc.UaFx.Client;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPF_App.Services;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for AutomaticView.xaml
    /// </summary>
    public partial class AutomaticView : UserControl, INotifyPropertyChanged
    {
        private string _playStopIcon = "Play";  // Default icon
        private string _playStopText = "Start";
        private string _confirmationMessage = "Are you sure you want to start the line?";
        private string _popupAction;
        private bool _ovenStatus;
        private readonly OpcUaClientService _opcUaClient = new OpcUaClientService();
        private DispatcherTimer _messageTimer = new DispatcherTimer();
        private bool _previousReadyState = false;
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
            InitializeComponent();
            DataContext = this;
            OpcServerConnection();

            //UpdateRobot1ReadyStatus();
            //UpdateRobot2ReadyStatus();
            Task.Run(UpdateRobot1ReadyStatus);
            Task.Run(UpdateRobot2ReadyStatus);
            Task.Run(ReadOven1Temperature);
            Task.Run(UpdateOven1TemperatureStatus);
            Task.Run(UpdateOven1State);
            Task.Run(UpdateOven1ReadyStatus);
            Task.Run(UpdateOven2State);
            Task.Run(UpdateOven2ReadyStatus);
            Task.Run(ReadOven2Temperature);
            Task.Run(UpdateOven2TemperatureStatus);
            Task.Run(StateButton);
            Task.Run(UpdateGeneralLightsState);
            //UpdateGeneralRedLightState();
            //UpdateGeneralOrangeLightState();
            //UpdateGeneralGreenLightState();
            Task.Run(UpdateSelectorStatus);
        }

        private async void OpcServerConnection()
        {
            await _opcUaClient.ConnectAsync();
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
            await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_pausa", true);
            ShowMessage("The line has paused", MessageType.Info);
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Proceed with starting the line and changing the icon/text
            if (PopupAction == "Start")
            {
                // Handle Start logic
                PlayStopIcon = "Stop"; // Change icon
                PlayStopText = "Stop"; // Change text
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_start_stop", true);

                //ShowMessage("The line has started", Colors.DarkGreen, (Color)ColorConverter.ConvertFromString("#def0d8"));
                ShowMessage("The line has started", MessageType.Success);
            }
            else if (PopupAction == "Stop")
            {
                // Handle Stop logic
                PlayStopIcon = "Play";
                PlayStopText = "Start";
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_start_stop", false);
                ShowMessage("The line has stopped", MessageType.Warning);
            }
            else if (PopupAction == "Reset")
            {
                // Handle Reset logic (you can add any reset logic here)
                //MessageBox.Show("Line has been reset.");
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_reset_generale", true);
                ShowMessage("The line has been reset", MessageType.Info);
            }

            // Close the popup after confirming
            ConfirmPopup.IsOpen = false;
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

        private async void StateButton()
        {
            while (true)
            {
                var status = await _opcUaClient.ReadIntegerAsync("ns=2;s=Tags.Eren_robin/pc_stato_macchina");

                if (status != _previousState)
                {
                    Dispatcher.Invoke(() =>
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
                                ShowMessage("The system is running in automatic mode.", MessageType.Info);
                                break;
                            case 2:
                                StateText.Text = "Manual";
                                StateIcon.Fill = new SolidColorBrush(Colors.Orange);
                                ShowMessage("Manual mode enabled. Operator control required.", MessageType.Warning);
                                break;
                            case 3:
                                StateText.Text = "Cycle";
                                StateIcon.Fill = new SolidColorBrush(Colors.Green);
                                ShowMessage("The system is executing a cycle.", MessageType.Success);
                                break;
                            case 4:
                                StateText.Text = "Alarm";
                                StateIcon.Fill = new SolidColorBrush(Colors.DarkOrange);
                                ShowMessage("An alarm has been triggered. Please check the system.", MessageType.Error);
                                break;
                            default:
                                StateText.Text = "Status";
                                StateIcon.Fill = new SolidColorBrush(Colors.White);
                                ShowMessage("Unknown status received. Verify OPC UA connection.", MessageType.Warning);
                                break;
                        }
                    });

                    _previousState = status; // Update the stored state
                }

                await Task.Delay(1000); // Check every second
            }
        }

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
            if (Robot1Toggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_robot_robin_primer", true);
                Robot1AvailabilityTextBlock.Text = "Included";
                ShowMessage("Robot 1 has been included", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_robot_robin_primer", false);
                Robot1AvailabilityTextBlock.Text = "Excluded";
                ShowMessage("Robot 1 has been excluded", MessageType.Info);
            }
        }

        private async void UpdateRobot1ReadyStatus()
        {
            while (true)
            {
                bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_robot_robin_primer_ready");

                Dispatcher.Invoke(() =>
                {
                    if (isReady)
                    {
                        ReadyLamp.Foreground = new SolidColorBrush(Colors.Green);

                        if (!_previousReadyState) // Show message only if state changes
                        {
                            ShowMessage("Robot 1 is ready.", MessageType.Success);
                            _previousReadyState = true;
                        }
                    }
                    else
                    {
                        ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);

                        if (_previousReadyState) // Show message only if state changes
                        {
                            ShowMessage("Robot 1 is not ready.", MessageType.Warning);
                            _previousReadyState = false;
                        }
                    }
                });

                await Task.Delay(1000); // Check every second
            }
        }

        public async void Robot2Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (RobotToggle2.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_robot_robin_colla", true);
                Robot2AvailabilityTextBlock.Text = "Included";
                ShowMessage("Robot 2 has been included", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_robot_robin_colla", false);
                Robot2AvailabilityTextBlock.Text = "Excluded";
                ShowMessage("Robot 2 has been excluded", MessageType.Info);
            }
        }

        private async void UpdateRobot2ReadyStatus()
        {
            while (true)
            {
                bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_robot_robin_colla_ready");

                // Update the UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    if (isReady)
                    {
                        //ReadyLamp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("green"));
                        ReadyLamp.Foreground = new SolidColorBrush(Colors.Green);
                        //ReadyRobot2TextBlock.Text = "Ready";
                    }
                    else
                    {
                        ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);
                        //ReadyRobot2TextBlock.Text = "Not Ready";
                    }
                });

                await Task.Delay(1000); // Check every second
            }
        }

        public async void Oven1Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven1Toggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_forno_primer", true);
                AvailabilityTextBlock.Text = "Included";
                ShowMessage("Oven 1 has been included", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_forno_primer", false);
                AvailabilityTextBlock.Text = "Excluded";
                ShowMessage("Oven 1 has been excluded", MessageType.Info);
            }
        }

        private async void Oven1LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int lampsPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("ns=2;s=Tags.Eren_robin/pc_percentuale_accensione_lampade_forno_primer", lampsPercentage);
            }
        }

        private async void Oven1FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int fanPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("ns=2;s=Tags.Eren_robin/pc_percentuale_velocita_ventole_forno_primer", fanPercentage);
            }
        }

        private async void Oven1TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int tempSetpoint)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("ns=2;s=Tags.Eren_robin/pc_setpoint_temperatura_forno_primer", tempSetpoint);
            }
        }

        private async void ReadOven1Temperature()
        {
            // Read the temperature from the OPC UA server
            var temperature = await _opcUaClient.ReadIntegerAsync("ns=2;s=Tags.Eren_robin/pc_temperatura_forno_primer");

            // Update the UI with the temperature value
            TemperatureTextBlock.Text = $"{temperature}°";
        }

        private async void UpdateOven1TemperatureStatus()
        {
            // Read the temperature reached status from the OPC UA server
            var temperatureReached = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_primer_in_temperatura");

            // Update UI only if the status has changed
            if (temperatureReached != _lastTemperatureStatus)
            {
                if (temperatureReached)
                {
                    // If the value is 1 (true), set "Reached" and green check icon
                    ReachedTextBlock.Text = "Reached";
                    StatusIcon.Icon = FontAwesome.Sharp.IconChar.Check;
                    StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                    StatusBorder.Background = new SolidColorBrush(Colors.White);

                    ShowMessage("Oven 1 has reached the set temperature.", MessageType.Success);
                }
                else
                {
                    // If the value is 0 (false), set "Not Reached" and red X icon
                    ReachedTextBlock.Text = "Not Reached";
                    StatusIcon.Icon = FontAwesome.Sharp.IconChar.Times;
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                    StatusBorder.Background = new SolidColorBrush(Colors.White);

                    ShowMessage("Oven 1 temperature has not been reached yet.", MessageType.Warning);
                }

                // Update the last status
                _lastTemperatureStatus = temperatureReached;
            }
        }

        private async void UpdateOven1ReadyStatus()
        {
            while (true)
            {
                //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Robot1.Ready");
                bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_primer_ready");

                // Update the UI on the main thread
                if (isReady != _lastOvenReadyStatus)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (isReady)
                        {
                            // Set green color for "Ready" state
                            LampIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                            ReadyOven1TextBlock.Text = "Ready";

                            ShowMessage("Oven 1 is ready for operation.", MessageType.Success);
                        }
                        else
                        {
                            // Set red color for "Not Ready" state
                            LampIcon.Foreground = new SolidColorBrush(Colors.Red);
                            ReadyOven1TextBlock.Text = "Not Ready";

                            ShowMessage("Oven 1 is not ready yet.", MessageType.Warning);
                        }
                    });

                    // Update the last status
                    _lastOvenReadyStatus = isReady;
                }

                await Task.Delay(1000); // Check every second
            }
        }

        private async void UpdateOven1State()
        {
            // Read the oven mode from the OPC UA server
            var isAutomatic = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_primer");

            // Update UI elements based on the oven mode
            if (isAutomatic)
            {
                // Automatic Mode (Blue)
                OvenStateText.Text = "Automatic";
                OvenStateIcon.Fill = new SolidColorBrush(Colors.Blue);
                ShowMessage("The oven 1 is running in automatic mode.", MessageType.Info);
            }
            else
            {
                // Manual Mode (Orange)
                OvenStateText.Text = "Manual";
                OvenStateIcon.Fill = new SolidColorBrush(Colors.Orange);
                ShowMessage("The oven 1 is running in manual mode.", MessageType.Warning);
            }
        }

        public async void Oven2Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven2Toggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_forno_colla", true);
                Availability2TextBlock.Text = "Included";
                ShowMessage("Oven 2 has been included", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("ns=2;s=Tags.Eren_robin/pc_inclusione_esclusione_forno_colla", false);
                Availability2TextBlock.Text = "Excluded";
                ShowMessage("Oven 2 has been excluded", MessageType.Info);
            }
        }

        private async void UpdateOven2State()
        {
            // Read the oven mode from the OPC UA server
            var isAutomatic = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_colla");

            // Update UI elements based on the oven mode
            if (isAutomatic)
            {
                // Automatic Mode (Blue)
                Oven2StateText.Text = "Automatic";
                Oven2StateIcon.Fill = new SolidColorBrush(Colors.Blue);
            }
            else
            {
                // Manual Mode (Orange)
                Oven2StateText.Text = "Manual";
                Oven2StateIcon.Fill = new SolidColorBrush(Colors.Orange);
            }
        }

        private async void UpdateOven2ReadyStatus()
        {
            while (true)
            {
                //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Robot1.Ready");
                bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_colla_ready");

                // Update the UI on the main thread
                if (isReady != _lastOvenReadyStatus)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (isReady)
                        {
                            // Set green color for "Ready" state
                            Oven2LampIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                            ReadyOven2TextBlock.Text = "Ready";

                            ShowMessage("Oven 2 is ready for operation.", MessageType.Success);
                        }
                        else
                        {
                            // Set red color for "Not Ready" state
                            Oven2LampIcon.Foreground = new SolidColorBrush(Colors.Red);
                            ReadyOven2TextBlock.Text = "Not Ready";

                            ShowMessage("Oven 2 is not ready yet.", MessageType.Warning);
                        }
                    });

                    // Update the last status
                    _lastOvenReadyStatus = isReady;
                }

                await Task.Delay(1000); // Check every second
            }
        }

        private async void Oven2LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int lampsPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("ns=2;s=Tags.Eren_robin/pc_percentuale_accensione_lampade_forno_colla", lampsPercentage);
            }
        }

        private async void Oven2FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int fanPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("ns=2;s=Tags.Eren_robin/pc_percentuale_velocita_ventole_forno_colla", fanPercentage);
            }
        }

        private async void Oven2TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int tempSetpoint)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("ns=2;s=Tags.Eren_robin/pc_setpoint_temperatura_forno_colla", tempSetpoint);
            }
        }

        private async void ReadOven2Temperature()
        {
            // Read the temperature from the OPC UA server
            var temperature = await _opcUaClient.ReadIntegerAsync("ns=2;s=Tags.Eren_robin/pc_temperatura_forno_colla");

            // Update the UI with the temperature value
            // Make sure to run this on the UI thread
            Oven2TemperatureTextBlock.Text = $"{temperature}°";
        }

        private async void UpdateOven2TemperatureStatus()
        {
            // Read the temperature reached status from the OPC UA server
            var temperatureReached = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_forno_colla_in_temperatura");

            // Update UI only if the status has changed
            if (temperatureReached != _lastTemperatureStatus)
            {
                if (temperatureReached)
                {
                    // If the value is 1 (true), set "Reached" and green check icon
                    Oven2ReachedTextBlock.Text = "Reached";
                    Oven2StatusIcon.Icon = FontAwesome.Sharp.IconChar.Check;
                    Oven2StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                    Oven2StatusBorder.Background = new SolidColorBrush(Colors.White);

                    ShowMessage("Oven 2 has reached the set temperature.", MessageType.Success);
                }
                else
                {
                    // If the value is 0 (false), set "Not Reached" and red X icon
                    Oven2ReachedTextBlock.Text = "Not Reached";
                    Oven2StatusIcon.Icon = FontAwesome.Sharp.IconChar.Times;
                    Oven2StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                    Oven2StatusBorder.Background = new SolidColorBrush(Colors.White);

                    ShowMessage("Oven 2 temperature has not been reached yet.", MessageType.Warning);
                }

                // Update the last status
                _lastTemperatureStatus = temperatureReached;
            }
        }

        private async void UpdateGeneralLightsState()
        {
            try
            {
                // Read the entire array from OPC UA
                bool[] lightArray = await _opcUaClient.ReadArrayAsync<bool>("ns=2;s=Tags.Eren_robin/pc_output_plc_linea");

                // Define light mappings (Index -> UI Border Element + Color)
                var lights = new (int Index, System.Windows.Controls.Border Light, Color OnColor)[]
                {
                    (9, RedLight, Colors.Red),
                    (10, OrangeLight, Colors.Orange),
                    (11, GreenLight, Colors.Green)
                };

                foreach (var (index, light, onColor) in lights)
                {
                    if (lightArray.Length > index)
                    {
                        bool isLightOn = lightArray[index];

                        light.Dispatcher.Invoke(() =>
                        {
                            light.Background = new SolidColorBrush(isLightOn ? onColor : Colors.DarkGreen);
                            if (isLightOn)
                            {
                                ShowMessage("Something is wrong.", MessageType.Info);
                            }
                        });
                    }
                    else
                    {
                        ShowMessage($"OPC UA array is smaller than expected (missing index {index})!", MessageType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error reading OPC UA tag array: {ex.Message}", MessageType.Error);
            }
        }

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

        private async void UpdateSelectorStatus()
        {
            try
            {
                bool[] isAutomatic = await _opcUaClient.ReadArrayAsync<bool>("ns=2;s=Tags.Eren_robin/pc_input_plc_linea");

                // Use the value from index 2 for the selector status
                bool isSelectorAutomatic = isAutomatic[2];

                Selector.Dispatcher.Invoke(() =>
                {
                    Selector.Text = isSelectorAutomatic ? "Selector in Automatic" : "Selector in Manual";
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Error reading OPC UA tag: {ex.Message}", MessageType.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}