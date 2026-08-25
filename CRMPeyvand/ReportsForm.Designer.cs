namespace CRMPeyvand
{
    partial class ReportsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportsForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.rbPrintThisYear = new System.Windows.Forms.RadioButton();
            this.rbPrintThismonth = new System.Windows.Forms.RadioButton();
            this.rbPrintThisWeek = new System.Windows.Forms.RadioButton();
            this.rbPrintActivities = new System.Windows.Forms.RadioButton();
            this.rbPrintCustomer = new System.Windows.Forms.RadioButton();
            this.rbPrintProducts = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rbPrintCustomerD = new System.Windows.Forms.RadioButton();
            this.rbPrintActivitiesD = new System.Windows.Forms.RadioButton();
            this.rbPrintInvoicesD = new System.Windows.Forms.RadioButton();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.Start = new BehComponents.DateTimePickerX();
            this.End = new BehComponents.DateTimePickerX();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.groupBox1.Controls.Add(this.pictureBox2);
            this.groupBox1.Controls.Add(this.rbPrintThisYear);
            this.groupBox1.Controls.Add(this.rbPrintThismonth);
            this.groupBox1.Controls.Add(this.rbPrintThisWeek);
            this.groupBox1.Controls.Add(this.rbPrintActivities);
            this.groupBox1.Controls.Add(this.rbPrintCustomer);
            this.groupBox1.Controls.Add(this.rbPrintProducts);
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(143)))), ((int)(((byte)(231)))));
            this.groupBox1.Location = new System.Drawing.Point(1057, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(481, 348);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "گزارش چاپی";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox2.Image = global::CRMPeyvand.Properties.Resources.Printer;
            this.pictureBox2.Location = new System.Drawing.Point(17, 267);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(60, 60);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // rbPrintThisYear
            // 
            this.rbPrintThisYear.AutoSize = true;
            this.rbPrintThisYear.ForeColor = System.Drawing.Color.Black;
            this.rbPrintThisYear.Location = new System.Drawing.Point(124, 234);
            this.rbPrintThisYear.Name = "rbPrintThisYear";
            this.rbPrintThisYear.Size = new System.Drawing.Size(333, 27);
            this.rbPrintThisYear.TabIndex = 4;
            this.rbPrintThisYear.TabStop = true;
            this.rbPrintThisYear.Text = "لیست فروش سال گذشته (365 روز گذشته)";
            this.rbPrintThisYear.UseVisualStyleBackColor = true;
            // 
            // rbPrintThismonth
            // 
            this.rbPrintThismonth.AutoSize = true;
            this.rbPrintThismonth.ForeColor = System.Drawing.Color.Black;
            this.rbPrintThismonth.Location = new System.Drawing.Point(138, 186);
            this.rbPrintThismonth.Name = "rbPrintThismonth";
            this.rbPrintThismonth.Size = new System.Drawing.Size(319, 27);
            this.rbPrintThismonth.TabIndex = 3;
            this.rbPrintThismonth.TabStop = true;
            this.rbPrintThismonth.Text = "لیست فروش ماه گذشته (سی روز گذشته)";
            this.rbPrintThismonth.UseVisualStyleBackColor = true;
            // 
            // rbPrintThisWeek
            // 
            this.rbPrintThisWeek.AutoSize = true;
            this.rbPrintThisWeek.ForeColor = System.Drawing.Color.Black;
            this.rbPrintThisWeek.Location = new System.Drawing.Point(217, 137);
            this.rbPrintThisWeek.Name = "rbPrintThisWeek";
            this.rbPrintThisWeek.Size = new System.Drawing.Size(240, 27);
            this.rbPrintThisWeek.TabIndex = 2;
            this.rbPrintThisWeek.TabStop = true;
            this.rbPrintThisWeek.Text = "لیست فروش هفت روز گذشته";
            this.rbPrintThisWeek.UseVisualStyleBackColor = true;
            // 
            // rbPrintActivities
            // 
            this.rbPrintActivities.AutoSize = true;
            this.rbPrintActivities.ForeColor = System.Drawing.Color.Black;
            this.rbPrintActivities.Location = new System.Drawing.Point(237, 88);
            this.rbPrintActivities.Name = "rbPrintActivities";
            this.rbPrintActivities.Size = new System.Drawing.Size(220, 27);
            this.rbPrintActivities.TabIndex = 1;
            this.rbPrintActivities.TabStop = true;
            this.rbPrintActivities.Text = "تمام فعالیت های ثبت شده";
            this.rbPrintActivities.UseVisualStyleBackColor = true;
            // 
            // rbPrintCustomer
            // 
            this.rbPrintCustomer.AutoSize = true;
            this.rbPrintCustomer.ForeColor = System.Drawing.Color.Black;
            this.rbPrintCustomer.Location = new System.Drawing.Point(269, 41);
            this.rbPrintCustomer.Name = "rbPrintCustomer";
            this.rbPrintCustomer.Size = new System.Drawing.Size(188, 27);
            this.rbPrintCustomer.TabIndex = 0;
            this.rbPrintCustomer.TabStop = true;
            this.rbPrintCustomer.Text = "مشتریان ثبت نام شده";
            this.rbPrintCustomer.UseVisualStyleBackColor = true;
            // 
            // rbPrintProducts
            // 
            this.rbPrintProducts.AutoSize = true;
            this.rbPrintProducts.ForeColor = System.Drawing.Color.Black;
            this.rbPrintProducts.Location = new System.Drawing.Point(267, 282);
            this.rbPrintProducts.Name = "rbPrintProducts";
            this.rbPrintProducts.Size = new System.Drawing.Size(190, 27);
            this.rbPrintProducts.TabIndex = 5;
            this.rbPrintProducts.TabStop = true;
            this.rbPrintProducts.Text = "موجودی محصولات انبار";
            this.rbPrintProducts.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.groupBox2.Controls.Add(this.End);
            this.groupBox2.Controls.Add(this.Start);
            this.groupBox2.Controls.Add(this.pictureBox4);
            this.groupBox2.Controls.Add(this.pictureBox3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.rbPrintCustomerD);
            this.groupBox2.Controls.Add(this.rbPrintActivitiesD);
            this.groupBox2.Controls.Add(this.rbPrintInvoicesD);
            this.groupBox2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(143)))), ((int)(((byte)(231)))));
            this.groupBox2.Location = new System.Drawing.Point(1057, 366);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(481, 410);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "گزارش بر اساس تاریخ";
            // 
            // pictureBox4
            // 
            this.pictureBox4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox4.Image = global::CRMPeyvand.Properties.Resources.Printer;
            this.pictureBox4.Location = new System.Drawing.Point(17, 334);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(60, 60);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 5;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.pictureBox4_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox3.Image = global::CRMPeyvand.Properties.Resources.Display;
            this.pictureBox3.Location = new System.Drawing.Point(397, 334);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(60, 60);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 5;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(391, 277);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 23);
            this.label2.TabIndex = 10;
            this.label2.Text = "تا تاریخ :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(429, 236);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 23);
            this.label1.TabIndex = 9;
            this.label1.Text = "از :";
            // 
            // rbPrintCustomerD
            // 
            this.rbPrintCustomerD.AutoSize = true;
            this.rbPrintCustomerD.ForeColor = System.Drawing.Color.Black;
            this.rbPrintCustomerD.Location = new System.Drawing.Point(269, 61);
            this.rbPrintCustomerD.Name = "rbPrintCustomerD";
            this.rbPrintCustomerD.Size = new System.Drawing.Size(188, 27);
            this.rbPrintCustomerD.TabIndex = 6;
            this.rbPrintCustomerD.TabStop = true;
            this.rbPrintCustomerD.Text = "مشتریان ثبت نام شده";
            this.rbPrintCustomerD.UseVisualStyleBackColor = true;
            // 
            // rbPrintActivitiesD
            // 
            this.rbPrintActivitiesD.AutoSize = true;
            this.rbPrintActivitiesD.ForeColor = System.Drawing.Color.Black;
            this.rbPrintActivitiesD.Location = new System.Drawing.Point(180, 108);
            this.rbPrintActivitiesD.Name = "rbPrintActivitiesD";
            this.rbPrintActivitiesD.Size = new System.Drawing.Size(277, 27);
            this.rbPrintActivitiesD.TabIndex = 7;
            this.rbPrintActivitiesD.TabStop = true;
            this.rbPrintActivitiesD.Text = "فعالیت ها ثبت شده توسط هر کاربر";
            this.rbPrintActivitiesD.UseVisualStyleBackColor = true;
            // 
            // rbPrintInvoicesD
            // 
            this.rbPrintInvoicesD.AutoSize = true;
            this.rbPrintInvoicesD.ForeColor = System.Drawing.Color.Black;
            this.rbPrintInvoicesD.Location = new System.Drawing.Point(325, 157);
            this.rbPrintInvoicesD.Name = "rbPrintInvoicesD";
            this.rbPrintInvoicesD.Size = new System.Drawing.Size(132, 27);
            this.rbPrintInvoicesD.TabIndex = 8;
            this.rbPrintInvoicesD.TabStop = true;
            this.rbPrintInvoicesD.Text = "فروش هر کاربر";
            this.rbPrintInvoicesD.UseVisualStyleBackColor = true;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(12, 78);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Chart";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(1028, 632);
            this.chart1.TabIndex = 4;
            this.chart1.Text = "chart";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(848, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 23);
            this.label3.TabIndex = 12;
            this.label3.Text = "نمودار خطی";
            this.label3.Click += new System.EventHandler(this.pictureBox5_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(490, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 23);
            this.label4.TabIndex = 13;
            this.label4.Text = "نمودار ستونی";
            this.label4.Click += new System.EventHandler(this.pictureBox6_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(127, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 23);
            this.label5.TabIndex = 14;
            this.label5.Text = "نمودار نقطه ای";
            this.label5.Click += new System.EventHandler(this.pictureBox7_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox5.Image = global::CRMPeyvand.Properties.Resources.Fully_Charged;
            this.pictureBox5.Location = new System.Drawing.Point(940, 13);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(40, 40);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox5.TabIndex = 5;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.Click += new System.EventHandler(this.pictureBox5_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::CRMPeyvand.Properties.Resources.Home;
            this.pictureBox1.Location = new System.Drawing.Point(12, 716);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(60, 60);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // pictureBox6
            // 
            this.pictureBox6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox6.Image = global::CRMPeyvand.Properties.Resources.Fully_Charged;
            this.pictureBox6.Location = new System.Drawing.Point(598, 13);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(40, 40);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox6.TabIndex = 15;
            this.pictureBox6.TabStop = false;
            this.pictureBox6.Click += new System.EventHandler(this.pictureBox6_Click);
            // 
            // pictureBox7
            // 
            this.pictureBox7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox7.Image = global::CRMPeyvand.Properties.Resources.Fully_Charged;
            this.pictureBox7.Location = new System.Drawing.Point(244, 13);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(40, 40);
            this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox7.TabIndex = 16;
            this.pictureBox7.TabStop = false;
            this.pictureBox7.Click += new System.EventHandler(this.pictureBox7_Click);
            // 
            // Start
            // 
            this.Start.AnchorSize = new System.Drawing.Size(329, 32);
            this.Start.BackColor = System.Drawing.Color.White;
            this.Start.CalendarBoldedDayForeColor = System.Drawing.Color.Blue;
            this.Start.CalendarBorderColor = System.Drawing.Color.CadetBlue;
            this.Start.CalendarDayRectTickness = 2F;
            this.Start.CalendarDaysBackColor = System.Drawing.Color.LightGray;
            this.Start.CalendarDaysFont = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.Start.CalendarDaysForeColor = System.Drawing.Color.DodgerBlue;
            this.Start.CalendarEnglishAnnuallyBoldedDates = new System.DateTime[0];
            this.Start.CalendarEnglishBoldedDates = new System.DateTime[0];
            this.Start.CalendarEnglishHolidayDates = new System.DateTime[0];
            this.Start.CalendarEnglishMonthlyBoldedDates = new System.DateTime[0];
            this.Start.CalendarHolidayForeColor = System.Drawing.Color.Red;
            this.Start.CalendarHolidayWeekly = BehComponents.MonthCalendarX.DayOfWeekForHoliday.Friday;
            this.Start.CalendarLineWeekColor = System.Drawing.Color.Black;
            this.Start.CalendarPersianAnnuallyBoldedDates = new BehComponents.PersianDateTime[0];
            this.Start.CalendarPersianBoldedDates = new BehComponents.PersianDateTime[0];
            this.Start.CalendarPersianHolidayDates = new BehComponents.PersianDateTime[0];
            this.Start.CalendarPersianMonthlyBoldedDates = new BehComponents.PersianDateTime[0];
            this.Start.CalendarShowToday = true;
            this.Start.CalendarShowTodayRect = true;
            this.Start.CalendarShowToolTips = false;
            this.Start.CalendarShowTrailing = true;
            this.Start.CalendarStyle_DaysButton = BehComponents.ButtonX.ButtonStyles.Simple;
            this.Start.CalendarStyle_GotoTodayButton = BehComponents.ButtonX.ButtonStyles.Green;
            this.Start.CalendarStyle_MonthButton = BehComponents.ButtonX.ButtonStyles.Blue;
            this.Start.CalendarStyle_NextMonthButton = BehComponents.ButtonX.ButtonStyles.Green;
            this.Start.CalendarStyle_PreviousMonthButton = BehComponents.ButtonX.ButtonStyles.Green;
            this.Start.CalendarStyle_YearButton = BehComponents.ButtonX.ButtonStyles.Blue;
            this.Start.CalendarTitleBackColor = System.Drawing.Color.Wheat;
            this.Start.CalendarTitleFont = new System.Drawing.Font("Tahoma", 7.8F);
            this.Start.CalendarTitleForeColor = System.Drawing.Color.Black;
            this.Start.CalendarTodayBackColor = System.Drawing.Color.Wheat;
            this.Start.CalendarTodayFont = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.Start.CalendarTodayForeColor = System.Drawing.Color.Black;
            this.Start.CalendarTodayRectColor = System.Drawing.Color.Coral;
            this.Start.CalendarTodayRectTickness = 2F;
            this.Start.CalendarTrailingForeColor = System.Drawing.Color.DarkGray;
            this.Start.CalendarType = BehComponents.CalendarTypes.Persian;
            this.Start.CalendarWeekDaysBackColor = System.Drawing.Color.Wheat;
            this.Start.CalendarWeekDaysFont = new System.Drawing.Font("Tahoma", 7.8F);
            this.Start.CalendarWeekDaysForeColor = System.Drawing.Color.OrangeRed;
            this.Start.CalendarWeekStartsOn = BehComponents.MonthCalendarX.WeekDays.Saturday;
            this.Start.ClearButtonAlignment = BehComponents.DropDownEmpty.Alignments.Left;
            this.Start.ClearButtonBackColor = System.Drawing.Color.White;
            this.Start.ClearButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.Start.ClearButtonImage = ((System.Drawing.Image)(resources.GetObject("Start.ClearButtonImage")));
            this.Start.ClearButtonImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Start.ClearButtonImageFixedSize = new System.Drawing.Size(0, 0);
            this.Start.ClearButtonImageSizeMode = BehComponents.DropDownEmpty.ImageSizeModes.Zoom;
            this.Start.ClearButtonText = "";
            this.Start.ClearButtonTextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Start.ClearButtonToolTip = "";
            this.Start.ClearButtonWidth = 17;
            this.Start.ClearDateTimeWhenDownDeleteKey = true;
            this.Start.CustomFormat = "";
            this.Start.DockSide = BehComponents.DropDownEmpty.Alignments.Left;
            this.Start.DropDownClosedWhenClickOnDays = false;
            this.Start.DropDownClosedWhenSelectedDateChanged = false;
            this.Start.Format = BehComponents.DateTimePickerX.FormatDate.Long;
            this.Start.Format4Binding = "yyyy/MM/dd";
            this.Start.Location = new System.Drawing.Point(37, 232);
            this.Start.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.Start.Name = "Start";
            this.Start.RightToLeftLayout = true;
            this.Start.ShowClearButton = false;
            this.Start.Size = new System.Drawing.Size(329, 32);
            this.Start.TabIndex = 11;
            this.Start.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Start.TextWhenClearButtonClicked = "";
            // 
            // End
            // 
            this.End.AnchorSize = new System.Drawing.Size(329, 32);
            this.End.BackColor = System.Drawing.Color.White;
            this.End.CalendarBoldedDayForeColor = System.Drawing.Color.Blue;
            this.End.CalendarBorderColor = System.Drawing.Color.CadetBlue;
            this.End.CalendarDayRectTickness = 2F;
            this.End.CalendarDaysBackColor = System.Drawing.Color.LightGray;
            this.End.CalendarDaysFont = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(254)));
            this.End.CalendarDaysForeColor = System.Drawing.Color.DodgerBlue;
            this.End.CalendarEnglishAnnuallyBoldedDates = new System.DateTime[0];
            this.End.CalendarEnglishBoldedDates = new System.DateTime[0];
            this.End.CalendarEnglishHolidayDates = new System.DateTime[0];
            this.End.CalendarEnglishMonthlyBoldedDates = new System.DateTime[0];
            this.End.CalendarHolidayForeColor = System.Drawing.Color.Red;
            this.End.CalendarHolidayWeekly = BehComponents.MonthCalendarX.DayOfWeekForHoliday.Friday;
            this.End.CalendarLineWeekColor = System.Drawing.Color.Black;
            this.End.CalendarPersianAnnuallyBoldedDates = new BehComponents.PersianDateTime[0];
            this.End.CalendarPersianBoldedDates = new BehComponents.PersianDateTime[0];
            this.End.CalendarPersianHolidayDates = new BehComponents.PersianDateTime[0];
            this.End.CalendarPersianMonthlyBoldedDates = new BehComponents.PersianDateTime[0];
            this.End.CalendarShowToday = true;
            this.End.CalendarShowTodayRect = true;
            this.End.CalendarShowToolTips = false;
            this.End.CalendarShowTrailing = true;
            this.End.CalendarStyle_DaysButton = BehComponents.ButtonX.ButtonStyles.Simple;
            this.End.CalendarStyle_GotoTodayButton = BehComponents.ButtonX.ButtonStyles.Green;
            this.End.CalendarStyle_MonthButton = BehComponents.ButtonX.ButtonStyles.Blue;
            this.End.CalendarStyle_NextMonthButton = BehComponents.ButtonX.ButtonStyles.Green;
            this.End.CalendarStyle_PreviousMonthButton = BehComponents.ButtonX.ButtonStyles.Green;
            this.End.CalendarStyle_YearButton = BehComponents.ButtonX.ButtonStyles.Blue;
            this.End.CalendarTitleBackColor = System.Drawing.Color.Wheat;
            this.End.CalendarTitleFont = new System.Drawing.Font("Tahoma", 7.8F);
            this.End.CalendarTitleForeColor = System.Drawing.Color.Black;
            this.End.CalendarTodayBackColor = System.Drawing.Color.Wheat;
            this.End.CalendarTodayFont = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold);
            this.End.CalendarTodayForeColor = System.Drawing.Color.Black;
            this.End.CalendarTodayRectColor = System.Drawing.Color.Coral;
            this.End.CalendarTodayRectTickness = 2F;
            this.End.CalendarTrailingForeColor = System.Drawing.Color.DarkGray;
            this.End.CalendarType = BehComponents.CalendarTypes.Persian;
            this.End.CalendarWeekDaysBackColor = System.Drawing.Color.Wheat;
            this.End.CalendarWeekDaysFont = new System.Drawing.Font("Tahoma", 7.8F);
            this.End.CalendarWeekDaysForeColor = System.Drawing.Color.OrangeRed;
            this.End.CalendarWeekStartsOn = BehComponents.MonthCalendarX.WeekDays.Saturday;
            this.End.ClearButtonAlignment = BehComponents.DropDownEmpty.Alignments.Left;
            this.End.ClearButtonBackColor = System.Drawing.Color.White;
            this.End.ClearButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.End.ClearButtonImage = ((System.Drawing.Image)(resources.GetObject("End.ClearButtonImage")));
            this.End.ClearButtonImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.End.ClearButtonImageFixedSize = new System.Drawing.Size(0, 0);
            this.End.ClearButtonImageSizeMode = BehComponents.DropDownEmpty.ImageSizeModes.Zoom;
            this.End.ClearButtonText = "";
            this.End.ClearButtonTextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.End.ClearButtonToolTip = "";
            this.End.ClearButtonWidth = 17;
            this.End.ClearDateTimeWhenDownDeleteKey = true;
            this.End.CustomFormat = "";
            this.End.DockSide = BehComponents.DropDownEmpty.Alignments.Left;
            this.End.DropDownClosedWhenClickOnDays = false;
            this.End.DropDownClosedWhenSelectedDateChanged = false;
            this.End.Format = BehComponents.DateTimePickerX.FormatDate.Long;
            this.End.Format4Binding = "yyyy/MM/dd";
            this.End.Location = new System.Drawing.Point(37, 273);
            this.End.Margin = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.End.Name = "End";
            this.End.RightToLeftLayout = true;
            this.End.ShowClearButton = false;
            this.End.Size = new System.Drawing.Size(329, 32);
            this.End.TabIndex = 12;
            this.End.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.End.TextWhenClearButtonClicked = "";
            // 
            // ReportsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            this.ClientSize = new System.Drawing.Size(1550, 800);
            this.Controls.Add(this.pictureBox7);
            this.Controls.Add(this.pictureBox6);
            this.Controls.Add(this.pictureBox5);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.Font = new System.Drawing.Font("Shabnam FD", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ReportsForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ReportsForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ReportsForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbPrintCustomer;
        private System.Windows.Forms.RadioButton rbPrintThisYear;
        private System.Windows.Forms.RadioButton rbPrintThismonth;
        private System.Windows.Forms.RadioButton rbPrintThisWeek;
        private System.Windows.Forms.RadioButton rbPrintActivities;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.RadioButton rbPrintCustomerD;
        private System.Windows.Forms.RadioButton rbPrintProducts;
        private System.Windows.Forms.RadioButton rbPrintActivitiesD;
        private System.Windows.Forms.RadioButton rbPrintInvoicesD;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.PictureBox pictureBox7;
        private BehComponents.DateTimePickerX Start;
        private BehComponents.DateTimePickerX End;
    }
}