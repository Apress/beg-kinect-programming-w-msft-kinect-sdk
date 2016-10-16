using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Kinect;


namespace WaveDetection
{
    public class WaveGesture
    {
        #region Member Variables
        private const float WAVE_THRESHOLD          = 0.1f;
        private const int WAVE_MOVEMENT_TIMEOUT     = 5000;
        private const int LEFT_HAND                 = 0;
        private const int RIGHT_HAND                = 1;
        private const int REQUIRED_ITERATIONS       = 4;


        private WaveGestureTracker[,] _PlayerWaveTracker = new WaveGestureTracker[6,2];

        public event EventHandler GestureDetected;
        #endregion Member Variables


        #region Methods
        public void Update(Skeleton[] skeletons, long frameTimestamp)
        {
            if(skeletons != null)
            {
                Skeleton skeleton;

                for(int i = 0; i < skeletons.Length; i++)
                {
                    skeleton = skeletons[i];
                
                    if(skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                    {
                        TrackWave(skeleton, true, ref this._PlayerWaveTracker[i, LEFT_HAND], frameTimestamp);
                        TrackWave(skeleton, false, ref this._PlayerWaveTracker[i, RIGHT_HAND], frameTimestamp);
                    }
                    else
                    {
                        this._PlayerWaveTracker[i, LEFT_HAND].Reset();
                        this._PlayerWaveTracker[i, RIGHT_HAND].Reset();                    
                    }
                }
            }
        }        


        private void TrackWave(Skeleton skeleton, bool isLeft, ref WaveGestureTracker tracker, long timestamp)
        {
            JointType handJointId       = (isLeft) ? JointType.HandLeft : JointType.HandRight;
            JointType elbowJointId      = (isLeft) ? JointType.ElbowLeft : JointType.ElbowRight;
            Joint hand                  = skeleton.Joints[handJointId];
            Joint elbow                 = skeleton.Joints[elbowJointId];            


            if(hand.TrackingState != JointTrackingState.NotTracked && elbow.TrackingState != JointTrackingState.NotTracked)
            {
                if(tracker.State == WaveGestureState.InProgress && tracker.Timestamp + WAVE_MOVEMENT_TIMEOUT < timestamp)
                {
                    tracker.UpdateState(WaveGestureState.Failure, timestamp);
                    System.Diagnostics.Debug.WriteLine("Fail!");
                }                
                else if(hand.Position.Y > elbow.Position.Y)
                {
                    //Using the raw values where (0, 0) is the middle of the screen.  From the user's perspective, the X-Axis grows more negative left and more positive right.
                    if(hand.Position.X <= elbow.Position.X - WAVE_THRESHOLD)
                    {
                        tracker.UpdatePosition(WavePosition.Left, timestamp);
                    } 
                    else if(hand.Position.X >= elbow.Position.X + WAVE_THRESHOLD)
                    {
                        tracker.UpdatePosition(WavePosition.Right, timestamp);
                    }
                    else
                    {
                        tracker.UpdatePosition(WavePosition.Neutral, timestamp);
                    }


                    if(tracker.State != WaveGestureState.Success && tracker.IterationCount == REQUIRED_ITERATIONS)
                    {
                        tracker.UpdateState(WaveGestureState.Success, timestamp);
                        System.Diagnostics.Debug.WriteLine("Success!");

                        if(GestureDetected != null)
                        {
                            GestureDetected(this, new EventArgs());
                        }                        
                    }
                }
                else
                {
                    if(tracker.State == WaveGestureState.InProgress)
                    {
                        tracker.UpdateState(WaveGestureState.Failure, timestamp);
                        System.Diagnostics.Debug.WriteLine("Fail!");
                    }
                    else
                    {
                        tracker.Reset();
                    }
                }
            }
            else
            {
                tracker.Reset();
            }
        }
        #endregion Methods


        #region Helper Objects
        private enum WavePosition
        {
            None    = 0,
            Left    = 1,
            Right   = 2,
            Neutral = 3
        }


        private enum WaveGestureState
        {
            None        = 0,
            Success     = 1,
            Failure     = 2,
            InProgress  = 3
        }


        private struct WaveGestureTracker
        {        
            public int IterationCount;
            public WaveGestureState State;
            public long Timestamp;
            public WavePosition StartPosition;
            public WavePosition CurrentPosition;


            public void UpdateState(WaveGestureState state, long timestamp)
            {
                State       = state;
                Timestamp   = timestamp;
            }


            public void Reset()
            {
                IterationCount      = 0;
                State               = WaveGestureState.None;
                Timestamp           = 0;
                StartPosition       = WavePosition.None;
                CurrentPosition     = WavePosition.None;                
            }


            public void UpdatePosition(WavePosition position, long timestamp)
            {
                if(CurrentPosition != position)
                {
                    if(position == WavePosition.Left || position == WavePosition.Right)
                    {
                        if(State != WaveGestureState.InProgress)
                        {
                            State           = WaveGestureState.InProgress;
                            IterationCount  = 0;
                            StartPosition   = position;
                        }

                        IterationCount++;      
                    }

                    CurrentPosition = position;
                    Timestamp       = timestamp;
                }
            }
        }
        #endregion Helper Objects
    }    
}

