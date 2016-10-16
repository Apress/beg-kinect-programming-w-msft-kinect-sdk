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
using System.ComponentModel;
using System.Threading;
using Microsoft.Kinect;
using System.Diagnostics;

namespace FindAudioDirection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += delegate { ListenForBeamChanges(); };
        }

        private KinectAudioSource CreateAudioSource()
        {
            var source = KinectSensor.KinectSensors[0].AudioSource;
            source.NoiseSuppression = true;
            source.AutomaticGainControlEnabled = true;
            source.BeamAngleMode = BeamAngleMode.Adaptive;
            return source;
        }

        private void ListenForBeamChanges()
        {
            KinectSensor.KinectSensors[0].Start();
            var audioSource = CreateAudioSource();
            audioSource.BeamAngleChanged += audioSource_BeamAngleChanged;
            audioSource.Start();
        }

        void audioSource_BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            BeamAngle = -1 * e.Angle;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private double _beamAngle;
        public double BeamAngle
        {
            get { return _beamAngle; }
            set
            {
                _beamAngle = value;
                OnPropertyChanged("BeamAngle");
            }
        }
    }
}
