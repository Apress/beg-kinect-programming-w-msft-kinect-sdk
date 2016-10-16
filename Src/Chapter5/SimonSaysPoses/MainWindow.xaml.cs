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
using System.Windows.Media.Animation;
using System.Windows.Threading;


using Microsoft.Kinect;


namespace BeginningKinect.Chapter5.SimonSaysPoses
{
    public enum GamePhase
    {        
        GameOver            = 0,
        SimonInstructing    = 1,
        PlayerPerforming    = 2        
    }


    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectDevice;
        private Skeleton[] _FrameSkeletons;
        private GamePhase _CurrentPhase;
        private int[] _InstructionSequence;
        private int _InstructionPosition;
        private int _CurrentLevel;
        private Random rnd = new Random();
        private Pose[] _PoseLibrary;
        private Pose _StartPose; 
        private DispatcherTimer _PoseTimer;       
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            this._CurrentLevel = 0;

            this._PoseTimer     = new DispatcherTimer();
            this._PoseTimer.Interval    = TimeSpan.FromSeconds(10);
            this._PoseTimer.Tick += (s, e) => { ChangePhase(GamePhase.GameOver); };
            this._PoseTimer.Stop();

            PopulatePoseLibrary();
            ChangePhase(GamePhase.GameOver);

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
                        ChangePhase(GamePhase.GameOver);              
                    }
                    else
                    {
                        if(this._CurrentPhase == GamePhase.SimonInstructing)
                        {
                            LeftHandElement.Visibility = System.Windows.Visibility.Collapsed;
                            RightHandElement.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            TrackHand(skeleton.Joints[JointType.HandLeft], LeftHandElement, LayoutRoot);
                            TrackHand(skeleton.Joints[JointType.HandRight], RightHandElement, LayoutRoot);

                            switch(this._CurrentPhase)
                            {
                                case GamePhase.GameOver:
                                    ProcessGameOver(skeleton);
                                    break;

                                case GamePhase.PlayerPerforming:
                                    ProcessPlayerPerforming(skeleton);                            
                                    break;
                            }
                        }
                    } 
                }                                          
            }
        }


        private void TrackHand(Joint hand, FrameworkElement cursorElement, FrameworkElement container)
        {
            if(hand.TrackingState == JointTrackingState.NotTracked)
            {
                cursorElement.Visibility = Visibility.Collapsed;
            }
            else
            {
                cursorElement.Visibility = Visibility.Visible;
                Point jointPoint = GetJointPoint(this.KinectDevice, hand, container.RenderSize, new Point(cursorElement.ActualWidth / 2.0, cursorElement.ActualHeight / 2.0));                                
                Canvas.SetLeft(cursorElement, jointPoint.X);
                Canvas.SetTop(cursorElement, jointPoint.Y);                
            }
        }


        private void ProcessGameOver(Skeleton skeleton)
        {
            if(IsPose(skeleton, this._StartPose))
            {
                ChangePhase(GamePhase.SimonInstructing);
            }         
        }


        private static Point GetJointPoint(KinectSensor kinectDevice, Joint joint, Size containerSize, Point offset)
        {
            DepthImagePoint point = kinectDevice.MapSkeletonPointToDepth(joint.Position, kinectDevice.DepthStream.Format);
            point.X = (int) ((point.X * containerSize.Width/kinectDevice.DepthStream.FrameWidth) - offset.X);
            point.Y = (int)((point.Y * containerSize.Height / kinectDevice.DepthStream.FrameHeight) - offset.Y);
                        
            return new Point(point.X, point.Y);
        }


        // Listing 5-15
        private double GetJointAngle(Joint centerJoint, Joint angleJoint)
        {
            Point primaryPoint  = GetJointPoint(this.KinectDevice, centerJoint, this.LayoutRoot.RenderSize, new Point());
            Point anglePoint    = GetJointPoint(this.KinectDevice, angleJoint, this.LayoutRoot.RenderSize, new Point());
            Point x             = new Point(primaryPoint.X + anglePoint.X, primaryPoint.Y);

            double a;
            double b;
            double c;

            a = Math.Sqrt(Math.Pow(primaryPoint.X - anglePoint.X, 2) + Math.Pow(primaryPoint.Y - anglePoint.Y, 2));
            b = anglePoint.X;
            c = Math.Sqrt(Math.Pow(anglePoint.X - x.X, 2) + Math.Pow(anglePoint.Y - x.Y, 2));

            double angleRad = Math.Acos((a * a + b * b - c * c) / (2 * a * b));
            double angleDeg = angleRad * 180 / Math.PI;

            if(primaryPoint.Y < anglePoint.Y)
            {
                angleDeg = 360 - angleDeg;                            
            }

            return angleDeg;
        }


        // Listing 5-13
        private void PopulatePoseLibrary()
        {
            this._PoseLibrary = new Pose[4];


            //Start Pose - Arms Extended
            this._StartPose             = new Pose();
            this._StartPose.Title       = "Start Pose";
            this._StartPose.Angles      = new PoseAngle[4];
            this._StartPose.Angles[0]   = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            this._StartPose.Angles[1]   = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 180, 20);
            this._StartPose.Angles[2]   = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            this._StartPose.Angles[3]   = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 0, 20);             


            //Pose 1 - Both Hands Up
            this._PoseLibrary[0]            = new Pose();
            this._PoseLibrary[0].Title      = "Arms Up";
            this._PoseLibrary[0].Angles     = new PoseAngle[4];
            this._PoseLibrary[0].Angles[0]  = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            this._PoseLibrary[0].Angles[1]  = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 90, 20);
            this._PoseLibrary[0].Angles[2]  = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            this._PoseLibrary[0].Angles[3]  = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 90, 20);


            //Pose 2 - Both Hands Down
            this._PoseLibrary[1]            = new Pose();
            this._PoseLibrary[1].Title      = "Arms Down";
            this._PoseLibrary[1].Angles     = new PoseAngle[4];            
            this._PoseLibrary[1].Angles[0]  = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            this._PoseLibrary[1].Angles[1]  = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 270, 20);            
            this._PoseLibrary[1].Angles[2]  = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            this._PoseLibrary[1].Angles[3]  = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 270, 20);


            //Pose 3 - Left Up and Right Down
            this._PoseLibrary[2]            = new Pose();
            this._PoseLibrary[2].Title      = "Left Up and Right Down";
            this._PoseLibrary[2].Angles     = new PoseAngle[4];
            this._PoseLibrary[2].Angles[0]  = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            this._PoseLibrary[2].Angles[1]  = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 90, 20);
            this._PoseLibrary[2].Angles[2]  = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            this._PoseLibrary[2].Angles[3]  = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 270, 20);


            //Pose 4 - Right Up and Left Down
            this._PoseLibrary[3]            = new Pose();
            this._PoseLibrary[3].Title      = "Right Up and Left Down";
            this._PoseLibrary[3].Angles     = new PoseAngle[4];
            this._PoseLibrary[3].Angles[0]  = new PoseAngle(JointType.ShoulderLeft, JointType.ElbowLeft, 180, 20);
            this._PoseLibrary[3].Angles[1]  = new PoseAngle(JointType.ElbowLeft, JointType.WristLeft, 270, 20);
            this._PoseLibrary[3].Angles[2]  = new PoseAngle(JointType.ShoulderRight, JointType.ElbowRight, 0, 20);
            this._PoseLibrary[3].Angles[3]  = new PoseAngle(JointType.ElbowRight, JointType.WristRight, 90, 20);
        }


        // Listing 5-14
        private bool IsPose(Skeleton skeleton, Pose pose)
        {
            bool isPose = true;
            double angle;
            double poseAngle;
            double poseThreshold;
            double loAngle;
            double hiAngle;

            for(int i = 0; i < pose.Angles.Length && isPose; i++)
            {
                poseAngle       = pose.Angles[i].Angle;
                poseThreshold   = pose.Angles[i].Threshold;
                angle           = GetJointAngle(skeleton.Joints[pose.Angles[i].CenterJoint], skeleton.Joints[pose.Angles[i].AngleJoint]);

                hiAngle = poseAngle + poseThreshold;
                loAngle = poseAngle - poseThreshold;

                if(hiAngle >= 360 || loAngle < 0)
                {
                    loAngle = (loAngle < 0) ? 360 + loAngle : loAngle;
                    hiAngle = hiAngle % 360;

                    isPose = !(loAngle > angle && angle > hiAngle);
                }
                else
                {
                    isPose = (loAngle <= angle && hiAngle >= angle);
                }
            }

            return isPose;
        }


        // Listing 5-17
        private void ProcessPlayerPerforming(Skeleton skeleton)
        {           
            int instructionSeq = this._InstructionSequence[this._InstructionPosition];

            if(IsPose(skeleton, this._PoseLibrary[instructionSeq]))
            {     
                this._PoseTimer.Stop();           
                this._InstructionPosition++;

                if(this._InstructionPosition >= this._InstructionSequence.Length)
                {
                    ChangePhase(GamePhase.SimonInstructing);
                }
                else
                {
                    //TODO: Notify the user of correct pose
                    this._PoseTimer.Start();
                }
            }
        }


        private void ChangePhase(GamePhase newPhase)
        {
            if(newPhase != this._CurrentPhase)
            {
                this._CurrentPhase = newPhase;
                this._PoseTimer.Stop();

                switch(this._CurrentPhase)
                {
                    case GamePhase.GameOver:     
                        this._CurrentLevel              = 0;                                           
                        GameStateElement.Text           = "GAME OVER!";
                        GameInstructionsElement.Text    = "Place hands over the targets to start a new game.";
                        break;                    

                    case GamePhase.SimonInstructing:
                        this._CurrentLevel++;
                        GameStateElement.Text           = string.Format("Level {0}", this._CurrentLevel);
                        GameInstructionsElement.Text    = "Watch for Simon's instructions";
                        GenerateInstructions();
                        DisplayInstructions();                       
                        break;
                        
                    case GamePhase.PlayerPerforming:                       
                        this._PoseTimer.Start();
                        this._InstructionPosition       = 0;                        
                        break;                                                
                }
            }
        }


        private void GenerateInstructions()
        {     
            this._InstructionSequence = new int[this._CurrentLevel];
                        
            for(int i = 0; i < this._CurrentLevel; i++)
            {  
                this._InstructionSequence[i] = rnd.Next(0, this._PoseLibrary.Length - 1);              
            }
        }


        private void DisplayInstructions()
        {
            GameInstructionsElement.Text = string.Empty;
            StringBuilder text = new StringBuilder();
            int instructionsSeq;

            for(int i = 0; i < this._InstructionSequence.Length; i++)
            {
                instructionsSeq = this._InstructionSequence[i];
                text.AppendFormat("{0}, ", this._PoseLibrary[instructionsSeq].Title);
            }

            GameInstructionsElement.Text = text.ToString();
            ChangePhase(GamePhase.PlayerPerforming);
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
