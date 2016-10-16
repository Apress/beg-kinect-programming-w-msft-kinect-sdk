using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Media;
using System.ComponentModel;

namespace RecordAudio
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string _recordingFileName;
        MediaPlayer _mplayer;
        bool _isPlaying;
        bool _isNoiseSuppressionOn;
        bool _isAutomaticGainOn;
        bool _isAECOn;


        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += delegate { KinectSensor.KinectSensors[0].Start(); };
            _mplayer = new MediaPlayer();
            _mplayer.MediaEnded += delegate { _mplayer.Close(); IsPlaying = false; };
            this.DataContext = this;
        }



        private void Play()
        {
            IsPlaying = true;
            _mplayer.Open(new Uri(_recordingFileName, UriKind.Relative));
            _mplayer.Play();
        }


        private void Record()
        {
            Thread thread = new Thread(new ThreadStart(RecordKinectAudio));
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

        private void Stop()
        {
            KinectSensor.KinectSensors[0].AudioSource.Stop();
            IsRecording = false;
        }
        private object lockObj = new object();
        private void RecordKinectAudio()
        {
            lock (lockObj)
            {
                IsRecording = true;

                var source = CreateAudioSource();

                var time = DateTime.Now.ToString("hhmmss");
                _recordingFileName = time + ".wav";
                using (var fileStream =
                new FileStream(_recordingFileName, FileMode.Create))
                {
                    RecorderHelper.WriteWavFile(source, fileStream);
                }

                IsRecording = false;
            }

        }

        private KinectAudioSource CreateAudioSource()
        {
            var source = KinectSensor.KinectSensors[0].AudioSource;
            source.BeamAngleMode = BeamAngleMode.Adaptive;
            source.NoiseSuppression = _isNoiseSuppressionOn;
            source.AutomaticGainControlEnabled = _isAutomaticGainOn;

            if (IsAECOn)
            {
                source.EchoCancellationMode = EchoCancellationMode.CancellationOnly;
                source.AutomaticGainControlEnabled = false;
                IsAutomaticGainOn = false;
                source.EchoCancellationSpeakerIndex = 0;
            }

            return source;
        }

        #region user interaction handlers

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Record();
        }


        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged("IsRecordingEnabled");
                }
            }
        }

        private bool IsRecording
        {
            get
            {
                return RecorderHelper.IsRecording;
            }
            set
            {
                if (RecorderHelper.IsRecording != value)
                {
                    RecorderHelper.IsRecording = value;
                    OnPropertyChanged("IsPlayingEnabled");
                    OnPropertyChanged("IsRecordingEnabled");
                    OnPropertyChanged("IsStopEnabled");
                }
            }
        }
        public bool IsPlayingEnabled
        {
            get { return !IsRecording; }
        }

        public bool IsRecordingEnabled
        {
            get { return !IsPlaying && !IsRecording; }
        }

        public bool IsStopEnabled
        {
            get { return IsRecording; }
        }


        public bool IsNoiseSuppressionOn
        {
            get
            {
                return _isNoiseSuppressionOn;
            }
            set
            {
                if (_isNoiseSuppressionOn != value)
                {
                    _isNoiseSuppressionOn = value;
                    OnPropertyChanged("IsNoiseSuppressionOn");
                }
            }
        }

        public bool IsAutomaticGainOn
        {
            get
            {
                return _isAutomaticGainOn;
            }
            set
            {
                if (_isAutomaticGainOn != value)
                {
                    _isAutomaticGainOn = value;
                    OnPropertyChanged("IsAutomaticGainOn");
                }
            }
        }


        public bool IsAECOn
        {
            get
            {
                return _isAECOn;
            }
            set
            {
                if (_isAECOn != value)
                {
                    _isAECOn = value;
                    OnPropertyChanged("IsAECOn");
                }
            }
        }

        #endregion


    }
}
