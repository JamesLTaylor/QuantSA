using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace QuantSAInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cancellationSource;
        Installer installer;

        public MainWindow()
        {
            InitializeComponent();
            cancellationSource = new CancellationTokenSource();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            // btnCancel.IsEnabled = true;
            
            cancellationSource.Dispose();
            cancellationSource = new CancellationTokenSource();

            installer = new Installer();

            var progressOutput = new Progress<string>(s => tbOutput.AppendText("\n" + s));
            var progressStep = new Progress<string>(s => UpdateStep(s));

            string installPath = tbInstallPath.Text;

            await Task.Factory.StartNew(() => installer.Start(installPath, progressOutput, progressStep, cancellationSource.Token),
                                        TaskCreationOptions.LongRunning);
            MessageBox.Show("Installation Complete!\n\nClose the window when done.", "Finished", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateStep(string stepName)
        {
            lblStep.Content = "Step: " + stepName;
            tbOutput.AppendText("\n****************************\n" + stepName + "\n****************************\n");
            tbOutput.ScrollToEnd();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
