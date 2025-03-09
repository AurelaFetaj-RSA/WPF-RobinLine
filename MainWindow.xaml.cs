using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using WPF_App.Services;
using WPF_App.ViewModels;

namespace WPF_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        //private readonly OpcUaClientService _opcUaClient;

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
    }
}