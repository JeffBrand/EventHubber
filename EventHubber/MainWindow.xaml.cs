using EventHubber.Services;
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

namespace EventHubber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EventHubService service;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void openButton_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            service = new EventHubService(txtConnectionString.Text);
            await service.OpenEventHubAsync(txtEventHub.Text);
            //service = new EventHubService("Endpoint=sb://jbrandhackathon.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=M6p0Uz+OoSN5cUHySyYSaXKQ//f4ba4P5tz0n1HSFOA=");
            //await service.OpenEventHubAsync("hackhub");

            partitionList.ItemsSource = service.Partitions;
            messageList.ItemsSource = service.Messages;
            progressBar.Visibility = Visibility.Hidden;
           
        }

        private async void partitionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = partitionList.SelectedIndex;
            var partition = service.Partitions[index];

            await service.StartReadingAsync(partition.PartitionId, DateTime.MinValue);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            service.StopReading();
            
        }

        private async void openAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var p in service.Partitions)
            {
                await service.StartReadingAsync(p.PartitionId, DateTime.MinValue);
            }
        }
    }
}
