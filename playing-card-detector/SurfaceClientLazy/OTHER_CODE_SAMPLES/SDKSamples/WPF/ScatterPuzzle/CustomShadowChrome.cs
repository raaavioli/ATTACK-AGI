using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScatterPuzzle
{
    /// <summary>
    /// Shadow Chrome for a ScatterViewItem. This chrome can render a custom shaped
    /// shadow if a shape is provided.
    /// </summary>
    public class CustomShadowChrome : Decorator
    {
        #region Private Members
        
        // Number of layers in the custom shadow 
        private const int customShadowDepth = 9;

        // The drawn offset for the shadow.
        private Vector shadowOffset = new Vector(0, 0);

        #endregion

        #region Dependency and Public Properties

        /// <summary>
        /// Identifies an attached property that specifies the distance and direction from a light
        /// source to an element.
        /// </summary>
        public static readonly DependencyProperty ShadowVectorProperty =
                DependencyProperty.RegisterAttached(
                        "ShadowVector",
                        typeof(Vector),
                        typeof(CustomShadowChrome),
                        new FrameworkPropertyMetadata(new Vector(0, 0), OnShadowPropertyChanged));

        /// <summary>
        /// Sets the <strong><see cref="ShadowVector"/></strong> attached property.
        /// </summary>
        /// <param name="element">The UI element on which to set the <strong>ShadowVector</strong>.</param>
        /// <param name="shadowVector">The <strong>Vector</strong> value for the shadow.</param>
        /// <remarks>A shadow vector represents the distance and direction from a light source.</remarks>
        public static void SetShadowVector(DependencyObject element, Vector shadowVector)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(ShadowVectorProperty, shadowVector);
        }

        /// <summary>
        /// Reads the <strong><see cref="ShadowVector"/></strong> attached property.
        /// </summary>
        /// <param name="element">The UI element from which to get the <strong>ShadowVector</strong> property.</param>
        /// <remarks>A shadow vector represents the distance and direction from a light source.</remarks>
        /// <returns>The <strong>ShadowVector</strong> property on the specified <em>element</em>.</returns>
        public static Vector GetShadowVector(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return ((Vector)element.GetValue(ShadowVectorProperty));
        }

        /// <summary>
        /// Gets or sets the shadow vector.
        /// </summary>
        /// <value>A <strong>Vector</strong> object. The default value is (0, 0).</value>
        /// <remarks>A shadow vector represents the distance and direction from a light source. 
        /// The <strong><see cref="ShadowVector"/></strong> property is a dependency property.</remarks>
        public Vector ShadowVector
        {
            get
            {
                return (Vector)GetValue(ShadowVectorProperty);
            }
            set
            {
                SetValue(ShadowVectorProperty, value);
            }
        }

        /// <summary>
        /// Identifies a dependency property that specifies the maximum length of a calculated shadow
        /// vector.
        /// </summary>
        /// <remarks><para>A <em>shadow vector</em> represents the disance
        /// and direction from a light source.</para>
        /// <para>The default value is 0.</para></remarks>
        public static readonly DependencyProperty MaximumShadowOffsetProperty =
                DependencyProperty.Register(
                        "MaximumShadowOffset",
                        typeof(double),
                        typeof(CustomShadowChrome),
                        new FrameworkPropertyMetadata(0.0, OnShadowPropertyChanged),
                        OnValidateMaximumShadowOffset);

        /// <summary>
        /// Gets or sets the maximum length that is allowed for a shadow vector. 
        /// </summary>
        /// <value>A <strong>double</strong> value that specifies the maximum length. The default value is 0.</value>
        /// <remarks>The <strong>MaximumShadowOffset</strong> property is a
        /// dependency property. This property must always be a finite, non-negative value.</remarks>
        public double MaximumShadowOffset
        {
            get
            {
                return (double)GetValue(MaximumShadowOffsetProperty);
            }
            set
            {
                SetValue(MaximumShadowOffsetProperty, value);
            }
        }

        /// <summary>
        /// A ValidationCallback method that ensures a maximum shadow offset value is non-negative.
        /// </summary>
        /// <param name="value">the value to validate</param>
        /// <returns>true if the value is a non-negative finite double, false otherwise</returns>
        private static bool OnValidateMaximumShadowOffset(object value)
        {
            if (value is double)
            {
                double offset = (double)value;
                return !double.IsNaN(offset) &&
                       !double.IsInfinity(offset) &&
                       offset >= 0.0;
            }
            return false;
        }

        /// <summary>
        /// A PropertyChangedCallback method that is called whenever
        /// the values of MaximumShadowOffset or ShadowVector are
        /// changed. Use this method to update the rendered offset.
        /// </summary>
        /// <param name="d">the object upon which the property was modified</param>
        /// <param name="e">the details of the changes</param>
        private static void OnShadowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Update the rendered offset.
            CustomShadowChrome chrome = d as CustomShadowChrome;
            if (chrome != null)
            {
                chrome.UpdateShadowOffset();
            }
        }

        /// <summary>
        /// Identifies a dependency property for the <strong><see cref="Color" /></strong> property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
                DependencyProperty.Register(
                        "Color",
                        typeof(Color),
                        typeof(CustomShadowChrome),
                        new FrameworkPropertyMetadata(
                                Color.FromArgb(0xFF, 0x00, 0x00, 0x00),
                                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the color that fills a shadow region.
        /// </summary>
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Identifies a dependency property for the <strong><see cref="ShadowShape" /></strong> property.
        /// </summary>
        public static readonly DependencyProperty ShadowShapeProperty =
            DependencyProperty.Register(
                "ShadowShape",
                typeof(Geometry),
                typeof(CustomShadowChrome),
                new PropertyMetadata(null, OnShadowShapePropertyChanged));

        /// <summary>
        /// Gets or sets the shape of the shadow.
        /// </summary>
        public Geometry ShadowShape
        {
            get { return (Geometry)GetValue(ShadowShapeProperty); }
            set { SetValue(ShadowShapeProperty, value); }
        }

        /// <summary>
        /// A PropertyChangedCallback method that is called whenever
        /// the value of ShadowShape is changed. 
        /// </summary>
        /// <param name="d">the object upon which the property was modified</param>
        /// <param name="e">the details of the changes</param>
        private static void OnShadowShapePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as UIElement).InvalidateVisual();
        }

        #endregion Dynamic Properties

        #region Initalization

        /// <summary>
        /// Initializes a new instance of the <strong><see cref="CustomShadowChrome"/></strong> class.
        /// </summary>
        public CustomShadowChrome()
        {
        }

        #endregion Constructors

        #region Rendering and Positioning

        /// <summary>
        /// Render callback.  
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Only render if there's a shape to render
            if (ShadowShape != null)
            {
                // Get a brush to draw the shadow
                SolidColorBrush brush = new SolidColorBrush(Color);
                brush.Opacity = 1.0 / customShadowDepth;

                for (int i = 0;i < customShadowDepth;i++)
                {
                    // Clone a shape so it can be modified without altering the original shape
                    Geometry geo = ShadowShape.Clone();

                    // Determine the size for this layer, and the amount of scale that must be applied
                    // to make the shape be that size
                    double targetWidth = Math.Round(geo.Bounds.Width, 0) + (2.0 * i) - 6;
                    double xScale = targetWidth / Math.Round(geo.Bounds.Width, 0);
                    double targetHeight = Math.Round(geo.Bounds.Height, 0) + (2.0 * i) - 6;
                    double yScale = targetHeight / Math.Round(geo.Bounds.Height, 0);

                    // Scale the shadow layer, then center it
                    TransformGroup group = new TransformGroup();
                    group.Children.Add(new ScaleTransform(xScale, yScale));
                    group.Children.Add(new TranslateTransform((customShadowDepth / 3) - i, (customShadowDepth / 3) - i));
                    geo.Transform = group;

                    // Draw the geometry
                    drawingContext.DrawGeometry(brush, null, geo);
                }
            }
        }

        /// <summary>
        /// Updates the vector actually used for the rendering offset
        /// of the specified CustomShadowChrome object.
        /// </summary>
        private void UpdateShadowOffset()
        {
            // The shadow offset is the ShadowVector, capped at a length
            // of the MaximumShadowOffset.
            Vector offset = ShadowVector;
            double max = MaximumShadowOffset;
            if (!double.IsNaN(max) && !double.IsInfinity(max) &&
                offset.Length > max)
            {
                offset *= max / offset.Length;
            }

            // Set the new value if it is changed.
            if (offset != shadowOffset)
            {
                TranslateTransform tx = RenderTransform as TranslateTransform;
                if (tx != null)
                {
                    tx.X = offset.X;
                    tx.Y = offset.Y;
                }
                else
                {
                    RenderTransform = new TranslateTransform(offset.X, offset.Y);
                }
                RenderTransformOrigin = new Point(0.5, 0.5);
                shadowOffset = offset;
            }
        }

        #endregion
    }
}
