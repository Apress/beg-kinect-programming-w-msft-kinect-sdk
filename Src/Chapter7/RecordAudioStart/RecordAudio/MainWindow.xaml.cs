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
            //TODO: implement
        }

        private void Play()
        {
            //TODO: implement
        }

        private void Record()
        {
            //TODO: implement
        }

        private void Stop()
        {
            //TODO: implement
        }

        private void RecordKinectAudio()
        {
            //TODO: implement

        }

        private KinectAudioSource CreateAudioSource()
        {
            //TODO: implement
        }

        #region user interaction handlers

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //TODO: implement
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //TODO: implement
        }


        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //TODO: implement
        }

        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            //TODO: implement
        }

        #endregion


    }
}
