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
        private double _beamAngle;

        public MainWindow()
        {
            //TODO: implement
        }

        private KinectAudioSource CreateAudioSource()
        {
            //TODO: implement
        }

        private void ListenForBeamChanges()
        {
            //TODO: implement
        }

        void audioSource_BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            //TODO: implement
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            //TODO: implement
        }



    }
}
