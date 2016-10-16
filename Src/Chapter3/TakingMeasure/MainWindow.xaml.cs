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



namespace BeginningKinect.Chapter3.TakingMeasure
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
                    CalculatePlayerSize(frame, this._DepthPixelData);            
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
            int loThreshold         = 1220;
            int hiThreshold         = 3048;

            
            for(int i = 0, j = 0; i < pixelData.Length; i++, j += bytesPerPixel)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if(depth < loThreshold || depth > hiThreshold)
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
        

        private void CalculatePlayerSize(DepthImageFrame depthFrame, short[] pixelData)
        {
            int depth;
            int playerIndex;
            int pixelIndex;            
            int bytesPerPixel = depthFrame.BytesPerPixel;                      
            PlayerDepthData[] players = new PlayerDepthData[6];


            //First pass - Calculate stats from the pixel data
            for(int row = 0; row < depthFrame.Height; row++)
            {
                for(int col = 0; col < depthFrame.Width; col++)
                {
                    pixelIndex = col + (row * depthFrame.Width);
                    depth = pixelData[pixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    if(depth != 0)
                    {
                        playerIndex = (pixelData[pixelIndex] & DepthImageFrame.PlayerIndexBitmask) - 1;

                        if(playerIndex > -1)
                        {
                            if(players[playerIndex] == null)
                            {
                                players[playerIndex] = new PlayerDepthData(playerIndex + 1, depthFrame.Width, depthFrame.Height);
                            }

                            players[playerIndex].UpdateData(col, row, depth);
                        }
                    }
                }
            }
            
            
            PlayerDepthData.ItemsSource = players;
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
                            this._DepthImage             = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                            this._DepthImageRect         = new Int32Rect(0, 0, (int) Math.Ceiling(this._DepthImage.Width), (int) Math.Ceiling(this._DepthImage.Height));
                            this._DepthImageStride       = depthStream.FrameWidth * 4;
                            this._DepthPixelData         = new short[depthStream.FramePixelDataLength];
                            this.DepthImage.Source       = this._DepthImage;                           

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
