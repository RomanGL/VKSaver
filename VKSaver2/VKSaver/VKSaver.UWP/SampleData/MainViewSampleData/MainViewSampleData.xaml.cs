//      *********    НЕ ИЗМЕНЯЙТЕ ЭТОТ ФАЙЛ     *********
//      Этот файл обновляется средством разработки. Внесение
//      изменений в этот файл может привести к ошибкам.
namespace Blend.SampleData.MainViewSampleData
{
    using System; 
    using System.ComponentModel;

// Чтобы значительно уменьшить объем примеров данных в рабочем приложении, можно задать
// константу условной компиляции DISABLE_SAMPLE_DATA и отключить пример данных во время выполнения.
#if DISABLE_SAMPLE_DATA
    internal class MainViewSampleData { }
#else

    public class MainViewSampleData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainViewSampleData()
        {
            try
            {
                Uri resourceUri = new Uri("ms-appx:/SampleData/MainViewSampleData/MainViewSampleData.xaml", UriKind.RelativeOrAbsolute);
                Windows.UI.Xaml.Application.LoadComponent(this, resourceUri);
            }
            catch
            {
            }
        }

        private TopArtistsLF _TopArtistsLF = new TopArtistsLF();

        public TopArtistsLF TopArtistsLF
        {
            get
            {
                return this._TopArtistsLF;
            }
        }

        private UserTracks _UserTracks = new UserTracks();

        public UserTracks UserTracks
        {
            get
            {
                return this._UserTracks;
            }
        }

        private RecommendedTracksVK _RecommendedTracksVK = new RecommendedTracksVK();

        public RecommendedTracksVK RecommendedTracksVK
        {
            get
            {
                return this._RecommendedTracksVK;
            }
        }
    }

    public class TopArtistsLF : System.Collections.ObjectModel.ObservableCollection<TopArtistsLFItem>
    { 
    }

    public class TopArtistsLFItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private MegaImage _MegaImage = new MegaImage();

        public MegaImage MegaImage
        {
            get
            {
                return this._MegaImage;
            }

            set
            {
                if (this._MegaImage != value)
                {
                    this._MegaImage = value;
                    this.OnPropertyChanged("MegaImage");
                }
            }
        }

        private string _Name = string.Empty;

        public string Name
        {
            get
            {
                return this._Name;
            }

            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }

        private double _PlayCount = 0;

        public double PlayCount
        {
            get
            {
                return this._PlayCount;
            }

            set
            {
                if (this._PlayCount != value)
                {
                    this._PlayCount = value;
                    this.OnPropertyChanged("PlayCount");
                }
            }
        }
    }

    public class MegaImage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Windows.UI.Xaml.Media.ImageSource _URL = null;

        public Windows.UI.Xaml.Media.ImageSource URL
        {
            get
            {
                return this._URL;
            }

            set
            {
                if (this._URL != value)
                {
                    this._URL = value;
                    this.OnPropertyChanged("URL");
                }
            }
        }
    }

    public class UserTracks : System.Collections.ObjectModel.ObservableCollection<UserTracksItem>
    { 
    }

    public class RecommendedTracksVK : System.Collections.ObjectModel.ObservableCollection<UserTracksItem>
    {
    }

    public class UserTracksItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _Title = string.Empty;

        public string Title
        {
            get
            {
                return this._Title;
            }

            set
            {
                if (this._Title != value)
                {
                    this._Title = value;
                    this.OnPropertyChanged("Title");
                }
            }
        }

        private string _Artist = string.Empty;

        public string Artist
        {
            get
            {
                return this._Artist;
            }

            set
            {
                if (this._Artist != value)
                {
                    this._Artist = value;
                    this.OnPropertyChanged("Artist");
                }
            }
        }

        private double _Duration = 0;

        public double Duration
        {
            get
            {
                return this._Duration;
            }

            set
            {
                if (this._Duration != value)
                {
                    this._Duration = value;
                    this.OnPropertyChanged("Duration");
                }
            }
        }
    }
#endif
}
