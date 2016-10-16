using System.Collections.Generic;
using System.Windows;
using System;
using Beginning.Kinect.Framework.Input;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Kinect;

namespace Beginning.Kinect.Framework
{
    /// <summary>
    /// Cursor Manager coordinates hand tracking.
    /// </summary>
    public class KinectCursorManager
    {
        // local members for managing kinect and hand tracking objects
        private KinectSensor _kinectSensor;
        private CursorAdorner _cursorAdorner;
        private readonly Window _window;
        private UIElement _lastElementOver;
        private bool _isHandTrackingActivated;
        private static bool _isInitialized;
        private static KinectCursorManager _instance;
        private List<GesturePoint> _gesturePoints;
        private bool _gesturePointTrackingEnabled;
        private double _swipeLength, _swipeDeviation;
        private int _swipeTime;
        private bool _hasHandThreshold = true;

        private double _xOutOfBoundsLength;
        private static double _initialSwipeX;

        //TODO: complete implementation
    }
}
