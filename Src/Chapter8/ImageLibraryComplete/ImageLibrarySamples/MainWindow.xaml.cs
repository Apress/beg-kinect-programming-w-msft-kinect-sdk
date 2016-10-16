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
using ImageManipulationExtensionMethods;

namespace ImageLibrarySamples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Microsoft.Kinect.KinectSensor _kinectSensor;

        public MainWindow()
        {
            InitializeComponent();

            this.Unloaded += delegate
            {
                _kinectSensor.ColorStream.Disable();
                _kinectSensor.DepthStream.Disable();
            };

            this.Loaded += delegate
                {
                    _kinectSensor = KinectSensor.KinectSensors[0];
                    _kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    _kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    _kinectSensor.ColorFrameReady += ColorFrameReady;
                    _kinectSensor.DepthFrameReady += DepthFrameReady;

                    _kinectSensor.Start();
                };
        }



        void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            this.depthImage.Source = e.OpenDepthImageFrame().ToBitmap().ToBitmapSource();
        }

        void ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            this.rgbImage.Source = e.OpenColorImageFrame().ToBitmapSource();
        }


    }
}
