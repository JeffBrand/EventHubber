using EventHubber.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EventHubber.ViewModel
{
    public enum CheckPointTypes
    {
        Now, 
        Start,
        PastMinutes,
        PastMessages
    }

    public class EventHubViewModel : ViewModelBase
    {
        IEventHubService _service;

        public bool SaveSettings { get; set; }

        public string EventHubConnectionString {
            get {
                return Properties.Settings.Default["eventhubConnectionString"] as string; }
            set {
                
                Properties.Settings.Default["eventhubConnectionString"] = value;
                if (SaveSettings)
                    Properties.Settings.Default.Save();
            } }
        public string HubName
        {
            get
            {
                return Properties.Settings.Default["hubName"] as string;
            }
            set
            {
              
                Properties.Settings.Default["hubName"] = value;
                if (SaveSettings)
                    Properties.Settings.Default.Save();
            }
        }

        CheckPointTypes _checkpoint;
        public CheckPointTypes CheckPoint
        {
            get { return _checkpoint; }
            set {
                if (_checkpoint == value)
                    return;

                _checkpoint = value;
            }

        }
        public int PastMinutes { get; set; }
        public int PastMessages { get; set; }


        public ObservableCollection<PartitionViewModel> Partitions { get; set; }
        public ObservableCollection<MessageViewModel> Messages{ get; set; }

       
       

       

        public RelayCommand Open { get; set; }
        public RelayCommand ReadAll { get; set; }

        public RelayCommand<PartitionViewModel> Read { get; set; }

        public RelayCommand Stop { get; set; }

        public EventHubViewModel(IEventHubService service)
        {
            this.SaveSettings = true;

            _service = service;
            this.Partitions = new ObservableCollection<PartitionViewModel>();
            this.Messages = new ObservableCollection<MessageViewModel>();

            _service.PartitionFound.Subscribe((p) =>
            {
                App.Current.Dispatcher.BeginInvoke(new Action(()=> {
                    Partitions.Add(new PartitionViewModel(p));
                }), null);
            });

            _service.MessageReceived.Subscribe((m) =>
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => {
                    Messages.Insert(0,new MessageViewModel(m));
                }), null);
            });
            SetupCommands();

        }

        private void SetupCommands()
        {
            this.Open = new RelayCommand(async () => {
                this.Partitions.Clear();
                this.Messages.Clear();
                await _service.OpenEventHubAsync(EventHubConnectionString, HubName);
                UpdateCommands();
            });

            this.ReadAll = new RelayCommand(() => {
                this.Messages.Clear();
                switch (CheckPoint)
                {
                    case CheckPointTypes.Start:
                        _service.ReadAllAsync(DateTime.MinValue);
                        break;
                    case CheckPointTypes.Now:
                        _service.ReadAllAsync(DateTime.Now);
                        break;
                    case CheckPointTypes.PastMinutes:
                        break;
                    case CheckPointTypes.PastMessages:
                        break;
                    default:
                        break;
                }
                UpdateCommands();
            },()=> {

                return !_service.IsReading && _service.IsOpen;
            });

            this.Read = new RelayCommand<PartitionViewModel>((p) => {
                if (_service.IsReading)
                    StopReading();

                this.Messages.Clear();
                switch (CheckPoint)
                {
                    case CheckPointTypes.Start:
                        _service.ReadAsync(p.PartitionId, DateTime.MinValue);
                        break;
                    case CheckPointTypes.Now:
                        _service.ReadAsync(p.PartitionId,DateTime.Now);
                        break;
                    case CheckPointTypes.PastMinutes:
                        break;
                    case CheckPointTypes.PastMessages:
                        break;
                    default:
                        break;
                }
                UpdateCommands();
            }
            //, 
            //    (p) => {
            //    return !_service.IsReading && _service.IsOpen;
            //}
                );

            this.Stop = new RelayCommand(() =>
            {
                StopReading();
            }, ()=>{ return _service.IsReading; });
        }

        private void StopReading()
        {
            if (!_service.IsReading)
                return;

            _service.Stop();
            UpdateCommands();
        }

        private void UpdateCommands()
        {
            this.ReadAll.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
            this.Read.RaiseCanExecuteChanged();
        }
    }
}
