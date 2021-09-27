using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Input;
using System.Linq;


namespace DataVisualizer
{
    /// <summary>
    /// This UserControl is designed to display diagrams of touch devices, demonstrating
    /// their available properties.
    /// </summary>
    public partial class TouchDeviceDataVisualizer : UserControl
    {
        /// <summary>
        /// The diagrams Dictionary is used to keep track of TouchDeviceDiagrams and the TouchDevice
        /// each diagram is associated with.
        /// </summary>
        private Dictionary<TouchDevice, TouchDeviceDiagram> diagrams;

        /// <summary>
        /// Constructor
        /// </summary>
        public TouchDeviceDataVisualizer()
        {
            diagrams = new Dictionary<TouchDevice, TouchDeviceDiagram>();
            
            InitializeComponent();
            UpdateStatistics();

            InteractiveSurface.PrimarySurfaceDevice.Changed += OnPrimarySurfaceDeviceChanged;
            InteractiveSurface.PrimarySurfaceDevice.TiltChanged += OnTiltChanged;
            //Event handlers for TouchDown, TouchMove and LostTouchCapture are
            //added in TouchDeviceDataVisualizer.xaml.
        }

        /// <summary>
        /// This handler is called when the primary surface device changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPrimarySurfaceDeviceChanged(object sender, DeviceChangedEventArgs e)
        {
            UpdateStatistics();
        }

        /// <summary>
        /// This handler is called when the tilt angle on the primary surface
        /// device changes.
        /// </summary>
        /// <param name="sender">the object raising the TiltChanged event</param>
        /// <param name="args">information about this TiltChanged event</param>
        private void OnTiltChanged(object sender, System.EventArgs args)
        {
            UpdateStatistics();
        }

        /// <summary>
        /// This handler is called when a TouchDevice is first recognized.
        /// </summary>
        /// <param name="sender">the element raising the TouchDownEvent</param>
        /// <param name="args">information about this TouchDownEvent</param>
        private void OnTouchDown(object sender, TouchEventArgs args)
        {
            // Add a diagram to the main window to represent the new touch device
            AddDiagram(args.TouchDevice);

            // Fill the diagram with information about the touch device and display it
            UpdateDiagram(args.TouchDevice);

            //Acquire TouchDevice capture so ActiveArea will receive all events for this TouchDevice.
            //The LostTouchCapture event is used here for notification that this TouchDevice has been
            //completely removed. LostTouchCapture is raised after the TouchUp event.
            //The TouchDevice will still be counted when calling UIElement.TouchesOver even when both the
            //TouchUp and LostTouchCapture events are raised. But when calling UIElement.TouchesCaptured,
            //the TouchDevice will not be counted when the LostTouchCapture event is raised, while it
            //will be when the Touchup event is raised. Therefore to update the statistics, it is more
            //appropriate to use UIElement.Captured during the LostTouchCapture event.
            args.TouchDevice.Capture(ActiveArea);
            UpdateStatistics();
        }

        /// <summary>
        /// This handler is called when any of a TouchDevice's properties have changed.
        /// </summary>
        /// <param name="sender">the element raising the TouchMoveEvent</param>
        /// <param name="args">information about this TouchMoveEvent</param>
        private void OnTouchMove(object sender, TouchEventArgs args)
        {
            UpdateDiagram(args.TouchDevice);
        }

        /// <summary>
        /// This handler is called when TouchDevice capture is lost.
        /// </summary>
        /// <param name="sender">the element raising the LostTouchCapture</param>
        /// <param name="args">information about this LostTouchCapture event</param>
        private void OnLostTouchCapture(object sender, TouchEventArgs args)
        {
            RemoveDiagram(args.TouchDevice);
            UpdateStatistics();
        }

        /// <summary>
        /// Called when any of the checkboxes to show/hide the touch device info elements is changed
        /// </summary>
        /// <param name="sender">the element raising the checked/unchecked event</param>
        /// <param name="e">information about this checked/unchecked event</param>
        void DisplayOptionsChanged(object sender, RoutedEventArgs e)
        {
            foreach (TouchDevice touchDevice in diagrams.Keys)
            {
                UpdateDiagram(touchDevice);
            }
        }

        /// <summary>
        /// Create a TouchDeviceDiagram and add it to the diagrams Dictionary and the DiagramContainerGrid.
        /// </summary>
        /// <param name="touchDevice">the touch device to diagram</param>
        private void AddDiagram(TouchDevice touchDevice)
        {
            TouchDeviceDiagram diagram = new TouchDeviceDiagram();
            diagrams.Add(touchDevice, diagram);
            DiagramContainerGrid.Children.Add(diagram);
        }

        /// <summary>
        /// Update a TouchDeviceDiagram.
        /// </summary>
        /// <param name="touchDevice"></param>
        private void UpdateDiagram(TouchDevice touchDevice)
        {
            TouchDeviceDiagram diagram;
            if (diagrams.TryGetValue(touchDevice, out diagram))
            {
                diagram.Update(DiagramContainerGrid,
                               touchDevice, 
                               (bool)ShowBoundingRectCheckBox.IsChecked,
                               (bool)ShowTouchDeviceInfoCheckBox.IsChecked);
            }
        }

        /// <summary>
        /// Remove a TouchDeviceDiagram from the diagrams Dictionary and the DiagramContainerGrid.
        /// </summary>
        /// <param name="touchDevice"></param>
        private void RemoveDiagram(TouchDevice touchDevice)
        {
            TouchDeviceDiagram diagram;
            if (diagrams.TryGetValue(touchDevice, out diagram))
            {
                diagrams.Remove(touchDevice);
                DiagramContainerGrid.Children.Remove(diagram);
            }
        }

        /// <summary>
        /// Updates the text description of touch devices over the ActiveArea Rectangle.
        /// </summary>
        private void UpdateStatistics()
        {
            //Get all of the touch devices captured to the ActiveArea Rectangle.
            //The ActiveArea Rectangle is defined in XAML.
            IEnumerable<TouchDevice> touchDevicesCaptured = ActiveArea.TouchesCaptured;

            int tiltAngle = (int)InteractiveSurface.PrimarySurfaceDevice.TiltAngle;
            int totalTouchDevices = touchDevicesCaptured.Count();
            int blobCount = 0;
            int tagCount = 0;
            int fingerCount = 0;
            foreach (TouchDevice touchDevice in touchDevicesCaptured)
            {
                if (touchDevice.GetIsTagRecognized())
                {
                    tagCount++;
                }
                else if (touchDevice.GetIsFingerRecognized())
                {
                    fingerCount++;
                }
                else
                {
                    blobCount++;
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total Fingers {0}", fingerCount));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total Blobs: {0}", blobCount));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total Tags: {0}", tagCount));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total TouchDevices: {0}", totalTouchDevices));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Tilt Angle: {0}", tiltAngle));
            Statistics.Text = sb.ToString();
        }
    }
}