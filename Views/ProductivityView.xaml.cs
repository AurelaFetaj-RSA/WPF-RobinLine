using System.Windows;
using System.Windows.Controls;
using WPF_RobinLine.Models;
using static Xceed.Wpf.Toolkit.Calculator;

namespace WPF_App.Views
{
    /// <summary>
    /// Interaction logic for RFIDView.xaml
    /// </summary>
    public partial class ProductivityView : UserControl
    {
        public ProductivityView()
        {
            InitializeComponent();
        }

        public async void OperatorIsActive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)OperatorIsActive.IsChecked;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling operator is active toggle: {ex.Message}");
            }
        }

        private void AddOperatorButton_Click(object sender, RoutedEventArgs e)
        {
            //// Get values from UI controls
            //string operatorName = OperatorName.Text;
            //string operatorRole = OperatorRole.Text;
            //DateTime? hiredDate = OperatorHiredDate.SelectedDate;  // DatePicker value
            //bool isActive = OperatorIsActive.IsChecked ?? false;  // ToggleButton value

            //// Check if the required fields are filled out
            //if (string.IsNullOrEmpty(operatorName) || string.IsNullOrEmpty(operatorRole) || !hiredDate.HasValue)
            //{
            //    MessageBox.Show("Please fill in all fields.");
            //    return;
            //}

            //// Create a new operator object (assuming you have an Operator class)
            //var newOperator = new Operator
            //{
            //    Name = operatorName,
            //    Role = operatorRole,
            //    HiredDate = hiredDate.Value,
            //    IsActive = isActive
            //};

            //using (var context = new ProductivityDbContext())  
            //{
            //    context.Operators.Add(newOperator);  
            //    context.SaveChanges();
            //}

            //MessageBox.Show("Operator added successfully!");

            //OperatorName.Clear();
            //OperatorRole.Clear();
            //OperatorHiredDate.SelectedDate = null;
            //OperatorIsActive.IsChecked = false;
        }

    }
}
