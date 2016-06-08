
namespace Hystrix.Example
{
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
    using Hystrix.NET.MetricsEventStream;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow instance;

        private HystrixMetricsStreamServer metricsServer;

        private List<CurrentTimeBackgroundWorker> backgroundWorkers = new List<CurrentTimeBackgroundWorker>();

        public MainWindow()
        {
            InitializeComponent();

            instance = this;

            this.metricsServer = new HystrixMetricsStreamServer("http://+:8080/Hystrix/", 2, TimeSpan.FromSeconds(0.5));
            this.metricsServer.Stopped += MetricsServer_Stopped;
        }

        public static void LogStatic(string level, string message)
        {
            if (instance == null)
            {
                return;
            }

            instance.Log(level, message);
        }

        private void Log(string level, string message)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                string log = LogTextBox.Text;
                log += level.PadRight(5) + " | " + message + Environment.NewLine;
                if (log.Length > 1000)
                {
                    log = log.Substring(log.Length - 1000, 1000);
                }
                LogTextBox.Text = log;
                LogTextBox.ScrollToEnd();
            }));
        }

        private void StartMetricsServerButton_Click(object sender, RoutedEventArgs e)
        {
            this.metricsServer.Start();
            this.StartMetricsServerButton.IsEnabled = false;
            this.StopMetricsServerButton.IsEnabled = true;
        }

        private void StopMetricsServerButton_Click(object sender, RoutedEventArgs e)
        {
            this.StopMetricsServerButton.IsEnabled = false;
            this.metricsServer.Stop();
        }

        private void MetricsServer_Stopped(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                this.StartMetricsServerButton.IsEnabled = true;
            }));
        }

        private void CreateWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            lock (this.backgroundWorkers)
            {
                CurrentTimeBackgroundWorker backgroundWorker = new CurrentTimeBackgroundWorker();
                backgroundWorker.Stopped += BackgroundWorker_Stopped;
                backgroundWorker.Start();
                this.backgroundWorkers.Add(backgroundWorker);
                DestroyWorkerButton.IsEnabled = true;
            }
        }

        private void BackgroundWorker_Stopped(object sender, EventArgs e)
        {
            lock (this.backgroundWorkers)
            {
                CurrentTimeBackgroundWorker backgroundWorker = (CurrentTimeBackgroundWorker)sender;
                backgroundWorker.Stopped -= BackgroundWorker_Stopped;
            }
        }

        private void DestroyWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            lock (this.backgroundWorkers)
            {
                this.backgroundWorkers[0].Stop();
                this.backgroundWorkers.RemoveAt(0);
                if (this.backgroundWorkers.Count == 0)
                {
                    DestroyWorkerButton.IsEnabled = false;
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Netflix.Hystrix.Hystrix.Reset();
            lock (this.backgroundWorkers)
            {
                this.backgroundWorkers.ForEach(w => w.Stop());
            }
            base.OnClosing(e);
        }
    }
}
