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


namespace BeginningKinect.Chapter2.Polling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        private byte[] _ColorImagePixelData;
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
        #endregion Constructor


        #region Methods
        // Listing 2-11
        private void DiscoverKinectSensor()
        {
            if(this._Kinect != null && this._Kinect.Status != KinectStatus.Connected)
            {
                this._Kinect = null;
            }


            if(this._Kinect == null)
            {
                this._Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

                if(this._Kinect != null)
                {
                    this._Kinect.ColorStream.Enable();
                    this._Kinect.Start();

                    ColorImageStream colorStream    = this._Kinect.ColorStream;
                    this._ColorImageBitmap          = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                    this._ColorImageBitmapRect      = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                    this._ColorImageStride          = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                    this.ColorImageElement.Source   = this._ColorImageBitmap;
                    this._ColorImagePixelData       = new byte[colorStream.FramePixelDataLength];
                }
            }
        }


        // Listing 2-12
        private void PollColorImageStream()
        {
            if(this._Kinect == null)
            {
                //TODO: Display a message to plug-in a Kinect.
            }
            else
            {
                try
                {
                    using(ColorImageFrame frame = this._Kinect.ColorStream.OpenNextFrame(100))
                    {
                        if(frame != null)
                        {                            
                            frame.CopyPixelDataTo(this._ColorImagePixelData);
                            this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, this._ColorImagePixelData, this._ColorImageStride, 0);                    
                        }
                    }
                }
                catch(Exception ex)
                {
                    //TODO: Report an error message
                }   
            }
        }


        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            DiscoverKinectSensor();            
            PollColorImageStream();            
        }
        #endregion Methods
    }
}
