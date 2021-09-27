using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace ItemCompare
{
    /// <summary>
    /// The main window for the application.
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        private const string itemDataFile = "ItemData.xml";
        private const double tableHorizontalMargin = 100;
        private readonly Items.ItemData itemData;
        private readonly VisualizationAxis visualizationAxis;
        private readonly MatrixTransform tableTransform;
        private readonly DispatcherTimer hideTableTimer;
        private bool wasPreviouslyInverted;

        private Items.Item item1;
        private Items.Item item2;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            // Plug our item data into our visualizations
            itemData = new Items.ItemData(itemDataFile);
            ComparedItem1.ItemData = itemData;
            ComparedItem2.ItemData = itemData;
            ComparedItem1.GotTag += OnComparedItemGotTag;
            ComparedItem2.GotTag += OnComparedItemGotTag;

            // Set up our visualization axis to track the visualizations
            // Use "RootGrid" as a reference container for the coordinate space to work
            // with 
            visualizationAxis = new VisualizationAxis(this.RootGrid);
            visualizationAxis.SetVisualization1(ComparedItem1);
            visualizationAxis.SetVisualization2(ComparedItem2);
            visualizationAxis.SetMinimumLength(Table.MinWidth - 2 * tableHorizontalMargin);
            visualizationAxis.Show += OnShowTable;
            visualizationAxis.Hide += OnHideTable;
            visualizationAxis.Moved += OnMoveTable;

            // Set up a render transform to use with our comparison table
            tableTransform = new MatrixTransform(Matrix.Identity);
            Table.RenderTransform = tableTransform;
            Table.RenderTransformOrigin = new Point(0.5, 0.5);

            // Prepare a timer to hide our table when needed
            hideTableTimer = new DispatcherTimer();
            hideTableTimer.Interval = TimeSpan.FromMilliseconds(300);
            hideTableTimer.Tick += OnHideTableTick;

            ComparisonCanvas.Loaded += OnComparisonCanvasLoaded;

            // Check the hardware, and modify the UI based on the supported capabilities.
            bool areTagsSupported = InteractiveSurface.PrimarySurfaceDevice.IsTagRecognitionSupported;
            if (!areTagsSupported)
            {
                TagsNotSupportedText.Visibility = Visibility.Visible;
                TagInfoText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Here when we need to show our item comparison table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowTable(object sender, EventArgs e)
        {
            // Stop our "hide" timer, in case it's already running
            hideTableTimer.Stop();

            // Set the contents of the table
            item1 = itemData.Find((byte)ComparedItem1.VisualizedTag.Value);
            item2 = itemData.Find((byte)ComparedItem2.VisualizedTag.Value);

            if (visualizationAxis.IsInverted)
            {
                Items.Item swap = item1;
                item1 = item2;
                item2 = swap;
            }
            Table.SetItems(itemData.Properties, item1, item2);

            // Hide the individual visualizations for our compared
            // objects, since we'll have the table to work with
            HideElement(ComparedItem1);
            HideElement(ComparedItem2);

            // Set the table's position and display it
            UpdateTablePosition();
            ShowElement(Table);
        }

        /// <summary>
        /// Here when we need to hide our item comparison table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHideTable(object sender, EventArgs e)
        {
            // We don't hide the table right away; instead, we give it a brief
            // "grace period" so that if there's a momentary loss of one of
            // our tagged touch devices, the table doesn't flicker.
            hideTableTimer.Start();
        }

        /// <summary>
        /// Here when our "grace period" has expired and we actually need
        /// to hide the table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHideTableTick(object sender, EventArgs e)
        {
            hideTableTimer.Stop();

            // Table is no longer required
            HideElement(Table);

            // Show the individual visualizations for our compared
            // objects, since the table is going away
            ShowElement(ComparedItem1);
            ShowElement(ComparedItem2);
        }

        /// <summary>
        /// Here when we need to move our item comparison table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMoveTable(object sender, EventArgs e)
        {
            UpdateTablePosition();
        }

        /// <summary>
        /// Update the positioning of our comparison table.
        /// </summary>
        private void UpdateTablePosition()
        {
            // Don't do anything if VisualizationAxis is not active.
            if (visualizationAxis.IsActive)
            {
                // Adjust the table's width and height appropriately based on the distance
                // between the items.
                Debug.Assert(!Double.IsNaN(visualizationAxis.Length));
                Table.Width = 2 * tableHorizontalMargin + visualizationAxis.Length;
                Table.Height = visualizationAxis.Length;

                if (wasPreviouslyInverted != visualizationAxis.IsInverted)
                {
                    wasPreviouslyInverted = !wasPreviouslyInverted;

                    // flip table columns order.
                    Items.Item swap = item1;
                    item1 = item2;
                    item2 = swap;

                    Table.SetItems(itemData.Properties, item1, item2);
                }

                // Set its orientation and position
                Matrix matrix = Matrix.Identity;
                Debug.Assert(!Double.IsNaN(visualizationAxis.Orientation));
                matrix.Rotate(visualizationAxis.Orientation);
                matrix.Translate(-0.5 * Table.ActualWidth, -0.5 * Table.ActualHeight);
                Debug.Assert(!Double.IsNaN(visualizationAxis.Center.X) && !Double.IsNaN(visualizationAxis.Center.Y));
                matrix.Translate(visualizationAxis.Center.X, visualizationAxis.Center.Y);
                tableTransform.Matrix = matrix;
            }
        }

        /// <summary>
        /// Here when one of our compared items gets a tag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComparedItemGotTag(object sender, RoutedEventArgs e)
        {
            TagVisualization visualization = (TagVisualization)sender;
            ByteTagDefinition definition = ByteTagDefinition.Find(visualization.VisualizedTag);
            if (definition == null)
            {                
                visualization.PhysicalCenterOffsetFromTag = new Vector(2.5, -0.5);
                visualization.OrientationOffsetFromTag = -90;
            }
            else
            {
                visualization.PhysicalCenterOffsetFromTag = definition.PhysicalCenterOffsetFromTag;
                visualization.OrientationOffsetFromTag = definition.OrientationOffsetFromTag;
            }
        }

        /// <summary>
        /// Causes the specified element to begin fading in.
        /// </summary>
        /// <param name="element"></param>
        private static void ShowElement(UIElement element)
        {
            if (element.Visibility != Visibility.Visible)
            {
                element.Opacity = 0;
                element.Visibility = Visibility.Visible;
            }

            // get the initial opacity
            double fromOpacity = element.Opacity;

            // cancel any previous animation
            element.BeginAnimation(UIElement.OpacityProperty, null);

            // start our new animation
            element.Opacity = 1.0;
            DoubleAnimation fadeIn = new DoubleAnimation(
                fromOpacity,
                1.0,
                new Duration(TimeSpan.FromMilliseconds(200)),
                FillBehavior.Stop);
            fadeIn.Completed += delegate(object sender, EventArgs e)
            {
                element.Visibility = Visibility.Visible;
            };

            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Causes the specified element to begin fading out.
        /// </summary>
        /// <param name="element"></param>
        private static void HideElement(UIElement element)
        {
            // get the initial opacity
            double fromOpacity = element.Opacity;

            // cancel any previous animation
            element.BeginAnimation(UIElement.OpacityProperty, null);

            // start our new animation
            DoubleAnimation fadeOut = new DoubleAnimation(
                fromOpacity,
                0.0,
                new Duration(TimeSpan.FromMilliseconds(200)),
                FillBehavior.HoldEnd);
            fadeOut.Completed += delegate(object sender, EventArgs e)
            {
                element.Visibility = Visibility.Hidden;
            };
            element.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        /// <summary>
        /// Here the first time the layout is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnComparisonCanvasLoaded(object sender, RoutedEventArgs e)
        {
            // Place our initial visualizations the way we want them
            double CenterOffset = 45.0;
            double ItemMargin = 116.0;
            double x1 = RootGrid.ActualWidth / 2 - ItemMargin + CenterOffset;
            double x2 = RootGrid.ActualWidth / 2 + ItemMargin + ComparedItem2.TagDownArea.ActualWidth + CenterOffset;
            double y = 0.5 * RootGrid.ActualHeight;

            TagVisualizerCanvas.SetCenter(ComparedItem1, new Point(x1, y));
            TagVisualizerCanvas.SetCenter(ComparedItem2, new Point(x2, y));
            TagVisualizerCanvas.SetOrientation(ComparedItem1, 0.0);
            TagVisualizerCanvas.SetOrientation(ComparedItem2, 0.0);

        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
    }
}