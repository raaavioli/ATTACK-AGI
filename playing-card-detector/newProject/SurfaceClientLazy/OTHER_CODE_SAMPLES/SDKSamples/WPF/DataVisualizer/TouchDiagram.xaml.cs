using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Input;


namespace DataVisualizer
{
    /// <summary>
    /// A diagram of a touch device's properties.
    /// This diagram includes a text description of the touch device's properties. If the
    /// touch device has ellipse data available, this diagram will also include a visual
    /// representation of that ellipse data, and the bounding rectangle for that ellipse.
    /// A connecting line is drawn from the center of the touch device to the text description.
    /// There is some simple logic to make sure the text description is always completely
    /// inside a specified bounds.
    /// </summary>
    public partial class TouchDeviceDiagram : UserControl
    {
        // The following distances specify how far the description textbox should be
        // from the center of the touch device.
        private const double nonTagDescriptionXDistance = 40.0;
        private const double tagDescriptionXDistance = 190.0;
        private const double descriptionYDistance = 30.0;
                
        // This ellipse will be updated to match the touch device shape in screen coordinates.
        private Ellipse twoToneEllipse;

        // This rectangle will be updated to match the bounding rectangle for the touch device. 
        private Rectangle boundingRectangle;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TouchDeviceDiagram()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Update this diagram with the most recent touch device data.
        /// </summary>
        /// <param name="parentGrid">the container for this diagram-
        /// description text will not go outside of this container's bounds</param>
        /// <param name="touchDevice">the touch device to diagram</param>
        public void Update(Grid parentGrid, TouchDevice touchDevice, bool showBoundingRect, bool showDescription)
        {
            // Update the two-tone Ellipse.
            UpdateEllipse(touchDevice);

            // Update the bounding rect on the touchDevice
            UpdateRectangle(touchDevice, showBoundingRect);
            
            // Update the arrow that demonstrates orientation.
            UpdateOrientationArrow(touchDevice);

            // Update the text description of the touchDevice properties.
            UpdateDescription(parentGrid, touchDevice, showDescription);
        }

