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

        /// <summary>
        /// Occurs when [swipe detected].
        /// </summary>
        public event KinectCursorEventHandler SwipeDetected;
        /// <summary>
        /// Occurs when [swipe out of bounds detected].
        /// </summary>
        public event KinectCursorEventHandler SwipeOutOfBoundsDetected;

        /// <summary>
        /// Creates the specified window.
        /// </summary>
        /// <param name="window">The window.</param>
        public static void Create(Window window)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window);
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Creates the specified window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="cursor">The cursor.</param>
        public static void Create(Window window, FrameworkElement cursor)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window, cursor);
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Creates the specified window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="sensor">The sensor.</param>
        public static void Create(Window window, KinectSensor sensor)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window, sensor);
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Creates the specified window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="sensor">The sensor.</param>
        /// <param name="cursor">The cursor.</param>
        public static void Create(Window window, KinectSensor sensor, FrameworkElement cursor)
        {
            if (!_isInitialized)
            {
                _instance = new KinectCursorManager(window, sensor, cursor);
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static KinectCursorManager Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="KinectCursorManager"/> class from being created.
        /// </summary>
        /// <param name="window">The window.</param>
        private KinectCursorManager(Window window)
            : this(window, KinectSensor.KinectSensors[0])
        {

        }

        /// <summary>
        /// Prevents a default instance of the <see cref="KinectCursorManager"/> class from being created.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="cursor">The cursor.</param>
        private KinectCursorManager(Window window, FrameworkElement cursor)
            : this(window, KinectSensor.KinectSensors[0], cursor)
        {

        }

        /// <summary>
        /// Prevents a default instance of the <see cref="KinectCursorManager"/> class from being created.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="sensor">The sensor.</param>
        private KinectCursorManager(Window window, KinectSensor sensor)
            : this(window, sensor, null)
        {

        }

        /// <summary>
        /// Prevents a default instance of the <see cref="KinectCursorManager"/> class from being created.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="sensor">The sensor.</param>
        /// <param name="cursor">The cursor.</param>
        private KinectCursorManager(Window window, KinectSensor sensor, FrameworkElement cursor)
        {
            this._window = window;
            this._gesturePoints = new List<GesturePoint>();
            // ensure kinects are present
            if (KinectSensor.KinectSensors.Count > 0)
            {
                _window.Unloaded += delegate
                {
                    if (this._kinectSensor.SkeletonStream.IsEnabled)
                        this._kinectSensor.SkeletonStream.Disable();
                    _kinectSensor.Stop();
                };

                _window.Loaded += delegate
                {
                    if (cursor == null)
                        _cursorAdorner = new CursorAdorner((FrameworkElement)window.Content);
                    else
                        _cursorAdorner = new CursorAdorner((FrameworkElement)window.Content, cursor);

                    this._kinectSensor = sensor;

                    this._kinectSensor.SkeletonFrameReady += SkeletonFrameReady;
                    this._kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
                    {
                        Correction = 0.5f
                        , JitterRadius = 0.05f
                        , MaxDeviationRadius = 0.04f
                        , Smoothing = 0.5f
                    });
                    this._kinectSensor.Start();
                };
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has hand threshold.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has hand threshold; otherwise, <c>false</c>.
        /// </value>
        public bool HasHandThreshold
        {
            get { return _hasHandThreshold; }
            set { _hasHandThreshold = value; }
        }

        /// <summary>
        /// Skeletons the frame ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.Kinect.SkeletonFrameReadyEventArgs"/> instance containing the event data.</param>
        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null || frame.SkeletonArrayLength == 0)
                    return;

                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                Skeleton skeleton = GetPrimarySkeleton(skeletons);

                if (skeleton == null)
                {
                    SetHandTrackingDeactivated();

                }
                else
                {
                    Joint? primaryHand = GetPrimaryHand(skeleton);
                    if (primaryHand.HasValue)
                    {
                        UpdateCursor(primaryHand.Value);
                    }
                    else
                    {
                        SetHandTrackingDeactivated();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the cursor.
        /// </summary>
        /// <param name="hand">The hand.</param>
        private void UpdateCursor(Joint hand)
        {
            var point = _kinectSensor.MapSkeletonPointToDepth(hand.Position, _kinectSensor.DepthStream.Format);
            float x = point.X;
            float y = point.Y;
            float z = point.Depth;

            SetHandTrackingActivated();
            x = (float)(x * _window.ActualWidth / _kinectSensor.DepthStream.FrameWidth);
            y = (float)(y * _window.ActualHeight / _kinectSensor.DepthStream.FrameHeight);
            Point cursorPoint = new Point(x, y);
            HandleGestureTracking(x, y, z);
            HandleCursorEvents(cursorPoint, z, hand);
            _cursorAdorner.UpdateCursor(cursorPoint);
        }

        /// <summary>
        /// Sets the hand tracking to activated state.
        /// </summary>
        private void SetHandTrackingActivated()
        {
            _cursorAdorner.SetVisibility(true);
            if (_lastElementOver != null && _isHandTrackingActivated == false) { _lastElementOver.RaiseEvent(new RoutedEventArgs(KinectInput.KinectCursorActivatedEvent)); };
            _isHandTrackingActivated = true;
        }

        /// <summary>
        /// Sets the hand tracking to deactivated state.
        /// </summary>
        private void SetHandTrackingDeactivated()
        {
            _cursorAdorner.SetVisibility(false);
            if (_lastElementOver != null && _isHandTrackingActivated == true) { _lastElementOver.RaiseEvent(new RoutedEventArgs(KinectInput.KinectCursorDeactivatedEvent)); };
            _isHandTrackingActivated = false;
        }

        /// <summary>
        /// Gets the primary skeleton.
        /// </summary>
        /// <param name="skeletons">The skeletons.</param>
        /// <returns></returns>
        private static Skeleton GetPrimarySkeleton(IEnumerable<Skeleton> skeletons)
        {
            Skeleton primarySkeleton = null;
            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (primarySkeleton == null)
                    primarySkeleton = skeleton;
                else if (primarySkeleton.Position.Z > skeleton.Position.Z)
                    primarySkeleton = skeleton;
            }
            return primarySkeleton;
        }
        /// <summary>
        /// Gets the primary hand.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns></returns>
        private Joint? GetPrimaryHand(Skeleton skeleton)
        {
            Joint leftHand = skeleton.Joints[JointType.HandLeft];
            Joint rightHand = skeleton.Joints[JointType.HandRight];

            if (rightHand.TrackingState == JointTrackingState.Tracked)
            {
                if (leftHand.TrackingState != JointTrackingState.Tracked)
                    return rightHand;
                else if (leftHand.Position.Z > rightHand.Position.Z)
                    return rightHand;
                else
                    return leftHand;
            }

            if (leftHand.TrackingState == JointTrackingState.Tracked)
                return leftHand;
            else
                return null;
        }

        /// <summary>
        /// Handles the gesture tracking.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        private void HandleGestureTracking(float x, float y, float z)
        {
            if (!_gesturePointTrackingEnabled)
                return;
            // check to see if xOutOfBounds is being used
            if (_xOutOfBoundsLength != 0 && _initialSwipeX == 0) 
            {
                _initialSwipeX = x;
            }

            GesturePoint newPoint = new GesturePoint() { X = x, Y = y, Z = z, T = DateTime.Now };
            GesturePoints.Add(newPoint);

            GesturePoint startPoint = GesturePoints[0];
            var point = new Point(x, y);


            //check for deviation
            if (Math.Abs(newPoint.Y - startPoint.Y) > _swipeDeviation)
            {
                //Debug.WriteLine("Y out of bounds");
                if (SwipeOutOfBoundsDetected != null)
                    SwipeOutOfBoundsDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = _cursorAdorner });
                ResetGesturePoint(GesturePoints.Count);
                return;
            }
            if ((newPoint.T - startPoint.T).Milliseconds > _swipeTime) //check time
            {
                GesturePoints.RemoveAt(0);
                startPoint = GesturePoints[0];
            }
            if ((_swipeLength < 0 && newPoint.X - startPoint.X < _swipeLength) // check to see if distance has been achieved swipe left
                || (_swipeLength > 0 && newPoint.X - startPoint.X > _swipeLength)) // check to see if distance has been achieved swipe right
            {
                GesturePoints.Clear();

                //throw local event
                if (SwipeDetected != null)
                    SwipeDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = _cursorAdorner });
                return;
            }
            if (_xOutOfBoundsLength != 0 &&
                ((_xOutOfBoundsLength < 0 && newPoint.X - _initialSwipeX < _xOutOfBoundsLength) // check to see if distance has been achieved swipe left
                || (_xOutOfBoundsLength > 0 && newPoint.X - _initialSwipeX > _xOutOfBoundsLength))
                )
            {
                if (SwipeOutOfBoundsDetected != null)
                    SwipeOutOfBoundsDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = _cursorAdorner });
            }
        }

        /// <summary>
        /// Handles the cursor events.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="z">The z.</param>
        /// <param name="joint">The joint.</param>
        private void HandleCursorEvents(Point point, double z, Joint joint)
        {

            UIElement element = GetElementAtScreenPoint(point, _window);
            if (element != null)
            {
                element.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorMoveEvent, point, z) { Cursor = _cursorAdorner });
                if (element != _lastElementOver)
                {
                    if (_lastElementOver != null)
                    {
                        _lastElementOver.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorLeaveEvent, point, z) { Cursor = _cursorAdorner });
                    }

                    element.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorEnterEvent, point, z) { Cursor = _cursorAdorner });
                }
            }

            _lastElementOver = element;
        }

        /// <summary>
        /// Retrieve the input element at the given screen point.
        /// </summary>
        /// <param name="point">The point for hit testing.</param>
        /// <param name="window">The TouchWindow for hit testing.</param>
        /// <returns>UIElement result of the hit test. Null if no elements were located.</returns>
        private static UIElement GetElementAtScreenPoint(Point point, Window window)
        {
            if (!window.IsVisible)
                return null;

            Point windowPoint = window.PointFromScreen(point);

            IInputElement element = window.InputHitTest(windowPoint);
            if (element is UIElement)
                return (UIElement)element;
            else
                return null;
        }

        /// <summary>
        /// Gets the gesture points.
        /// </summary>
        public IList<GesturePoint> GesturePoints
        {
            get { return _gesturePoints; }
        }

        /// <summary>
        /// Gestures the point tracking initialize.
        /// </summary>
        /// <param name="swipeLength">Length of the swipe.</param>
        /// <param name="swipeDeviation">The swipe deviation.</param>
        /// <param name="swipeTime">The swipe time.</param>
        /// <param name="xOutOfBounds">The x out of bounds.</param>
        public void GesturePointTrackingInitialize(double swipeLength, double swipeDeviation, int swipeTime, double xOutOfBounds)
        {
            _swipeLength = swipeLength;
            _swipeDeviation = swipeDeviation;
            _swipeTime = swipeTime;
            _xOutOfBoundsLength = xOutOfBounds;
        }

        /// <summary>
        /// Gestures the point tracking start.
        /// </summary>
        public void GesturePointTrackingStart()
        {
            if (_swipeLength == 0 || _swipeDeviation == 0 || _swipeTime == 0)
                throw (new InvalidOperationException("Swipe detection not initialized."));
            _gesturePointTrackingEnabled = true;
        }

        /// <summary>
        /// Gestures the point tracking stop.
        /// </summary>
        public void GesturePointTrackingStop()
        {
            _xOutOfBoundsLength = 0;
            _gesturePointTrackingEnabled = false;
            _gesturePoints.Clear();
        }


        /// <summary>
        /// Gets a value indicating whether [gesture point tracking enabled].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [gesture point tracking enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool GesturePointTrackingEnabled
        {
            get { return _gesturePointTrackingEnabled; }
        }

        /// <summary>
        /// Resets the gesture point.
        /// </summary>
        /// <param name="point">The point.</param>
        private void ResetGesturePoint(GesturePoint point)
        {
            bool startRemoving = false;
            for (int i = GesturePoints.Count; i >= 0; i--)
            {
                if (startRemoving)
                    GesturePoints.RemoveAt(i);
                else
                    if (GesturePoints[i].Equals(point))
                        startRemoving = true;
            }
        }

        /// <summary>
        /// Resets the gesture point.
        /// </summary>
        /// <param name="point">The point.</param>
        private void ResetGesturePoint(int point)
        {
            if (point < 1)
                return;
            for (int i = point - 1; i >= 0; i--)
            {
                GesturePoints.RemoveAt(i);
            }
        }
    }
}
