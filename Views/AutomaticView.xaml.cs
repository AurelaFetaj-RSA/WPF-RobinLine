using Opc.UaFx.Client;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            //_opcUaClient = opcUaClient;
            UpdateRobot1ReadyStatus();
            UpdateRobot2ReadyStatus();
            ReadOven1Temperature();
            UpdateOven1TemperatureStatus();
            UpdateOven1State();
            UpdateOven1ReadyStatus();
            UpdateOven2State();
            UpdateOven2ReadyStatus();
            ReadOven2Temperature();
            UpdateOven2TemperatureStatus();
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

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Proceed with starting the line and changing the icon/text
            if (PopupAction == "Start")
            {
                // Handle Start logic
                PlayStopIcon = "Stop"; // Change icon
                PlayStopText = "Stop"; // Change text
            }
            else if (PopupAction == "Stop")
            {
                // Handle Stop logic
                PlayStopIcon = "Play";
                PlayStopText = "Start";
            }
            else if (PopupAction == "Reset")
            {
                // Handle Reset logic (you can add any reset logic here)
                MessageBox.Show("Line has been reset.");
            }

            // Close the popup after confirming
            ConfirmPopup.IsOpen = false;
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
                //await _opcUaClient.WriteBooleanAsync("ns=2;s=Robot1.Availability", true);
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_robot_robin_primer", true);
                Robot1AvailabilityTextBlock.Text = "Included";
            }
            else
            {
                //await _opcUaClient.WriteBooleanAsync("ns=2;s=Robot1.Availability", false);
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_robot_robin_primer", false);
                Robot1AvailabilityTextBlock.Text = "Excluded";
            }
        }

        private async void UpdateRobot1ReadyStatus()
        {
            while (true)
            {
                //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Robot1.Ready");
                bool isReady = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_robot_robin_primer_ready");

                // Update the UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    if (isReady)
                    {
                        ReadyLamp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                        ReadyRobot1TextBlock.Text = "Ready";
                    }
                    else
                    {
                        ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);
                        ReadyRobot1TextBlock.Text = "Not Ready";
                    }
                });

                await Task.Delay(1000); // Check every second
            }
        }

        public async void Robot2Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (RobotToggle2.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_robot_robin_colla", true);
                Robot1AvailabilityTextBlock.Text = "Included";
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_robot_robin_colla", false);
                Robot1AvailabilityTextBlock.Text = "Excluded";
            }
        }

        private async void UpdateRobot2ReadyStatus()
        {
            while (true)
            {
                //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Robot1.Ready");
                bool isReady = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_robot_robin_colla_ready");

                // Update the UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    if (isReady)
                    {
                        ReadyLamp.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                        ReadyRobot2TextBlock.Text = "Ready";
                    }
                    else
                    {
                        ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);
                        ReadyRobot2TextBlock.Text = "Not Ready";
                    }
                });

                await Task.Delay(1000); // Check every second
            }
        }

        public async void Oven1Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven1Toggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_forno_primer", true);
                AvailabilityTextBlock.Text = "Included";
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_forno_primer", false);
                AvailabilityTextBlock.Text = "Excluded";
            }
        }

        private async void Oven1LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int lampsPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("Tags.Robin_eren/pc_percentuale_accensione_lampade_forno_primer", lampsPercentage);
            }
        }

        private async void Oven1FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int fanPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("Tags.Robin_eren/pc_percentuale_velocita_ventole_forno_primer", fanPercentage);
            }
        }
        
        private async void Oven1TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int tempSetpoint)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("Tags.Robin_eren/pc_setpoint_temperatura_forno_primer", tempSetpoint);
            }
        }

        private async void ReadOven1Temperature()
        {
            // Read the temperature from the OPC UA server
            var temperature = await _opcUaClient.ReadIntegerAsync("Tags.Robin_eren/pc_temperatura_forno_primer");

            // Update the UI with the temperature value
            // Make sure to run this on the UI thread
            TemperatureTextBlock.Text = $"{temperature}°";
        }

        private async void UpdateOven1TemperatureStatus()
        {
            // Read the temperature reached status from the OPC UA server
            var temperatureReached = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_forno_primer_in_temperatura");

            // Update the UI based on the temperature reached status
            if (temperatureReached)
            {
                // If the value is 1 (true), set "Reached" and green check icon
                ReachedTextBlock.Text = "Reached";
                StatusIcon.Icon = FontAwesome.Sharp.IconChar.Check;
                StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a")); 
                StatusBorder.Background = new SolidColorBrush(Colors.White); 
            }
            else
            {
                // If the value is 0 (false), set "Not Reached" and red X icon
                ReachedTextBlock.Text = "Not Reached";
                StatusIcon.Icon = FontAwesome.Sharp.IconChar.Times;
                StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                StatusBorder.Background = new SolidColorBrush(Colors.White);
            }
        }

        private async void UpdateOven1ReadyStatus()
        {
            while (true)
            {
                //bool isReady = await _opcUaClient.ReadBooleanAsync("ns=2;s=Robot1.Ready");
                bool isReady = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_forno_primer_ready");

                // Update the UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    if (isReady)
                    {
                        //ReadyLamp.Foreground = new SolidColorBrush(Colors.Green);
                        LampIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                        ReadyOven1TextBlock.Text = "Ready";
                    }
                    else
                    {
                        //ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);
                        LampIcon.Foreground = new SolidColorBrush(Colors.Red); // Change icon color to red
                        ReadyOven1TextBlock.Text = "Not Ready";
                    }
                });

                await Task.Delay(1000); // Check every second
            }
        }

        public void UpdateState(int state)
        {
            switch (state)
            {
                case 0:
                    StateText.Text = "Emergency";
                    StateIcon.Fill = Brushes.Red;
                    break;
                case 1:
                    StateText.Text = "Automatic";
                    StateIcon.Fill = Brushes.Blue;
                    break;
                case 2:
                    StateText.Text = "Manual";
                    StateIcon.Fill = Brushes.Orange;
                    break;
                case 3:
                    StateText.Text = "Cycle";
                    StateIcon.Fill = Brushes.Green;
                    break;
                case 4:
                    StateText.Text = "Alarm";
                    StateIcon.Fill = Brushes.DarkOrange;
                    break;
                default:
                    StateText.Text = "Unknown";
                    StateIcon.Fill = Brushes.Gray;
                    break;
            }
        }

        private async void UpdateOven1State()
        {
            // Read the oven mode from the OPC UA server
            var isAutomatic = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_manuale_automatico_forno_primer");

            // Update UI elements based on the oven mode
            if (isAutomatic)
            {
                // Automatic Mode (Blue)
                OvenStateText.Text = "Automatic";
                OvenStateIcon.Fill = new SolidColorBrush(Colors.Blue);
            }
            else
            {
                // Manual Mode (Orange)
                OvenStateText.Text = "Manual";
                OvenStateIcon.Fill = new SolidColorBrush(Colors.Orange);
            }
        }

        public async void Oven2Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven1Toggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_forno_colla", true);
                Availability2TextBlock.Text = "Included";
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_inclusione_esclusione_forno_colla", false);
                Availability2TextBlock.Text = "Excluded";
            }
        }

        private async void UpdateOven2State()
        {
            // Read the oven mode from the OPC UA server
            var isAutomatic = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_manuale_automatico_forno_colla");

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
                bool isReady = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_forno_colla_ready");

                // Update the UI on the main thread
                Dispatcher.Invoke(() =>
                {
                    if (isReady)
                    {
                        //ReadyLamp.Foreground = new SolidColorBrush(Colors.Green);
                        Oven2LampIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                        ReadyOven2TextBlock.Text = "Ready";
                    }
                    else
                    {
                        //ReadyLamp.Foreground = new SolidColorBrush(Colors.Red);
                        Oven2LampIcon.Foreground = new SolidColorBrush(Colors.Red); // Change icon color to red
                        ReadyOven2TextBlock.Text = "Not Ready";
                    }
                });

                await Task.Delay(1000); // Check every second
            }
        }

        private async void Oven2LampsPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int lampsPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("Tags.Robin_eren/pc_percentuale_accensione_lampade_forno_colla", lampsPercentage);
            }
        }

        private async void Oven2FanPercentageUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int fanPercentage)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("Tags.Robin_eren/pc_percentuale_velocita_ventole_forno_colla", fanPercentage);
            }
        }

        private async void Oven2TempSetpointUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if e.NewValue is not null and then cast it to int
            if (e.NewValue is int tempSetpoint)
            {
                // Write the value to the OPC UA server
                await _opcUaClient.WriteIntegerAsync("Tags.Robin_eren/ pc_setpoint_temperatura_forno_colla", tempSetpoint);
            }
        }

        private async void ReadOven2Temperature()
        {
            // Read the temperature from the OPC UA server
            var temperature = await _opcUaClient.ReadIntegerAsync("Tags.Robin_eren/pc_temperatura_forno_colla");

            // Update the UI with the temperature value
            // Make sure to run this on the UI thread
            Oven2TemperatureTextBlock.Text = $"{temperature}°";
        }

        private async void UpdateOven2TemperatureStatus()
        {
            // Read the temperature reached status from the OPC UA server
            var temperatureReached = await _opcUaClient.ReadBooleanAsync("Tags.Robin_eren/pc_forno_colla_in_temperatura");

            // Update the UI based on the temperature reached status
            if (temperatureReached)
            {
                // If the value is 1 (true), set "Reached" and green check icon
                Oven2ReachedTextBlock.Text = "Reached";
                Oven2StatusIcon.Icon = FontAwesome.Sharp.IconChar.Check;
                Oven2StatusIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#02a29a"));
                Oven2StatusBorder.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                // If the value is 0 (false), set "Not Reached" and red X icon
                Oven2ReachedTextBlock.Text = "Not Reached";
                Oven2StatusIcon.Icon = FontAwesome.Sharp.IconChar.Times;
                Oven2StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                Oven2StatusBorder.Background = new SolidColorBrush(Colors.White);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}