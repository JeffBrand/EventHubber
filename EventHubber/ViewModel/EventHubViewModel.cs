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
        private readonly int DEFAULT_MESSAGE_PER_PARTIION = 100;

        IEventHubService _service;
        IDisposable _partitionSubscription;

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


        bool _isReading;
        public bool IsReading
        {
            get { return _isReading; }
            set
            {
                if (_isReading == value)
                    return;
                _isReading = value;
                RaisePropertyChanged("IsReading");
            }
        }

        bool _isOpening;
        public bool IsOpening
        {
            get { return _isOpening; }
            set
            {
                if (_isOpening == value)
                    return;
                _isOpening = value;
                RaisePropertyChanged("IsOpening");
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


        long _totalMessageCount;
        public long TotalMessageCount
        {
            get { return _totalMessageCount; }
            set
            {
                if (_totalMessageCount == value)
                    return;
                _totalMessageCount = value;
                RaisePropertyChanged("TotalMessageCount");
            }
        }


        long _numMessageReceived;

        public long NumberOfMessagesReceived
        {
            get { return _numMessageReceived; }
            set
            {
                if (_numMessageReceived == value)
                    return;
                _numMessageReceived = value;
                RaisePropertyChanged("NumberOfMessagesReceived");
            }
        }

        public int MessagePerPartition { get; set; }

        public int PastMinutes { get; set; }
        public int PastMessages { get; set; }


        public ObservableCollection<PartitionViewModel> Partitions { get; set; }
        public ObservableCollection<MessageViewModel> Messages{ get; set; }

       
       

       

        public RelayCommand Open { get; set; }
        public RelayCommand ReadAll { get; set; }

        public RelayCommand<PartitionViewModel> Read { get; set; }

        public RelayCommand<string> FindPublisher { get; set; }

        public RelayCommand Stop { get; set; }

        public EventHubViewModel(IEventHubService service)
        {
            this.SaveSettings = true;
            this.PastMinutes = 1;
            this.PastMessages = 1;
            this.MessagePerPartition = DEFAULT_MESSAGE_PER_PARTIION;

            _service = service;
            this.Partitions = new ObservableCollection<PartitionViewModel>();
            this.Messages = new ObservableCollection<MessageViewModel>();

            

            _service.MessageReceived.Subscribe((m) =>
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => {
                    Messages.Insert(0,new MessageViewModel(m));
                    NumberOfMessagesReceived++;
                    if (Messages.Count > Partitions.Count * MessagePerPartition)
                        Messages.RemoveAt(Messages.Count - 1);
                }), null);
            });
            SetupCommands();

        }

        private void SetupCommands()
        {
            this.Open = new RelayCommand(async () => {
                this.IsOpening = true;
                this.Partitions.Clear();
                this.Messages.Clear();
                
                _partitionSubscription = _service.PartitionFound.Subscribe((p) =>
                {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                        this.TotalMessageCount = _service.MessageCount;
                        Partitions.Add(new PartitionViewModel(p));
                    }), null);
                }, 
                ()=> {
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.IsOpening = false;
                    }));
                });
                await _service.OpenEventHubAsync(EventHubConnectionString, HubName);
                UpdateCommands();
            });

            this.ReadAll = new RelayCommand(() =>
            {
                ResetReading();
                switch (CheckPoint)
                {
                    case CheckPointTypes.Start:
                        _service.ReadAllAsync(DateTime.MinValue);
                        break;
                    case CheckPointTypes.Now:
                        _service.ReadAllAsync(DateTime.Now);
                        break;
                    case CheckPointTypes.PastMinutes:
                        _service.ReadAllAsync(TimeSpan.FromMinutes(this.PastMinutes));
                        break;
                    case CheckPointTypes.PastMessages:
                        _service.ReadAllAsync(this.PastMessages);
                        break;
                    default:
                        break;
                }
                IsReading = true;
                UpdateCommands();
            }, ()=> {

                return !_service.IsReading && _service.IsOpen;
            });

            this.Read = new RelayCommand<PartitionViewModel>((p) => {
                ResetReading();
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
                IsReading = true;
                UpdateCommands();
            }
                );

            this.FindPublisher = new RelayCommand<string>((publisher) =>
            {
                if (string.IsNullOrWhiteSpace(publisher))
                    return;

                var found = (from m in Messages where m.Publisher == publisher select m).ToList();
                Messages.Clear();
                foreach (var f in found)
                {
                    Messages.Add(f);
                }

            });

            this.Stop = new RelayCommand(() =>
            {
                StopReading();
            }, ()=>{ return _service.IsReading; });
        }

        private void ResetReading()
        {
            if (_service.IsReading)
                StopReading();
            this.Messages.Clear();
        }

        private void StopReading()
        {
            if (!_service.IsReading)
                return;
            IsReading = false;
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
