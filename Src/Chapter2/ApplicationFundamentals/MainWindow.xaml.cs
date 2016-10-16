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
using System.IO;

using Microsoft.Kinect;


/*
 * 
 *   This project contains the source code for listings 2-1, 2-2, 2-4, 2-5, 2-6, 2-7, and 2-9.
 *   While the project contains the original code listings (mostly in comments) the project has been refactored.
 * 
 */

namespace BeginningKinect.Chapter2.ApplicationFundamentals
{
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

            this.Loaded   += (s, e) => { DiscoverKinectSensor(); };                        
            this.Unloaded += (s, e) => { this.Kinect = null; };                      
        }
        #endregion Constructor


        #region Methods
        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);                       
        }


        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        { 
            switch(e.Status)
            {
                case KinectStatus.Connected:
                    if(this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                        UpdateDisplayStatus("Sensor connected.");                        
                    }
                    break;

                case KinectStatus.Disconnected:
                    if(this.Kinect == e.Sensor)
                    {                        
                        this.Kinect = null;
                        this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

                        if(this.Kinect == null)
                        {
                            UpdateDisplayStatus("No connected device.");
                        }
                    }
                    break;

                //TODO: Handle all other statuses according to needs
            }
            if(e.Status == KinectStatus.Connected)
            {
                this.Kinect = e.Sensor;
            }           
        }


        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {            
            using(ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if(frame != null)
                {                    
                    frame.CopyPixelDataTo(this._ColorImagePixelData);

                    FrameworkElement element = ImageTreatmentSelector.SelectedItem as FrameworkElement;

                    if(element != null)
                    {
                        string tagValue = (string) element.Tag;

                        switch(tagValue)
                        {
                            case "1":
                                ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, this._ColorImagePixelData, frame.Width * frame.BytesPerPixel);                                    
                                break;

                            case "2": 
                                this.ColorImageElement.Source = this._ColorImageBitmap;                                                               
                                this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, this._ColorImagePixelData, this._ColorImageStride, 0);
                                break;

                            case "3":
                                this.ColorImageElement.Source = this._ColorImageBitmap;
                                ShadePixelDataRed(this._ColorImagePixelData, frame);
                                this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, this._ColorImagePixelData, this._ColorImageStride, 0);
                                break;
                        }
                    }
                }
            }
        }


        /*
         *  Listing 2-4
         *  
         
        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using(ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if(frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixelData, frame.Width * frame.BytesPerPixel);
                }
            }
        }
         
         * 
         */ 

    
        /*
         *  Listing 2-7
         * 
         
        private void Kinect_ColorFrameReady (object sender, ImageFrameReadyEventArgs e)
        {
            using(ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if(frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    
                    for(int i = 0; i < pixelData.Length; i += frame.BytesPerPixel)
                    {
                        pixelData[i] = 0x00; //Blue
                        pixelData[i + 1] = 0x00; //Green
                    }
                    
                    this._ColorImageBitmap.WritePixels(this._ColorImageBitmapRect, pixelData, this._ColorImageStride, 0);
                }
            }
        }
         
         * 
         */


        private void ShadePixelDataRed(byte[] pixelData, ColorImageFrame frame)
        {
            for(int i = 0; i < pixelData.Length; i += frame.BytesPerPixel)
            {
                pixelData[i]        = 0x00;     //Blue 
                pixelData[i + 1]    = 0x00;     //Green
            }
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.Kinect != null)
            {
                this.Kinect.Start();
            }
        }


        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.Kinect != null)
            {
                this.Kinect.Stop();
                this.Kinect.Dispose();
                this._Kinect = null;
            }
        }


        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if(sensor != null)
            {
                ColorImageStream colorStream = sensor.ColorStream;

                colorStream.Enable();

                /* Added in Listing 2-5 */
                this._ColorImageBitmap          = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this._ColorImageBitmapRect      = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                this._ColorImageStride          = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                this.ColorImageElement.Source   = this._ColorImageBitmap;
                this._ColorImagePixelData       = new byte[colorStream.FramePixelDataLength];
                /* Added in Listing 2-5 */

                sensor.ColorFrameReady += Kinect_ColorFrameReady;
                sensor.Start();
            }
        }


        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if(sensor != null)
            {
                sensor.Stop();
                sensor.ColorFrameReady -= Kinect_ColorFrameReady;                    
            }
        }


        // Listing 2-9
        private void TakePictureButton_Click(object sender, RoutedEventArgs e)
        {            
            string fileName = "snapshot.jpg";

            if(File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using(FileStream savedSnapshot = new FileStream(fileName, FileMode.CreateNew))
            {
                BitmapSource image = (BitmapSource) ColorImageElement.Source;

                JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
                jpgEncoder.QualityLevel = 70;
                jpgEncoder.Frames.Add(BitmapFrame.Create(image));
                jpgEncoder.Save(savedSnapshot);

                savedSnapshot.Flush();
                savedSnapshot.Close();
                savedSnapshot.Dispose();
            }
        }


        private void UpdateDisplayStatus(string message)
        {
            StatusElement.Text = message;
        }
        #endregion Methods


        #region Properties
        public KinectSensor Kinect 
        {
            get { return this._Kinect; }
            set
            {
                if(this._Kinect != null)
                {
                    UpdateDisplayStatus("No connected device.");
                    UninitializeKinectSensor(this._Kinect);
                    this._Kinect = null;
                }


                if(value != null && value.Status == KinectStatus.Connected)
                {
                    this._Kinect = value;
                    InitializeKinectSensor(this._Kinect);
                    StatusElement.Text = string.Format("{0} - {1}", this._Kinect.UniqueKinectId, this._Kinect.Status);                   
                }
                else
                {
                    UpdateDisplayStatus("No connected device.");
                }                
            }
        }
        #endregion Properties
    }
}
