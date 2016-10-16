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
using System.Windows.Media.Animation;


using Microsoft.Kinect;


namespace BeginningKinect.Chapter5.SimonSays
{
    // Listing 5-3
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
        private UIElement[] _InstructionSequence;
        private int _InstructionPosition;
        private int _CurrentLevel;
        private Random rnd = new Random();
        private IInputElement _LeftHandTarget;
        private IInputElement _RightHandTarget;
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

            this._CurrentLevel = 0;
            ChangePhase(GamePhase.GameOver);
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
        
        
        // Listing 5-2 & 5-4
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
                            LeftHandElement.Visibility  = Visibility.Collapsed;
                            RightHandElement.Visibility = Visibility.Collapsed;
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


        // Listing 5-2
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
        
        
        private static Point GetJointPoint(KinectSensor kinectDevice, Joint joint, Size containerSize, Point offset)
        {
            DepthImagePoint point = kinectDevice.MapSkeletonPointToDepth(joint.Position, kinectDevice.DepthStream.Format);
            point.X = (int) ((point.X * containerSize.Width/kinectDevice.DepthStream.FrameWidth) - offset.X);
            point.Y = (int)((point.Y * containerSize.Height / kinectDevice.DepthStream.FrameHeight) - offset.Y);
                        
            return new Point(point.X, point.Y);
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
        
        
        // Listing 5-5
        private void ProcessGameOver(Skeleton skeleton)
        {
            //Determine if the user triggers to start of a new game
            if(HitTest(skeleton.Joints[JointType.HandLeft], LeftHandStartElement) && HitTest(skeleton.Joints[JointType.HandRight], RightHandStartElement))
            {
                ChangePhase(GamePhase.SimonInstructing);
            }
        }

       
        private bool HitTest(Joint joint, UIElement target)
        {
            return (GetHitTarget(joint, target) != null);
        }


        // Listing 5-5
        private IInputElement GetHitTarget(Joint joint, UIElement target)
        {
            Point targetPoint = LayoutRoot.TranslatePoint(GetJointPoint(this.KinectDevice, joint, LayoutRoot.RenderSize, new Point()), target);
            return target.InputHitTest(targetPoint);
        }                                
        
        
        // Listing 5-6
        private void ChangePhase(GamePhase newPhase)
        {
            if(newPhase != this._CurrentPhase)
            {
                this._CurrentPhase = newPhase;

                switch(this._CurrentPhase)
                {
                    case GamePhase.GameOver:     
                        this._CurrentLevel          = 0;                                           
                        RedBlock.Opacity            = 0.2;            
                        BlueBlock.Opacity           = 0.2;
                        GreenBlock.Opacity          = 0.2;
                        YellowBlock.Opacity         = 0.2;

                        GameStateElement.Text           = "GAME OVER!";
                        ControlCanvas.Visibility        = Visibility.Visible;
                        GameInstructionsElement.Text    = "Place hands over the targets to start a new game.";
                        break;                    

                    case GamePhase.SimonInstructing:
                        this._CurrentLevel++;
                        GameStateElement.Text           = string.Format("Level {0}", this._CurrentLevel);
                        ControlCanvas.Visibility        = Visibility.Collapsed;
                        GameInstructionsElement.Text    = "Watch for Simon's instructions";
                        GenerateInstructions();
                        DisplayInstructions();
                        break;
                        
                    case GamePhase.PlayerPerforming:
                        this._InstructionPosition       = 0;
                        GameInstructionsElement.Text    = "Repeat Simon's instructions";
                        break;                                                
                }
            }
        }
        
        
        // Listing 5-7
        private void GenerateInstructions()
        {            
            this._InstructionSequence = new UIElement[this._CurrentLevel];
                        
            for(int i = 0; i < this._CurrentLevel; i++)
            {
                switch(rnd.Next(1, 4))
                {
                    case 1:
                        this._InstructionSequence[i] = RedBlock;
                        break;

                    case 2:
                        this._InstructionSequence[i] = BlueBlock;
                        break;

                    case 3:
                        this._InstructionSequence[i] = GreenBlock;
                        break;

                    case 4:
                        this._InstructionSequence[i] = YellowBlock;
                        break;
                }
            }          
        }


        // Listing 5-7
        private void DisplayInstructions()
        {
            Storyboard instructionsSequence = new Storyboard();
            DoubleAnimationUsingKeyFrames animation;
            
            for(int i = 0; i < this._InstructionSequence.Length; i++)
            {
                this._InstructionSequence[i].ApplyAnimationClock(FrameworkElement.OpacityProperty, null);

                animation = new DoubleAnimationUsingKeyFrames();
                animation.FillBehavior = FillBehavior.Stop;
                animation.BeginTime = TimeSpan.FromMilliseconds(i * 1500);
                Storyboard.SetTarget(animation, this._InstructionSequence[i]);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                instructionsSequence.Children.Add(animation);

                animation.KeyFrames.Add(new EasingDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))));
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1300))));
            }


            instructionsSequence.Completed += (s, e) => { ChangePhase(GamePhase.PlayerPerforming); };
            instructionsSequence.Begin(LayoutRoot);            
        }                                                   


        /*
        //As shown in listing 5-8 with UI bug
        private void ProcessPlayerPerforming(Skeleton skeleton)
        {
            //Determine if user is hitting a target and if that target is in the correct sequence.
            UIElement correctTarget     = this._InstructionSequence[this._InstructionPosition];
            IInputElement leftTarget    = GetHitTarget(skeleton.Joints[JointType.HandLeft], GameCanvas);
            IInputElement rightTarget   = GetHitTarget(skeleton.Joints[JointType.HandRight], GameCanvas);


            if(leftTarget != null && rightTarget != null)
            {
                ChangePhase(GamePhase.GameOver);
            }
            else if(leftTarget == null && rightTarget == null)
            {
                //Do nothing - target found
            }
            else if((leftTarget == correctTarget && rightTarget == null) ||
                    (rightTarget == correctTarget && leftTarget == null))
            {                
                this._InstructionPosition++;

                if(this._InstructionPosition >= this._InstructionSequence.Length)
                {
                    ChangePhase(GamePhase.SimonInstructing);
                }
            }
            else
            {                    
                ChangePhase(GamePhase.GameOver);
            }
        }
        */


        //As shown in listing 5-9 that fixes the UI bug
        private void ProcessPlayerPerforming(Skeleton skeleton)
        {
            //Determine if user is hitting a target and if that target is in the correct sequence.
            UIElement correctTarget     = this._InstructionSequence[this._InstructionPosition];
            IInputElement leftTarget    = GetHitTarget(skeleton.Joints[JointType.HandLeft], GameCanvas);
            IInputElement rightTarget   = GetHitTarget(skeleton.Joints[JointType.HandRight], GameCanvas);
            bool hasTargetChange        = (leftTarget != this._LeftHandTarget) || (rightTarget != this._RightHandTarget);
            

            if(hasTargetChange)
            {
                if(leftTarget != null && rightTarget != null)
                {
                    ChangePhase(GamePhase.GameOver);
                }
                else if((_LeftHandTarget == correctTarget && _RightHandTarget == null) ||
                        (_RightHandTarget == correctTarget && _LeftHandTarget == null))
                {                
                    this._InstructionPosition++;

                    if(this._InstructionPosition >= this._InstructionSequence.Length)
                    {
                        ChangePhase(GamePhase.SimonInstructing);
                    }
                }
                else if(leftTarget != null || rightTarget != null)
                {
                    //Do nothing - target found
                }
                else
                {                    
                    ChangePhase(GamePhase.GameOver);
                }


                if(leftTarget != this._LeftHandTarget)
                {
                    if(this._LeftHandTarget != null)
                    {
                        ((FrameworkElement) this._LeftHandTarget).Opacity = 0.2;
                    }
                                        
                    if(leftTarget != null)
                    {
                        ((FrameworkElement) leftTarget).Opacity = 1;                    
                    }

                    this._LeftHandTarget = leftTarget;                    
                }


                if(rightTarget != this._RightHandTarget)
                {
                    if(this._RightHandTarget != null)
                    {
                        ((FrameworkElement) this._RightHandTarget).Opacity = 0.2;
                    }
                                        
                    if(rightTarget != null)
                    {
                        ((FrameworkElement) rightTarget).Opacity = 1;                    
                    }

                    this._RightHandTarget = rightTarget;
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
