using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

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

        public void Robot1Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Robot1Toggle.IsChecked == true)
            {
                Robot1AvailabilityTextBlock.Text = "Included";
            }
            else
            {
                Robot1AvailabilityTextBlock.Text = "Excluded";
            }
        }

        public void Robot2Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (RobotToggle2.IsChecked == true)
            {
                Robot2AvailabilityTextBlock.Text = "Included";
            }
            else
            {
                Robot2AvailabilityTextBlock.Text = "Excluded";
            }
        }

        public void Oven1Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven1Toggle.IsChecked == true)
            {
                AvailabilityTextBlock.Text = "Included";
            }
            else
            {
                AvailabilityTextBlock.Text = "Excluded";
            }
        }

        public void Oven2Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Oven2Toggle.IsChecked == true)
            {
                Availability2TextBlock.Text = "Included";
            }
            else
            {
                Availability2TextBlock.Text = "Excluded";
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


        public void OvenUpdateState(int state)
        {
            switch (state)
            {
                case 0:
                    OvenStateText.Text = "Manual";
                    StateIcon.Fill = Brushes.Orange;
                    break;
                case 1:
                    OvenStateText.Text = "Automatic";
                    StateIcon.Fill = Brushes.Blue;
                    break;
                default:
                    OvenStateText.Text = "Status";
                    StateIcon.Fill = Brushes.White;
                    break;
            }
        }


        //private void UpdateUI(int state)
        //{
        //    switch (state)
        //    {
        //        case 0:
        //            StateText.Text = "Emergency";
        //            StateIcon.Fill = Brushes.Red;
        //            break;
        //        case 1:
        //            StateText.Text = "Automatic";
        //            StateIcon.Fill = Brushes.Blue;
        //            break;
        //        case 2:
        //            StateText.Text = "Manual";
        //            StateIcon.Fill = Brushes.Orange;
        //            break;
        //        case 3:
        //            StateText.Text = "Cycle";
        //            StateIcon.Fill = Brushes.Green;
        //            break;
        //        case 4:
        //            StateText.Text = "Alarm";
        //            StateIcon.Fill = Brushes.DarkOrange;
        //            break;
        //    }
        //}


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}