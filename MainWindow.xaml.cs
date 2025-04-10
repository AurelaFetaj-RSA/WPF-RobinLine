using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using WPF_App.ViewModels;

namespace WPF_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private string _confirmationMessage;
        private string _popupAction;
        //private readonly OpcUaClientService _opcUaClient;

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

        public MainWindow()
        {
            InitializeComponent();
            StartTimer();
            //_opcUaClient = opcUaClient;
            DataContext = new MainViewModel();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Update every second
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //DateTimeText.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            DateTimeText.Text = DateTime.Now.ToString("ddd, dd.MM.yy / HH:mm:ss");

        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationMessage = "Are you sure you want to exit the application?";
            ConfirmPopup.IsOpen = true;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the app stop running the app
            Application.Current.Shutdown();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            //nothing happends close the popup
            ConfirmPopup.IsOpen = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}