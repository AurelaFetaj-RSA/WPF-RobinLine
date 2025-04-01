using System.Windows.Controls;
using static WPF_App.Views.AutomaticView;
using System.Windows.Media;
using WPF_App.Services;
using System.Windows.Media.Imaging;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for DeviceView.xaml
    /// </summary>
    public partial class DeviceView : UserControl
    {
        private readonly OpcUaClientService _opcUaClient = new OpcUaClientService();

        public DeviceView()
        {
            InitializeComponent();
            UpdateIOT();
            UpdateRobot1();
            UpdateRobot2();
        }

        private async void UpdateIOT()
        {
            var isConnected = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_primer");

            if (!isConnected)
            {
                //IOT_image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Robots/iot_off.jpg"));
                //ShowMessage("The oven 1 is running in automatic mode.", MessageType.Info);
            }
        }

        private async void UpdateRobot1()
        {
            var isConnected = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_primer");

            if (!isConnected)
            {
                //IOT_image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Robots/robotic-arm-off.jpg"));
                //ShowMessage("The oven 1 is running in automatic mode.", MessageType.Info);
            }
        }

        private async void UpdateRobot2()
        {
            var isConnected = await _opcUaClient.ReadBooleanAsync("ns=2;s=Tags.Eren_robin/pc_manuale_automatico_forno_primer");

            if (!isConnected)
            {
                // IOT_image.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Robots/robotic-arm-off.jpg"));
                //ShowMessage("The oven 1 is running in automatic mode.", MessageType.Info);
            }
        }
    }
}
