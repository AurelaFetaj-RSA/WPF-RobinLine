﻿using DataAccess.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WPF_RobinLine.Services;
using Separator = LiveCharts.Wpf.Separator;
using Size = DataAccess.Models.Size;

namespace WPF_App.Views
{
    public partial class ProductivityView : UserControl
    {
        private readonly DatabaseService _dbService;
        private int _currentItemId;
        private int _currentOperatorId;
        private int _currentProductionId;
        private int _currentResultId;
        private string _currentItemModelName;
        private string selectedModel = "";
        private enum FilterType { None, SingleDate, DateRange, Month }
        private FilterType _currentFilterType = FilterType.None;
        private readonly Dictionary<Type, Action> _columnConfigurations;



        public ProductivityView()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadRolesIntoComboBox();
            Loaded += (s, e) => LoadOperatorsIntoComboBox();
            Loaded += (s, e) => LoadShiftsIntoComboBox();
            string dbPath = WPF_RobinLine.Properties.Settings.Default.DbLocalPath;
            _dbService = new DatabaseService(
                $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbPath}"
            );
            //LoadModelNames();
            LoadSizes();
            //LoadProductionSummaries();

            PickedDateFilter.SelectedDateChanged += PickedDateFilter_SelectedDateChanged;
            StartDateFilter.SelectedDateChanged += RangeDateFilter_SelectedDateChanged;
            EndDateFilter.SelectedDateChanged += RangeDateFilter_SelectedDateChanged;

