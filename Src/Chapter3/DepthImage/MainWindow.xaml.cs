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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Kinect;

/*
 * 
 *  This project covers code listings 3-1, 3-2, 3-4, 3-5, 3-6, 3-7, 3-8, & 3-9.
 * 
 */
namespace BeginningKinect.Chapter3.DepthImage
{
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectDevice;
        private WriteableBitmap _RawDepthImage;
        private Int32Rect _RawDepthImageRect;
        private int _RawDepthImageStride;
        private short[] _DepthImagePixelData;
        private int _TotalFrames;
        private DateTime _StartFrameTime;
        private DepthImageFrame _LastDepthFrame;
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                        
            DepthImage.MouseLeftButtonUp += DepthImage_MouseLeftButtonUp; 
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


        /*
         *   Listing 3-2
         
        private void KinectDevice_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {                      
            using(DepthImageFrame frame = e.OpenDepthImageFrame())
            {  
                if(frame != null)
                {
                    short[] pixelData = new short[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    int stride = frame.Width * frame.BytesPerPixel;
                    DepthImage.Source = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Gray16, null, pixelData, stride);
                }
            }             
        }
          
         * 
         */


        private void KinectDevice_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {             
            if(this._LastDepthFrame != null)
            {
                this._LastDepthFrame.Dispose();
                this._LastDepthFrame = null;
            }

            this._LastDepthFrame = e.OpenDepthImageFrame();

            if(this._LastDepthFrame != null)
            {
                this._LastDepthFrame.CopyPixelDataTo(this._DepthImagePixelData);


                FrameworkElement element = ImageTreatmentSelector.SelectedItem as FrameworkElement;

                if(element != null)
                {
                    string tagValue = (string) element.Tag;

                    switch(tagValue)
                    {
                        case "1":
                            CreateLighterShadesOfGray(this._LastDepthFrame, this._DepthImagePixelData);            
                            break;

                        case "2": 
                            CreateBetterShadesOfGray(this._LastDepthFrame, this._DepthImagePixelData);            
                            break;

                        case "3":
                            CreateColorDepthImage(this._LastDepthFrame, this._DepthImagePixelData);            
                            break;
                    }
                }

                                                
                this._RawDepthImage.WritePixels(this._RawDepthImageRect, this._DepthImagePixelData, this._RawDepthImageStride, 0);
            }
            

            FramesPerSecondElement.Text = string.Format("{0:0} fps", (this._TotalFrames++ / DateTime.Now.Subtract(this._StartFrameTime).TotalSeconds));
        }        

      
        // Listing 3-5
        private void DepthImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p             = e.GetPosition(DepthImage);

            if(this._DepthImagePixelData != null && this._DepthImagePixelData.Length > 0)
            {                
                int width           = this._LastDepthFrame.Width;
                int pixelIndex      = (int) (p.X + ((int) p.Y * width)); 
                int depth           = this._DepthImagePixelData[pixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                int depthInches     = (int) (depth * 0.0393700787);
                int depthFt         = depthInches / 12;
                depthInches         = depthInches % 12;

                PixelDepth.Text = string.Format("{0}mm ~ {1}'{2}\"", depth, depthFt, depthInches);
            }
        }

        
        // Listing 3-7  
        private void CreateLighterShadesOfGray(DepthImageFrame depthFrame, short[] pixelData)
        {   
            int depth;         
            int loThreshold         = 1220;
            int hiThreshold         = 3048;            
            short[] enhPixelData    = new short[depthFrame.Width * depthFrame.Height]; 

            for(int i = 0; i < pixelData.Length; i++)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if(depth < loThreshold || depth > hiThreshold)
                {
                    enhPixelData[i] = 0xFF;
                }
                else
                {
                    enhPixelData[i] = (short) ~pixelData[i];
                }
            }

            EnhancedDepthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Gray16, null, enhPixelData, depthFrame.Width * depthFrame.BytesPerPixel);
        }

      
        // Listing 3-8
        private void CreateBetterShadesOfGray(DepthImageFrame depthFrame, short[] pixelData)
        {  
            int depth;         
            int gray;
            int loThreshold         = 1220;
            int hiThreshold         = 3048;
            int bytesPerPixel       = 4;
            byte[] enhPixelData     = new byte[depthFrame.Width * depthFrame.Height * bytesPerPixel];
            
            for(int i = 0, j = 0; i < pixelData.Length; i++, j += bytesPerPixel)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if(depth < loThreshold || depth > hiThreshold)
                {
                    gray = 0xFF;
                }
                else
                {    
                    gray = (255 * depth / 0xFFF);
                }

                enhPixelData[j]        = (byte) gray;
                enhPixelData[j + 1]    = (byte) gray;
                enhPixelData[j + 2]    = (byte) gray;                
            }

            EnhancedDepthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytesPerPixel);                           
        }

        
        // Listing 3-9 
        private void CreateColorDepthImage(DepthImageFrame depthFrame, short[] pixelData)
        {
            int depth;              
            double hue;       
            int loThreshold     = 1220;
            int hiThreshold     = 3048;
            int bytesPerPixel   = 4;
            byte[] rgb          = new byte[3];            
            byte[] enhPixelData = new byte[depthFrame.Width * depthFrame.Height * bytesPerPixel];
            
            for(int i = 0, j = 0; i < pixelData.Length; i++, j += bytesPerPixel)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if(depth < loThreshold || depth > hiThreshold)
                {
                    enhPixelData[j]      = 0x00;
                    enhPixelData[j + 1]  = 0x00;                    
                    enhPixelData[j + 2]  = 0x00;
                }
                else
                {    
                    hue = ((360 * depth / 0xFFF) + loThreshold);                    
                    ConvertHslToRgb(hue, 100, 100, rgb);

                    enhPixelData[j]     = rgb[2];  //Blue
                    enhPixelData[j + 1] = rgb[1];  //Green
                    enhPixelData[j + 2] = rgb[0];  //Red
                }            
            }

            EnhancedDepthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, enhPixelData, depthFrame.Width * bytesPerPixel);            
        }

        
        public void ConvertHslToRgb(double hue, double saturation, double lightness, byte[] rgb)
        {
            double red      = 0.0;
            double green    = 0.0;
            double blue     = 0.0;
            hue             = hue % 360.0;
            saturation      = saturation / 100.0;
            lightness       = lightness / 100.0;

            if(saturation == 0.0)
            {
                red     = lightness;
                green   = lightness;
                blue    = lightness;
            }
            else
            {               
                double huePrime = hue / 60.0;
                int x           = (int) huePrime;
                double xPrime   = huePrime - (double) x;
                double L0     = lightness * (1.0 - saturation);
                double L1     = lightness * (1.0 - (saturation * xPrime));                
                double L2     = lightness * (1.0 - (saturation * (1.0 - xPrime)));

                switch (x)
                {
                    case 0:
                        red     = lightness;
                        green   = L2;
                        blue    = L0;
                        break;
                    case 1:
                        red     = L1;
                        green   = lightness;
                        blue    = L0;
                        break;
                    case 2:
                        red     = L0;
                        green   = lightness;
                        blue    = L2;
                        break;
                    case 3:
                        red     = L0;
                        green   = L1;
                        blue    = lightness;
                        break;
                    case 4:
                        red     = L2;
                        green   = L0;
                        blue    = lightness;
                        break;
                    case 5:
                        red     = lightness;
                        green   = L0;
                        blue    = L1;
                        break;
                }
            }

            rgb[0] = (byte)(255.0 * red);
            rgb[1] = (byte)(255.0 * green);
            rgb[2] = (byte)(255.0 * blue);
        }
        
        
        private void InitializeRawDepthImage(DepthImageStream depthStream)
        {
            if(depthStream == null)
            {
                this._RawDepthImage         = null;
                this._RawDepthImageRect     = new Int32Rect();
                this._RawDepthImageStride   = 0;
                this._DepthImagePixelData   = null;
            }
            else
            {
                this._RawDepthImage         = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                this._RawDepthImageRect     = new Int32Rect(0, 0, depthStream.FrameWidth, depthStream.FrameHeight);
                this._RawDepthImageStride   = depthStream.FrameBytesPerPixel * depthStream.FrameWidth;
                this._DepthImagePixelData   = new short[depthStream.FramePixelDataLength];
            }

            this.DepthImage.Source = this._RawDepthImage;
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
                        InitializeRawDepthImage(null);

                        this.DepthImage.Source = null;
                        this._RawDepthImage = null;
                        this.EnhancedDepthImage.Source = null;
                    }
                   
                    this._KinectDevice = value;

                    //Initialize
                    if(this._KinectDevice != null)
                    {
                        if(this._KinectDevice.Status == KinectStatus.Connected)
                        {                            
                            this._KinectDevice.DepthStream.Enable();                                                                                  
                            InitializeRawDepthImage(this._KinectDevice.DepthStream);                                                          
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
