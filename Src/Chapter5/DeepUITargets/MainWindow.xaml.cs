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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Kinect;



namespace BeginningKinect.Chapter5.DeepUITargets
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private const float FeetPerMeters = 3.2808399f;

        private KinectSensor _KinectDevice;
        private WriteableBitmap _DepthImage;
        private Int32Rect _DepthImageRect;
        private short[] _DepthPixelData;
        private int _DepthImageStride;
        private Skeleton[] _FrameSkeletons;
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


        private void KinectDevice_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using(DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if(depthFrame != null)
                {
                    using(SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                    {
                        if(skeletonFrame != null)
                        {
                            ProcessDepthFrame(depthFrame);
                            ProcessSkeletonFrame(skeletonFrame);
                        }
                    }
                }
            }            
        }  


        private void ProcessDepthFrame(DepthImageFrame depthFrame)
        {
            int depth;         
            int gray;
            int bytesPerPixel       = 4;                                    
            byte[] enhPixelData     = new byte[depthFrame.Width * depthFrame.Height * bytesPerPixel];

            depthFrame.CopyPixelDataTo(this._DepthPixelData);
            
            for(int i = 0, j = 0; i < this._DepthPixelData.Length; i++, j += bytesPerPixel)
            {
                depth = this._DepthPixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                
                if(depth == 0)
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

            this._DepthImage.WritePixels(this._DepthImageRect, enhPixelData, this._DepthImageStride, 0);                            
        }


        private void ProcessSkeletonFrame(SkeletonFrame skeletonFrame)
        {    
            skeletonFrame.CopySkeletonDataTo(this._FrameSkeletons);
            Skeleton skeleton = GetPrimarySkeleton(this._FrameSkeletons);
                               
            if(skeleton != null)
            {
                TrackHand(skeleton.Joints[JointType.HandLeft], LeftHandElement, LeftHandScaleTransform, LayoutRoot, true);
                TrackHand(skeleton.Joints[JointType.HandRight], RightHandElement, RightHandScaleTransform, LayoutRoot, false);
            }            
        }


        // Listing 5-11
        private void TrackHand(Joint hand, FrameworkElement cursorElement, ScaleTransform cursorScale, FrameworkElement container, bool isLeft)
        {
            if(hand.TrackingState != JointTrackingState.NotTracked)
            {
                double z = hand.Position.Z * FeetPerMeters;
                cursorElement.Visibility = Visibility.Visible;                
                Point jointPoint = GetJointPoint(this.KinectDevice, hand, container.RenderSize, new Point(cursorElement.ActualWidth / 2.0, cursorElement.ActualHeight / 2.0));                                
                Canvas.SetLeft(cursorElement, jointPoint.X);
                Canvas.SetTop(cursorElement, jointPoint.Y);
                Canvas.SetZIndex(cursorElement, (int) (1200 - (z * 100)));
                
                cursorScale.ScaleX = 12 / z * ((isLeft) ? -1 : 1);
                cursorScale.ScaleY = 12 / z;

                if(hand.JointType == JointType.HandLeft)
                {
                    DebugLeftHand.Text = string.Format("Left Hand: {0:0.00}", z);
                }
                else
                {
                    DebugRightHand.Text = string.Format("Right Hand: {0:0.00}", z);
                }
            }
            else
            {
                DebugLeftHand.Text = string.Empty;
                DebugRightHand.Text = string.Empty;
            }
        }


        private static Point GetJointPoint(KinectSensor kinectDevice, Joint joint, Size containerSize, Point offset)
        {
            DepthImagePoint point = kinectDevice.MapSkeletonPointToDepth(joint.Position, kinectDevice.DepthStream.Format);

            point.X = (int) ((point.X * containerSize.Width/kinectDevice.DepthStream.FrameWidth) - offset.X + 0.5f);
            point.Y = (int)((point.Y * containerSize.Height/kinectDevice.DepthStream.FrameHeight) - offset.Y + 0.5f);
                        
            return new Point(point.X, point.Y);
        }


        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;

            if(skeletons != null)
            {
                //Find the closest skeleton       
                for(int i = 0; i < skeletons.Length; i++)
                {
                    if(skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if(skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }   
                        else
                        {
                            if(skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }
                }
            }

            return skeleton;
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
                        this._KinectDevice.DepthStream.Disable();
                        this._KinectDevice.SkeletonStream.Disable();
                        this._KinectDevice.AllFramesReady -= KinectDevice_AllFramesReady;
                        
                        this.DepthImage.Source  = null;
                        this._DepthImage        = null;                        
                        this._DepthImageStride  = 0;
                        
                        this._FrameSkeletons    = null;                          
                    }
                   
                    this._KinectDevice = value;

                    //Initialize
                    if(this._KinectDevice != null)
                    {
                        if(this._KinectDevice.Status == KinectStatus.Connected)
                        {                            
                            this.KinectDevice.AllFramesReady += KinectDevice_AllFramesReady;
                            this._KinectDevice.SkeletonStream.Enable();                        
                            this._KinectDevice.DepthStream.Enable();

                            DepthImageStream depthStream    = this._KinectDevice.DepthStream;
                            this._DepthImage                = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                            this._DepthImageRect            = new Int32Rect(0, 0, (int) Math.Ceiling(this._DepthImage.Width), (int) Math.Ceiling(this._DepthImage.Height));
                            this._DepthImageStride          = depthStream.FrameWidth * 4;
                            this._DepthPixelData            = new short[depthStream.FramePixelDataLength];
                            this.DepthImage.Source          = this._DepthImage;

                            this._FrameSkeletons            = new Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];

                            this._KinectDevice.Start();                             
                        }
                    }                
                }
            }
        }      
        #endregion Properties
    }
}

