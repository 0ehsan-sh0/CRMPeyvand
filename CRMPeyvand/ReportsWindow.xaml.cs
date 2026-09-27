using BE;
using BLL;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using CRMPeyvand.Reports.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CRMPeyvand
{
    public partial class ReportsWindow : Window
    {
        private readonly UserBLL _ubll = new UserBLL();
        private readonly CustomerBLL _cbll = new CustomerBLL();
        private readonly ActivityBLL _abll = new ActivityBLL();
        private readonly CatalogItemBLL _pbll = new CatalogItemBLL();

        private enum ChartType
        {
            Column,
            Line,
            Point
        }

        private ChartType _currentChartType = ChartType.Column;
        private List<string> _currentLabels = new List<string>();
        private List<double> _currentValues = new List<double>();
        private string _currentSeriesTitle = "تعداد";

        public ReportsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpStart.SelectedDate = DateTime.Now.Date.AddDays(-30);
            dpEnd.SelectedDate = DateTime.Now.Date;

            // Load initial chart
            LoadInvoicesChartData();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static string ToPersianDate(DateTime date)
        {
            return PersianReportStyle.FormatPersianDate(date);
        }

        #region Chart Logic

        private void btnColumnChart_Click(object sender, RoutedEventArgs e)
        {
            _currentChartType = ChartType.Column;
            UpdateChartDisplay();
        }

        private void btnLineChart_Click(object sender, RoutedEventArgs e)
        {
            _currentChartType = ChartType.Line;
            UpdateChartDisplay();
        }

        private void btnPointChart_Click(object sender, RoutedEventArgs e)
        {
            _currentChartType = ChartType.Point;
            UpdateChartDisplay();
        }

        private void btnDisplayChart_Click(object sender, RoutedEventArgs e)
        {
            if (rbPrintInvoicesD.IsChecked == true)
            {
                LoadInvoicesChartData();
            }
            else if (rbPrintActivitiesD.IsChecked == true)
            {
                LoadActivitiesChartData();
            }
            else if (rbPrintCustomerD.IsChecked == true)
            {
                LoadCustomersChartData();
            }
        }

        private void LoadInvoicesChartData()
        {
            DateTime startDate = dpStart.SelectedDate ?? DateTime.Now.Date.AddDays(-30);
            DateTime endDate = dpEnd.SelectedDate ?? DateTime.Now.Date;

            var users = _ubll.ReadInvoicesList() ?? new List<User>();
            _currentLabels.Clear();
            _currentValues.Clear();
            _currentSeriesTitle = "تعداد فاکتورهای فروش";

            foreach (var user in users)
            {
                int count = user.Invoices?
                    .Count(i => !i.DeleteStatus && i.RegDate.Date >= startDate.Date && i.RegDate.Date <= endDate.Date) ?? 0;

                _currentLabels.Add(user.Name ?? user.UserName ?? "نامشخص");
                _currentValues.Add(count);
            }

            UpdateChartDisplay();
        }

        private void LoadActivitiesChartData()
        {
            DateTime startDate = dpStart.SelectedDate ?? DateTime.Now.Date.AddDays(-30);
            DateTime endDate = dpEnd.SelectedDate ?? DateTime.Now.Date;

            var users = _ubll.ReadActivitiesList() ?? new List<User>();
            _currentLabels.Clear();
            _currentValues.Clear();
            _currentSeriesTitle = "تعداد فعالیت‌ها";

            foreach (var user in users)
            {
                int count = user.Activities?
                    .Count(a => !a.DeleteStatus && a.RegDate.Date >= startDate.Date && a.RegDate.Date <= endDate.Date) ?? 0;

                _currentLabels.Add(user.Name ?? user.UserName ?? "نامشخص");
                _currentValues.Add(count);
            }

            UpdateChartDisplay();
        }

        private void LoadCustomersChartData()
        {
            DateTime startDate = dpStart.SelectedDate ?? DateTime.Now.Date.AddDays(-30);
            DateTime endDate = dpEnd.SelectedDate ?? DateTime.Now.Date;

            var customers = _cbll.ReadWithDateTime() ?? new List<Customer>();
            var filtered = customers.Where(c => !c.DeleteStatus && c.RegDate.Date >= startDate.Date && c.RegDate.Date <= endDate.Date).ToList();

            _currentLabels.Clear();
            _currentValues.Clear();
            _currentSeriesTitle = "ثبت‌نام مشتریان";

            // Group by registration date
            var grouped = filtered.GroupBy(c => c.RegDate.Date).OrderBy(g => g.Key);
            foreach (var group in grouped)
            {
                _currentLabels.Add(ToPersianDate(group.Key));
                _currentValues.Add(group.Count());
            }

            if (_currentLabels.Count == 0)
            {
                _currentLabels.Add("بدون داده");
                _currentValues.Add(0);
            }

            UpdateChartDisplay();
        }

        private void UpdateChartDisplay()
        {
            var seriesCollection = new SeriesCollection();
            var chartValues = new ChartValues<double>(_currentValues);

            if (_currentChartType == ChartType.Column)
            {
                seriesCollection.Add(new ColumnSeries
                {
                    Title = _currentSeriesTitle,
                    Values = chartValues,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB")),
                    MaxColumnWidth = 45
                });
            }
            else if (_currentChartType == ChartType.Line)
            {
                seriesCollection.Add(new LineSeries
                {
                    Title = _currentSeriesTitle,
                    Values = chartValues,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D9488")),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#150D9488")),
                    PointGeometrySize = 10
                });
            }
            else if (_currentChartType == ChartType.Point)
            {
                seriesCollection.Add(new ScatterSeries
                {
                    Title = _currentSeriesTitle,
                    Values = chartValues,
                    MinPointShapeDiameter = 12,
                    MaxPointShapeDiameter = 18,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E11D48"))
                });
            }

            MainChart.Series = seriesCollection;
            AxisX.Labels = _currentLabels.ToArray();
        }

        #endregion

        #region PDF Reporting Logic

        private void btnPrintGeneralReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rbPrintCustomer.IsChecked == true)
                {
                    var customers = _cbll.ReadWithDateTime() ?? new List<Customer>();
                    var model = new CustomerReportModel
                    {
                        ReportTitle = "لیست تمام مشتریان ثبت نام شده",
                        GeneratedDatePersian = ToPersianDate(DateTime.Now),
                        Customers = customers.Where(c => !c.DeleteStatus).Select((c, idx) => new CustomerRowModel
                        {
                            RowIndex = idx + 1,
                            Name = c.Name ?? "",
                            Phone = c.Phone ?? "",
                            RegDatePersian = ToPersianDate(c.RegDate)
                        }).ToList()
                    };
                    ReportViewerService.OpenReportPdf(new CustomerListDocument(model), "Customers");
                }
                else if (rbPrintActivities.IsChecked == true)
                {
                    var activities = _abll.ReadAllWithDetails() ?? new List<Activity>();
                    var model = new ActivityReportModel
                    {
                        ReportTitle = "لیست تمام فعالیت‌های ثبت شده",
                        GeneratedDatePersian = ToPersianDate(DateTime.Now),
                        Activities = activities.Where(a => !a.DeleteStatus).Select((a, idx) => new ActivityRowModel
                        {
                            RowIndex = idx + 1,
                            CustomerName = a.Customer?.Name ?? "",
                            UserName = a.User?.Name ?? a.User?.UserName ?? "",
                            CategoryTitle = a.ActivityCategory?.CategoryName ?? "",
                            Description = a.Info ?? a.Title ?? "",
                            DatePersian = ToPersianDate(a.RegDate),
                            Status = "ثبت شده"
                        }).ToList()
                    };
                    ReportViewerService.OpenReportPdf(new ActivityListDocument(model), "Activities");
                }
                else if (rbPrintThisWeek.IsChecked == true)
                {
                    GeneratePeriodicSalesReport("لیست فروش هفت روز گذشته", DateTime.Now.Date.AddDays(-7), DateTime.Now.Date, "WeeklySales");
                }
                else if (rbPrintThisMonth.IsChecked == true)
                {
                    GeneratePeriodicSalesReport("لیست فروش ماه گذشته (سی روز گذشته)", DateTime.Now.Date.AddDays(-30), DateTime.Now.Date, "MonthlySales");
                }
                else if (rbPrintThisYear.IsChecked == true)
                {
                    GeneratePeriodicSalesReport("لیست فروش سال گذشته (365 روز گذشته)", DateTime.Now.Date.AddDays(-365), DateTime.Now.Date, "YearlySales");
                }
                else if (rbPrintProducts.IsChecked == true)
                {
                    var products = _pbll.ReadAll() ?? new List<CatalogItem>();
                    var model = new CatalogItemReportModel
                    {
                        ReportTitle = "موجودی و ارزش محصولات انبار",
                        GeneratedDatePersian = ToPersianDate(DateTime.Now),
                        Items = products.Where(p => !p.DeleteStatus).Select((p, idx) => new CatalogItemRowModel
                        {
                            RowIndex = idx + 1,
                            Name = p.Name ?? "",
                            Kind = p.Kind == ItemKind.Good ? "کالا" : "خدمات",
                            Stock = p.Stock,
                            Price = (double)p.SalePrice
                        }).ToList()
                    };
                    ReportViewerService.OpenReportPdf(new CatalogItemListDocument(model), "ProductsTotal");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ایجاد گزارش:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPrintFilteredReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime startDate = dpStart.SelectedDate ?? DateTime.Now.Date.AddDays(-30);
                DateTime endDate = dpEnd.SelectedDate ?? DateTime.Now.Date;

                if (rbPrintInvoicesD.IsChecked == true)
                {
                    var users = _ubll.ReadInvoicesList() ?? new List<User>();
                    var userSales = new List<UserSalesRowModel>();
                    int rowIdx = 1;
                    foreach (var user in users)
                    {
                        var matching = user.Invoices?
                            .Where(i => !i.DeleteStatus && i.RegDate.Date >= startDate.Date && i.RegDate.Date <= endDate.Date)
                            .ToList() ?? new List<Invoice>();

                        userSales.Add(new UserSalesRowModel
                        {
                            RowIndex = rowIdx++,
                            UserName = user.Name ?? user.UserName ?? "",
                            InvoicesCount = matching.Count,
                            TotalAmount = (double)matching.Sum(i => i.Payable)
                        });
                    }

                    var model = new SalesSummaryReportModel
                    {
                        ReportTitle = "گزارش فروش کاربران بر اساس بازه زمانی",
                        GeneratedDatePersian = ToPersianDate(DateTime.Now),
                        StartDatePersian = ToPersianDate(startDate),
                        EndDatePersian = ToPersianDate(endDate),
                        UserSales = userSales
                    };
                    ReportViewerService.OpenReportPdf(new SalesSummaryDocument(model), "UserSalesPeriodic");
                }
                else if (rbPrintActivitiesD.IsChecked == true)
                {
                    var activities = (_abll.ReadAllWithDetails() ?? new List<Activity>())
                        .Where(a => !a.DeleteStatus && a.RegDate.Date >= startDate.Date && a.RegDate.Date <= endDate.Date)
                        .ToList();

                    var model = new ActivityReportModel
                    {
                        ReportTitle = "گزارش فعالیت‌ها بر اساس بازه زمانی",
                        GeneratedDatePersian = ToPersianDate(DateTime.Now),
                        StartDatePersian = ToPersianDate(startDate),
                        EndDatePersian = ToPersianDate(endDate),
                        Activities = activities.Select((a, idx) => new ActivityRowModel
                        {
                            RowIndex = idx + 1,
                            CustomerName = a.Customer?.Name ?? "",
                            UserName = a.User?.Name ?? a.User?.UserName ?? "",
                            CategoryTitle = a.ActivityCategory?.CategoryName ?? "",
                            Description = a.Info ?? a.Title ?? "",
                            DatePersian = ToPersianDate(a.RegDate),
                            Status = "ثبت شده"
                        }).ToList()
                    };
                    ReportViewerService.OpenReportPdf(new ActivityListDocument(model), "UserActivitiesPeriodic");
                }
                else if (rbPrintCustomerD.IsChecked == true)
                {
                    var customers = (_cbll.ReadWithDateTime() ?? new List<Customer>())
                        .Where(c => !c.DeleteStatus && c.RegDate.Date >= startDate.Date && c.RegDate.Date <= endDate.Date)
                        .ToList();

                    var model = new CustomerReportModel
                    {
                        ReportTitle = "گزارش ثبت‌نام مشتریان بر اساس بازه زمانی",
                        GeneratedDatePersian = ToPersianDate(DateTime.Now),
                        StartDatePersian = ToPersianDate(startDate),
                        EndDatePersian = ToPersianDate(endDate),
                        Customers = customers.Select((c, idx) => new CustomerRowModel
                        {
                            RowIndex = idx + 1,
                            Name = c.Name ?? "",
                            Phone = c.Phone ?? "",
                            RegDatePersian = ToPersianDate(c.RegDate)
                        }).ToList()
                    };
                    ReportViewerService.OpenReportPdf(new CustomerListDocument(model), "CustomersPeriodic");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ایجاد گزارش:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GeneratePeriodicSalesReport(string title, DateTime startDate, DateTime endDate, string filePrefix)
        {
            var users = _ubll.ReadInvoicesList() ?? new List<User>();
            var userSales = new List<UserSalesRowModel>();
            int rowIdx = 1;
            foreach (var user in users)
            {
                var matching = user.Invoices?
                    .Where(i => !i.DeleteStatus && i.RegDate.Date >= startDate.Date && i.RegDate.Date <= endDate.Date)
                    .ToList() ?? new List<Invoice>();

                userSales.Add(new UserSalesRowModel
                {
                    RowIndex = rowIdx++,
                    UserName = user.Name ?? user.UserName ?? "",
                    InvoicesCount = matching.Count,
                    TotalAmount = (double)matching.Sum(i => i.Payable)
                });
            }

            var model = new SalesSummaryReportModel
            {
                ReportTitle = title,
                GeneratedDatePersian = ToPersianDate(DateTime.Now),
                StartDatePersian = ToPersianDate(startDate),
                EndDatePersian = ToPersianDate(endDate),
                UserSales = userSales
            };

            ReportViewerService.OpenReportPdf(new SalesSummaryDocument(model), filePrefix);
        }

        #endregion
    }
}
