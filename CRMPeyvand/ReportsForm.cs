using BE;
using BLL;
using HandyControl.Controls;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
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
using static Stimulsoft.Report.Func;

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
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public class UserSells
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }
        
        private static string GetReportPath(string mrtFileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Reports", mrtFileName);
            if (File.Exists(path))
                return path;
            string devPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Reports", mrtFileName));
            if (File.Exists(devPath))
                return devPath;
            return path;
        }

        private void RenderAndShowReport(string mrtFileName)
        {
            StiReport sti = new StiReport();
            string path = GetReportPath(mrtFileName);
            sti.Load(path);
            sti.CalculationMode = StiCalculationMode.Interpretation;
            string connStr = ConfigurationManager.ConnectionStrings["conStr"]?.ConnectionString;
            if (!string.IsNullOrEmpty(connStr))
            {
                foreach (var database in sti.Dictionary.Databases.OfType<Stimulsoft.Report.Dictionary.StiSqlDatabase>())
                {
                    database.ConnectionString = connStr;
                }
            }
            if (sti.Dictionary.Variables.Contains("Date"))
            {
                sti.Dictionary.Variables["Date"].Value = DateTime.Now.Date.ToString("yyyy/MM/dd");
            }
            sti.Dictionary.Synchronize();
            sti.Render(false);
            sti.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (rbPrintCustomer.Checked)
            {
                RenderAndShowReport("Customers.mrt");
            }
            else if (rbPrintActivities.Checked)
            {
                RenderAndShowReport("Activities.mrt");
            }
            else if (rbPrintThisWeek.Checked)
            {
                RenderAndShowReport("LastWeekInvoices.mrt");
            }
            else if (rbPrintThismonth.Checked)
            {
                RenderAndShowReport("LastMonthInvoices.mrt");
            }
            else if (rbPrintThisYear.Checked)
            {
                RenderAndShowReport("LastYearInvoices.mrt");
            }
            else if (rbPrintProducts.Checked)
            {
                RenderAndShowReport("ProductsTotal.mrt");
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
            if (rbPrintInvoicesD.Checked)
            {
                List<UserSells> userSells = new List<UserSells>();
                foreach (var item in ubll.ReadInvoicesList())
                {
                    int x = 0;
                    UserSells usersell = new UserSells();
                    foreach (var i in item.Invoices)
                    {
                        if (i.RegDate.Date >= Start.SelectedDateInDateTime.Date && i.RegDate.Date <= End.SelectedDateInDateTime.Date)
                        {
                            x++;
                        }
                    }
                    chart1.Series["Chart"].Points.AddXY(item.Name, x);
                    usersell.Name = item.Name;
                    usersell.Count = x;
                    userSells.Add(usersell);
                }
                var usCulture = new CultureInfo("fa-IR");
                StiReport sti = new StiReport();
                sti.Load(GetReportPath("UsersSells.mrt"));
                sti.CalculationMode = StiCalculationMode.Interpretation;
                sti.Dictionary.Variables["Date"].Value = DateTime.Now.Date.ToString("yyyy,MM,d");
                sti.Dictionary.Variables["Start"].Value = Start.SelectedDateInDateTime.Date.ToString("yyyy,MM,d");
                sti.Dictionary.Variables["End"].Value = End.SelectedDateInDateTime.Date.ToString("yyyy,MM,d");
                sti.RegBusinessObject("UsersSells", userSells);
                sti.Dictionary.Synchronize();
                sti.Render(false);
                sti.Show();
            }
            else if (rbPrintActivitiesD.Checked)
            {
                List<UserSells> userSells = new List<UserSells>();
                foreach (var item in ubll.ReadActivitiesList())
                {
                    int x = 0;
                    UserSells usersell = new UserSells();
                    foreach (var i in item.Activities)
                    {
                        if (i.RegDate.Date >= Start.SelectedDateInDateTime.Date && i.RegDate.Date <= End.SelectedDateInDateTime.Date)
                        {
                            x++;
                        }
                    }
                    chart1.Series["Chart"].Points.AddXY(item.Name, x);
                    usersell.Name = item.Name;
                    usersell.Count = x;
                    userSells.Add(usersell);
                }
                var usCulture = new CultureInfo("fa-IR");
                StiReport sti = new StiReport();
                sti.Load(GetReportPath("UserActivities.mrt"));
                sti.CalculationMode = StiCalculationMode.Interpretation;
                sti.Dictionary.Variables["Date"].Value = DateTime.Now.Date.ToString("yyyy,MM,d");
                sti.Dictionary.Variables["Start"].Value = Start.SelectedDateInDateTime.Date.ToString("yyyy,MM,d");
                sti.Dictionary.Variables["End"].Value = End.SelectedDateInDateTime.Date.ToString("yyyy,MM,d");
                sti.RegBusinessObject("UserActivities", userSells);
                sti.Dictionary.Synchronize();
                sti.Render(false);
                sti.Show();
            }
            else if (rbPrintCustomerD.Checked)
            {
                List<Customer> customers = new List<Customer>();
                foreach (var item in Cbll.ReadWithDateTime())
                {
                    Customer customer = new Customer();
                    if (item.RegDate.Date >= Start.SelectedDateInDateTime.Date && item.RegDate.Date <= End.SelectedDateInDateTime.Date)
                    {
                        customer.Name = item.Name;
                        customer.Phone = item.Phone;
                        customer.RegDate = item.RegDate;
                        customers.Add(customer);
                    }
                }
                var usCulture = new CultureInfo("fa-IR");
                StiReport sti = new StiReport();
                sti.Load(GetReportPath("CustomersD.mrt"));
                sti.CalculationMode = StiCalculationMode.Interpretation;
                sti.Dictionary.Variables["Date"].Value = DateTime.Now.Date.ToString("yyyy,MM,d");
                sti.Dictionary.Variables["Start"].Value = Start.SelectedDateInDateTime.Date.ToString("yyyy,MM,d");
                sti.Dictionary.Variables["End"].Value = End.SelectedDateInDateTime.Date.ToString("yyyy,MM,d");
                sti.RegBusinessObject("Customers", customers);
                sti.Dictionary.Synchronize();
                sti.Render(false);
                sti.Show();
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
