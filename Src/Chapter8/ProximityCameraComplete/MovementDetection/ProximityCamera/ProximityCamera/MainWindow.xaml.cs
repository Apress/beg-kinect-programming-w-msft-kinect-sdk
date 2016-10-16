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
using Emgu.CV;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ImageManipulationExtensionMethods;

namespace ProximityCamera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _kinectSensor;
        private MotionHistory _motionHistory;
        private IBGFGDetector<Bgr> _forgroundDetector;
        bool _isTracking = false;


        public MainWindow()
        {
            InitializeComponent();

            this.Unloaded += delegate
            {
                _kinectSensor.ColorStream.Disable();
            };

            this.Loaded += delegate
            {

                _motionHistory = new MotionHistory(
                    1.0, //in seconds, the duration of motion history you wants to keep
                    0.05, //in seconds, parameter for cvCalcMotionGradient
                    0.5); //in seconds, parameter for cvCalcMotionGradient

                _kinectSensor = KinectSensor.KinectSensors[0];

                _kinectSensor.ColorStream.Enable();
                _kinectSensor.Start();

                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += (a, b) => Pulse();
                bw.RunWorkerCompleted += (c, d) => bw.RunWorkerAsync();
                bw.RunWorkerAsync();
            };
        }


        private void Pulse()
        {
            using (ColorImageFrame imageFrame = _kinectSensor.ColorStream.OpenNextFrame(200))
            {
                if (imageFrame == null)
                    return;

                using (Image<Bgr, byte> image = imageFrame.ToOpenCVImage<Bgr, byte>())
                using (MemStorage storage = new MemStorage()) //create storage for motion components
                {
                    if (_forgroundDetector == null)
                    {
                        _forgroundDetector = new BGStatModel<Bgr>(image
                            , Emgu.CV.CvEnum.BG_STAT_TYPE.GAUSSIAN_BG_MODEL);
                    }

                    _forgroundDetector.Update(image);

                    //update the motion history
                    _motionHistory.Update(_forgroundDetector.ForgroundMask);

                    //get a copy of the motion mask and enhance its color
                    double[] minValues, maxValues;
                    System.Drawing.Point[] minLoc, maxLoc;
                    _motionHistory.Mask.MinMax(out minValues, out maxValues
                        , out minLoc, out maxLoc);
                    Image<Gray, Byte> motionMask = _motionHistory.Mask
                        .Mul(255.0 / maxValues[0]);

                    //create the motion image 
                    Image<Bgr, Byte> motionImage = new Image<Bgr, byte>(motionMask.Size);
                    motionImage[0] = motionMask;

                    //Threshold to define a motion area
                    //reduce the value to detect smaller motion
                    double minArea = 100;

                    storage.Clear(); //clear the storage
                    Seq<MCvConnectedComp> motionComponents = _motionHistory.GetMotionComponents(storage);
                    bool isMotionDetected = false;
                    //iterate through each of the motion component
                    for (int c = 0; c < motionComponents.Count(); c++)
                    {
                        MCvConnectedComp comp = motionComponents[c];
                        //reject the components that have small area;
                        if (comp.area < minArea) continue;

                        OnDetection();
                        isMotionDetected = true;
                        break;
                    }
                    if (isMotionDetected == false)
                    {
                        OnDetectionStopped();
                        this.Dispatcher.Invoke(new Action(() => rgbImage.Source = null));
                        return;
                    }

                    this.Dispatcher.Invoke(
                        new Action(() => rgbImage.Source = imageFrame.ToBitmapSource())
                        );
                }
            }
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

    }
}
