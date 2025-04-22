using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using WPF_App.Services;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly OpcUaClientService _opcUaClient = new OpcUaClientService();

        public SettingsView()
        {
            InitializeComponent();
            //LoadLanguage("en-US"); // Set default language
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            LanguagePopup.IsOpen = !LanguagePopup.IsOpen;
        }

        private void SetLanguage(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string cultureCode)
            {
                LoadLanguage(cultureCode);
                LanguagePopup.IsOpen = false;
            }
        }

        private void LoadLanguage(string cultureCode)
        {
            try
            {
                // Create CultureInfo instance
                CultureInfo culture = new CultureInfo(cultureCode);

                // Create ResourceDictionary and load the appropriate language file
                ResourceDictionary dict = new ResourceDictionary
                {
                    Source = new Uri($"/Resources/Localization/Dictionary-{cultureCode}.xaml", UriKind.Relative)
                };

                // Remove old dictionaries and add the new one
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(dict);

                // Set the application culture for UI threads
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                // Debugging: Output to verify dictionary is loaded and culture is set
                Console.WriteLine($"Language changed to: {cultureCode}");
                Console.WriteLine($"Dictionary file used: /Resources/Localization/Dictionary-{cultureCode}.xaml");

                // Refresh the UI to reflect changes
                Application.Current.MainWindow?.UpdateLayout(); // Force layout update
            }
            catch (Exception ex)
            {
                // Show error message in case of failure
                MessageBox.Show($"Error loading language: {ex.Message}");
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string serverAddress = OpcUaServerAddressTextBox.Text.Trim();

            if (string.IsNullOrEmpty(serverAddress))
            {
                MessageBox.Show("Please enter an OPC UA server address.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Disable the button during connection attempt
                ConnectButton.IsEnabled = false;
                ConnectButton.Content = "Connecting...";

                var config = App.ServiceProvider.GetService<OpcUaConfigService>();
                config.ServerAddress = serverAddress;

                await _opcUaClient.InitializeAsync();
                await _opcUaClient.ConnectAsync(config.ServerAddress);
                MessageBox.Show("Connected successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable the button
                ConnectButton.IsEnabled = true;
                ConnectButton.Content = "Connect";
            }
        }

    }
}
