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
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using System.ComponentModel;
using ImageManipulationExtensionMethods;

namespace FaceFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _kinectSensor;

        String faceFileName = "haarcascade_frontalface_default.xml";


        public MainWindow()
        {
            InitializeComponent();

            this.Unloaded += delegate
            {
                _kinectSensor.ColorStream.Disable();
            };

            this.Loaded += delegate
            {
                _kinectSensor = KinectSensor.KinectSensors[0];
                _kinectSensor.ColorStream.Enable();
                _kinectSensor.Start();

                BackgroundWorker bw = new BackgroundWorker();
                bw.RunWorkerCompleted += (a, b) => bw.RunWorkerAsync();
                bw.DoWork += delegate { Pulse(); };
                bw.RunWorkerAsync();
            };


        }


        public void Pulse()
        {
            using (HaarCascade face = new HaarCascade(faceFileName))
            {
                var frame = _kinectSensor.ColorStream.OpenNextFrame(100);
                var image = frame.ToOpenCVImage<Rgb, Byte>();
                using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>()) //Convert it to Grayscale
                {
                    //normalizes brightness and increases contrast of the image
                    gray._EqualizeHist();

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel
                    MCvAvgComp[] facesDetected = face.Detect(
                       gray,
                       1.1,
                       10,
                       Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                       new System.Drawing.Size(20, 20));

                    Image<Rgb, Byte> happyMan = new Image<Rgb, byte>("happy-man.png");
                    foreach (MCvAvgComp f in facesDetected)
                    {

                        //image.Draw(f.rect, new Rgb(System.Drawing.Color.Blue), 2);
                        var rect = new System.Drawing.Rectangle(f.rect.X - f.rect.Width / 2
                            , f.rect.Y - f.rect.Height / 2
                            , f.rect.Width * 2
                            , f.rect.Height * 2);

                        var newImage = happyMan.Resize(rect.Width, rect.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

                        for (int i = 0; i < (rect.Height); i++)
                        {
                            for (int j = 0; j < (rect.Width); j++)
                            {
                                // mask black image background with 'or' logic
                                if (newImage[i, j].Blue != 0 || newImage[i, j].Red != 0 || newImage[i, j].Green != 0)
                                {
                                    if (j + rect.X < image.Width && j + rect.X > 0)
                                    {
                                        var dot = newImage[i, j];
                                        // additional safety logic
                                        // don't attempt if we are outside the bounds of the image
                                        if (i + rect.Y > image.Height || i + rect.Y < 0 || j + rect.X > image.Width || j + rect.X < 0)
                                            continue;
                                        image[i + rect.Y, j + rect.X] = dot;
                                    }
                                }
                            }

                        }
                    }

                    Dispatcher.BeginInvoke(new Action(() => { rgbImage.Source = image.ToBitmapSource(); }));
                }
            }
        }
    }
}