        /// <summary>
        /// Rotate the arrow to demonstrate orientation.
        /// </summary>
        /// <param name="touchDevice">the touch device to diagram</param>
        private void UpdateOrientationArrow(TouchDevice touchDevice)
        {
            bool isTag = touchDevice.GetIsTagRecognized();
            
            UIElement relativeTo = this;
            double? touchDeviceOrientation = touchDevice.GetOrientation(relativeTo);

            // Only show orientation arrow if this touchDevice is recognized as a tag
            // and there is orientation data available.
            if (isTag && touchDeviceOrientation != null)
            {
                // Show the arrow.
                OrientationArrow.Visibility = Visibility.Visible;

                // Set the arrow orientation.
                ArrowRotateTransform.Angle = (double)touchDeviceOrientation;

                // Set the arrow position.
                Point position = touchDevice.GetPosition(relativeTo);
                ArrowTranslateTransform.X = position.X;
                ArrowTranslateTransform.Y = position.Y;
            }
            else
            {
                // Hide the arrow.
                OrientationArrow.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Update ellipse with the most recent touch device data. If an ellipse has not
        /// been created yet, create one.
        /// </summary>
        /// <param name="touchDevice">the touch device to diagram</param>
        private void UpdateEllipse(TouchDevice touchDevice)
        {
            UIElement relativeTo = this;
            
            // Create an ellipse if one does not exist already.
            if (twoToneEllipse == null)
            {
                // Request an ellipse with the proper dimensions and RenderTransform.
                twoToneEllipse = touchDevice.GetEllipse(relativeTo);

                // Give the ellipse a two color fill.
                LinearGradientBrush twoToneBrush = new LinearGradientBrush();
                twoToneBrush.StartPoint = new Point(1.0, 0.5);
                twoToneBrush.EndPoint = new Point(0.0, 0.5);
                GradientStopCollection gradientStops = new GradientStopCollection();
                gradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF1, 0x57, 0x27), 0.49));
                gradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF3, 0x7A, 0x53), 0.51)); 
                twoToneBrush.GradientStops = gradientStops;
                twoToneEllipse.Fill = twoToneBrush;

                // Add the ellipse to the MainCanvas.
                MainCanvas.Children.Add(twoToneEllipse);

                // Give the Ellipse a lower ZIndex than the ConnectingLine and Description
                // so it is not drawn on top of them.
                Canvas.SetZIndex(twoToneEllipse, -1);
            }
            else
            {
                // This diagram already has an ellipse. Update the existing ellipse
                // dimensions and RenderTransform so the shape will match the touchDevice.
                touchDevice.UpdateEllipse(twoToneEllipse, relativeTo);
            }
        }

        /// <summary>
        /// Update the bounding rectangle with the most recent touch device info. If a
        /// rectangle has not been created yet, create one.
        /// </summary>
        /// <param name="touchDevice">the touch device to diagram</param>
        /// <param name="showBoundingRectangle">Whether or not the rectangle should be shown</param>
        private void UpdateRectangle(TouchDevice touchDevice, bool showBoundingRectangle)
        {
            // Create an rectangle if one does not exist already.
            if(boundingRectangle == null)
            {
                // Make a new Rectangle
                boundingRectangle = new Rectangle();
                 
                // Give the rectangle a fill.
                boundingRectangle.Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6));

                // Add the rectangle to the MainCanvas.
                MainCanvas.Children.Add(boundingRectangle);

                // Give the rectangle a lower ZIndex than everything else on the main canvas
                // so it is not drawn on top of anything else
                Canvas.SetZIndex(boundingRectangle, int.MinValue );
            }

            // Get the bounding rect for the touchDevice
            Rect touchDeviceRect = touchDevice.GetBounds(this);
            Rect bounds = new Rect(touchDeviceRect.X, touchDeviceRect.Y, touchDeviceRect.Width, touchDeviceRect.Height);

            // Update the properties of boundingRectangle
            boundingRectangle.Height = bounds.Height;
            boundingRectangle.Width = bounds.Width;
            Canvas.SetLeft(boundingRectangle, bounds.Left);
            Canvas.SetTop(boundingRectangle, bounds.Top);

            // Hide the rectangle if the user does not want to view bounding rectangles
            boundingRectangle.Visibility = showBoundingRectangle ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Gets a string that describes the type of touch device on the surface
        /// </summary>
        /// <param name="touchDevice">the touch device to be examined</param>
        /// <returns>a string that describes the type of touch device</returns>
        private static string GetTouchDeviceTypeString(TouchDevice touchDevice)
        {
            if (touchDevice.GetTagData() != TagData.None)
            {
                return "Tag";
            }
            if (touchDevice.GetIsFingerRecognized())
            {
                return "Finger";
            }

            return "Blob";
        }
        
        /// <summary>
        /// Update the text description with the most recent property values. Position
        /// the textbox so that it does not go offscreen (outside parentGrid). Also
        /// position the connecting line between the touch device and the textbox.
        /// </summary>
        /// <param name="parentGrid">the container for this diagram-
        /// description text will not go outside of this container's bounds</param>
        /// <param name="touchDevice">the touch device to diagram</param>
        /// <param name="showTouchDeviceInfo">Whether or not the touch device info will be visible</param>
        private void UpdateDescription(Grid parentGrid, TouchDevice touchDevice, bool showTouchDeviceInfo)
        {
            // Show or hide the touchDevice info based on showTouchDeviceInfo
            Description.Visibility = showTouchDeviceInfo ? Visibility.Visible : Visibility.Hidden;
            ConnectingLine.Visibility = showTouchDeviceInfo ? Visibility.Visible : Visibility.Hidden;

            if (!showTouchDeviceInfo)
            {
                // Don't need to do the calculations if info isn't going to be shown
                return;
            }

            Point position = touchDevice.GetPosition(parentGrid);
            Rect bounds = new Rect(0, 0, parentGrid.ActualWidth, parentGrid.ActualHeight);
            // Determine where around the touchDevice the description should be displayed.
            // The default position is above and to the left.
            bool isAbove = true;
            bool isLeft = true;

            // Description text for tags is different than non-tags
            double descriptionXDistance;
            bool isTag = touchDevice.GetIsTagRecognized();

            if (isTag)
            {
                descriptionXDistance = tagDescriptionXDistance;
            }
            else
            {
                descriptionXDistance = nonTagDescriptionXDistance;
            }

            // Put description below touchDevice if default location is out of bounds.
            Rect upperLeftBounds = GetDescriptionBounds(position, isAbove, isLeft, descriptionXDistance, descriptionYDistance);
            if (upperLeftBounds.Top < bounds.Top)
            {
                isAbove = false;
            }

            // Put description to the right of the touchDevice if default location is out of bounds.
            if (upperLeftBounds.Left < bounds.Left)
            {
                isLeft = false;
            }

            // Calculate the final bounds that will be used for the textbox position
            // based on the updated isAbove and isLeft values.
            Rect finalBounds = GetDescriptionBounds(position, isAbove, isLeft, descriptionXDistance, descriptionYDistance);
            Canvas.SetLeft(Description, finalBounds.Left);
            Canvas.SetTop(Description, finalBounds.Top);

            // Set the justification of the type in the textbox based
            // on which side of the touchDevice the textbox is on.
            if(isLeft)
            {
                Description.TextAlignment = TextAlignment.Right;
            }
            else
            {
                Description.TextAlignment = TextAlignment.Left;
            }

            // Create the description string.
            StringBuilder descriptionText = new StringBuilder();
            descriptionText.AppendLine(String.Format(CultureInfo.InvariantCulture, "RecognizedTypes: {0}", GetTouchDeviceTypeString(touchDevice)));
            descriptionText.AppendLine(String.Format(CultureInfo.InvariantCulture, "Id: {0}", touchDevice.Id));

            // Use the "f1" format specifier to limit the amount of decimal positions shown.
            descriptionText.AppendLine(String.Format(CultureInfo.InvariantCulture, "X: {0}", position.X.ToString("f1", CultureInfo.InvariantCulture)));
            descriptionText.AppendLine(String.Format(CultureInfo.InvariantCulture, "Y: {0}", position.Y.ToString("f1", CultureInfo.InvariantCulture)));

            // Display "null" for Orientation if the touchDevice does not have an orientation value.
            string orientationString;
            double? touchDeviceOrientation = touchDevice.GetOrientation(parentGrid);
            if (touchDeviceOrientation == null)
            {
                orientationString = "null";
            }
            else
            {
                orientationString = ((double)touchDeviceOrientation).ToString("f1", CultureInfo.InvariantCulture);
            }
            descriptionText.AppendLine(String.Format(CultureInfo.InvariantCulture, "Orientation: {0}", orientationString));

            if (touchDevice.GetTagData() != TagData.None)
            {
                descriptionText.AppendLine("Schema: 0x" + touchDevice.GetTagData().Schema.ToString("x8", CultureInfo.InvariantCulture));
                descriptionText.AppendLine("Series:  0x" + touchDevice.GetTagData().Series.ToString("x16", CultureInfo.InvariantCulture));
                descriptionText.AppendLine("ExtendedValue: 0x" + touchDevice.GetTagData().ExtendedValue.ToString("x16", CultureInfo.InvariantCulture));
                descriptionText.AppendLine("Value:  0x" + touchDevice.GetTagData().Value.ToString("x16", CultureInfo.InvariantCulture));
            }

            // Update the description textbox.
            Description.Text = descriptionText.ToString();

            // Update the line that connects the touchDevice to the description textbox.
            double x2;
            if(isLeft)
            {
                x2 = finalBounds.Right;
            }
            else
            {
                x2 = finalBounds.Left;
            }
            // Position (X1,Y1) is the center of the touchDevice.
            // Position (X2,Y2) is the edge of the description text box.
            ConnectingLine.X1 = position.X;
            ConnectingLine.Y1 = position.Y;
            ConnectingLine.X2 = x2;
            ConnectingLine.Y2 = finalBounds.Top + finalBounds.Height * 0.5;
        }
         

        /// <summary>
        /// Calculate the bounds of the textbox for the specified location and quadrant.
        /// </summary>
        /// <param name="position">position of the touch device to diagram</param>
        /// <param name="isAbove">specifies if the requested bounds is above or below the touch device position</param>
        /// <param name="isLeft">specifies if the requested bounds is right or left of the touch device position</param>
        /// <param name="xDistance">the x distance from the touch evice position</param>
        /// <param name="yDistance">the y distance from the touch device position</param>
        /// <returns>the bounds at the specified location</returns>
        private Rect GetDescriptionBounds(Point position, bool isAbove, bool isLeft, double xDistance, double yDistance)
        {
            double left;
            if (isLeft)
            {
                left = position.X - xDistance - Description.Width;
            }
            else
            {
                left = position.X + xDistance;
            }
            double top;
            if (isAbove)
            {
                top = position.Y - yDistance - Description.Height;
            }
            else
            {
                top = position.Y + yDistance;
            }
            return new Rect(left, top, Description.Width, Description.Height);
        }
    }
}