            _columnConfigurations = new Dictionary<Type, Action>
            {
                { typeof(Operator), () => ConfigureOperatorColumns() },
                { typeof(Role), () => ConfigureRoleColumns() }
                // Add more configurations as needed
            };
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            ShowItemsWaste(null, null);
            
        }

        private void ShowItemsWaste(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new ProductivityDbContext())
                {
                    DateTime today = DateTime.Today;

                    string sql = $@"
            SELECT 
                CONVERT(DATE, p.Timestamp) AS ProductionDate,
                DATEPART(HOUR, p.Timestamp) AS HourOfDay,
                FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
                FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
                COUNT(*) AS ItemsProduced,
                SUM(CASE WHEN r.is_defective = 1 THEN 1 ELSE 0 END) AS WasteProduced,
                0 AS SizeProduced,
                0 AS ModelsProduced,
                0 AS TypeProduced,
                0 AS PartTypeProduced
            FROM 
                Production p
            JOIN 
                Result r ON p.result_id = r.result_id
            WHERE 
                CONVERT(DATE, p.Timestamp) = '{today:yyyy-MM-dd}'
            GROUP BY 
                CONVERT(DATE, p.Timestamp),
                DATEPART(HOUR, p.Timestamp),
                FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
                FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00')
            ORDER BY 
                ProductionDate,
                HourOfDay";

                    var summaries = context.HourlyProductions
                        .FromSqlRaw(sql)
                        .AsNoTracking()
                        .ToList();

                    var itemsValues = new ChartValues<int>();
                    var wasteValues = new ChartValues<int>();
                    var labels = new List<string>();

                    foreach (var summary in summaries)
                    {
                        itemsValues.Add(summary.ItemsProduced);
                        wasteValues.Add(summary.WasteProduced);
                        labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductionChart.Series.Clear();

                        var itemsSeries = new ColumnSeries
                        {
                            Title = "Items Produced",
                            Fill = new SolidColorBrush(Colors.Blue),
                            Values = itemsValues
                        };

                        var wasteSeries = new ColumnSeries
                        {
                            Title = "Waste Produced",
                            Fill = new SolidColorBrush(Colors.Green),
                            Values = wasteValues
                        };

                        ProductionChart.Series.Add(itemsSeries);
                        ProductionChart.Series.Add(wasteSeries);

                        //ProductionChart.AxisX[0].Labels = labels;
                        ProdChart.Text = "Items & Waste (Today)";
                        ProductionChart.AxisX.Clear();

                        ProductionChart.AxisX.Add(new Axis
                        {
                            Title = "Hour",
                            Labels = labels,
                            FontSize = 9,
                            LabelsRotation = 20,

                            Separator = new LiveCharts.Wpf.Separator
                            {
                                Step = 2,
                                IsEnabled = false
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading production data: {ex.Message}", "Error");
            }
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
            // Get values from UI controls
            string operatorName = OperatorName.Text;
            string operatorRole = OperatorRoleComboBox.Text;
            DateTime? hiredDate = OperatorHiredDate.SelectedDate;
            bool isActive = OperatorIsActive.IsChecked ?? false;

            if (OperatorRoleComboBox.SelectedItem == null ||
                OperatorRoleComboBox.SelectedItem is not Role selectedRole ||
                selectedRole.RoleId == -1) // Check if placeholder is selected
            {
                MessageBox.Show("Please select a valid role.");
                return;
            }

            // Check if the required fields are filled out
            if (string.IsNullOrEmpty(operatorName) || string.IsNullOrEmpty(operatorRole) || !hiredDate.HasValue)
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Create a new operator object (assuming you have an Operator class)
            var newOperator = new Operator
            {
                Name = operatorName,
                Role = operatorRole,
                HiredDate = hiredDate.Value,
                IsActive = isActive,
                RoleId = selectedRole.RoleId
            };

            using (var context = new ProductivityDbContext())
            {
                context.Operators.Add(newOperator);
                context.SaveChanges();
            }

            MessageBox.Show("Operator added successfully!");

            OperatorName.Clear();
            //OperatorRoleComboBox.SelectedIndex = 0;
            OperatorRoleComboBox.SelectedItem = null;
            OperatorHiredDate.SelectedDate = null;
            OperatorIsActive.IsChecked = false;
        }

        private void AddRoleButton_Click(object sender, RoutedEventArgs e)
        {
            string roleName = RoleNameTxt.Text;

            if (string.IsNullOrEmpty(roleName))
            {
                MessageBox.Show("Please fill in the field.");
                return;
            }

            var newRole = new Role
            {
                RoleName = roleName
            };

            using (var context = new ProductivityDbContext())
            {
                context.Roles.Add(newRole);
                context.SaveChanges();
            }

            MessageBox.Show("Role added successfully!");

            RoleNameTxt.Clear();

            LoadRolesIntoComboBox();
        }

        private void AddShiftButton_Click(object sender, RoutedEventArgs e)
        {
            string shiftDescription = ShiftDescTxt.Text;
            TimeSpan startTime = StartTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero;
            TimeSpan endTime = EndTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero;
            DateTime? hiredDate = OperatorHiredDate.SelectedDate;
            bool isActive = ShiftIsActiveToggl.IsChecked ?? false;

            if (!int.TryParse(TargetProductionTxt.Text, out int targetProduction) || targetProduction < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Target Production");
                TargetProductionTxt.Focus();
                return;
            }

            // Validate time range
            //if (startTime >= endTime)
            //{
            //    MessageBox.Show("End time must be after start time");
            //    return;
            //}

            var newShift = new Shift
            {
                Description = shiftDescription,
                TargetProd = targetProduction,
                StartTime = startTime,
                EndTime = endTime,
                IsActive = isActive
            };

            using (var context = new ProductivityDbContext())
            {
                context.Shifts.Add(newShift);
                context.SaveChanges();
            }

            MessageBox.Show("Shift added successfully!");

            ShiftDescTxt.Clear();
            TargetProductionTxt.Clear();
            StartTimePicker.Value = null;
            EndTimePicker.Value = null;
            ShiftIsActiveToggl.IsChecked = false;
        }

        private void AddSizeButton_Click(object sender, RoutedEventArgs e)
        {
            string sizeValue = SizeValueTxt.Text;

            if (string.IsNullOrEmpty(sizeValue))
            {
                MessageBox.Show("Please fill in the field.");
                return;
            }

            if (!int.TryParse(sizeValue, out int numericValue))
            {
                MessageBox.Show("Please enter a valid number.");
                return;
            }

            var newSize = new Size
            {
                SizeType = SizeTypeComboBox.Text,
                SizeValue = sizeValue
            };

            using (var context = new ProductivityDbContext())
            {
                context.Sizes.Add(newSize);
                context.SaveChanges();
            }

            MessageBox.Show("Size added successfully!");

            SizeValueTxt.Clear();
            SizeTypeComboBox.SelectedItem = null;
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(OperatorComboBox.SelectedItem is Operator selectedOperator) || selectedOperator.OperatorId <= 0)
            {
                MessageBox.Show("Please select a valid operator.");
                return;
            }

            if (!(TypeComboBox.SelectedItem is ComboBoxItem selectedType) || !int.TryParse(selectedType.Tag?.ToString(), out int typeValue))
            {
                MessageBox.Show("Please select a valid type.");
                return;
            }

            if (!(PartTypeComboBox.SelectedItem is ComboBoxItem selectedPartType) || !int.TryParse(selectedPartType.Tag?.ToString(), out int partTypeValue))
            {
                MessageBox.Show("Please select a valid part type.");
                return;
            }

            //if (!(SizeComboBox.SelectedItem is ComboBoxItem selectedSizeItem) ||
            //    !int.TryParse(selectedSizeItem.Content?.ToString(), out int sizeValue))
            //{
            //    MessageBox.Show("Please select a valid shoe size.");
            //    return;
            //}

            if (ShoeSizeComboBox.SelectedValue == null || !int.TryParse(ShoeSizeComboBox.SelectedValue.ToString(), out int selectedSizeId) || selectedSizeId <= 0)
            {
                MessageBox.Show("Please select a valid shoe size.");
                return;
            }

            var newItem = new Item
            {
                ModelName = ModelNamesComboBox.Text,
                Type = typeValue,
                SizeId = selectedSizeId,
                PartType = partTypeValue,
                OperatorId = selectedOperator.OperatorId,
                Timestamp = DateTime.Now
            };

            using (var context = new ProductivityDbContext())
            {
                context.Items.Add(newItem);
                context.SaveChanges();
            }

            MessageBox.Show("Item added successfully!");

            _currentItemId = newItem.ItemId;
            _currentOperatorId = newItem.OperatorId.Value;

            FaliurePanel.Visibility = Visibility.Hidden;

            ProductionPanel.Visibility = Visibility.Visible;
            CurrentProdItemText.Text = $"Item #{_currentItemId} - {newItem.ModelName}";

            _currentItemModelName = newItem.ModelName;

            ModelNamesComboBox.SelectedItem = null;
            TypeComboBox.SelectedItem = null;
            PartTypeComboBox.SelectedItem = null;
            ShoeSizeComboBox.SelectedItem = null;
            OperatorComboBox.SelectedItem = null;
        }

        private void StartProduction_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new ProductivityDbContext())
            {
                var _timeNow = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                //var now = DateTime.Now.TimeOfDay;

                //var currentShift = context.Shifts
                //.Where(s => _timeNow >= s.StartTime && _timeNow <= s.EndTime && s.IsActive == true)
                //.FirstOrDefault();

                var currentShift = context.Shifts
                .Where(s => s.IsActive == true && (
                    // Same day shift
                    (s.StartTime <= s.EndTime && _timeNow >= s.StartTime && _timeNow <= s.EndTime) ||
                    // Overnight shift
                    (s.StartTime > s.EndTime && (_timeNow >= s.StartTime || _timeNow <= s.EndTime))
                ))
                .FirstOrDefault();

                var production = new Production
                {
                    ItemId = _currentItemId,
                    ShiftId = currentShift.ShiftId,
                    OperatorId = _currentOperatorId,
                    Timestamp = DateTime.Now
                };

                context.Productions.Add(production);
                context.SaveChanges();
                _currentProductionId = production.ProdId;
            }

            MessageBox.Show("Production started successfully!");

            // Show completion controls
            CompletionPanel.Visibility = Visibility.Visible;
        }

        private void AddResult_Click(object sender, RoutedEventArgs e)
        {
            bool isDefective = ResultIsDefective.IsChecked ?? false;

            if (!int.TryParse(CycleTimeTxt.Text, out int cycleTime) || cycleTime < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Cycle time");
                CycleTimeTxt.Focus();
                return;
            }

            var newResult = new Result
            {
                CycleTime = cycleTime,
                IsDefective = isDefective
            };

            using (var context = new ProductivityDbContext())
            {
                context.Results.Add(newResult);
                context.SaveChanges();

                // Update production with result ID
                //var production = context.Productions.Find(_currentProductionId);
                //production.ResultId = newResult.ResultId;
                //context.SaveChanges();

                context.Database.ExecuteSqlRaw(
                    @"UPDATE Production 
                      SET result_id = {0}
                      WHERE prod_id = {1}",
                    newResult.ResultId,
                    _currentProductionId
                );

                if (isDefective)
                {
                    // Get the failure that was created by the trigger
                    var failure = context.Failures
                        .FirstOrDefault(f => f.ResultId == newResult.ResultId);

                    if (failure != null)
                    {
                        MessageBox.Show("Result added successfully!");

                        _currentResultId = newResult.ResultId;

                        CompletionPanel.Visibility = Visibility.Hidden;
                        ProductionPanel.Visibility = Visibility.Hidden;


                        FaliurePanel.Visibility = Visibility.Visible;
                        CurrentFailureItemText.Text = $"Item #{_currentItemId} - {_currentItemModelName}";
                        CurrentFailureDesc.Text = $"{failure.Description}";
                        CurrentFailureSeverity.Text = $"{failure.Severity}";
                    }
                }
            }

            MessageBox.Show("Result added successfully!");

            _currentResultId = newResult.ResultId;

            CompletionPanel.Visibility = Visibility.Hidden;
            ProductionPanel.Visibility = Visibility.Hidden;
        }

        private void LoadSummary_Click(object sender, RoutedEventArgs e)
        {
            if (ProductionSummaryGrid.Visibility == Visibility.Collapsed)
            {
                //ProductionSummaryGrid.Visibility = Visibility.Visible;

                using (var context = new ProductivityDbContext())
                {
                    var summaries = context.ProductionSummaries.FromSqlRaw(@"
                    SELECT 
                    CONVERT(DATE, p.Timestamp) AS ProductionDate,
                    FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00–' + 
                    FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS ProductionHourRange,
                    COUNT(*) AS ItemsProduced,
                    SUM(CASE WHEN r.is_defective = 1 THEN 1 ELSE 0 END) AS DefectiveItems,
                    s.Description AS ShiftName
                    FROM 
                        Production p
                    JOIN 
                        Result r ON p.result_id = r.result_id
                    JOIN 
                        Shift s ON p.shift_id = s.shift_id
                    WHERE 
                        p.Timestamp >= DATEADD(DAY, -7, GETDATE())
                    GROUP BY 
                        CONVERT(DATE, p.Timestamp),
                        DATEPART(HOUR, p.Timestamp),
                        s.shift_id,
                        s.Description
                    ORDER BY 
                    ProductionDate, DATEPART(HOUR, p.Timestamp), s.shift_id
                    ")
                     .ToList();

                    //return result;

                    ProductionSummaryGrid.ItemsSource = summaries;
                    ProductionSummaryGrid.Visibility = Visibility.Visible;
                    ProductionColumnGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ProductionSummaryGrid.Visibility = Visibility.Collapsed;
                ProductionColumnGrid.Visibility = Visibility.Visible;
            }
        }

        private void PickedDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PickedDateFilter.SelectedDate.HasValue)
            {
                // Only clear range filters if this is an explicit user action
                if (_currentFilterType != FilterType.SingleDate)
                {
                    StartDateFilter.SelectedDate = null;
                    EndDateFilter.SelectedDate = null;
                }
                _currentFilterType = FilterType.SingleDate;
            }
            else if (_currentFilterType == FilterType.SingleDate)
            {
                _currentFilterType = FilterType.None;
            }
        }

        private void RangeDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue)
            {
                // Only clear single date filter if this is an explicit user action
                if (_currentFilterType != FilterType.DateRange)
                {
                    PickedDateFilter.SelectedDate = null;
                }
                _currentFilterType = FilterType.DateRange;
            }
            else if (_currentFilterType == FilterType.DateRange)
            {
                _currentFilterType = FilterType.None;
            }
        }

        //private void ShowItems(object sender, RoutedEventArgs e)
        //{
        //    DateTime? selectedDate = PickedDateFilter.SelectedDate;
        //    DateTime? startDate = StartDateFilter.SelectedDate;
        //    DateTime? endDate = EndDateFilter.SelectedDate;
        //    var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

        //    try
        //    {
        //        using (var context = new ProductivityDbContext())
        //        {
        //            DateTime fromDate;
        //            DateTime toDate;
        //            bool isSingleDay;

        //            if (_currentFilterType == FilterType.SingleDate && PickedDateFilter.SelectedDate.HasValue)
        //            {
        //                fromDate = toDate = PickedDateFilter.SelectedDate.Value;
        //                isSingleDay = true;
        //            }
        //            else if (_currentFilterType == FilterType.DateRange &&
        //                    (StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue))
        //            {
        //                fromDate = StartDateFilter.SelectedDate ?? DateTime.MinValue;
        //                toDate = EndDateFilter.SelectedDate ?? DateTime.MaxValue;
        //                isSingleDay = false;
        //            }
        //            else
        //            {
        //                // Default to today if no active filters
        //                fromDate = toDate = DateTime.Today;
        //                isSingleDay = true;
        //                _currentFilterType = FilterType.None;
        //            }

        //            string sql;

        //            if (isSingleDay)
        //            {
        //                // Single day - show hourly breakdown
        //                sql = $@"
        //                SELECT 
        //                    CONVERT(DATE, p.Timestamp) AS ProductionDate,
        //                    DATEPART(HOUR, p.Timestamp) AS HourOfDay,
        //                    FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
        //                    FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
        //                    COUNT(*) AS ItemsProduced,
        //                    0 AS WasteProduced,
        //                    0 AS SizeProduced,
        //                    0 AS ModelsProduced,
        //                    0 AS TypeProduced,
        //                    0 AS PartTypeProduced
        //                FROM 
        //                    Production p
        //                WHERE 
        //                    CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'";

        //                if (selectedShift != null && selectedShift.Value != 0)
        //                    sql += $" AND p.shift_Id = {selectedShift.Value}";

        //                sql += @"
        //                GROUP BY 
        //                    CONVERT(DATE, p.Timestamp),
        //                    DATEPART(HOUR, p.Timestamp)
        //                ORDER BY 
        //                    ProductionDate,
        //                    HourOfDay";
        //            }
        //            else
        //            {
        //                // Multiple days - show daily totals
        //                sql = $@"
        //                SELECT 
        //                    CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
        //                    0 AS HourOfDay,
        //                    '' AS HourRange,
        //                    COUNT(*) AS ItemsProduced,
        //                    0 AS WasteProduced,
        //                    0 AS SizeProduced,
        //                    0 AS ModelsProduced,
        //                    0 AS TypeProduced,
        //                    0 AS PartTypeProduced
        //                FROM 
        //                    Production p
        //                WHERE 
        //                    CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'";

        //                if (selectedShift != null && selectedShift.Value != 0)
        //                    sql += $" AND p.shift_Id = {selectedShift.Value}";

        //                sql += @"
        //                GROUP BY 
        //                    CONVERT(DATE, p.Timestamp)
        //                ORDER BY 
        //                    ProductionDate";
        //            }

        //            var summaries = context.HourlyProductions
        //                .FromSqlRaw(sql)
        //                .AsNoTracking()
        //                .ToList();

        //            // Update chart data
        //            var values = new ChartValues<int>();
        //            var labels = new List<string>();

        //            foreach (var summary in summaries)
        //            {
        //                values.Add(summary.ItemsProduced);
        //                if (isSingleDay)
        //                {
        //                    // Use HourRange label like "08:00"
        //                    labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
        //                }
        //                else
        //                {
        //                    // Use date label like "May 19" or whatever format you prefer
        //                    labels.Add(summary.ProductionDate?.ToString("MMM dd") ?? "");
        //                }
        //            }

        //            Dispatcher.Invoke(() =>
        //            {
        //                ProductionChart.Series.Clear();

        //                var itemsSeries = new ColumnSeries
        //                {
        //                    Title = "Items",
        //                    Fill = new SolidColorBrush(Colors.Blue),
        //                    Values = values
        //                };

        //                ProductionChart.Series.Add(itemsSeries);

        //                // Configure X axis
        //                ProductionChart.AxisX.Clear();
        //                ProductionChart.AxisX.Add(new Axis
        //                {
        //                    Title = isSingleDay ? "Hour" : "Date",
        //                    Labels = labels,
        //                    FontSize = 9,
        //                    LabelsRotation = 20,

        //                    Separator = new LiveCharts.Wpf.Separator 
        //                    { 
        //                        Step = 2, 
        //                        IsEnabled = false 
        //                    }
        //                });

        //                // Configure Y axis conditionally
        //                ProductionChart.AxisY.Clear();

        //                if (!isSingleDay)
        //                {
        //                    // Daily view - use steps of 100
        //                    ProductionChart.AxisY.Add(new Axis
        //                    {
        //                        MinValue = 0,
        //                        Separator = new LiveCharts.Wpf.Separator
        //                        {
        //                            Step = 200,
        //                            IsEnabled = true
        //                        },
        //                        LabelFormatter = value => value.ToString("N0")
        //                    });
        //                }
        //                else
        //                {
        //                    ProductionChart.AxisY.Add(new Axis
        //                    {
        //                        MinValue = 0,
        //                        Separator = new LiveCharts.Wpf.Separator
        //                        {
        //                            Step = 10,
        //                            IsEnabled = true
        //                        },
        //                        LabelFormatter = value => value.ToString("N0")
        //                    });
        //                }

        //                ProdChart.Text = "Items";
        //            });

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error loading production data: {ex.Message}", "Error");
        //    }
        //}

        private void ShowItems(object sender, RoutedEventArgs e)
        {
            try
            {
                // First determine which filter is being applied
                bool isMonthFilterSelected = MonthFilterComboBox.SelectedItem != null;
                bool isSingleDateSelected = PickedDateFilter.SelectedDate.HasValue;
                bool isDateRangeSelected = StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue;

                // Clear other filters based on which one is selected
                if (isMonthFilterSelected)
                {
                    // Clear date filters when month is selected
                    PickedDateFilter.SelectedDate = null;
                    StartDateFilter.SelectedDate = null;
                    EndDateFilter.SelectedDate = null;
                }
                else if (isSingleDateSelected || isDateRangeSelected)
                {
                    // Clear month filter when date filters are selected
                    MonthFilterComboBox.SelectedItem = null;
                }

                DateTime? selectedDate = PickedDateFilter.SelectedDate;
                DateTime? startDate = StartDateFilter.SelectedDate;
                DateTime? endDate = EndDateFilter.SelectedDate;
                var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

                // Safer month parsing
                int? selectedMonth = null;
                if (MonthFilterComboBox.SelectedItem is ComboBoxItem selectedMonthItem &&
                    selectedMonthItem.Tag != null &&
                    int.TryParse(selectedMonthItem.Tag.ToString(), out int month))
                {
                    selectedMonth = month;
                }

                using (var context = new ProductivityDbContext())
                {
                    DateTime fromDate;
                    DateTime toDate;
                    bool isSingleDay;
                    bool isMonthFilter = false;

                    // Determine which filter is active (order matters!)
                    if (selectedDate.HasValue)
                    {
                        // Single date filter is active
                        fromDate = toDate = selectedDate.Value;
                        isSingleDay = true;
                        _currentFilterType = FilterType.SingleDate;
                    }
                    else if (startDate.HasValue || endDate.HasValue)
                    {
                        // Date range filter is active
                        fromDate = startDate ?? DateTime.MinValue;
                        toDate = endDate ?? DateTime.MaxValue;
                        isSingleDay = false;
                        _currentFilterType = FilterType.DateRange;
                    }
                    else if (selectedMonth.HasValue)
                    {
                        // Month filter is active
                        fromDate = new DateTime(DateTime.Now.Year, selectedMonth.Value, 1);
                        toDate = fromDate.AddMonths(1).AddDays(-1);
                        isSingleDay = false;
                        isMonthFilter = true;
                        _currentFilterType = FilterType.Month;
                    }
                    else
                    {
                        // NO FILTERS SELECTED - DEFAULT TO TODAY
                        fromDate = toDate = DateTime.Today;
                        isSingleDay = true;
                        _currentFilterType = FilterType.None;
                    }

                    string sql;

                    if (isSingleDay)
                    {
                        sql = $@"
                SELECT 
                    CONVERT(DATE, p.Timestamp) AS ProductionDate,
                    DATEPART(HOUR, p.Timestamp) AS HourOfDay,
                    FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
                    FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
                    COUNT(*) AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS SizeProduced,
                    0 AS ModelsProduced,
                    0 AS TypeProduced,
                    0 AS PartTypeProduced
                FROM 
                    Production p
                WHERE 
                    CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'";
                    }
                    else if (isMonthFilter)
                    {
                        sql = $@"
                SELECT 
                    CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
                    0 AS HourOfDay,
                    '' AS HourRange,
                    COUNT(*) AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS SizeProduced,
                    0 AS ModelsProduced,
                    0 AS TypeProduced,
                    0 AS PartTypeProduced
                FROM 
                    Production p
                WHERE 
                    MONTH(p.Timestamp) = {selectedMonth.Value}
                    AND YEAR(p.Timestamp) = {fromDate.Year}";
                    }
                    else
                    {
                        sql = $@"
                SELECT 
                    CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
                    0 AS HourOfDay,
                    '' AS HourRange,
                    COUNT(*) AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS SizeProduced,
                    0 AS ModelsProduced,
                    0 AS TypeProduced,
                    0 AS PartTypeProduced
                FROM 
                    Production p
                WHERE 
                    CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'";
                    }

                    if (selectedShift != null && selectedShift.Value != 0)
                    {
                        sql += $" AND p.shift_Id = {selectedShift.Value}";
                    }

                    sql += isSingleDay ? @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp),
                    DATEPART(HOUR, p.Timestamp)
                ORDER BY 
                    ProductionDate,
                    HourOfDay" : @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp)
                ORDER BY 
                    ProductionDate";

                    var summaries = context.HourlyProductions
                        .FromSqlRaw(sql)
                        .AsNoTracking()
                        .ToList();

                    UpdateChart(summaries, isSingleDay);

                    ClearAllFilters();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading production data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateChart(List<HourlyProduction> summaries, bool isSingleDay)
        {
            var values = new ChartValues<int>();
            var labels = new List<string>();

            foreach (var summary in summaries)
            {
                values.Add(summary.ItemsProduced);
                labels.Add(isSingleDay
                    ? summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00"
                    : summary.ProductionDate?.ToString("MMM dd") ?? "");
            }

            Dispatcher.Invoke(() =>
            {
                ProductionChart.Series.Clear();
                ProductionChart.Series.Add(new ColumnSeries
                {
                    Title = "Items",
                    Fill = new SolidColorBrush(Colors.Blue),
                    Values = values
                });

                ProductionChart.AxisX.Clear();
                ProductionChart.AxisX.Add(new Axis
                {
                    Title = isSingleDay ? "Hour" : "Date",
                    Labels = labels,
                    FontSize = 9,
                    LabelsRotation = 20,
                    Separator = new LiveCharts.Wpf.Separator { Step = 2, IsEnabled = false }
                });

                ProductionChart.AxisY.Clear();
                ProductionChart.AxisY.Add(new Axis
                {
                    MinValue = 0,
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = isSingleDay ? 10 : 200,
                        IsEnabled = true
                    },
                    LabelFormatter = value => value.ToString("N0")
                });

                ProdChart.Text = "Items";
            });
        }

        private void ClearAllFilters()
        {
            PickedDateFilter.SelectedDate = null;
            StartDateFilter.SelectedDate = null;
            EndDateFilter.SelectedDate = null;
            MonthFilterComboBox.SelectedItem = null;
            ShiftFilterComboBox.SelectedIndex = -1;
            _currentFilterType = FilterType.None;
        }

        //private void ShowWaste(object sender, RoutedEventArgs e)
        //{
        //    DateTime? selectedDate = PickedDateFilter.SelectedDate;
        //    DateTime? startDate = StartDateFilter.SelectedDate;
        //    DateTime? endDate = EndDateFilter.SelectedDate;
        //    var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

        //    try
        //    {
        //        using (var context = new ProductivityDbContext())
        //        {
        //            DateTime fromDate;
        //            DateTime toDate;
        //            bool isSingleDay;

        //            // Handle date logic
        //            //if (startDate.HasValue && endDate.HasValue)
        //            //{
        //            //    fromDate = startDate.Value.Date;
        //            //    toDate = endDate.Value.Date;
        //            //}
        //            //else if (startDate.HasValue)
        //            //{
        //            //    fromDate = toDate = startDate.Value.Date;
        //            //}
        //            //else if (endDate.HasValue)
        //            //{
        //            //    fromDate = toDate = endDate.Value.Date;
        //            //}
        //            //else
        //            //{
        //            //    fromDate = toDate = selectedDate ?? DateTime.Today;
        //            //}

        //            //bool isSingleDay = fromDate == toDate;

        //            if (_currentFilterType == FilterType.SingleDate && PickedDateFilter.SelectedDate.HasValue)
        //            {
        //                fromDate = toDate = PickedDateFilter.SelectedDate.Value;
        //                isSingleDay = true;
        //            }
        //            else if (_currentFilterType == FilterType.DateRange &&
        //                    (StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue))
        //            {
        //                fromDate = StartDateFilter.SelectedDate ?? DateTime.MinValue;
        //                toDate = EndDateFilter.SelectedDate ?? DateTime.MaxValue;
        //                isSingleDay = false;
        //            }
        //            else
        //            {
        //                // Default to today if no active filters
        //                fromDate = toDate = DateTime.Today;
        //                isSingleDay = true;
        //                _currentFilterType = FilterType.None;
        //            }

        //            string sql;

        //            if (isSingleDay)
        //            {
        //                // Single day - show hourly breakdown
        //                sql = $@"
        //        SELECT 
        //            CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
        //            DATEPART(HOUR, p.Timestamp) AS HourOfDay,
        //            FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
        //            FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
        //            0 AS ItemsProduced,
        //            0 AS SizeProduced,
        //            0 AS ModelsProduced,
        //            0 AS TypeProduced,
        //            0 AS PartTypeProduced,
        //            SUM(CASE WHEN r.is_defective = 1 THEN 1 ELSE 0 END) AS WasteProduced
        //        FROM 
        //            Production p
        //        JOIN 
        //            Result r 
        //        ON 
        //            p.result_id = r.result_id
        //        WHERE 
        //            CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'";

        //                if (selectedShift != null && selectedShift.Value != 0)
        //                    sql += $" AND p.shift_Id = {selectedShift.Value}";

        //                sql += @"
        //        GROUP BY 
        //            CONVERT(DATE, p.Timestamp),
        //            DATEPART(HOUR, p.Timestamp)
        //        ORDER BY 
        //            ProductionDate,
        //            HourOfDay";
        //            }
        //            else
        //            {
        //                // Multiple days - show daily totals
        //                sql = $@"
        //        SELECT 
        //            CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
        //            0 AS HourOfDay,
        //            '' AS HourRange,
        //            0 AS ItemsProduced,
        //            0 AS SizeProduced,
        //            0 AS ModelsProduced,
        //            0 AS TypeProduced,
        //            0 AS PartTypeProduced,
        //            SUM(CASE WHEN r.is_defective = 1 THEN 1 ELSE 0 END) AS WasteProduced
        //        FROM 
        //            Production p
        //        JOIN 
        //            Result r 
        //        ON 
        //            p.result_id = r.result_id
        //        WHERE 
        //            CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'";

        //                if (selectedShift != null && selectedShift.Value != 0)
        //                    sql += $" AND p.shift_Id = {selectedShift.Value}";

        //                sql += @"
        //        GROUP BY 
        //            CONVERT(DATE, p.Timestamp)
        //        ORDER BY 
        //            ProductionDate";
        //            }

        //            var summaries = context.HourlyProductions
        //                .FromSqlRaw(sql)
        //                .AsNoTracking()
        //                .ToList();

        //            // Update chart data
        //            var values = new ChartValues<int>();
        //            var labels = new List<string>();

        //            foreach (var summary in summaries)
        //            {
        //                values.Add(summary.WasteProduced);
        //                if (isSingleDay)
        //                {
        //                    // Use HourRange label like "08:00"
        //                    labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
        //                }
        //                else
        //                {
        //                    // Use date label like "May 19" or whatever format you prefer
        //                    labels.Add(summary.ProductionDate?.ToString("MMM dd") ?? "");
        //                }
        //            }

        //            Dispatcher.Invoke(() =>
        //            {
        //                ProductionChart.Series.Clear();

        //                var wasteSeries = new ColumnSeries
        //                {
        //                    Title = "Waste",
        //                    Fill = new SolidColorBrush(Colors.Green),
        //                    Values = values
        //                };

        //                ProductionChart.Series.Add(wasteSeries);
        //                ProductionChart.AxisX[0].Labels = labels;
        //                ProdChart.Text = "Waste";
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error loading waste data: {ex.Message}", "Error");
        //    }
        //}

        private void ShowWaste(object sender, RoutedEventArgs e)
        {
            try
            {
                // First determine which filter is being applied
                bool isMonthFilterSelected = MonthFilterComboBox.SelectedItem != null;
                bool isSingleDateSelected = PickedDateFilter.SelectedDate.HasValue;
                bool isDateRangeSelected = StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue;

                // Clear other filters based on which one is selected
                if (isMonthFilterSelected)
                {
                    // Clear date filters when month is selected
                    PickedDateFilter.SelectedDate = null;
                    StartDateFilter.SelectedDate = null;
                    EndDateFilter.SelectedDate = null;
                }
                else if (isSingleDateSelected || isDateRangeSelected)
                {
                    // Clear month filter when date filters are selected
                    MonthFilterComboBox.SelectedItem = null;
                }

                DateTime? selectedDate = PickedDateFilter.SelectedDate;
                DateTime? startDate = StartDateFilter.SelectedDate;
                DateTime? endDate = EndDateFilter.SelectedDate;
                var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

                // Safer month parsing
                int? selectedMonth = null;
                if (MonthFilterComboBox.SelectedItem is ComboBoxItem selectedMonthItem &&
                    selectedMonthItem.Tag != null &&
                    int.TryParse(selectedMonthItem.Tag.ToString(), out int month))
                {
                    selectedMonth = month;
                }

                using (var context = new ProductivityDbContext())
                {
                    DateTime fromDate;
                    DateTime toDate;
                    bool isSingleDay;
                    bool isMonthFilter = false;

                    // Determine which filter is active (order matters!)
                    if (selectedDate.HasValue)
                    {
                        // Single date filter is active
                        fromDate = toDate = selectedDate.Value;
                        isSingleDay = true;
                        _currentFilterType = FilterType.SingleDate;
                    }
                    else if (startDate.HasValue || endDate.HasValue)
                    {
                        // Date range filter is active
                        fromDate = startDate ?? DateTime.MinValue;
                        toDate = endDate ?? DateTime.MaxValue;
                        isSingleDay = false;
                        _currentFilterType = FilterType.DateRange;
                    }
                    else if (selectedMonth.HasValue)
                    {
                        // Month filter is active
                        fromDate = new DateTime(DateTime.Now.Year, selectedMonth.Value, 1);
                        toDate = fromDate.AddMonths(1).AddDays(-1);
                        isSingleDay = false;
                        isMonthFilter = true;
                        _currentFilterType = FilterType.Month;
                    }
                    else
                    {
                        // NO FILTERS SELECTED - DEFAULT TO TODAY
                        fromDate = toDate = DateTime.Today;
                        isSingleDay = true;
                        _currentFilterType = FilterType.None;
                    }

                    string sql;

                    if (isSingleDay)
                    {
                        // Single day - show hourly breakdown
                        sql = $@"
        SELECT 
            CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
            DATEPART(HOUR, p.Timestamp) AS HourOfDay,
            FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
            FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
            0 AS ItemsProduced,
            0 AS SizeProduced,
            0 AS ModelsProduced,
            0 AS TypeProduced,
            0 AS PartTypeProduced,
            SUM(CASE WHEN r.is_defective = 1 THEN 1 ELSE 0 END) AS WasteProduced
        FROM 
            Production p
        JOIN 
            Result r 
        ON 
            p.result_id = r.result_id
        WHERE 
            CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
        GROUP BY 
            CONVERT(DATE, p.Timestamp),
            DATEPART(HOUR, p.Timestamp)
        ORDER BY 
            ProductionDate,
            HourOfDay";
                    }
                    else if (isMonthFilter)
                    {
                        // Month filter - show daily totals for the month
                        sql = $@"
        SELECT 
            CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
            0 AS HourOfDay,
            '' AS HourRange,
            0 AS ItemsProduced,
            0 AS SizeProduced,
            0 AS ModelsProduced,
            0 AS TypeProduced,
            0 AS PartTypeProduced,
            SUM(CASE WHEN r.is_defective = 1 THEN 1 ELSE 0 END) AS WasteProduced
        FROM 
            Production p
        JOIN 
            Result r 
        ON 
            p.result_id = r.result_id
        WHERE 
            MONTH(p.Timestamp) = {selectedMonth.Value}
            AND YEAR(p.Timestamp) = {fromDate.Year}";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
        GROUP BY 
            CONVERT(DATE, p.Timestamp)
        ORDER BY 
            ProductionDate";
                    }
                    else
                    {
                        // Multiple days - show daily totals
                        sql = $@"
        SELECT 
            CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
            0 AS HourOfDay,
            '' AS HourRange,
            0 AS ItemsProduced,
            0 AS SizeProduced,
            0 AS ModelsProduced,
            0 AS TypeProduced,
            0 AS PartTypeProduced,
            SUM(CASE WHEN r.is_defective = 1 THEN 1 ELSE 0 END) AS WasteProduced
        FROM 
            Production p
        JOIN 
            Result r 
        ON 
            p.result_id = r.result_id
        WHERE 
            CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
        GROUP BY 
            CONVERT(DATE, p.Timestamp)
        ORDER BY 
            ProductionDate";
                    }

                    var summaries = context.HourlyProductions
                        .FromSqlRaw(sql)
                        .AsNoTracking()
                        .ToList();

                    // Update chart data
                    var values = new ChartValues<int>();
                    var labels = new List<string>();

                    foreach (var summary in summaries)
                    {
                        values.Add(summary.WasteProduced);
                        if (isSingleDay)
                        {
                            // Use HourRange label like "08:00"
                            labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
                        }
                        else
                        {
                            // Use date label like "May 19" or whatever format you prefer
                            labels.Add(summary.ProductionDate?.ToString("MMM dd") ?? "");
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductionChart.Series.Clear();

                        var wasteSeries = new ColumnSeries
                        {
                            Title = "Waste",
                            Fill = new SolidColorBrush(Colors.Red),  // Changed to red for waste to differentiate
                            Values = values
                        };

                        ProductionChart.Series.Add(wasteSeries);

                        // Update X-axis labels based on time period
                        ProductionChart.AxisX.Clear();
                        ProductionChart.AxisX.Add(new Axis
                        {
                            Title = isSingleDay ? "Hour" : "Date",
                            Labels = labels,
                            FontSize = 9,
                            LabelsRotation = 20,
                            Separator = new LiveCharts.Wpf.Separator { Step = 2, IsEnabled = false }
                        });

                        // Update Y-axis
                        ProductionChart.AxisY.Clear();
                        ProductionChart.AxisY.Add(new Axis
                        {
                            MinValue = 0,
                            Separator = new LiveCharts.Wpf.Separator
                            {
                                Step = isSingleDay ? 1 : 5,  // Adjusted step for waste counts which are typically lower
                                IsEnabled = true
                            },
                            LabelFormatter = value => value.ToString("N0")
                        });

                        ProdChart.Text = "Waste";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading waste data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Model_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get filter values
                string modelName = Model.SelectedItem as string;
                if (string.IsNullOrEmpty(modelName)) return;

                var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

                using (var context = new ProductivityDbContext())
                {
                    DateTime fromDate;
                    DateTime toDate;
                    bool isSingleDay;

                    // Use the same filter logic as other methods
                    if (_currentFilterType == FilterType.SingleDate && PickedDateFilter.SelectedDate.HasValue)
                    {
                        fromDate = toDate = PickedDateFilter.SelectedDate.Value;
                        isSingleDay = true;
                    }
                    else if (_currentFilterType == FilterType.DateRange &&
                            (StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue))
                    {
                        fromDate = StartDateFilter.SelectedDate ?? DateTime.MinValue;
                        toDate = EndDateFilter.SelectedDate ?? DateTime.MaxValue;
                        isSingleDay = false;
                    }
                    else
                    {
                        // Default to today if no active filters
                        fromDate = toDate = DateTime.Today;
                        isSingleDay = true;
                    }

                    string sql;

                    if (isSingleDay)
                    {
                        // Single day query
                        sql = $@"
                        SELECT 
                            CONVERT(DATE, p.Timestamp) AS ProductionDate,
                            DATEPART(HOUR, p.Timestamp) AS HourOfDay,
                            FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
                            FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
                            COUNT(*) AS ModelsProduced,
                            0 AS SizeProduced,
                            0 AS ItemsProduced,
                            0 AS WasteProduced,
                            0 AS TypeProduced,
                            0 AS PartTypeProduced
                        FROM 
                            Production p
                        JOIN 
                            Item i ON p.item_id = i.item_id
                        WHERE 
                            CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'
                        AND    
                            i.model_name = '{modelName}'";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                        GROUP BY 
                            CONVERT(DATE, p.Timestamp),
                            DATEPART(HOUR, p.Timestamp)
                        ORDER BY 
                            ProductionDate,
                            HourOfDay";
                    }
                    else
                    {
                        // Date range query
                        sql = $@"
                        SELECT 
                            CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
                            0 AS HourOfDay,
                            '' AS HourRange,
                            COUNT(*) AS ModelsProduced,
                            0 AS SizeProduced,
                            0 AS ItemsProduced,
                            0 AS WasteProduced,
                            0 AS TypeProduced,
                            0 AS PartTypeProduced
                        FROM 
                            Production p
                        JOIN 
                            Item i ON p.item_id = i.item_id
                        WHERE 
                            CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'
                        AND    
                            i.model_name = '{modelName}'";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                        GROUP BY 
                            CONVERT(DATE, p.Timestamp)
                        ORDER BY 
                            ProductionDate";
                    }

                    var summaries = context.HourlyProductions
                        .FromSqlRaw(sql)
                        .AsNoTracking()
                        .ToList();

                    // Update chart data
                    var values = new ChartValues<int>();
                    var labels = new List<string>();

                    foreach (var summary in summaries)
                    {
                        values.Add(summary.ModelsProduced);
                        if (isSingleDay)
                        {
                            labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
                        }
                        else
                        {
                            labels.Add(summary.ProductionDate?.ToString("MMM dd") ?? "");
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductionChart.Series.Clear();

                        var modelSeries = new ColumnSeries
                        {
                            Title = $"Model {modelName}",
                            Fill = new SolidColorBrush(Colors.Orange),
                            Values = values
                        };

                        ProductionChart.Series.Add(modelSeries);
                        ProductionChart.AxisX[0].Labels = labels;
                        ProdChart.Text = $"Model {modelName}";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading model production data: {ex.Message}", "Error");
            }
        }

        private void Size_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get filter values
                if (SizeChart.SelectedValue == null || !int.TryParse(SizeChart.SelectedValue.ToString(), out int selectedSizeId) || selectedSizeId <= 0)
                {
                    MessageBox.Show("Please select a valid shoe size.");
                    return;
                }

                string sizeName = "";
                if (SizeChart.SelectedItem != null)
                {
                    dynamic selectedItem = SizeChart.SelectedItem;
                    sizeName = selectedItem.DisplayText;
                }

                var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

                using (var context = new ProductivityDbContext())
                {
                    DateTime fromDate;
                    DateTime toDate;
                    bool isSingleDay;

                    // Use the same filter logic as other methods
                    if (_currentFilterType == FilterType.SingleDate && PickedDateFilter.SelectedDate.HasValue)
                    {
                        fromDate = toDate = PickedDateFilter.SelectedDate.Value;
                        isSingleDay = true;
                    }
                    else if (_currentFilterType == FilterType.DateRange &&
                            (StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue))
                    {
                        fromDate = StartDateFilter.SelectedDate ?? DateTime.MinValue;
                        toDate = EndDateFilter.SelectedDate ?? DateTime.MaxValue;
                        isSingleDay = false;
                    }
                    else
                    {
                        // Default to today if no active filters
                        fromDate = toDate = DateTime.Today;
                        isSingleDay = true;
                    }

                    string sql;

                    if (isSingleDay)
                    {
                        // Single day query
                        sql = $@"
                SELECT 
                    CONVERT(DATE, p.Timestamp) AS ProductionDate,
                    DATEPART(HOUR, p.Timestamp) AS HourOfDay,
                    FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
                    FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
                    COUNT(*) AS SizeProduced,
                    0 AS ModelsProduced,
                    0 AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS TypeProduced,
                    0 AS PartTypeProduced
                FROM 
                    Production p
                JOIN 
                    Item i ON p.item_id = i.item_id
                WHERE 
                    CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'
                AND    
                    i.size_id = {selectedSizeId}";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp),
                    DATEPART(HOUR, p.Timestamp)
                ORDER BY 
                    ProductionDate,
                    HourOfDay";
                    }
                    else
                    {
                        // Date range query
                        sql = $@"
                SELECT 
                    CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
                    0 AS HourOfDay,
                    '' AS HourRange,
                    COUNT(*) AS SizeProduced,
                    0 AS ModelsProduced,
                    0 AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS TypeProduced,
                    0 AS PartTypeProduced
                FROM 
                    Production p
                JOIN 
                    Item i ON p.item_id = i.item_id
                WHERE 
                    CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'
                AND    
                    i.size_id = {selectedSizeId}";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp)
                ORDER BY 
                    ProductionDate";
                    }

                    var summaries = context.HourlyProductions
                        .FromSqlRaw(sql)
                        .AsNoTracking()
                        .ToList();

                    // Update chart data
                    var values = new ChartValues<int>();
                    var labels = new List<string>();

                    foreach (var summary in summaries)
                    {
                        values.Add(summary.SizeProduced);
                        if (isSingleDay)
                        {
                            labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
                        }
                        else
                        {
                            labels.Add(summary.ProductionDate?.ToString("MMM dd") ?? "");
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductionChart.Series.Clear();

                        var sizeSeries = new ColumnSeries
                        {
                            Title = $"Size {sizeName}",
                            Fill = new SolidColorBrush(Colors.Purple),
                            Values = values
                        };

                        ProductionChart.Series.Add(sizeSeries);
                        ProductionChart.AxisX[0].Labels = labels;
                        ProdChart.Text = $"Size {sizeName}";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading size production data: {ex.Message}", "Error");
            }
        }

        private void Type_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get filter values
                if (!(TypeChart.SelectedItem is ComboBoxItem selectedType) || !int.TryParse(selectedType.Tag?.ToString(), out int typeValue))
                {
                    MessageBox.Show("Please select a valid type.");
                    return;
                }

                string typeName = selectedType.Content.ToString();
                var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

                using (var context = new ProductivityDbContext())
                {
                    DateTime fromDate;
                    DateTime toDate;
                    bool isSingleDay;

                    // Use the same filter logic as other methods
                    if (_currentFilterType == FilterType.SingleDate && PickedDateFilter.SelectedDate.HasValue)
                    {
                        fromDate = toDate = PickedDateFilter.SelectedDate.Value;
                        isSingleDay = true;
                    }
                    else if (_currentFilterType == FilterType.DateRange &&
                            (StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue))
                    {
                        fromDate = StartDateFilter.SelectedDate ?? DateTime.MinValue;
                        toDate = EndDateFilter.SelectedDate ?? DateTime.MaxValue;
                        isSingleDay = false;
                    }
                    else
                    {
                        // Default to today if no active filters
                        fromDate = toDate = DateTime.Today;
                        isSingleDay = true;
                    }

                    string sql;

                    if (isSingleDay)
                    {
                        // Single day query
                        sql = $@"
                SELECT 
                    CONVERT(DATE, p.Timestamp) AS ProductionDate,
                    DATEPART(HOUR, p.Timestamp) AS HourOfDay,
                    FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
                    FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
                    COUNT(*) AS TypeProduced,
                    0 AS ModelsProduced,
                    0 AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS SizeProduced,
                    0 AS PartTypeProduced
                FROM 
                    Production p
                JOIN 
                    Item i ON p.item_id = i.item_id
                WHERE 
                    CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'
                AND    
                    i.type = {typeValue}";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp),
                    DATEPART(HOUR, p.Timestamp)
                ORDER BY 
                    ProductionDate,
                    HourOfDay";
                    }
                    else
                    {
                        // Date range query
                        sql = $@"
                SELECT 
                    CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
                    0 AS HourOfDay,
                    '' AS HourRange,
                    COUNT(*) AS TypeProduced,
                    0 AS ModelsProduced,
                    0 AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS SizeProduced,
                    0 AS PartTypeProduced
                FROM 
                    Production p
                JOIN 
                    Item i ON p.item_id = i.item_id
                WHERE 
                    CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'
                AND    
                    i.type = {typeValue}";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp)
                ORDER BY 
                    ProductionDate";
                    }

                    var summaries = context.HourlyProductions
                        .FromSqlRaw(sql)
                        .AsNoTracking()
                        .ToList();

                    // Update chart data
                    var values = new ChartValues<int>();
                    var labels = new List<string>();

                    foreach (var summary in summaries)
                    {
                        values.Add(summary.TypeProduced);
                        if (isSingleDay)
                        {
                            labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
                        }
                        else
                        {
                            labels.Add(summary.ProductionDate?.ToString("MMM dd") ?? "");
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductionChart.Series.Clear();

                        var typeSeries = new ColumnSeries
                        {
                            Title = $"{typeName} Type",
                            Fill = new SolidColorBrush(Colors.Pink),
                            Values = values
                        };

                        ProductionChart.Series.Add(typeSeries);
                        ProductionChart.AxisX[0].Labels = labels;
                        ProdChart.Text = $"{typeName} Type";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading type production data: {ex.Message}", "Error");
            }
        }

        private void PartType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get filter values
                if (!(PartTypeChart.SelectedItem is ComboBoxItem selectedPartType) ||
                    !int.TryParse(selectedPartType.Tag?.ToString(), out int partTypeValue))
                {
                    MessageBox.Show("Please select a valid part type.");
                    return;
                }

                string partTypeName = selectedPartType.Content.ToString();
                var selectedShift = ShiftFilterComboBox.SelectedValue as int?;

                using (var context = new ProductivityDbContext())
                {
                    DateTime fromDate;
                    DateTime toDate;
                    bool isSingleDay;

                    // Use the same filter logic as other methods
                    if (_currentFilterType == FilterType.SingleDate && PickedDateFilter.SelectedDate.HasValue)
                    {
                        fromDate = toDate = PickedDateFilter.SelectedDate.Value;
                        isSingleDay = true;
                    }
                    else if (_currentFilterType == FilterType.DateRange &&
                            (StartDateFilter.SelectedDate.HasValue || EndDateFilter.SelectedDate.HasValue))
                    {
                        fromDate = StartDateFilter.SelectedDate ?? DateTime.MinValue;
                        toDate = EndDateFilter.SelectedDate ?? DateTime.MaxValue;
                        isSingleDay = false;
                    }
                    else
                    {
                        // Default to today if no active filters
                        fromDate = toDate = DateTime.Today;
                        isSingleDay = true;
                    }

                    string sql;

                    if (isSingleDay)
                    {
                        // Single day query
                        sql = $@"
                SELECT 
                    CONVERT(DATE, p.Timestamp) AS ProductionDate,
                    DATEPART(HOUR, p.Timestamp) AS HourOfDay,
                    FORMAT(DATEPART(HOUR, p.Timestamp), '00') + ':00-' + 
                    FORMAT((DATEPART(HOUR, p.Timestamp) + 1) % 24, '00') + ':00' AS HourRange,
                    COUNT(*) AS PartTypeProduced,
                    0 AS ModelsProduced,
                    0 AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS SizeProduced,
                    0 AS TypeProduced
                FROM 
                    Production p
                JOIN 
                    Item i ON p.item_id = i.item_id
                WHERE 
                    CONVERT(DATE, p.Timestamp) = '{fromDate:yyyy-MM-dd}'
                AND    
                    i.part_type = {partTypeValue}";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp),
                    DATEPART(HOUR, p.Timestamp)
                ORDER BY 
                    ProductionDate,
                    HourOfDay";
                    }
                    else
                    {
                        // Date range query
                        sql = $@"
                SELECT 
                    CAST(CONVERT(DATE, p.Timestamp) AS DATETIME) AS ProductionDate,
                    0 AS HourOfDay,
                    '' AS HourRange,
                    COUNT(*) AS PartTypeProduced,
                    0 AS ModelsProduced,
                    0 AS ItemsProduced,
                    0 AS WasteProduced,
                    0 AS SizeProduced,
                    0 AS TypeProduced
                FROM 
                    Production p
                JOIN 
                    Item i ON p.item_id = i.item_id
                WHERE 
                    CONVERT(DATE, p.Timestamp) BETWEEN '{fromDate:yyyy-MM-dd}' AND '{toDate:yyyy-MM-dd}'
                AND    
                    i.part_type = {partTypeValue}";

                        if (selectedShift != null && selectedShift.Value != 0)
                            sql += $" AND p.shift_Id = {selectedShift.Value}";

                        sql += @"
                GROUP BY 
                    CONVERT(DATE, p.Timestamp)
                ORDER BY 
                    ProductionDate";
                    }

                    var summaries = context.HourlyProductions
                        .FromSqlRaw(sql)
                        .AsNoTracking()
                        .ToList();

                    // Update chart data
                    var values = new ChartValues<int>();
                    var labels = new List<string>();

                    foreach (var summary in summaries)
                    {
                        values.Add(summary.PartTypeProduced);
                        if (isSingleDay)
                        {
                            labels.Add(summary.HourRange?.Split('-')[0] ?? summary.HourOfDay.ToString("00") + ":00");
                        }
                        else
                        {
                            labels.Add(summary.ProductionDate?.ToString("MMM dd") ?? "");
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProductionChart.Series.Clear();

                        var partTypeSeries = new ColumnSeries
                        {
                            Title = $"{partTypeName} Part",
                            Fill = new SolidColorBrush(Colors.Red),
                            Values = values
                        };

                        ProductionChart.Series.Add(partTypeSeries);
                        ProductionChart.AxisX[0].Labels = labels;
                        ProdChart.Text = $"{partTypeName} Part";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading part type production data: {ex.Message}", "Error");
            }
        }

        private void LoadRolesIntoComboBox()
        {
            using (var context = new ProductivityDbContext())
            {
                // Get all roles from the database
                var roles = context.Roles.ToList();

                OperatorRoleComboBox.ItemsSource = roles;
                OperatorRoleComboBox.DisplayMemberPath = "RoleName";
                OperatorRoleComboBox.SelectedValuePath = "Id";
            }
        }

        private void LoadOperatorsIntoComboBox()
        {
            using (var context = new ProductivityDbContext())
            {
                // Get all roles from the database
                var roles = context.Operators.ToList();

                OperatorComboBox.ItemsSource = roles;
                OperatorComboBox.DisplayMemberPath = "Name";
                OperatorComboBox.SelectedValuePath = "Id";
            }
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
                System.Windows.MessageBox.Show($"Error loading models: {ex.Message}");
            }
        }

        private void LoadSizes()
        {
            using (var db = new ProductivityDbContext())
            {
                var sizes = db.Sizes
                    .Select(s => new
                    {
                        SizeId = s.SizeId,
                        DisplayText = $"{s.SizeType} {s.SizeValue}"
                    })
                    .ToList();

                //sizes.Insert(0, new { SizeId = 0, DisplayText = "-- Select Size --" });

                ShoeSizeComboBox.Items.Clear();
                ShoeSizeComboBox.ItemsSource = sizes;  // Bind directly
                ShoeSizeComboBox.DisplayMemberPath = "DisplayText";  // Show combined text
                ShoeSizeComboBox.SelectedValuePath = "SizeId";      // Store the ID

                SizeChart.Items.Clear();
                SizeChart.ItemsSource = sizes;
                SizeChart.DisplayMemberPath = "DisplayText";
                SizeChart.SelectedValuePath = "SizeId";
                //ShoeSizeComboBox.SelectedIndex = 0;
            }
        }

        private void LoadShiftsIntoComboBox()
        {
            using (var context = new ProductivityDbContext())
            {
                // Get all roles from the database
                var shifts = context.Shifts.ToList();

                shifts.Insert(0, new Shift
                {
                    ShiftId = 0,
                    Description = "Shift"
                });

                ShiftFilterComboBox.ItemsSource = shifts;
                ShiftFilterComboBox.DisplayMemberPath = "Description";
                ShiftFilterComboBox.SelectedValuePath = "ShiftId";
            }
        }

        private void valueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                fillPathRT.Angle = e.NewValue;

                var angle = 180 + e.NewValue;
                double radius = 62.5; // 250 / 4 (scaled down radius)
                double centerX = needleLine.X1;
                double centerY = needleLine.Y1;

                double radians = angle * Math.PI / 180;
                double x2 = radius * Math.Cos(radians) + centerX;
                double y2 = radius * Math.Sin(radians) + centerY;

                needleLine.X2 = x2;
                needleLine.Y2 = y2;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void OperatorData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OperatorDataGrid.Visibility = Visibility.Visible;
                ConfigureDataGrid(typeof(Operator));

                // Fetch data from database
                var operators = GetOperatorsFromDatabase();

                // Bind to DataGrid
                OperatorDataGrid.ItemsSource = operators;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operator data: {ex.Message}");
            }
        }

        private void RoleData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OperatorDataGrid.Visibility = Visibility.Visible;
                ConfigureDataGrid(typeof(Role));

                // Fetch data from database
                var roles = GetRolesFromDatabase(); 

                // Bind to DataGrid
                OperatorDataGrid.ItemsSource = roles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operator data: {ex.Message}");
            }
        }

        private void ShiftData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OperatorDataGrid.Visibility = Visibility.Visible;

                // Fetch data from database
                var shifts = GetShiftsFromDatabase();

                // Bind to DataGrid
                OperatorDataGrid.ItemsSource = shifts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operator data: {ex.Message}");
            }
        }

        private void SizeData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OperatorDataGrid.Visibility = Visibility.Visible;

                // Fetch data from database
                var sizes = GetSizesFromDatabase();

                // Bind to DataGrid
                OperatorDataGrid.ItemsSource = sizes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operator data: {ex.Message}");
            }
        }

        private void TargetData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OperatorDataGrid.Visibility = Visibility.Visible;

                // Fetch data from database
                var targets = GetTargetsFromDatabase();

                // Bind to DataGrid
                OperatorDataGrid.ItemsSource = targets;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operator data: {ex.Message}");
            }
        }

        private void ConfigureDataGrid(Type dataType)
        {
            OperatorDataGrid.Columns.Clear();

            if (_columnConfigurations.TryGetValue(dataType, out var configureAction))
            {
                configureAction();
            }
        }

        private void ConfigureOperatorColumns()
        {
            OperatorDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ID",
                Binding = new Binding("OperatorId"),
                Width = 80
            });
            OperatorDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Name",
                Binding = new Binding("Name"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            OperatorDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Role",
                Binding = new Binding("Role"),
                Width = 120
            });
            OperatorDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Hired Date",
                Binding = new Binding("HiredDate"),
                Width = 120
            });

            // Add other operator columns...
        }

        private void ConfigureRoleColumns()
        {
            OperatorDataGrid.Columns.Clear();

            OperatorDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Role ID",
                Binding = new Binding("RoleId"),
                Width = 80
            });
            OperatorDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Role Name",
                Binding = new Binding("RoleName"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            // Add other role columns...
        }


        private List<Operator> GetOperatorsFromDatabase()
        {
            try
            {
                using (var context = new ProductivityDbContext())
                {
                    return context.Operators.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operators: {ex.Message}");
                return new List<Operator>(); // Return empty list on error
            }
        }

        private List<Role> GetRolesFromDatabase()
        {
            try
            {
                using (var context = new ProductivityDbContext())
                {
                    return context.Roles.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operators: {ex.Message}");
                return new List<Role>(); // Return empty list on error
            }
        }

        private List<Shift> GetShiftsFromDatabase()
        {
            try
            {
                using (var context = new ProductivityDbContext())
                {
                    return context.Shifts.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operators: {ex.Message}");
                return new List<Shift>(); // Return empty list on error
            }
        }

        private List<Size> GetSizesFromDatabase()
        {
            try
            {
                using (var context = new ProductivityDbContext())
                {
                    return context.Sizes.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operators: {ex.Message}");
                return new List<Size>(); // Return empty list on error
            }
        }

        private List<Target> GetTargetsFromDatabase()
        {
            try
            {
                using (var context = new ProductivityDbContext())
                {
                    return context.Targets.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading operators: {ex.Message}");
                return new List<Target>(); // Return empty list on error
            }
        }

        private void YearTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(YearTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }


        public class HourlyProduction
        {
            public int HourOfDay { get; set; }
            public string? HourRange { get; set; }
            public DateTime? ProductionDate { get; set; }
            public int ItemsProduced { get; set; }
            public int WasteProduced { get; set; }
            public int ModelsProduced { get; set; }
            public int SizeProduced { get; set; }
            public int TypeProduced { get; set; }
            public int PartTypeProduced { get; set; }
        }
    }
}
