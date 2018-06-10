using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QuantSAInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancellationSource;
        private Installer _installer;

        public MainWindow()
        {
            InitializeComponent();
            _cancellationSource = new CancellationTokenSource();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            // btnCancel.IsEnabled = true;

            _cancellationSource.Dispose();
            _cancellationSource = new CancellationTokenSource();

            _installer = new Installer();

            var progressOutput = new Progress<string>(s => tbOutput.AppendText("\n" + s));
            var progressStep = new Progress<string>(s => UpdateStep(s));

            var installPath = tbInstallPath.Text;

            await Task.Factory.StartNew(
                () => _installer.Start(installPath, progressOutput, progressStep, _cancellationSource.Token),
                TaskCreationOptions.LongRunning);
            MessageBox.Show("Installation Complete!\n\nClose the window when done.", "Finished", MessageBoxButton.OK,
                MessageBoxImage.Information);
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