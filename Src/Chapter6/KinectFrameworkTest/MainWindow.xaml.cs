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
using Beginning.Kinect.Framework.Controls;
using Microsoft.Kinect;
using WaveDetection;
using Beginning.Kinect.Framework;

namespace KinectFrameworkTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _kinectDevice;
        private Skeleton[] _FrameSkeletons;
        private WaveGesture _WaveGesture;

        public MainWindow()
        {
            InitializeComponent();
            this._WaveGesture = new WaveGesture();
            this._WaveGesture.GestureDetected += new EventHandler(_WaveGesture_GestureDetected); 
            this._kinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            this._kinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as KinectButton;
            button.Background = new SolidColorBrush(Colors.Green);
        }

        private void Button_KinectCursorLeave(object sender, Beginning.Kinect.Framework.Input.KinectCursorEventArgs e)
        {
            var button = sender as KinectButton;
            button.Background = new SolidColorBrush(Colors.Red);
        }


        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    this._FrameSkeletons = new Skeleton[_kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                    frame.CopySkeletonDataTo(this._FrameSkeletons);

                    DateTime startMarker = DateTime.Now;
                    this._WaveGesture.Update(this._FrameSkeletons, frame.Timestamp);            
                }
            }
        }

        private void _WaveGesture_GestureDetected(object sender, EventArgs e)
        {
            listBox1.Items.Add(string.Format("Wave Detected {0}",DateTime.Now.ToLongTimeString()));

        }

    }
}
