using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using WPF_App.Services;

namespace WPF_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddSingleton<OpcUaConfigService>(); // Share config
            services.AddSingleton<OpcUaClientService>(); // Share OPC UA client
            ServiceProvider = services.BuildServiceProvider();


            // Force English on startup
            CultureInfo culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            base.OnStartup(e);
            //Task.Run(() => {
            //    var client = new OpcUaClientService();
            //    client.InitializeAsync();
            //});
        }
    }

}
