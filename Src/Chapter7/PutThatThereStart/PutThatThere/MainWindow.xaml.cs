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
using Microsoft.Kinect;
using System.Threading;
using Microsoft.Speech.Recognition;
using System.IO;
using Microsoft.Speech.AudioFormat;
using System.Diagnostics;

namespace PutThatThere
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        KinectSensor _kinectSensor;
        SpeechRecognitionEngine _sre;
        KinectAudioSource _source;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            //TODO: implement
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //TODO: implement
        }

        private KinectAudioSource CreateAudioSource()
        {
            //TODO: implement
        }

        private void StartSpeechRecognition()
        {
            //TODO: implement
        }

        private void CreateGrammars(RecognizerInfo ri)
        {
            //TODO: implement
        }

        void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //TODO: implement
        }

        void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //TODO: implement
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //TODO: implement
        }

        private void InterpretCommand(SpeechRecognizedEventArgs e)
        {
            //TODO: implement
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            //TODO: implement
        }
    }
}
