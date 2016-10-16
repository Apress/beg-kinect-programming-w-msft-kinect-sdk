

namespace Beginning.Kinect.Framework.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;


    public delegate void KinectCursorEventHandler(object sender, KinectCursorEventArgs e);

    /// <summary>
    /// KinectInput is a contains custom events used in the Beginning.Kinect.Framework.
    /// </summary>
    public static class KinectInput
    {
        #region KinectCursorEnter

        /// <summary>
        /// Identifies the KinectCursorEnterEvent.
        /// </summary>
        public static readonly RoutedEvent KinectCursorEnterEvent = EventManager.RegisterRoutedEvent("KinectCursorEnter", RoutingStrategy.Bubble,
            typeof(KinectCursorEventHandler), typeof(KinectInput));
        /// <summary>
        /// Adds a specified event handler for the KinectCursorEnter event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void AddKinectCursorEnterHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).AddHandler(KinectCursorEnterEvent, handler);
        }
        /// <summary>
        /// Remove the specified event handler for the KinectCursorEnter event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void RemoveKinectCursorEnterHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).RemoveHandler(KinectCursorEnterEvent, handler);
        }

        #endregion

        #region KinectCursorLeave

        /// <summary>
        /// Identifies the KinectCursorLeaveEvent.
        /// </summary>
        public static readonly RoutedEvent KinectCursorLeaveEvent = EventManager.RegisterRoutedEvent("KinectCursorLeave", RoutingStrategy.Bubble,
            typeof(KinectCursorEventHandler), typeof(KinectInput));
        /// <summary>
        /// Adds a specified event handler for the KinectCursorLeave event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void AddKinectCursorLeaveHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).AddHandler(KinectCursorEnterEvent, handler);
        }
        /// <summary>
        /// Remove the specified event handler for the KinectCursorLeave event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void RemoveKinectCursorLeaveHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).RemoveHandler(KinectCursorEnterEvent, handler);
        }

        #endregion

        #region KinectCursorActivated

        /// <summary>
        /// Identifies the KinectCursorActivatedEvent.
        /// </summary>
        public static readonly RoutedEvent KinectCursorActivatedEvent = EventManager.RegisterRoutedEvent("KinectCursorActivated", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(KinectInput));
        /// <summary>
        /// Adds a specified event handler for the KinectCursorActivated event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void AddKinectCursorActivatedHandler(DependencyObject o, RoutedEventHandler handler)
        {
            ((UIElement)o).AddHandler(KinectCursorActivatedEvent, handler);
        }
        /// <summary>
        /// Remove the specified event handler for the KinectCursorActivated event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void RemoveKinectCursorActivatedHandler(DependencyObject o, RoutedEventHandler handler)
        {
            ((UIElement)o).RemoveHandler(KinectCursorActivatedEvent, handler);
        }
        #endregion

        #region KinectCursorDeactivated
        /// <summary>
        /// Identifies the KinectCursorvctivatedEvent.
        /// </summary>
        public static readonly RoutedEvent KinectCursorDeactivatedEvent = EventManager.RegisterRoutedEvent("KinectCursorDeactivated", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(KinectInput));
        /// <summary>
        /// Adds a specified event handler for the KinectCursorDeactivated event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void AddKinectCursorDeactivatedHandler(DependencyObject o, RoutedEventHandler handler)
        {
            ((UIElement)o).AddHandler(KinectCursorDeactivatedEvent, handler);
        }
        /// <summary>
        /// Remove the specified event handler for the KinectCursorDeactivated event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void RemoveKinectCursorDeactivatedHandler(DependencyObject o, RoutedEventHandler handler)
        {
            ((UIElement)o).RemoveHandler(KinectCursorDeactivatedEvent, handler);
        }
        #endregion

        #region KinectCursorMove

        /// <summary>
        /// Identifies the KinectCursorMoveEvent.
        /// </summary>
        public static readonly RoutedEvent KinectCursorMoveEvent = EventManager.RegisterRoutedEvent("KinectCursorMove", RoutingStrategy.Bubble,
            typeof(KinectCursorEventHandler), typeof(KinectInput));
        /// <summary>
        /// Adds a specified event handler for the KinectCursorMove event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void AddKinectCursorMoveHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).AddHandler(KinectCursorMoveEvent, handler);
        }
        /// <summary>
        /// Remove the specified event handler for the KinectCursorMove event.
        /// </summary>
        /// <param name="o">The object that listens to the event.</param>
        /// <param name="handler">The event handler to add.</param>
        public static void RemoveKinectCursorMoveHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).RemoveHandler(KinectCursorMoveEvent, handler);
        }
        #endregion

        public static readonly RoutedEvent KinectCursorLockEvent = EventManager.RegisterRoutedEvent("KinectCursorLock", RoutingStrategy.Bubble,
    typeof(KinectCursorEventHandler), typeof(KinectInput));

        /// <summary>
        /// Adds the kinect cursor lock handler.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="handler">The handler.</param>
        public static void AddKinectCursorLockHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).AddHandler(KinectCursorLockEvent, handler);
        }

        public static readonly RoutedEvent KinectCursorUnlockEvent = EventManager.RegisterRoutedEvent("KinectCursorUnlock", RoutingStrategy.Bubble,
    typeof(KinectCursorEventHandler), typeof(KinectInput));

        /// <summary>
        /// Adds the kinect cursor unlock handler.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="handler">The handler.</param>
        public static void AddKinectCursorUnlockHandler(DependencyObject o, KinectCursorEventHandler handler)
        {
            ((UIElement)o).AddHandler(KinectCursorUnlockEvent, handler);
        }
    }
}
