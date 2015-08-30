using EventHubber.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace EventHubber.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:EventHubber.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:EventHubber.Controls;assembly=EventHubber.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:EventHubMonitor/>
    ///
    /// </summary>
    /// [
    /// 

    [TemplatePart(Name = RootGrid, Type = typeof(Grid))]
    public class EventHubMonitor : Control
    {
        bool _isReady;
  
        const string RootGrid = "RootGrid";

        Grid _rootGrid;
        List<StackPanel> _partitions = new List<StackPanel>();


       
        public IEnumerable<PartitionViewModel> Partitions
        {
            get { return (IEnumerable<PartitionViewModel>)GetValue(PartitionsProperty); }
            set { SetValue(PartitionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Partitions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PartitionsProperty =
            DependencyProperty.Register("Partitions", typeof(IEnumerable<PartitionViewModel>), typeof(EventHubMonitor), new PropertyMetadata(null, OnPartitionChange));

        private static void OnPartitionChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as EventHubMonitor;
            if (control != null)
            {
                control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
            }
        }

        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged
            var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

            if (null != oldValueINotifyCollectionChanged)
            {
                oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
            }
            // Add handler for newValue.CollectionChanged (if possible)
            var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
            }
        }

        void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null )
                return;

            _rootGrid.RowDefinitions.Add(new RowDefinition());
            var stackPanel = new StackPanel();
            stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            stackPanel.VerticalAlignment = VerticalAlignment.Stretch;
            stackPanel.Background = new SolidColorBrush(Colors.Blue);
            stackPanel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.SetValue(Grid.RowProperty, _rootGrid.RowDefinitions.Count - 1);
            _rootGrid.Children.Add(stackPanel);
        }


        static EventHubMonitor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EventHubMonitor), new FrameworkPropertyMetadata(typeof(EventHubMonitor)));
        }

        public EventHubMonitor()
        {
            this.DefaultStyleKey = typeof(EventHubMonitor);
        }

        public override void OnApplyTemplate()
        {
            _rootGrid = this.GetTemplateChild(RootGrid) as Grid;

            _isReady = true;
            base.OnApplyTemplate();
        }

        
       

    }
}
