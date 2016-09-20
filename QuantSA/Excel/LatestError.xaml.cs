using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Excel
{
    /// <summary>
    /// Interaction logic for LatestError.xaml
    /// </summary>
    public partial class LatestError : Window
    {
        public LatestError(string message)
        {
            InitializeComponent();
            this.lblMessage.Content = message;
            this.lblStackTrace.Content = "";
        }

        /// <summary>
        /// Construct a QuantSA error message with a stack trace
        /// </summary>
        /// <param name="e"></param>
        public LatestError(Exception e)
        {
            InitializeComponent();
            lblMessage.Content = e.Message;
            string[] lines = e.StackTrace.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string result = "";
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length < 80)
                    result = result + lines[i] + "\r\n";
                else
                {
                    for (int j = 0; j < Math.Ceiling(lines[i].Length / 80.0); j++)
                    {
                        if (j > 0)
                            result = result + "    ";
                        if (j < (Math.Ceiling(lines[i].Length / 80.0) - 1))
                            result = result + lines[i].Substring(j * 80, 80) + "\r\n";
                        else
                            result = result + lines[i].Substring(j * 80) + "\r\n";
                    }
                }
            }

            lblStackTrace.Content = result;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
