
using System.Windows;

namespace Beginning.Kinect.Framework.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Controls;
    using Beginning.Kinect.Framework;
    using System.Windows.Threading;
    using Beginning.Kinect.Framework.Input;
    using System.Diagnostics;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HoverButton : KinectButton
    {

        readonly DispatcherTimer _hoverTimer = new DispatcherTimer();
        protected bool _timerEnabled = true;

        /// <summary>
        /// Gets or sets the amount of time required for a hover to trigger the click event.
        /// </summary>
        /// <value>
        /// The hover interval.
        /// </value>
        public double HoverInterval
        {
            get { return (double)GetValue(HoverIntervalProperty); }
            set { SetValue(HoverIntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverIntervalProperty =
            DependencyProperty.Register("HoverInterval", typeof(double), typeof(HoverButton), new UIPropertyMetadata(2000d));


        public HoverButton()
        {

            _hoverTimer.Interval = TimeSpan.FromMilliseconds(HoverInterval);
            _hoverTimer.Tick += _hoverTimer_Tick;
            _hoverTimer.Stop();
        }


        

        override protected void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            
            if (_timerEnabled)
            {
                _hoverTimer.Interval = TimeSpan.FromMilliseconds(HoverInterval);
                e.Cursor.AnimateCursor(HoverInterval);
                _hoverTimer.Start();
            }
        }


        override protected void OnKinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            if (_timerEnabled)
            {
                e.Cursor.StopCursorAnimation();
                _hoverTimer.Stop();
            }
          
        }

        void _hoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

    }
}
