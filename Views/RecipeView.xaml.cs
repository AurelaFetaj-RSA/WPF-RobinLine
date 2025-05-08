using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using WPF_RobinLine.Services;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for RecipeView.xaml
    /// </summary>
    public partial class RecipeView : UserControl
    {
        private readonly DatabaseService _dbService;

        private int _r1centralLineToggleState = 0;
        private int _r1contourLineToggleState = 0;
        private int _r1internalContourLineToggleState = 0;
        private int _r2centralLineToggleState = 0;
        private int _r2contourLineToggleState = 0;
        private int _r2internalContourLineToggleState = 0;
        private int _editr1centralLineToggleState = 0;
        private int _editr1contourLineToggleState = 0;
        private int _editr1internalContourLineToggleState = 0;
        private int _editr2centralLineToggleState = 0;
        private int _editr2contourLineToggleState = 0;
        private int _editr2internalContourLineToggleState = 0;
        private string selectedModel = "";
        private List<string> _originalFlags = new List<string>();
        private Process onScreenKeyboardProc;

        public RecipeView()
        {
            InitializeComponent();

            string dbPath = WPF_RobinLine.Properties.Settings.Default.DbLocalPath;
            _dbService = new DatabaseService(
                $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbPath}"
            );

            EditR1CentralLineToggle.Checked += ToggleButton_CheckedUnchecked;
            EditR1CentralLineToggle.Unchecked += ToggleButton_CheckedUnchecked;
            EditR1ContourLineToggle.Checked += ToggleButton_CheckedUnchecked;
            EditR1ContourLineToggle.Unchecked += ToggleButton_CheckedUnchecked;
            EditR1InternalContourLineToggle.Checked += ToggleButton_CheckedUnchecked;
            EditR1InternalContourLineToggle.Unchecked += ToggleButton_CheckedUnchecked;

            EditR2CentralLineToggle.Checked += ToggleButton_CheckedUnchecked;
            EditR2CentralLineToggle.Unchecked += ToggleButton_CheckedUnchecked;
            EditR2ContourLineToggle.Checked += ToggleButton_CheckedUnchecked;
            EditR2ContourLineToggle.Unchecked += ToggleButton_CheckedUnchecked;
            EditR2InternalContourLineToggle.Checked += ToggleButton_CheckedUnchecked;
            EditR2InternalContourLineToggle.Unchecked += ToggleButton_CheckedUnchecked;
            LoadModelNames();
        }

        private void LoadModelNames()
        {
            try
            {
                var modelNames = _dbService.GetAllModelNames();

                // Bind to your ComboBox
                ModelNamesComboBox.ItemsSource = modelNames;

                // Optional: Add empty/default item
                modelNames.Insert(0, "Select Model");
                ModelNamesComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading models: {ex.Message}");
            }
        }

        private void ModelNamesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelNamesComboBox.SelectedItem == null ||
                ModelNamesComboBox.SelectedIndex == 0) return;

            selectedModel = ModelNamesComboBox.SelectedItem.ToString();
            var modelData = _dbService.GetModelNameRecord(selectedModel);

            // Update UI with current values
            EditR1CentralLineToggle.IsChecked = modelData[0] == "1";
            EditR1ContourLineToggle.IsChecked = modelData[1] == "1";
            EditR1InternalContourLineToggle.IsChecked = modelData[2] == "1";
            EditR2CentralLineToggle.IsChecked = modelData[3] == "1";
            EditR2ContourLineToggle.IsChecked = modelData[4] == "1";
            EditR2InternalContourLineToggle.IsChecked = modelData[5] == "1";

            _originalFlags = modelData;

            // Show update button
            UpdateModelButton.Visibility = Visibility.Visible;
            UpdateModelButton.IsEnabled = false;
        }

        private void ResetAddForm()
        {
            ModelNameTextBox.Clear();

            R1CentralLineToggle.IsChecked = false;
            R1ContourLineToggle.IsChecked = false;
            R1InternalContourLineToggle.IsChecked = false;
            R2CentralLineToggle.IsChecked = false;
            R2ContourLineToggle.IsChecked = false;
            R2InternalContourLineToggle.IsChecked = false;

            _r1centralLineToggleState = 0;
            _r1contourLineToggleState = 0;
            _r1internalContourLineToggleState = 0;
            _r2centralLineToggleState = 0;
            _r2contourLineToggleState = 0;
            _r2internalContourLineToggleState = 0;
        }

        private void ResetEditForm()
        {
            ModelNamesComboBox.SelectedIndex = 0;

            EditR1CentralLineToggle.IsChecked = false;
            EditR1ContourLineToggle.IsChecked = false;
            EditR1InternalContourLineToggle.IsChecked = false;
            EditR2CentralLineToggle.IsChecked = false;
            EditR2ContourLineToggle.IsChecked = false;
            EditR2InternalContourLineToggle.IsChecked = false;

            _editr1centralLineToggleState = 0;
            _editr1contourLineToggleState = 0;
            _editr1internalContourLineToggleState = 0;
            _editr2centralLineToggleState = 0;
            _editr2contourLineToggleState = 0;
            _editr2internalContourLineToggleState = 0;

            UpdateModelButton.Visibility = Visibility.Collapsed;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);
        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            IntPtr wow64Value = IntPtr.Zero;
            try
            {
                // Disable redirection
                Wow64DisableWow64FsRedirection(ref wow64Value);

                string oskPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "osk.exe");

                if (!File.Exists(oskPath))
                {
                    MessageBox.Show("On-Screen keyboard not found!");
                    return;
                }

                Process.Start(new ProcessStartInfo(oskPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                // Restore redirection
                if (wow64Value != IntPtr.Zero)
                    Wow64RevertWow64FsRedirection(wow64Value);
            }
        }

        public async void AddModelNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Get the model name from UI
                string modelName = ModelNameTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(modelName))
                {
                    MessageBox.Show("Please enter a model name");
                    return;
                }

                if (modelName.Length != 4)
                {
                    MessageBox.Show("Model name must be exactly 4 characters long.");
                    return;
                }

                // 2. Prepare the flags list (convert toggle states to strings)
                var flags = new List<string>
                {
                    _r1centralLineToggleState.ToString(),    // lav_00 equivalent
                    _r1contourLineToggleState.ToString(),    // au1_00 equivalent
                    _r1internalContourLineToggleState.ToString(), // au2_00 equivalent
                    _r2centralLineToggleState.ToString(),    // r2_lav_00 equivalent
                    _r2contourLineToggleState.ToString(),    // r2_au1_00 equivalent
                    _r2internalContourLineToggleState.ToString()  // r2_au2_00 equivalent
                };

                // 4. Call the database method
                _dbService.AddModelNameRecord(modelName, flags);

                ResetAddForm();

                MessageBox.Show("Model saved successfully!");
                ModelNameTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving model: {ex.Message}", "Error",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void DeleteModelNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedModel = ModelNamesComboBox.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selectedModel) || selectedModel == "Select Model")
                {
                    MessageBox.Show("Please select a model to delete");
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to delete model '{selectedModel}'?",
                                           "Confirm Delete",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Delete the model
                    _dbService.DeleteModelNameRecord(selectedModel);

                    // Refresh the model list
                    LoadModelNames();

                    MessageBox.Show($"Model '{selectedModel}' deleted successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting model: {ex.Message}", "Error",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //public async void ViewModelNameButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string selectedModel = ModelNamesComboBox.SelectedItem?.ToString();

        //        if (string.IsNullOrEmpty(selectedModel) || selectedModel == "-- Select Model --")
        //        {
        //            MessageBox.Show("Please select a model to view");
        //            return;
        //        }

        //        // Load model data
        //        var modelData = await Task.Run(() => _dbService.GetModelNameRecord(selectedModel));

        //        // Update UI with current values
        //        EditR1CentralLineToggle.IsChecked = modelData[0] == "1";
        //        EditR1ContourLineToggle.IsChecked = modelData[1] == "1";
        //        EditR1InternalContourLineToggle.IsChecked = modelData[2] == "1";
        //        EditR2CentralLineToggle.IsChecked = modelData[3] == "1";
        //        EditR2ContourLineToggle.IsChecked = modelData[4] == "1";
        //        EditR2InternalContourLineToggle.IsChecked = modelData[5] == "1";

        //        _originalFlags = modelData;

        //        // Show update button
        //        UpdateModelButton.Visibility = Visibility.Visible;
        //        UpdateModelButton.IsEnabled = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error viewing model: {ex.Message}");
        //    }
        //}

        private async void UpdateModelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedModel = ModelNamesComboBox.SelectedItem?.ToString();
                var currentFlags = GetCurrentToggleValues();

                var result = MessageBox.Show($"Are you sure you want to update model '{selectedModel}'?",
                                           "Confirm Update",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _dbService.UpdateModelNameRecord(selectedModel, currentFlags);

                    MessageBox.Show("Model updated successfully");

                    // Update original flags to new values
                    _originalFlags = currentFlags;
                    UpdateModelButton.IsEnabled = false;

                    ResetEditForm();
                }
                else
                {
                    ResetEditForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating model: {ex.Message}");
            }
        }

        private List<string> GetCurrentToggleValues()
        {
            return new List<string>
            {
                EditR1CentralLineToggle.IsChecked == true ? "1" : "0",
                EditR1ContourLineToggle.IsChecked == true ? "1" : "0",
                EditR1InternalContourLineToggle.IsChecked == true ? "1" : "0",
                EditR2CentralLineToggle.IsChecked == true ? "1" : "0",
                EditR2ContourLineToggle.IsChecked == true ? "1" : "0",
                EditR2InternalContourLineToggle.IsChecked == true ? "1" : "0"
            };
        }

        private void ToggleButton_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (UpdateModelButton.Visibility == Visibility.Visible)
            {
                // Compare current values with original
                var currentFlags = GetCurrentToggleValues();

                if (!_originalFlags.SequenceEqual(currentFlags))
                {
                    UpdateModelButton.IsEnabled = true;
                }
                else
                {
                    UpdateModelButton.IsEnabled = false;
                }
            }
        }

        public async void R1CentralLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)R1CentralLineToggle.IsChecked;
                _r1centralLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_r1centralLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 central line: {ex.Message}");
            }
        }

        public async void R1ContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)R1ContourLineToggle.IsChecked;
                _r1contourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_r1contourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 contour line: {ex.Message}");
            }
        }

        public async void R1InternalContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)R1InternalContourLineToggle.IsChecked;
                _r1internalContourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_r1internalContourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 internal contour line: {ex.Message}");
            }
        }

        public async void R2CentralLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)R2CentralLineToggle.IsChecked;
                _r2centralLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_r2centralLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 2 central line: {ex.Message}");
            }
        }

        public async void R2ContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)R2ContourLineToggle.IsChecked;
                _r2contourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_r2contourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 contour line: {ex.Message}");
            }
        }

        public async void R2InternalContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)R2InternalContourLineToggle.IsChecked;
                _r2internalContourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_r2internalContourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 internal contour line: {ex.Message}");
            }
        }

        public async void EditR1CentralLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)EditR1CentralLineToggle.IsChecked;
                _editr1centralLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_editr1centralLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 central line: {ex.Message}");
            }
        }

        public async void EditR1ContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)EditR1ContourLineToggle.IsChecked;
                _editr1contourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_editr1contourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 contour line: {ex.Message}");
            }
        }

        public async void EditR1InternalContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)EditR1InternalContourLineToggle.IsChecked;
                _editr1internalContourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_editr1internalContourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 1 internal contour line: {ex.Message}");
            }
        }

        public async void EditR2CentralLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)EditR2CentralLineToggle.IsChecked;
                _editr2centralLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_editr2centralLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 2 central line: {ex.Message}");
            }
        }

        public async void EditR2ContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)EditR2ContourLineToggle.IsChecked;
                _editr2contourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_editr2contourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 2 contour line: {ex.Message}");
            }
        }

        public async void EditR2InternalContourLineToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)EditR2InternalContourLineToggle.IsChecked;
                _editr2internalContourLineToggleState = isChecked ? 1 : 0;
                Console.WriteLine($"Toggle State: {_editr2internalContourLineToggleState}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling robot 2 internal contour line: {ex.Message}");
                MessageBox.Show($"Error toggling robot 2 internal contour line: {ex.Message}");
            }
        }
    }
}
