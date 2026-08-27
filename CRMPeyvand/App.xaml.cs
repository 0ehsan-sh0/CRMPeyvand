using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CRMPeyvand
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            this.DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show("خطایی در برنامه رخ داده است:\n" + args.Exception.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            try
            {
                Stimulsoft.Report.StiOptions.Engine.ForceInterpretationMode = true;
            }
            catch
            {
            }

            PersianCulture culture = new PersianCulture();
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            ConfigHelper.Instance.SetLang(culture.IetfLanguageTag);
            base.OnStartup(e);
        }
    }
}
