using System.Windows;
using System.Windows.Input;

namespace CRMPeyvand
{
    /// <summary>
    /// The database configuration guidelines, on their own page so the settings
    /// window stays short enough to fit on a 768px screen.
    /// </summary>
    public partial class DatabaseGuideForm : Window
    {
        public DatabaseGuideForm()
        {
            InitializeComponent();
        }

        private void BackToSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
