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



namespace BeginningKinect.Chapter3.PlayerIndexing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectDevice;
        private WriteableBitmap _RawDepthImage;
        private Int32Rect _RawDepthImageRect;
        private short[] _RawDepthPixelData;
        private int _RawDepthImageStride;
        private WriteableBitmap _EnhDepthImage;
        private Int32Rect _EnhDepthImageRect;
        private short[] _EnhDepthPixelData;
        private int _EnhDepthImageStride;
        private int _TotalFrames;
        private DateTime _StartFrameTime;
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
                    frame.CopyPixelDataTo(this._RawDepthPixelData);
                    this._RawDepthImage.WritePixels(this._RawDepthImageRect, this._RawDepthPixelData, this._RawDepthImageStride, 0);                    
                    CreatePlayerDepthImage(frame, this._RawDepthPixelData);                    
                }
            }


            FramesPerSecondElement.Text = string.Format("{0:0} fps", (this._TotalFrames++ / DateTime.Now.Subtract(this._StartFrameTime).TotalSeconds));
        }


        private void CreatePlayerDepthImage(DepthImageFrame depthFrame, short[] pixelData)
        {                             
            int playerIndex;            
            int depthBytePerPixel   = 4;
            byte[] enhPixelData     = new byte[depthFrame.Width * depthFrame.Height * depthBytePerPixel];
                        

            for(int i = 0, j = 0; i < pixelData.Length; i++, j += depthBytePerPixel)
            {                   
                playerIndex = pixelData[i] & DepthImageFrame.PlayerIndexBitmask;
                                    
                if(playerIndex == 0)
                {
                    enhPixelData[j]     = 0xFF;        
                    enhPixelData[j + 1] = 0xFF;
                    enhPixelData[j + 2] = 0xFF;
                }
                else
                {
                    enhPixelData[j]     = 0x00;        
                    enhPixelData[j + 1] = 0x00;
                    enhPixelData[j + 2] = 0x00;
                }
            }
            
            
            this._EnhDepthImage.WritePixels(this._EnhDepthImageRect, enhPixelData, this._EnhDepthImageStride, 0);                    
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
                        this._KinectDevice.SkeletonStream.Disable();

                        this.RawDepthImage.Source = null;
                        this.EnhDepthImage.Source = null;
                    }
                   
                    this._KinectDevice = value;

                    //Initialize
                    if(this._KinectDevice != null)
                    {
                        if(this._KinectDevice.Status == KinectStatus.Connected)
                        {
                            this._KinectDevice.SkeletonStream.Enable();                                
                            this._KinectDevice.DepthStream.Enable();                                                       

                            DepthImageStream depthStream    = this._KinectDevice.DepthStream;
                            this._RawDepthImage             = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                            this._RawDepthImageRect         = new Int32Rect(0, 0, (int) Math.Ceiling(this._RawDepthImage.Width), (int) Math.Ceiling(this._RawDepthImage.Height));
                            this._RawDepthImageStride       = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                            this._RawDepthPixelData         = new short[depthStream.FramePixelDataLength];
                            this.RawDepthImage.Source       = this._RawDepthImage;                           

                            this._EnhDepthImage             = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                            this._EnhDepthImageRect         = new Int32Rect(0, 0, (int) Math.Ceiling(this._EnhDepthImage.Width), (int) Math.Ceiling(this._EnhDepthImage.Height));
                            this._EnhDepthImageStride       = depthStream.FrameWidth * 4;
                            this._EnhDepthPixelData         = new short[depthStream.FramePixelDataLength];
                            this.EnhDepthImage.Source       = this._EnhDepthImage;                           


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
