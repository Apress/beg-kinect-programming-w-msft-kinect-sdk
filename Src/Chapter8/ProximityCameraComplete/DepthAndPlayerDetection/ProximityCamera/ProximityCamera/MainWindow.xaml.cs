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
using System.Windows.Threading;
using System.Diagnostics;
using ImageManipulationExtensionMethods;

namespace ProximityCamera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _kinectSensor;
        bool _isTracking = false;


        WriteableBitmap _videoFrameImage;
        Int32Rect _videoFrameImageRect;

        public MainWindow()
        {
            InitializeComponent();

            this.Unloaded += delegate
            {
                _kinectSensor.ColorStream.Disable();
                _kinectSensor.DepthStream.Disable();
                _kinectSensor.SkeletonStream.Disable();
            };

            this.Loaded += delegate
            {
                _kinectSensor = KinectSensor.KinectSensors[0];

                _kinectSensor.ColorFrameReady += ColorFrameReady;
                _kinectSensor.DepthFrameReady += DepthFrameReady;
                _kinectSensor.ColorStream.Enable();
                _kinectSensor.DepthStream.Enable();
                _kinectSensor.SkeletonStream.Enable();
                _videoFrameImage = new WriteableBitmap(640, 480, 96, 96,
                                           PixelFormats.Bgr32, null);
                _videoFrameImageRect = new Int32Rect(0, 0, 640, 480);
                _kinectSensor.Start();
            };

        }


        void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            bool isInRange = false;
            using (var imageData = e.OpenDepthImageFrame())
            {
                if (imageData == null || imageData.PixelDataLength == 0)
                    return;
                short[] bits = new short[imageData.PixelDataLength];
                imageData.CopyPixelDataTo(bits);
                int minThreshold = 1700;
                int maxThreshold = 2000;


                for (int i = 0; i < bits.Length; i += imageData.BytesPerPixel)
                {
                    var depth = bits[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    var player = bits[i] & DepthImageFrame.PlayerIndexBitmask;

                    if (player > 0 && depth > minThreshold && depth < maxThreshold)
                    {
                        isInRange = true;
                        OnDetection();
                        break;
                    }
                }
            }

            if(!isInRange)
                OnDetectionStopped();
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

            //using (ColorImageFrame imageData = e.OpenColorImageFrame())
            //{
            //    if (imageData == null || imageData.PixelDataLength == 0)
            //        return;

            //    byte[] bits = new byte[imageData.PixelDataLength];
            //    imageData.CopyPixelDataTo(bits);
            //    if (_isTracking)
            //    {
            //        if (rgbImage.Source == null)
            //            rgbImage.Source = this._videoFrameImage;

            //        _videoFrameImage.Lock();
            //        _videoFrameImage.WritePixels(_videoFrameImageRect, bits,
            //                                          imageData.BytesPerPixel * imageData.Width, 0);
            //        _videoFrameImage.Unlock();
            //    }
            //    else
            //        rgbImage.Source = null;
            //}
        }



    }
}
