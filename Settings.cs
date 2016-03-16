using System.ComponentModel;
using System.Runtime.CompilerServices;
using Weather_Images_Downloader.Annotations;

namespace Weather_Images_Downloader
{
    public class Settings : INotifyPropertyChanged
    {
        private bool _today;
        private bool _yesterday;
        private bool _yesterYesterday;
        private bool _rain;
        private bool _weather;
        private string _areas = "euro,DL; namk 0001.BAY °afri";

        public bool Today
        {
            get { return _today; }
            set
            {
                if (value == _today) return;
                _today = value;
                OnPropertyChanged();
            }
        }

        public bool Yesterday
        {
            get { return _yesterday; }
            set
            {
                if (value == _yesterday) return;
                _yesterday = value;
                OnPropertyChanged();
            }
        }

        public bool YesterYesterday
        {
            get { return _yesterYesterday; }
            set
            {
                if (value == _yesterYesterday) return;
                _yesterYesterday = value;
                OnPropertyChanged();
            }
        }

        public bool Rain
        {
            get { return _rain; }
            set
            {
                if (value == _rain) return;
                _rain = value;
                OnPropertyChanged();
            }
        }

        public bool Weather
        {
            get { return _weather; }
            set
            {
                if (value == _weather) return;
                _weather = value;
                OnPropertyChanged();
            }
        }

        public string Areas
        {
            get { return _areas; }
            set
            {
                if (value == _areas) return;
                _areas = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
