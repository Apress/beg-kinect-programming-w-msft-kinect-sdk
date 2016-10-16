
using System.Windows.Media;

namespace Beginning.Kinect.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Documents;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows.Media.Animation;
    using System.Diagnostics;


    /// <summary>
    /// CursorAdorner is a visual representation of the tracking hand.
    /// </summary>
    public class CursorAdorner : Adorner
    {
        // local members for managing cursor visuals
        private readonly UIElement _adorningElement;
        private VisualCollection _visualChildren;
        private Canvas _cursorCanvas;
        protected FrameworkElement _cursor;
        private bool _isVisible;
        private bool _isOverridden;
        Storyboard _gradientStopAnimationStoryboard;

        // default cursor colors
        readonly static  Color _backColor = Colors.White;
        readonly static Color _foreColor = Colors.Gray;


        /// <summary>
        /// Initializes a new instance of the <see cref="CursorAdorner"/> class.
        /// </summary>
        /// <param name="adorningElement">The adorning element.</param>
        public CursorAdorner(FrameworkElement adorningElement)
            : base(adorningElement)
        {
            if (adorningElement == null)
                throw new ArgumentNullException("adorningElement");
            this._adorningElement = adorningElement;
            CreateCursorAdorner();
            this.IsHitTestVisible = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CursorAdorner"/> class.
        /// </summary>
        /// <param name="adorningElement">The adorning element.</param>
        /// <param name="innerCursor">The inner cursor.</param>
        public CursorAdorner(FrameworkElement adorningElement, FrameworkElement innerCursor)
            : base(adorningElement)
        {
            if (adorningElement == null)
                throw new ArgumentNullException("Adorning Element parameter empty");
            this._adorningElement = adorningElement;
            CreateCursorAdorner(innerCursor);
            this.IsHitTestVisible = false;
        }

        /// <summary>
        /// Creates the cursor adorner.
        /// </summary>
        public void CreateCursorAdorner()
        {
            var innerCursor = CreateCursor();
            CreateCursorAdorner(innerCursor);
        }

        /// <summary>
        /// Updates the cursor position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="isOverride">if set to <c>true</c> [is override].</param>
        public void UpdateCursor(Point position, bool isOverride)
        {
            _isOverridden = isOverride;
            // center the cursor visual at the new position
            _cursor.SetValue(Canvas.LeftProperty, position.X - (_cursor.ActualWidth / 2));
            _cursor.SetValue(Canvas.TopProperty, position.Y - (_cursor.ActualHeight / 2));
        }

        /// <summary>
        /// Updates the cursor position.
        /// </summary>
        /// <param name="position">The position.</param>
        public void UpdateCursor(Point position)
        {
            if (_isOverridden)
                return;
            // center the cursor visual at the new position
            _cursor.SetValue(Canvas.LeftProperty, position.X - (_cursor.ActualWidth / 2));
            _cursor.SetValue(Canvas.TopProperty, position.Y - (_cursor.ActualHeight / 2));
        }

        /// <summary>
        /// Creates the cursor.
        /// </summary>
        /// <returns></returns>
        protected FrameworkElement CreateCursor()
        {
            var brush = new LinearGradientBrush();
            brush.EndPoint = new Point(0, 1);
            brush.StartPoint = new Point(0, 0);
            brush.GradientStops.Add(new GradientStop(_backColor, 1));
            brush.GradientStops.Add(new GradientStop(_foreColor, 1));
            var cursor = new Ellipse() {
                Width = 50,
                Height = 50,
                Fill = brush
            };
            return cursor;
        }

        /// <summary>
        /// Creates the cursor adorner.
        /// </summary>
        /// <param name="innerCursor">The inner cursor.</param>
        public void CreateCursorAdorner(FrameworkElement innerCursor)
        {
            _visualChildren = new VisualCollection(this);
            _cursorCanvas = new Canvas();
            _cursor = innerCursor;
            _cursorCanvas.Children.Add(_cursor);
            _visualChildren.Add(this._cursorCanvas);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_adorningElement);
            layer.Add(this);
        }

        #region Overridden methods

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return _visualChildren.Count;
            }
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Size"/> object representing the amount of layout space needed by the adorner.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            this._cursorCanvas.Measure(constraint);
            return this._cursorCanvas.DesiredSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size used.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this._cursorCanvas.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <summary>
        /// Sets the visibility of the cursor visual.
        /// </summary>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        public void SetVisibility(bool isVisible)
        {
            if (this._isVisible && !isVisible)
                _cursorCanvas.Visibility = Visibility.Hidden;
            if (!this._isVisible && isVisible)
                _cursorCanvas.Visibility = Visibility.Visible;
            this._isVisible = isVisible;
        }

        /// <summary>
        /// Animates the cursor.
        /// </summary>
        /// <param name="milliSeconds">The milli seconds.</param>
        public virtual void AnimateCursor(double milliSeconds)
        {
            CreateGradientStopAnimation(milliSeconds);
            if (_gradientStopAnimationStoryboard!= null)
                _gradientStopAnimationStoryboard.Begin(this, true);
        }

        /// <summary>
        /// Stops the cursor animation.
        /// </summary>
        public virtual void StopCursorAnimation()
        {
            if(_gradientStopAnimationStoryboard != null)
                _gradientStopAnimationStoryboard.Stop(this);
        }

        /// <summary>
        /// Gets the cursor visual.
        /// </summary>
        public FrameworkElement CursorVisual
        {
            get
            {
                return _cursor;
            }
    
        }

        /// <summary>
        /// Creates the gradient stop animation.
        /// </summary>
        /// <param name="milliSeconds">The milli seconds.</param>
        protected virtual void CreateGradientStopAnimation(double milliSeconds)
        {
            NameScope.SetNameScope(this, new NameScope());

            var cursor = _cursor as Shape;
            if (cursor == null)
                return;
            var brush = cursor.Fill as LinearGradientBrush;
            var stop1 = brush.GradientStops[0];
            var stop2 = brush.GradientStops[1];
            this.RegisterName("GradientStop1", stop1);
            this.RegisterName("GradientStop2", stop2);

            DoubleAnimation offsetAnimation = new DoubleAnimation();
            offsetAnimation.From = 1.0;
            offsetAnimation.To = 0.0;
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(milliSeconds);


            Storyboard.SetTargetName(offsetAnimation, "GradientStop1");
            Storyboard.SetTargetProperty(offsetAnimation,
                new PropertyPath(GradientStop.OffsetProperty));


            DoubleAnimation offsetAnimation2 = new DoubleAnimation();
            offsetAnimation2.From = 1.0;
            offsetAnimation2.To = 0.0;

            offsetAnimation2.Duration = TimeSpan.FromMilliseconds(milliSeconds);

            Storyboard.SetTargetName(offsetAnimation2, "GradientStop2");
            Storyboard.SetTargetProperty(offsetAnimation2,
                new PropertyPath(GradientStop.OffsetProperty));

            _gradientStopAnimationStoryboard = new Storyboard();
            _gradientStopAnimationStoryboard.Children.Add(offsetAnimation);
            _gradientStopAnimationStoryboard.Children.Add(offsetAnimation2);
            _gradientStopAnimationStoryboard.Completed += delegate { _gradientStopAnimationStoryboard.Stop(this); };

        }

        #endregion
    }
}
