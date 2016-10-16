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
using System.Windows.Media.Media3D;
using ImageManipulationExtensionMethods;

namespace Hologram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _kinectSensor;

        public MainWindow()
        {
            InitializeComponent();
            this.Unloaded += delegate
            {
                _kinectSensor.DepthStream.Disable();
                _kinectSensor.SkeletonStream.Disable();
            };

            this.Loaded += delegate
            {
                _kinectSensor = KinectSensor.KinectSensors[0];
                _kinectSensor.SkeletonFrameReady += SkeletonFrameReady;
                _kinectSensor.DepthFrameReady += DepthFrameReady;
                _kinectSensor.SkeletonStream.Enable();
                _kinectSensor.DepthStream.Enable();
                _kinectSensor.Start();
            };
        }

        void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var imageData = e.OpenDepthImageFrame())
            {
                if (imageData == null || imageData.PixelDataLength == 0)
                    return;

                this.image1.Source = imageData.ToBitmapSource();
            }
        }

        void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            float x=0, y=0, z = 0;
            //get angle of skeleton
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null || frame.SkeletonArrayLength == 0)
                    return;

                var skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                for (int s = 0; s < skeletons.Length; s++)
                {
                    if (skeletons[s].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        border.BorderBrush = new SolidColorBrush(Colors.Red);
                        var skeleton = skeletons[s];
                        x = skeleton.Position.X * 60;
                        z = skeleton.Position.Z * 120;
                        y = skeleton.Position.Y;
                        break;
                    }
                    else
                    {
                        border.BorderBrush = new SolidColorBrush(Colors.Black);
                    }

                }
            }
            if (Math.Abs(x) > 0)
            {
                camera.Position = new System.Windows.Media.Media3D.Point3D(x, y , z);
                camera.LookDirection = new System.Windows.Media.Media3D.Vector3D(-x, -y , -z);
            }
        }


    }
}
