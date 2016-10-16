using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Beginning.Kinect.Framework.Input
{
    public class KinectCursorEventArgs: RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public KinectCursorEventArgs(double x, double y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="point">The position as a Point.</param>
        public KinectCursorEventArgs(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent): base(routedEvent){}

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z distance in millimeters.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent, double x, double y, double z) : base(routedEvent) { X = x; Y = y; Z = z; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event.</param>
        /// <param name="point">The position as a Point.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent, Point point) : base(routedEvent) { X = point.X; Y = point.Y; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event.</param>
        /// <param name="point">The position as a Point.</param>
        /// <param name="z">The Z distance in millimeters.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent, Point point, double z) : base(routedEvent) { X = point.X; Y = point.Y; Z = z; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z distance in millimeters.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent, object source, double x, double y, double z) : base(routedEvent, source) { X = x; Y = y; Z = z; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        /// <param name="point">The position as a Point.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent, object source, Point point) : base(routedEvent, source) { X = point.X; Y = point.Y; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectCursorEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        /// <param name="point">The position as a Point.</param>
        /// /// <param name="z">The Z distance in millimeters.</param>
        public KinectCursorEventArgs(RoutedEvent routedEvent, object source, Point point, double z) : base(routedEvent, source) { X = point.X; Y = point.Y; Z = z; }

        /// <summary>
        /// Gets or sets the X coordinate.
        /// </summary>
        /// <value>
        /// The X.
        /// </value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate.
        /// </summary>
        /// <value>
        /// The Y.
        /// </value>
        public double Y { get; set; }
        /// <summary>
        /// Gets or sets the Z value in millimeters.
        /// </summary>
        /// <value>
        /// The Z.
        /// </value>
        public double Z { get; set; }
        /// <summary>
        /// Gets or sets the cursor.
        /// </summary>
        /// <value>
        /// The cursor.
        /// </value>
        public CursorAdorner Cursor { get; set; }
    }
}
