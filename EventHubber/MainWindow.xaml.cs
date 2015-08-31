using EventHubber.Services;
using EventHubber.ViewModel;
using MahApps.Metro.Controls;
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
    public partial class MainWindow : MetroWindow
    {
        EventHubService service;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void openButton_Click(object sender, RoutedEventArgs e)
        {
            //service = new EventHubService(txtConnectionString.Text);
            //await service.OpenEventHubAsync(txtEventHub.Text);
            //service = new EventHubService("Endpoint=sb://jbrandhackathon.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=M6p0Uz+OoSN5cUHySyYSaXKQ//f4ba4P5tz0n1HSFOA=");
            //await service.OpenEventHubAsync("hackhub");

            //partitionList.ItemsSource = 
            //messageList.ItemsSource = service.Messages;
           
        }

        private async void partitionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = partitionList.SelectedIndex;
            //var partition = service.Partitions[index];

            //await service.StartReadingAsync(partition.PartitionId, DateTime.MinValue);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            service.Stop();
            
        }

        private async void openAllButton_Click(object sender, RoutedEventArgs e)
        {
            //foreach (var p in service.Partitions)
            //{
            //    await service.StartReadingAsync(p.PartitionId, DateTime.MinValue);
            //}
        }

        private void checkPoint_Checked(object sender, RoutedEventArgs e)
        {
           

            var service = (rootGrid.DataContext as MainViewModel).EventHub;
            switch ((e.OriginalSource as RadioButton).Tag.ToString().ToUpper())
            {
                case "NOW":
                    service.CheckPoint = CheckPointTypes.Now;
                    break;
                case "START":
                    service.CheckPoint = CheckPointTypes.Start;
                    break;
                case "MINUTES":
                    service.CheckPoint = CheckPointTypes.PastMinutes;
                    break;
                case "MESSAGES":
                    service.CheckPoint = CheckPointTypes.PastMessages;
                    break;
                default:
                    throw new InvalidOperationException("Invalid Tag on Checkpoint Radio Button");
                    
            }

        }

        private void messageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
