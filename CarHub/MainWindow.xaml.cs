using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace CarHub
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Car>? Cars { get; set; }
        private CancellationTokenSource? _cts = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Cars = new();
        }

        private void AddCarsWithSingleThread(CancellationToken token)
        {

            Cars?.Clear();
            new Thread(() =>
            {
                var watch = Stopwatch.StartNew();
                var directory = new DirectoryInfo(@"..\..\..\Jsons");
                foreach (var file in directory.GetFiles())
                {

                    if (file.Extension == ".json")
                    {
                        var jsonTxt = File.ReadAllText(file.FullName);

                        var carlist = JsonSerializer.Deserialize<List<Car>>(jsonTxt);

                        if (carlist is not null)
                            foreach (var car in carlist)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    watch.Stop();
                                    Dispatcher.Invoke(() => tbTimeSpan.Text = watch.Elapsed.ToString());
                                    Dispatcher.Invoke(() => BtnStart.IsEnabled = true);
                                    Dispatcher.Invoke(() => BtnCancel.IsEnabled = false);
                                    break;
                                }

                                Dispatcher.Invoke(() => Cars?.Add(car));
                                Dispatcher.Invoke(() => tbTimeSpan.Text = watch.Elapsed.ToString());
                                Thread.Sleep(100);
                            }
                    }
                }
                watch.Stop();
                Dispatcher.Invoke(() => tbTimeSpan.Text = watch.Elapsed.ToString());
                Dispatcher.Invoke(() => BtnStart.IsEnabled = true);
                Dispatcher.Invoke(() => BtnCancel.IsEnabled = false);
            }).Start();
        }

        private void AddCarsWithMultiThread(CancellationToken token)
        {
            Cars?.Clear();

            var directory = new DirectoryInfo(@"..\..\..\Jsons");

            var sync = new object();
            var watch = Stopwatch.StartNew();

            foreach (var file in directory.GetFiles())
            {
                if (file.Extension == ".json")
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        var jsonTxt = File.ReadAllText(file.FullName);

                        var carlist = JsonSerializer.Deserialize<List<Car>>(jsonTxt);

                        if (carlist is not null)
                        {
                            foreach (var car in carlist)
                            {

                                if (token.IsCancellationRequested)
                                {
                                    watch.Stop();
                                    Dispatcher.Invoke(() => tbTimeSpan.Text = watch.Elapsed.ToString());
                                    Dispatcher.Invoke(() => BtnStart.IsEnabled = true);
                                    Dispatcher.Invoke(() => BtnCancel.IsEnabled = false);
                                    break;
                                }

                                lock (sync)
                                    Dispatcher.Invoke(() => Cars?.Add(car));

                                Dispatcher.Invoke(() => tbTimeSpan.Text = watch.Elapsed.ToString());
                                Thread.Sleep(100);
                            }
                        }
                        Dispatcher.Invoke(() => BtnStart.IsEnabled = true);
                        Dispatcher.Invoke(() => BtnCancel.IsEnabled = false);
                    });
                }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (rbMulti.IsChecked == false && rbSingle.IsChecked == false)
            {
                MessageBox.Show("Please choose adding method");
                return;
            }

            BtnStart.IsEnabled = false;
            BtnCancel.IsEnabled = true;

            _cts = new();

            if (rbSingle.IsChecked == true)
                AddCarsWithSingleThread(_cts.Token);

            if (rbMulti.IsChecked == true)
                AddCarsWithMultiThread(_cts.Token);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            BtnCancel.IsEnabled = false;
            BtnStart.IsEnabled = true;
            tbTimeSpan.Text = "00:00:00";
        }
    }
}
