using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beginning.Kinect.Framework.Input;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Controls;



namespace Beginning.Kinect.Framework.Controls
{
    public class MagnetButton : HoverButton
    {
        protected bool _isLockOn = true;
        public static readonly RoutedEvent KinectCursorLockEvent = KinectInput.KinectCursorUnlockEvent.AddOwner(typeof(MagnetButton));
        public static readonly RoutedEvent KinectCursorUnlockEvent = KinectInput.KinectCursorLockEvent.AddOwner(typeof(MagnetButton));

        public MagnetButton()
        {

        }

        public event KinectCursorEventHandler KinectCursorLock
        {
            add { base.AddHandler(KinectCursorLockEvent, value); }
            remove { base.RemoveHandler(KinectCursorLockEvent, value); }
        }

        public event KinectCursorEventHandler KinectCursorUnlock
        {
            add { base.AddHandler(KinectCursorUnlockEvent, value); }
            remove { base.RemoveHandler(KinectCursorUnlockEvent, value); }
        }


        /// <summary>
        /// Gets or sets the amount of time it takes the cursor to lock into place.
        /// </summary>
        /// <value>
        /// The lock interval.
        /// </value>
        public double LockInterval
        {
            get { return (double)GetValue(LockIntervalProperty); }
            set { SetValue(LockIntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LockInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LockIntervalProperty =
            DependencyProperty.Register("LockInterval", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(200d));


        public double UnlockInterval
        {
            get { return (double)GetValue(UnlockIntervalProperty); }
            set { SetValue(UnlockIntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnlockInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnlockIntervalProperty =
            DependencyProperty.Register("UnlockInterval", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(80d));

        public double LockXOffsetFromCenter
        {
            get { return (double)GetValue(LockXOffsetFromCenterProperty); }
            set { SetValue(LockXOffsetFromCenterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LockXOffsetFromCenter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LockXOffsetFromCenterProperty =
            DependencyProperty.Register("LockXOffsetFromCenter", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(0d));

        public double LockYOffsetFromCenter
        {
            get { return (double)GetValue(LockYOffsetFromCenterProperty); }
            set { SetValue(LockYOffsetFromCenterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LockXOffsetFromCenter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LockYOffsetFromCenterProperty =
            DependencyProperty.Register("LockYOffsetFromCenter", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(0d));

        Storyboard move;
        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            if (this.Opacity == 0)
                return;
            //Debug.WriteLine("Enter ");
            if (!_isLockOn)
                return;
            // get button position
            var rootVisual = FindAncestor<Window>(this);
            var point = this.TransformToAncestor(rootVisual)
                              .Transform(new Point(0, 0));

            var x = point.X + this.ActualWidth / 2;
            var y = point.Y + this.ActualHeight / 2;

            var cursor = e.Cursor;
            cursor.UpdateCursor(new Point(e.X, e.Y), true);

            // find target position
            Point lockPoint = new Point(x - cursor.CursorVisual.ActualWidth / 2 + LockXOffsetFromCenter, y - cursor.CursorVisual.ActualHeight / 2 + LockYOffsetFromCenter);

            // find current location
            Point cursorPoint = new Point(e.X - cursor.CursorVisual.ActualWidth / 2, e.Y - cursor.CursorVisual.ActualHeight / 2);

            // guide cursor to its final position
            DoubleAnimation moveLeft = new DoubleAnimation(cursorPoint.X, lockPoint.X, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveLeft, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Canvas.LeftProperty));
            DoubleAnimation moveTop = new DoubleAnimation(cursorPoint.Y, lockPoint.Y, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveTop, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveTop, new PropertyPath(Canvas.TopProperty));
            move = new Storyboard();
            move.Children.Add(moveTop);
            move.Children.Add(moveLeft);

            move.Completed += delegate
            {
                this.RaiseEvent(new KinectCursorEventArgs(KinectCursorLockEvent, new Point(x + LockXOffsetFromCenter, y + LockYOffsetFromCenter), e.Z) { Cursor = e.Cursor });
            };
            if (move != null)
                move.Stop(e.Cursor);
            move.Begin(cursor, false);
            base.OnKinectCursorEnter(sender, e);
        }


        protected override void OnKinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            if (this.Opacity == 0)
                return;
            //Debug.WriteLine("Leave ");
            base.OnKinectCursorLeave(sender, e);
            if (!_isLockOn)
                return;

            //if(move != null)
            //    move.Stop(e.Cursor);

            e.Cursor.UpdateCursor(new Point(e.X, e.Y), false);
             //get button position
            var rootVisual = FindAncestor<Window>(this);
            var point = this.TransformToAncestor(rootVisual)
                              .Transform(new Point(0, 0));

            var x = point.X + this.ActualWidth / 2;
            var y = point.Y + this.ActualHeight / 2;

            var cursor = e.Cursor;
            // find target position
            Point lockPoint = new Point(x - cursor.CursorVisual.ActualWidth / 2 + LockXOffsetFromCenter, y - cursor.CursorVisual.ActualHeight / 2 + LockYOffsetFromCenter);

            // find current location
            Point cursorPoint = new Point(e.X - cursor.CursorVisual.ActualWidth / 2, e.Y - cursor.CursorVisual.ActualHeight / 2);

            // guide cursor to its final position
            DoubleAnimation moveLeft = new DoubleAnimation(lockPoint.X, cursorPoint.X, new Duration(TimeSpan.FromMilliseconds(UnlockInterval)));
            Storyboard.SetTarget(moveLeft, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Canvas.LeftProperty));
            DoubleAnimation moveTop = new DoubleAnimation(lockPoint.Y, cursorPoint.Y, new Duration(TimeSpan.FromMilliseconds(UnlockInterval)));
            Storyboard.SetTarget(moveTop, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveTop, new PropertyPath(Canvas.TopProperty));
            move = new Storyboard();
            move.Children.Add(moveTop);
            move.Children.Add(moveLeft);
            move.Completed += delegate { 
                move.Stop(cursor); 
                cursor.UpdateCursor(new Point(e.X, e.Y), false); 
                this.RaiseEvent(new KinectCursorEventArgs(KinectCursorUnlockEvent, new Point(e.X, e.Y), e.Z) { Cursor = e.Cursor }); 
            };
            move.Begin(cursor, true);

        }

        private KinectCursorEventArgs _lastPointDetected;
        protected override void OnKinectCursorMove(object sender, KinectCursorEventArgs e)
        {
            _lastPointDetected = e;
        }

        protected override void OnKinectCursorDeactivated(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("deactivated");
            this.RaiseEvent(new KinectCursorEventArgs(KinectCursorUnlockEvent, new Point(_lastPointDetected.X, _lastPointDetected.Y), _lastPointDetected.Z) { Cursor = _lastPointDetected.Cursor });

        }

        protected override void OnKinectCursorActivated(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Activated");
            this.RaiseEvent(new KinectCursorEventArgs(KinectCursorEnterEvent, new Point(_lastPointDetected.X, _lastPointDetected.Y), _lastPointDetected.Z) { Cursor = _lastPointDetected.Cursor });
        }


        private T FindAncestor<T>(DependencyObject dependencyObject)
            where T : class
        {
            DependencyObject target = dependencyObject;
            do
            {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));
            return target as T;
        }

    }

}
