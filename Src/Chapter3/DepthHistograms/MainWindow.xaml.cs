/*
 * 
 *  Copyright (c) 2012 Jarrett Webb & James Ashley
 * 
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
 *  documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 *  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
 *  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 *  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 *  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
 *  IN THE SOFTWARE.
 * 
 * 
 */


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



namespace BeginningKinect.Chapter3.DepthHistograms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectDevice;
        private WriteableBitmap _DepthImage;
        private Int32Rect _DepthImageRect;
        private short[] _DepthPixelData;
        private int _DepthImageStride;
        private int _TotalFrames;
        private DateTime _StartFrameTime;        
        private const int LoDepthThreshold = 1220;
        private const int HiDepthThreshold = 3048;
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);            
        }
        #endregion Constructor


        #region Methods
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;                                        
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.                    
                    this.KinectDevice = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }


        private void KinectDevice_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using(DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if(frame != null)
                {
                    frame.CopyPixelDataTo(this._DepthPixelData);                    
                    CreateBetterShadesOfGray(frame, this._DepthPixelData);
                    CreateDepthHistogram(frame, this._DepthPixelData);            
                }
            }
                                    

            FramesPerSecondElement.Text = string.Format("{0:0} fps", (this._TotalFrames++ / DateTime.Now.Subtract(this._StartFrameTime).TotalSeconds));
        }

        
        private void CreateBetterShadesOfGray(DepthImageFrame depthFrame, short[] pixelData)
        {     
            int depth;         
            int gray;
            int bytesPerPixel       = 4;            
            byte[] enhPixelData     = new byte[depthFrame.Width * depthFrame.Height * bytesPerPixel];
            
            for(int i = 0, j = 0; i < pixelData.Length; i++, j += bytesPerPixel)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if(depth < LoDepthThreshold || depth > HiDepthThreshold)
                {
                    gray = 0xFF;
                }
                else
                {    
                    gray = 255 - (255 * depth / 0xFFF);
                }

                enhPixelData[j]        = (byte) gray;
                enhPixelData[j + 1]    = (byte) gray;
                enhPixelData[j + 2]    = (byte) gray;                
            }

            this._DepthImage.WritePixels(this._DepthImageRect, enhPixelData, this._DepthImageStride, 0);
        }

                
        private void CreateDepthHistogram(DepthImageFrame depthFrame, short[] pixelData)
        {
            int depth;            
            int[] depths            = new int[4096];            
            double chartBarWidth    = Math.Max(3, DepthHistogram.ActualWidth / depths.Length);
            int maxValue            = 0;


            DepthHistogram.Children.Clear();

            
            //First pass - Count the depths.
            for(int i = 0; i < pixelData.Length; i++)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if(depth >= LoDepthThreshold && depth <= HiDepthThreshold)
                {
                    depths[depth]++;                                
                }
            }

            
            //Second pass - Find the max depth count to scale the histogram to the space available.
            //              This is only to make the UI look nice.
            for(int i = 0; i < depths.Length; i++)
            {
                maxValue = Math.Max(maxValue, depths[i]);
            }
            

            //Third pass - Build the histogram.
            for(int i = 0; i < depths.Length; i++)
            {
                if(depths[i] > 0)
                {
                    Rectangle r         = new Rectangle();
                    r.Fill              = Brushes.Black;
                    r.Width             = chartBarWidth;
                    r.Height            = DepthHistogram.ActualHeight * (depths[i] / (double) maxValue);
                    r.Margin            = new Thickness(1,0,1,0);
                    r.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    DepthHistogram.Children.Add(r);
                }
            }
        }         
        #endregion Methods


        #region Properties
        public KinectSensor KinectDevice 
        {
            get { return this._KinectDevice; }
            set
            {
                if(this._KinectDevice != value)
                {
                    //Uninitialize
                    if(this._KinectDevice != null)
                    {
                        this._KinectDevice.Stop();                        
                        this._KinectDevice.DepthFrameReady -= KinectDevice_DepthFrameReady;
                        this._KinectDevice.DepthStream.Disable();

                        this.DepthImage.Source = null;
                        this._DepthImage = null;                        
                    }
                   
                    this._KinectDevice = value;

                    //Initialize
                    if(this._KinectDevice != null)
                    {
                        if(this._KinectDevice.Status == KinectStatus.Connected)
                        {                            
                            this._KinectDevice.DepthStream.Enable(); 
                            
                            DepthImageStream depthStream    = this._KinectDevice.DepthStream;
                            this._DepthImage                = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                            this._DepthImageRect            = new Int32Rect(0, 0, (int) Math.Ceiling(this._DepthImage.Width), (int) Math.Ceiling(this._DepthImage.Height));
                            this._DepthImageStride          = depthStream.FrameWidth * 4;
                            this._DepthPixelData            = new short[depthStream.FramePixelDataLength];
                            this.DepthImage.Source          = this._DepthImage;                           
                            
                            this._KinectDevice.DepthFrameReady += KinectDevice_DepthFrameReady;
                            this._KinectDevice.Start();

                            this._StartFrameTime = DateTime.Now;
                        }
                    }                
                }
            }
        }        
        #endregion Properties
    }
}
