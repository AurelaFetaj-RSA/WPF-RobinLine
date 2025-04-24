using FontAwesome.Sharp;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_App.Services;
using WPF_App.Views;

namespace WPF_App.ViewModels
{
    //public class MainViewModel : ViewModelBase
    //{
    //    private ViewModelBase _currentViewModel;

    //    public ViewModelBase CurrentViewModel
    //    {
    //        get => _currentViewModel;
    //        set
    //        {
    //            _currentViewModel = value;
    //            OnPropertyChanged(nameof(CurrentViewModel));
    //        }
    //    }

    //    public ICommand ShowAutomaticViewCommand { get; }
    //    //public ICommand ShowRicetteViewCommand { get; }

    //    public MainViewModel()
    //    {
    //        ShowAutomaticViewCommand = new RelayCommand(_ => CurrentViewModel = new AutomaticViewModel());
    //        //ShowRicetteViewCommand = new RelayCommand(_ => CurrentViewModel = new RicetteViewModel());

    //        // Set default view
    //        CurrentViewModel = new AutomaticViewModel();
    //    }
    //}

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
            }
        }

        public ICommand ShowAutomaticViewCommand { get; }
        public ICommand ShowManualViewCommand { get; }
        public ICommand ShowRFIDViewCommand { get; }
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

            ShowAutomaticViewCommand = new RelayCommand(ShowAutomaticView);
            ShowManualViewCommand = new RelayCommand(ShowManualView);
            ShowRFIDViewCommand = new RelayCommand(ShowRFIDView);
            ShowRecpieViewCommand = new RelayCommand(ShowRecipeView);
            ShowDeviceViewCommand = new RelayCommand(ShowDeviceView);
            ShowSettingsViewCommand = new RelayCommand(ShowSettingsView);
            ShowAlarmsViewCommand = new RelayCommand(ShowAlarmsView);
            ShowHideViewCommand = new RelayCommand(ShowHideView);
            // Initialize other commands
        }

        private void ShowAutomaticView()
        {
            CurrentView = new AutomaticView();
        }

        private void ShowManualView()
        {
            CurrentView = new ManualView();
        }

        private void ShowRFIDView()
        {
            CurrentView = new RFIDView();
        }

        private void ShowRecipeView()
        {
            CurrentView = new RecipeView();
        }

        private void ShowDeviceView()
        {
            CurrentView = new DeviceView();
        }

        private void ShowSettingsView()
        {
            CurrentView = new SettingsView();
        }

        private void ShowAlarmsView()
        {
            CurrentView = new AlarmsView();
        }

        private void ShowHideView()
        {
            CurrentView = new HideView();
        }

        // Implement other methods to show different views

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}