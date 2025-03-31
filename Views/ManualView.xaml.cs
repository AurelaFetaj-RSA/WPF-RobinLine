using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using WPF_App.Services;
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

        public ManualView()
        {
            InitializeComponent();
        }

        public async void Oven1LampsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven1LampsToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_accendi_lampade_forno_primer", true);
                LampsTextBlock.Text = "On";
                ShowMessage("Oven 1 lamps have been turned on", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_spegni_lampade_forno_primer", false);
                LampsTextBlock.Text = "Off";
                ShowMessage("Oven 1 lamps have been turned off", MessageType.Info);
            }
        }

        public async void Oven1FansToggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven1FansToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_accendi_ventilatori_forno_primer", true);
                FansTextBlock.Text = "On";
                ShowMessage("Oven 1 fans have been turned on", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_spegni_ventilatori_forno_primer", false);
                FansTextBlock.Text = "Off";
                ShowMessage("Oven 1 fans have been turned off", MessageType.Info);
            }
        }

        public async void Oven1BeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven1BeltToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_start_nastrino_forno_primer", true);
                Oven1BeltTextBlock.Text = "Start";
                ShowMessage("Oven 1 belt has been started", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_stop_nastrino_forno_primer", false);
                Oven1BeltTextBlock.Text = "Stop";
                ShowMessage("Oven 1 belt has been turned off", MessageType.Info);
            }
        }

        public async void Oven2LampsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven2LampsToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_accendi_lampade_forno_colla", true);
                Oven2LampsTextBlock.Text = "On";
                ShowMessage("Oven 2 lamps have been turned on", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_spegni_lampade_forno_colla", false);
                Oven2LampsTextBlock.Text = "Off";
                ShowMessage("Oven 2 lamps have been turned off", MessageType.Info);
            }
        }

        public async void Oven2FansToggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven2FansToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_accendi_ventilatori_forno_colla", true);
                Oven2FansTextBlock.Text = "On";
                ShowMessage("Oven 2 fans have been turned on", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_spegni_ventilatori_forno_colla", false);
                Oven2FansTextBlock.Text = "Off";
                ShowMessage("Oven 2 fans have been turned off", MessageType.Info);
            }
        }

        public async void Oven2BeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven2BeltToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_start_nastrino_forno_colla", true);
                Oven2BeltTextBlock.Text = "Start";
                ShowMessage("Oven 2 belt has started", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_stop_nastrino_forno_colla", false);
                Oven2BeltTextBlock.Text = "Stop";
                ShowMessage("Oven 2 belt has stopped", MessageType.Info);
            }
        }

        public async void InputBeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (InputBeltToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_start_nastro_ingresso", true);
                InputBeltTextBlock.Text = "Start";
                ShowMessage("Input belt has started", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_stop_nastro_ingresso", false);
                InputBeltTextBlock.Text = "Stop";
                ShowMessage("Input belt has stopped", MessageType.Info);
            }
        }

        public async void CentralBeltToggle_Click(object sender, RoutedEventArgs e)
        {
            if (CentralBeltToggle.IsChecked == true)
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_start_nastro_centrale", true);
                CentralBeltTextBlock.Text = "Start";
                ShowMessage("Central belt has started", MessageType.Info);
            }
            else
            {
                await _opcUaClient.WriteBooleanAsync("Tags.Robin_eren/pc_stop_nastro_centrale", false);
                CentralBeltTextBlock.Text = "Stop";
                ShowMessage("Central belt has stopped", MessageType.Info);
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
    }
}