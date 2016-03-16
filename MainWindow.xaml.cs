using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Weather_Images_Downloader.Annotations;

namespace Weather_Images_Downloader
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private double _progress;
        public double Progress
        {
            get { return _progress; }
            private set
            {
                if (Equals(_progress, value)) return;

                _progress = value;
                OnPropertyChanged();
            }
        }

        private string _progressString;
        public string ProgressString
        {
            get { return _progressString; }
            private set
            {
                if (Equals(_progressString, value)) return;

                _progressString = value;
                OnPropertyChanged();
            }
        }

        private string _error;
        public string Error
        {
            get { return _error; }
            set
            {
                if (value == _error) return;

                _error = value;
                OnPropertyChanged();
            }
        }

        private ImageSource _weatherImage;
        public ImageSource WeatherImage
        {
            get { return _weatherImage; }
            set
            {
                if (Equals(value, _weatherImage)) return;
                _weatherImage = value;
                OnPropertyChanged();
            }
        }

        private bool _liveView;
        public bool LiveView
        {
            get { return _liveView; }
            set
            {
                if (value == _liveView) return;
                _liveView = value;
                OnPropertyChanged();

                if (!LiveView)
                    WeatherImage = null;
            }
        }

        private int _total;
        private int _counter;

        public MainWindow()
        {
            InitializeComponent();
            Progress = 0f;
            ProgressString = "-";

            Static.LoadSettings();

            DataContext = Static.SettingsInstance;
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            var weatherImages = new List<WeatherImage>();

            var types = new List<string>();

            if(Static.SettingsInstance.Rain)
                types.Add("grey_flat");
            if (Static.SettingsInstance.Weather)
                types.Add("composite");

            var pattern = new Regex(@"(\w+)");
            var matches = pattern.Matches(areaTextBox.Text);

            foreach (Match match in matches)
            {
                foreach (var type in types)
                {
                    if (Static.SettingsInstance.Today)
                    {
                        var currentUtcTime = DateTime.UtcNow;
                        currentUtcTime = currentUtcTime.AddMinutes(- currentUtcTime.Minute % 5);
                        weatherImages.AddRange(CreateWeatherImages(match.Value, type, DateTime.Today, currentUtcTime));
                    }
                    if (Static.SettingsInstance.Yesterday)
                        weatherImages.AddRange(CreateWeatherImages(match.Value, type, DateTime.Today.AddDays(- 1), DateTime.Today.AddDays(- 1).AddHours(23)));
                    if (Static.SettingsInstance.YesterYesterday)
                        weatherImages.AddRange(CreateWeatherImages(match.Value, type, DateTime.Today.AddDays(- 2), DateTime.Today.AddDays(- 2).AddHours(23)));
                }
            }

            _total = weatherImages.Count;
            _counter = 0;

            var errorCounter = 0;
            Error = "";

            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    downloadButton.IsEnabled = false;
                });

                Parallel.For(0, weatherImages.Count, (i =>
                {
                    try
                    {
                        _counter++;
                        Progress = (double)(_counter) / _total;
                        ProgressString = (_counter) + "/" + _total + " - " + Math.Round(Progress * 100, 1) + "%";

                        var weatherImage = weatherImages[i];
                        var request = WebRequest.CreateHttp(weatherImage.Url);
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.87 Safari/537.36";
                        var response = request.GetResponse();
                        var responseStream = response.GetResponseStream();

                        var directory = Path.Combine(Environment.CurrentDirectory, "weatherimages", weatherImage.Area, weatherImage.Type);
                        var path = Path.Combine(directory, weatherImage.Filename);

                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        using (var fs = new FileStream(path, FileMode.Create))
                        {
                            var buffer = new byte[1024];
                            var totalBytesRead = 0;

                            while (totalBytesRead < response.ContentLength)
                            {
                                var bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                                fs.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                            }
                        }

                        if (LiveView)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    WeatherImage = new BitmapImage(new Uri(path));
                                }
                                catch
                                {
                                    // ignored
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = "Error (" + ++errorCounter + "): " + ex.Message;
                    }
                }));

                Dispatcher.Invoke(() =>
                {
                    downloadButton.IsEnabled = true;
                });
            });
        }

        private WeatherImage[] CreateWeatherImages(string area, string type, DateTime start, DateTime end)
        {
            var weatherImages = new List<WeatherImage>();

            do
            {
                weatherImages.Add(CreateWeatherImage(area, type, start));

                start = start.AddMinutes(5);
            } while (start < end);

            return weatherImages.ToArray();
        }

        private WeatherImage CreateWeatherImage(string area, string type, DateTime dateTime)
        {
            var year = dateTime.ToString("yyyy");
            var month = dateTime.ToString("MM");
            var day = dateTime.ToString("dd");
            var hour = dateTime.ToString("HH");
            var minute = dateTime.ToString("mm");
            
            var url = string.Format("http://www.wetteronline.de/?ireq=true&pid=p_radar_map&src=wmapsextract/vermarktung/global2maps/{0}/{1}/{2}/{5}/{6}/{0}{1}{2}{3}{4}_{5}.{7}", year, month, day, hour, minute, area, type, type == "grey_flat" ? "png" : "jpeg");
            var file = string.Format("{0}-{1}-{2} {3}-{4} {5} {6}.jpeg", year, month, day, hour, minute, area, type);

            return new WeatherImage(type, area, year, month, day, hour,minute, url, file);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void window_Closed(object sender, EventArgs e)
        {
            Static.SaveSettings();
        }
    }

    struct WeatherImage
    {
        public string Type { get; set; }
        public string Area { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Day { get; set; }
        public string Hour { get; set; }
        public string Minute { get; set; }

        public string Url { get; set; }
        public string Filename { get; set; }

        public WeatherImage(string type, string area, string year, string month, string day, string hour,string minute, string url = "", string filename = "")
        {
            Type = type;
            Area = area;
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Url = url;
            Filename = filename;
        }
    }
}
