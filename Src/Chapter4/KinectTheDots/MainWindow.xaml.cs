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
using System.Windows.Shapes;

using Microsoft.Kinect;
using Nui=Microsoft.Kinect;


namespace BeginningKinect.Chapter4.KinectTheDots
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectDevice;
        private DotPuzzle _Puzzle;
        private int _PuzzleDotIndex;
        private Skeleton[] _FrameSkeletons;
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            this._Puzzle = new DotPuzzle();
            this._Puzzle.Dots.Add(new Point(200, 300));
            this._Puzzle.Dots.Add(new Point(1600, 300));
            this._Puzzle.Dots.Add(new Point(1650, 400));
            this._Puzzle.Dots.Add(new Point(1600, 500));
            this._Puzzle.Dots.Add(new Point(1000, 500));
            this._Puzzle.Dots.Add(new Point(1000, 600));
            this._Puzzle.Dots.Add(new Point(1200, 700));
            this._Puzzle.Dots.Add(new Point(1150, 800));
            this._Puzzle.Dots.Add(new Point(750, 800));
            this._Puzzle.Dots.Add(new Point(700, 700));
            this._Puzzle.Dots.Add(new Point(900, 600));
            this._Puzzle.Dots.Add(new Point(900, 500));
            this._Puzzle.Dots.Add(new Point(200, 500));
            this._Puzzle.Dots.Add(new Point(150, 400));

            this._PuzzleDotIndex = -1;

            this.Loaded += (s,e) =>
            {
                KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
                this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

                DrawPuzzle(this._Puzzle);
            };
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

        // Listing 4-5
        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {      
            using(SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if(frame != null)
                {
                    frame.CopySkeletonDataTo(this._FrameSkeletons);
                    Skeleton skeleton = GetPrimarySkeleton(this._FrameSkeletons);
                    
                    if(skeleton == null)
                    {
                        HandCursorElement.Visibility = Visibility.Collapsed;
                    }
                    else
                    {                
                        Joint primaryHand = GetPrimaryHand(skeleton);
                        TrackHand(primaryHand);
                        TrackPuzzle(primaryHand.Position);                
                    }
                }                
            }
        }


        // Listing 4-5
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
        
        
        // Listing 4-6                       
        private static Joint GetPrimaryHand(Skeleton skeleton)
        {
            Joint primaryHand = new Joint();

            if(skeleton != null)
            {
                primaryHand     = skeleton.Joints[JointType.HandLeft];
                Joint righHand  = skeleton.Joints[JointType.HandRight];


                if(righHand.TrackingState != JointTrackingState.NotTracked)
                {
                    if(primaryHand.TrackingState == JointTrackingState.NotTracked)
                    {
                        primaryHand = righHand;
                    }
                    else
                    {
                        if(primaryHand.Position.Z > righHand.Position.Z)
                        {
                            primaryHand = righHand;
                        }                    
                    }    
                }
            }

            return primaryHand;
        }

      
        // Listing 4-7
        private void TrackHand(Joint hand)
        {
            if(hand.TrackingState == JointTrackingState.NotTracked)
            {
                HandCursorElement.Visibility = Visibility.Collapsed;
            }
            else
            {
                HandCursorElement.Visibility = Visibility.Visible;

                                
                DepthImagePoint point = this._KinectDevice.MapSkeletonPointToDepth(hand.Position, this._KinectDevice.DepthStream.Format);
                point.X = (int) ((point.X * LayoutRoot.ActualWidth/_KinectDevice.DepthStream.FrameWidth) - (HandCursorElement.ActualWidth / 2.0));
                point.Y = (int)((point.Y * LayoutRoot.ActualHeight / _KinectDevice.DepthStream.FrameHeight) - (HandCursorElement.ActualHeight / 2.0));

                Canvas.SetLeft(HandCursorElement, point.X);
                Canvas.SetTop(HandCursorElement, point.Y);                

                if(hand.JointType == JointType.HandRight)
                {
                    HandCursorScale.ScaleX = 1;                    
                }
                else
                {
                    HandCursorScale.ScaleX = -1;                    
                }
            }
        }

        
        // Listing 4-10 
        private void DrawPuzzle(DotPuzzle puzzle)
        {
            PuzzleBoardElement.Children.Clear();

            if(puzzle != null)
            {
                for(int i = 0; i < puzzle.Dots.Count; i++)
                {
                    Grid dotContainer   = new Grid();                    
                    dotContainer.Width  = 50;
                    dotContainer.Height = 50;
                    dotContainer.Children.Add(new Ellipse { Fill = Brushes.Gray });

                    TextBlock dotLabel              = new TextBlock();                    
                    dotLabel.Text                   = (i + 1).ToString();
                    dotLabel.Foreground             = Brushes.White;
                    dotLabel.FontSize               = 24;
                    dotLabel.HorizontalAlignment    = HorizontalAlignment.Center;
                    dotLabel.VerticalAlignment      = VerticalAlignment.Center;
                    dotContainer.Children.Add(dotLabel);

                    //Position the UI element centered on the dot point
                    Canvas.SetTop(dotContainer, puzzle.Dots[i].Y - (dotContainer.Height / 2) );
                    Canvas.SetLeft(dotContainer, puzzle.Dots[i].X - (dotContainer.Width / 2));
                    PuzzleBoardElement.Children.Add(dotContainer);                    
                }
            }
        }
         

        // Listing 4-11
        private void TrackPuzzle(SkeletonPoint position)
        {                
            if(this._PuzzleDotIndex == this._Puzzle.Dots.Count)
            {
                //Do nothing - Game is over
            }
            else
            {            
                Point dot;
                        
                if(this._PuzzleDotIndex + 1 < this._Puzzle.Dots.Count)
                {
                    dot = this._Puzzle.Dots[this._PuzzleDotIndex + 1];            
                }
                else
                {
                    dot = this._Puzzle.Dots[0];
                }
                
                
                DepthImagePoint point = this._KinectDevice.MapSkeletonPointToDepth(position, _KinectDevice.DepthStream.Format);
                point.X = (int) (point.X * LayoutRoot.ActualWidth/_KinectDevice.DepthStream.FrameWidth);
                point.Y = (int)(point.Y * LayoutRoot.ActualHeight / _KinectDevice.DepthStream.FrameHeight);
                Point handPoint = new Point(point.X, point.Y);  


                Point dotDiff = new Point(dot.X - handPoint.X, dot.Y - handPoint.Y);
                double length = Math.Sqrt(dotDiff.X * dotDiff.X + dotDiff.Y * dotDiff.Y);

                int lastPoint = this.CrayonElement.Points.Count - 1;

                if(length < 25)
                {
                    //Cursor is within the hit zone

                    if(lastPoint > 0)
                    {
                        //Remove the working end point
                        this.CrayonElement.Points.RemoveAt(lastPoint);
                    }

                    //Set line end point
                    this.CrayonElement.Points.Add(new Point(dot.X, dot.Y));

                    //Set new line start point
                    this.CrayonElement.Points.Add(new Point(dot.X, dot.Y));

                    //Move to the next dot 
                    this._PuzzleDotIndex++;

                    if(this._PuzzleDotIndex == this._Puzzle.Dots.Count)
                    {
                        //Notify the user that the game is over
                    }
                }
                else
                {
                    if(lastPoint > 0)
                    {   
                        //To refresh the Polyline visual you must remove the last point, update and add it back                     
                        Point lineEndpoint = this.CrayonElement.Points[lastPoint];
                        this.CrayonElement.Points.RemoveAt(lastPoint);
                        lineEndpoint.X = handPoint.X;
                        lineEndpoint.Y = handPoint.Y;
                        this.CrayonElement.Points.Add(lineEndpoint);
                    }
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
                        this._KinectDevice.SkeletonFrameReady -= KinectDevice_SkeletonFrameReady;
                        this._KinectDevice.SkeletonStream.Disable();
                        SkeletonViewerElement.KinectDevice = null;
                        this._FrameSkeletons = null;
                    }
                   
                    this._KinectDevice = value;

                    //Initialize
                    if(this._KinectDevice != null)
                    {
                        if(this._KinectDevice.Status == KinectStatus.Connected)
                        {
                            this._KinectDevice.SkeletonStream.Enable();
                            this._FrameSkeletons = new Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];                        
                            this._KinectDevice.Start(); 

                            SkeletonViewerElement.KinectDevice = this.KinectDevice;
                            this.KinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;                            
                        }
                    }                
                }
            }
        }        
        #endregion Properties
    }
}

