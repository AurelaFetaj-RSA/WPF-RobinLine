using FontAwesome.Sharp;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_App.Services;
using WPF_App.Views;

namespace WPF_App.ViewModels
{

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly OpcUaClientService _opcUaClient;
        private bool _isLoading;
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged(nameof(CanSwitchTabs));
            }
        }
        public bool CanSwitchTabs => !IsLoading;

        public ICommand ShowAutomaticViewCommand { get; }
        public ICommand ShowManualViewCommand { get; }
        public ICommand ShowProductivityViewCommand { get; }
        public ICommand ShowRecpieViewCommand { get; }
        public ICommand ShowDeviceViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }
        public ICommand ShowAlarmsViewCommand { get; }
        public ICommand ShowHideViewCommand { get; }
        // Add more commands for other views

        public MainViewModel()
        {
            //_opcUaClient = opcUaClient;

            CurrentView = new AutomaticView();

            ShowAutomaticViewCommand = new RelayCommand(ShowAutomaticView, () => CanSwitchTabs);
            ShowManualViewCommand = new RelayCommand(ShowManualView, () => CanSwitchTabs);
            ShowProductivityViewCommand = new RelayCommand(ShowProductivityView, () => CanSwitchTabs);
            ShowRecpieViewCommand = new RelayCommand(ShowRecipeView, () => CanSwitchTabs);
            ShowDeviceViewCommand = new RelayCommand(ShowDeviceView, () => CanSwitchTabs);
            ShowSettingsViewCommand = new RelayCommand(ShowSettingsView, () => CanSwitchTabs);
            ShowAlarmsViewCommand = new RelayCommand(ShowAlarmsView, () => CanSwitchTabs);
            ShowHideViewCommand = new RelayCommand(ShowHideView, () => CanSwitchTabs);
            // Initialize other commands
        }

        private async void ShowAutomaticView()
        {
            //CurrentView = new AutomaticView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new AutomaticView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowManualView()
        {
            //CurrentView = new ManualView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new ManualView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowProductivityView()
        {
            //CurrentView = new ProductivityView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new ProductivityView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowRecipeView()
        {
            //CurrentView = new RecipeView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new RecipeView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowDeviceView()
        {
            //CurrentView = new DeviceView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new DeviceView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowSettingsView()
        {
            //CurrentView = new SettingsView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new SettingsView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowAlarmsView()
        {
            //CurrentView = new AlarmsView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new AlarmsView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ShowHideView()
        {
            //CurrentView = new HideView();
            try
            {
                IsLoading = true;
                // Simulate loading (replace with your actual loading logic)
                await Task.Delay(200);
                CurrentView = new HideView();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Implement other methods to show different views

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}