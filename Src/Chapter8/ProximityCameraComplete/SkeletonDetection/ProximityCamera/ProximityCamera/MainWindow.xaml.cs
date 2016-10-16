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
using System.Windows.Threading;
using System.Diagnostics;
using ImageManipulationExtensionMethods;
using Microsoft.Kinect;

namespace ProximityCamera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Microsoft.Kinect.KinectSensor _kinectSensor;
        bool _isTracking = false;
       
        int _threshold = 100;
        DateTime _lastSkeletonTrackTime;
        DispatcherTimer _timer = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();
          
            this.Unloaded += delegate{
                _kinectSensor.ColorStream.Disable();
                _kinectSensor.SkeletonStream.Disable();
            };

            this.Loaded += delegate
            {
                _kinectSensor = Microsoft.Kinect.KinectSensor.KinectSensors[0];
                _kinectSensor.ColorFrameReady += ColorFrameReady;
                _kinectSensor.ColorStream.Enable();
             

                _kinectSensor.SkeletonFrameReady += Pulse;
                _kinectSensor.SkeletonStream.Enable();
                _timer.Interval = new TimeSpan(0, 0, 1);
                _timer.Tick += new EventHandler(_timer_Tick);

                _kinectSensor.Start();
            };
        }

        void ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            
            if (_isTracking)
            {
                using (var frame = e.OpenColorImageFrame())
                {
                    if (frame != null)
                        rgbImage.Source = frame.ToBitmapSource();
                };
            }
            else
                rgbImage.Source = null;
        }

        private void OnDetection()
        {
            if (!_isTracking)
                _isTracking = true;
        }

        private void OnDetectionStopped()
        {
            _isTracking = false;
        }



        #region  player detection

        void _timer_Tick(object sender, EventArgs e)
        {

            if (DateTime.Now.Subtract(_lastSkeletonTrackTime).TotalMilliseconds > _threshold)
            {
                _timer.Stop();
                OnDetectionStopped();
            }
        }

        private void Pulse(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null || skeletonFrame.SkeletonArrayLength == 0)
                    return; 

                Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletons);

                for (int s = 0; s < skeletons.Length; s++)
                {
                    if (skeletons[s].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        OnDetection();

                        _lastSkeletonTrackTime = DateTime.Now;

                        if (!_timer.IsEnabled)
                        {
                            _timer.Start();
                        }
                        break;
                    }
                }
            }
        }
        #endregion


    }
}
