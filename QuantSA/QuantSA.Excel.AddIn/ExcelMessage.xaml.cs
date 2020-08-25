using System;
using System.Windows;

namespace QuantSA.Excel.Addin
{
    /// <summary>
    /// Interaction logic for ExcelMessage.xaml
    /// </summary>
    public partial class ExcelMessage
    {
        public ExcelMessage(string title, string message)
        {
            InitializeComponent();
            Title = title;
            LabelMessage.Content = message;
            LabelStackTrace.Content = "";
        }

        public ExcelMessage(string message)
        {
            InitializeComponent();
            LabelMessage.Content = message;
            LabelStackTrace.Content = "";
        }

        /// <summary>
        /// Construct a QuantSA error message with a stack trace
        /// </summary>
        /// <param name="e"></param>
        public ExcelMessage(Exception e)
        {
            InitializeComponent();
            LabelMessage.Content = e.Message;
            var lines = e.StackTrace.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            var result = "";
            for (var i = 0; i < lines.Length; i++)
                if (lines[i].Length < 80)
                    result = result + lines[i] + "\r\n";
                else
                    for (var j = 0; j < Math.Ceiling(lines[i].Length / 80.0); j++)
                    {
                        if (j > 0)
                            result = result + "    ";
                        if (j < Math.Ceiling(lines[i].Length / 80.0) - 1)
                            result = result + lines[i].Substring(j * 80, 80) + "\r\n";
                        else
                            result = result + lines[i].Substring(j * 80) + "\r\n";
                    }

            LabelStackTrace.Content = result;
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