using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Beginning.Kinect.Framework.Input;
using System.Windows;
using System.Diagnostics;

namespace Beginning.Kinect.Framework.Controls
{
    public class KinectButton: Button
    {
        public static readonly RoutedEvent KinectCursorEnterEvent = KinectInput.KinectCursorEnterEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorLeaveEvent = KinectInput.KinectCursorLeaveEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorMoveEvent = KinectInput.KinectCursorMoveEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorActivatedEvent = KinectInput.KinectCursorActivatedEvent.AddOwner(typeof(KinectButton));
        public static readonly RoutedEvent KinectCursorDeactivatedEvent = KinectInput.KinectCursorDeactivatedEvent.AddOwner(typeof(KinectButton));
        

        public event KinectCursorEventHandler KinectCursorEnter
        {
            add { base.AddHandler(KinectCursorEnterEvent, value); }
            remove { base.RemoveHandler(KinectCursorEnterEvent, value); }
        }

        public event KinectCursorEventHandler KinectCursorLeave
        {
            add { base.AddHandler(KinectCursorLeaveEvent, value); }
            remove { base.RemoveHandler(KinectCursorLeaveEvent, value); }
        }

        public event KinectCursorEventHandler KinectCursorMove
        {
            add { base.AddHandler(KinectCursorMoveEvent, value); }
            remove { base.RemoveHandler(KinectCursorMoveEvent, value); }
        }

        public event RoutedEventHandler KinectCursorActivated
        {
            add { base.AddHandler(KinectCursorActivatedEvent, value); }
            remove { base.RemoveHandler(KinectCursorActivatedEvent, value); }
        }

        public event RoutedEventHandler KinectCursorDeactivated
        {
            add { base.AddHandler(KinectCursorDeactivatedEvent, value); }
            remove { base.RemoveHandler(KinectCursorDeactivatedEvent, value); }
        }

        public KinectButton()
        {
            //autogen the manager if needed
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                KinectCursorManager.Create(Application.Current.MainWindow);


            this.KinectCursorEnter += new KinectCursorEventHandler(OnKinectCursorEnter);
            this.KinectCursorLeave += new KinectCursorEventHandler(OnKinectCursorLeave);
            this.KinectCursorMove += new KinectCursorEventHandler(OnKinectCursorMove);
            this.KinectCursorActivated += new RoutedEventHandler(OnKinectCursorActivated);
            this.KinectCursorDeactivated += new RoutedEventHandler(OnKinectCursorDeactivated);
        }

        protected virtual void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        protected virtual void OnKinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            
        }

        protected virtual void OnKinectCursorMove(object sender, KinectCursorEventArgs e)
        {

        }
        protected virtual void OnKinectCursorActivated(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void OnKinectCursorDeactivated(object sender, RoutedEventArgs e)
        {

        }

    }
}
