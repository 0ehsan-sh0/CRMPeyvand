using BE;
using BLL;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using CRMPeyvand.Reports.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace CRMPeyvand
{
    public partial class ReportsForm : Form
    {
        #region for design Form
        // Design Form
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse
            );   // Design Form
        #endregion
        public ReportsForm()
        {
            InitializeComponent();
            #region for design Form
            this.FormBorderStyle = FormBorderStyle.None;   // Design Form
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 40, 40));   // Design Form
            #endregion
            #region DateTimePickers
            Application.CurrentCulture = new CultureInfo("fa-IR");
            
            Start.CustomFormat = Application.CurrentCulture.DateTimeFormat.ShortDatePattern;
            Application.CurrentCulture = new CultureInfo("fa-IR");
            
            End.CustomFormat = Application.CurrentCulture.DateTimeFormat.ShortDatePattern;
            #endregion
            
        }
        UserBLL ubll = new UserBLL();
        CustomerBLL Cbll = new CustomerBLL();
        ActivityBLL abll = new ActivityBLL();
        CatalogItemBLL pbll = new CatalogItemBLL();

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private static string ToPersianDate(DateTime date)
        {
            var pc = new PersianCalendar();
            return $"{pc.GetYear(date):0000}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00}";
        }

        private void GeneratePeriodicSalesReport(string title, DateTime startDate, DateTime endDate, string filePrefix)
        {
            var users = ubll.ReadInvoicesList();
            var userSales = new List<UserSalesRowModel>();
            int rowIdx = 1;
            foreach (var user in users)
            {
                var matching = user.Invoices?
                    .Where(i => !i.DeleteStatus && i.RegDate.Date >= startDate && i.RegDate.Date <= endDate)
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

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (rbPrintCustomer.Checked)
            {
                var customers = Cbll.ReadWithDateTime();
                var model = new CustomerReportModel
                {
                    ReportTitle = "مشتریان ثبت نام شده",
                    GeneratedDatePersian = ToPersianDate(DateTime.Now),
                    Customers = customers.Select((c, idx) => new CustomerRowModel
                    {
                        RowIndex = idx + 1,
                        Name = c.Name ?? "",
                        Phone = c.Phone ?? "",
                        RegDatePersian = ToPersianDate(c.RegDate)
                    }).ToList()
                };
                ReportViewerService.OpenReportPdf(new CustomerListDocument(model), "Customers");
            }
            else if (rbPrintActivities.Checked)
            {
                var activities = abll.ReadAllWithDetails();
                var model = new ActivityReportModel
                {
                    ReportTitle = "تمام فعالیت‌های ثبت شده",
                    GeneratedDatePersian = ToPersianDate(DateTime.Now),
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
                ReportViewerService.OpenReportPdf(new ActivityListDocument(model), "Activities");
            }
            else if (rbPrintThisWeek.Checked)
            {
                GeneratePeriodicSalesReport("لیست فروش هفت روز گذشته", DateTime.Now.Date.AddDays(-7), DateTime.Now.Date, "WeeklySales");
            }
            else if (rbPrintThismonth.Checked)
            {
                GeneratePeriodicSalesReport("لیست فروش ماه گذشته (سی روز گذشته)", DateTime.Now.Date.AddDays(-30), DateTime.Now.Date, "MonthlySales");
            }
            else if (rbPrintThisYear.Checked)
            {
                GeneratePeriodicSalesReport("لیست فروش سال گذشته (365 روز گذشته)", DateTime.Now.Date.AddDays(-365), DateTime.Now.Date, "YearlySales");
            }
            else if (rbPrintProducts.Checked)
            {
                var products = pbll.ReadAll();
                var model = new CatalogItemReportModel
                {
                    ReportTitle = "موجودی محصولات انبار",
                    GeneratedDatePersian = ToPersianDate(DateTime.Now),
                    Items = products.Select((p, idx) => new CatalogItemRowModel
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

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            chart1.Series["Chart"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            chart1.Series["Chart"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            chart1.Series["Chart"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            chart1.Series["Chart"].Points.Clear();
            DateTime startDate = Start.SelectedDateInDateTime.Date;
            DateTime endDate = End.SelectedDateInDateTime.Date;

            if (rbPrintInvoicesD.Checked)
            {
                var users = ubll.ReadInvoicesList();
                var userSales = new List<UserSalesRowModel>();
                int rowIdx = 1;
                foreach (var user in users)
                {
                    var matching = user.Invoices?
                        .Where(i => !i.DeleteStatus && i.RegDate.Date >= startDate && i.RegDate.Date <= endDate)
                        .ToList() ?? new List<Invoice>();

                    chart1.Series["Chart"].Points.AddXY(user.Name ?? user.UserName, matching.Count);
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
                    ReportTitle = "گزارش فروش کاربران بر اساس تاریخ",
                    GeneratedDatePersian = ToPersianDate(DateTime.Now),
                    StartDatePersian = ToPersianDate(startDate),
                    EndDatePersian = ToPersianDate(endDate),
                    UserSales = userSales
                };
                ReportViewerService.OpenReportPdf(new SalesSummaryDocument(model), "UsersSells");
            }
            else if (rbPrintActivitiesD.Checked)
            {
                var users = ubll.ReadActivitiesList();
                var activities = abll.ReadAllWithDetails()
                    .Where(a => a.RegDate.Date >= startDate && a.RegDate.Date <= endDate)
                    .ToList();

                foreach (var user in users)
                {
                    int count = user.Activities?
                        .Count(a => !a.DeleteStatus && a.RegDate.Date >= startDate && a.RegDate.Date <= endDate) ?? 0;
                    chart1.Series["Chart"].Points.AddXY(user.Name ?? user.UserName, count);
                }

                var model = new ActivityReportModel
                {
                    ReportTitle = "گزارش فعالیت کاربران بر اساس تاریخ",
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
                ReportViewerService.OpenReportPdf(new ActivityListDocument(model), "UserActivities");
            }
            else if (rbPrintCustomerD.Checked)
            {
                var customers = Cbll.ReadWithDateTime()
                    .Where(c => c.RegDate.Date >= startDate && c.RegDate.Date <= endDate)
                    .ToList();

                var model = new CustomerReportModel
                {
                    ReportTitle = "گزارش مشتریان بر اساس تاریخ",
                    GeneratedDatePersian = ToPersianDate(DateTime.Now),
                    Customers = customers.Select((c, idx) => new CustomerRowModel
                    {
                        RowIndex = idx + 1,
                        Name = c.Name ?? "",
                        Phone = c.Phone ?? "",
                        RegDatePersian = ToPersianDate(c.RegDate)
                    }).ToList()
                };
                ReportViewerService.OpenReportPdf(new CustomerListDocument(model), "CustomersD");
            }
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            chart1.Series["Chart"].Points.Clear();
            if (rbPrintInvoicesD.Checked)
            {
                foreach (var item in ubll.ReadInvoicesList())
                {
                    int x = 0;
                    foreach (var i in item.Invoices)
                    {
                        if (i.RegDate.Date >= Start.SelectedDateInDateTime.Date && i.RegDate.Date <= End.SelectedDateInDateTime.Date)
                        {
                            x++;
                        }
                    }
                    chart1.Series["Chart"].Points.AddXY(item.Name, x);
                }
            }
            else if (rbPrintActivitiesD.Checked)
            {
                foreach (var item in ubll.ReadActivitiesList())
                {
                    int x = 0;
                    foreach (var i in item.Activities)
                    {
                        if (i.RegDate.Date >= Start.SelectedDateInDateTime.Date && i.RegDate.Date <= End.SelectedDateInDateTime.Date)
                        {
                            x++;
                        }
                    }
                    chart1.Series["Chart"].Points.AddXY(item.Name, x);
                }
            }
            else if (rbPrintCustomerD.Checked)
            {
                System.Windows.Forms.MessageBox.Show("گزارش زیر فقط مخصوص چاپ است", "اطلاعیه");
            }
        }

        private void ReportsForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
