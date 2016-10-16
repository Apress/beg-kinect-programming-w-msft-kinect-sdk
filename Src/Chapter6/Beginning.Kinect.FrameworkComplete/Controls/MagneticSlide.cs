using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Beginning.Kinect.Framework.Input;
using System.Diagnostics;

namespace Beginning.Kinect.Framework.Controls
{
    public class MagneticSlide: MagnetButton
    {

        public MagneticSlide()
        {
            base._isLockOn = IsMagnetOn;
            base._timerEnabled = false;

        }

        private bool _isLookingForSwipes;

        private void InitializeSwipe()
        {
            if (_isLookingForSwipes)
                return;
            //Debug.WriteLine("initialize swipe");
            _isLookingForSwipes = true;
            var kinectMgr = KinectCursorManager.Instance;
            kinectMgr.GesturePointTrackingInitialize(SwipeLength, MaxDeviation, MaxSwipeTime, XOutOfBoundsLength);
            kinectMgr.SwipeDetected += kinectMgr_SwipeDetected;
            kinectMgr.SwipeOutOfBoundsDetected += kinectMgr_SwipeOutOfBoundsDetected;
            KinectCursorManager.Instance.GesturePointTrackingStart();
        }

        private void DeInitializeSwipe()
        {
            //Debug.WriteLine("deinitialize swipe");
            _isLookingForSwipes = false;
            var kinectMgr = KinectCursorManager.Instance;
            kinectMgr.GesturePointTrackingStop();
            kinectMgr.SwipeDetected -= kinectMgr_SwipeDetected;
            kinectMgr.SwipeOutOfBoundsDetected -= kinectMgr_SwipeOutOfBoundsDetected;

        }


        public static readonly RoutedEvent SwipeOutOfBoundsEvent = EventManager.RegisterRoutedEvent("SwipeOutOfBounds", RoutingStrategy.Bubble,
typeof(KinectCursorEventHandler), typeof(KinectInput));

        public event RoutedEventHandler SwipeOutOfBounds
        {
            add { AddHandler(SwipeOutOfBoundsEvent, value); }
            remove { RemoveHandler(SwipeOutOfBoundsEvent, value); }
        }

        void kinectMgr_SwipeOutOfBoundsDetected(object sender, Input.KinectCursorEventArgs e)
        {
            DeInitializeSwipe();
            RaiseEvent(new KinectCursorEventArgs(SwipeOutOfBoundsEvent));
        }

        void kinectMgr_SwipeDetected(object sender, Input.KinectCursorEventArgs e)
        {
            //if (!_isLookingForSwipes)
            //    return;
            //Debug.WriteLine("swipe detected in magnetic slide control");
            DeInitializeSwipe();
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        protected override void OnKinectCursorEnter(object sender, Input.KinectCursorEventArgs e)
        {
            InitializeSwipe();
            base.OnKinectCursorEnter(sender, e);
        }


        // Using a DependencyProperty as the backing store for IsMagnetOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMagnetOnProperty =
            DependencyProperty.Register("IsMagnetOn", typeof(Boolean), typeof(MagneticSlide), new UIPropertyMetadata(true));

        public bool IsMagnetOn
        {
            get { return (bool)GetValue(IsMagnetOnProperty); }
            set { SetValue(IsMagnetOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMagnetOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SwipeLengthProperty =
            DependencyProperty.Register("SwipeLength", typeof(double), typeof(MagneticSlide), new UIPropertyMetadata(-500d));

        public double SwipeLength
        {
            get { return (double)GetValue(SwipeLengthProperty); }
            set { SetValue(SwipeLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxYDeviation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxDeviationProperty =
            DependencyProperty.Register("MaxDeviation", typeof(double), typeof(MagneticSlide), new UIPropertyMetadata(100d));

        public double MaxDeviation
        {
            get { return (double)GetValue(MaxDeviationProperty); }
            set { SetValue(MaxDeviationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for XOutOfBound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XOutOfBoundsLengthProperty =
            DependencyProperty.Register("XOutOfBoundsLength", typeof(double), typeof(MagneticSlide), new UIPropertyMetadata(-700d));

        public double XOutOfBoundsLength
        {
            get { return (double)GetValue(XOutOfBoundsLengthProperty); }
            set { SetValue(XOutOfBoundsLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxSwipeTime in milliseconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxSwipeTimeProperty =
            DependencyProperty.Register("MaxSwipeTime", typeof(int), typeof(MagneticSlide), new UIPropertyMetadata(300));

        public int MaxSwipeTime
        {
            get { return (int)GetValue(MaxSwipeTimeProperty); }
            set { SetValue(MaxSwipeTimeProperty, value); }
        }
    }


}
