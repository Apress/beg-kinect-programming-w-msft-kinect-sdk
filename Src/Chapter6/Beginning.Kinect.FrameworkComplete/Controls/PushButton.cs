using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Beginning.Kinect.Framework.Input;
using System.Diagnostics;

namespace Beginning.Kinect.Framework.Controls
{
    public class PushButton: MagnetButton
    {
        protected double _handDepth;

        public double PushThreshold
        {
            get { return (double)GetValue(PushThresholdProperty); }
            set { SetValue(PushThresholdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Pushthreshold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PushThresholdProperty =
            DependencyProperty.Register("PushThreshold", typeof(double), typeof(PushButton), new UIPropertyMetadata(100d));

        
        protected override void OnKinectCursorMove(object sender, KinectCursorEventArgs e)
        {
            if (e.Z < _handDepth - PushThreshold)
            {
                RaiseEvent(new RoutedEventArgs(ClickEvent));
            }
        }

        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            _handDepth = e.Z;
        }

    }
}
