﻿/*
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


using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Microsoft.Kinect;


namespace BeginningKinect.Chapter5.SimonSays    
{
    /// <summary>
    /// Interaction logic for SkeletonViewer.xaml
    /// </summary>
    public partial class SkeletonViewer : UserControl
    { 
        #region Member Variables
        private readonly Brush[] _SkeletonBrushes = new Brush[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };
        private Skeleton[] _FrameSkeletons;
        #endregion Member Variables


        #region Constructor
        public SkeletonViewer()
        {
            InitializeComponent();
        }
        #endregion Constructor

        
        #region Methods
        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonsPanel.Children.Clear();            

            using(SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if(frame != null)
                {
                    if(this.IsEnabled)
                    {
                        frame.CopySkeletonDataTo(this._FrameSkeletons);                

                        for(int i = 0; i < this._FrameSkeletons.Length; i++)
                        {                        
                            DrawSkeleton(this._FrameSkeletons[i], this._SkeletonBrushes[i]);
                        }
                    }
                }                
            }
        }


        private void DrawSkeleton(Skeleton skeleton, Brush brush)
        {
            if(skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {            
                //Draw head and torso
                Polyline figure = CreateFigure(skeleton, brush, new [] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                             JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter,
                                                                             JointType.HipLeft, JointType.Spine, JointType.HipRight, JointType.HipCenter});              
                SkeletonsPanel.Children.Add(figure);

                //Draw left leg
                figure = CreateFigure(skeleton, brush, new [] { JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                SkeletonsPanel.Children.Add(figure);

                //Draw right leg
                figure = CreateFigure(skeleton, brush, new [] { JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });                
                SkeletonsPanel.Children.Add(figure);

                //Draw left arm
                figure = CreateFigure(skeleton, brush, new [] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                SkeletonsPanel.Children.Add(figure);

                //Draw right arm
                figure = CreateFigure(skeleton, brush, new [] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                SkeletonsPanel.Children.Add(figure);        
            }
        }


        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness  = 18;
            figure.Stroke           = brush;

            for(int i = 0; i < joints.Length; i++)
            {                
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }

                                                                                  
        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(joint.Position, KinectDevice.DepthStream.Format);
            point.X *= (int) this.LayoutRoot.ActualWidth/KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;
                        
            return new Point(point.X, point.Y);
        }                                      
        #endregion Methods
          

        #region Properties
        #region KinectDevice
        protected const string KinectDevicePropertyName = "KinectDevice";
        public static readonly DependencyProperty KinectDeviceProperty = DependencyProperty.Register(KinectDevicePropertyName, typeof(KinectSensor), typeof(SkeletonViewer), new PropertyMetadata(null, KinectDeviceChanged));


        private static void KinectDeviceChanged(DependencyObject owner, DependencyPropertyChangedEventArgs e)
        {
            SkeletonViewer viewer = (SkeletonViewer) owner;

            if(e.OldValue != null)
            {
                ((KinectSensor) e.OldValue).SkeletonFrameReady -= viewer.KinectDevice_SkeletonFrameReady;
                viewer._FrameSkeletons = null;
            }
            
            if(e.NewValue != null)
            {
                viewer.KinectDevice = (KinectSensor) e.NewValue;
                viewer.KinectDevice.SkeletonFrameReady += viewer.KinectDevice_SkeletonFrameReady;
                viewer._FrameSkeletons = new Skeleton[viewer.KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
            }    
        }


        public KinectSensor KinectDevice
        {
            get { return (KinectSensor)GetValue(KinectDeviceProperty); }
            set { SetValue(KinectDeviceProperty, value); }
        }
        #endregion KinectDevice
        #endregion Properties   
    }
}
