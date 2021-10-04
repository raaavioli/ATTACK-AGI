using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace ItemCompare
{
    /// <summary>
    /// Represents an axis controlled by two TagVisualizations.
    /// Moving the visualizations causes the axis to move, rotate, and scale.
    /// </summary>
    /// <remarks>
    /// This class works by tracking the behavior of two TagVisualization objects
    /// and raising appropriate events when the visualizations gain tags, lose
    /// tags, or move. The main app can then use those events to control the
    /// placement and display of the ComparisonTable.
    /// 
    /// Typical usage:
    /// 1. Construct a VisualizationAxis
    /// 2. Set desired minimum length (determines lower bound on table width)
    /// 3. Set visualizations to be used as endpoints
    /// 4. Register for the Show, Hide, and Moved events
    /// 5. Use the Show and Hide handlers to show/hide the comparison table
    /// 6. Use the Moved event to update the comparison table's position
    /// </remarks>
    internal class VisualizationAxis
    {
        private TagVisualization visualization1;
        private TagVisualization visualization2;
        private bool wasPreviouslyActive;
        private double minimumLength;
        private double length;
        private double orientation; // visualization axis orientation in degrees.
        private bool wasPreviouslyInverted;
        protected Point center;
        UIElement manipulationContainer;

        /// <summary>
        /// Raised when the axis should be shown.
        /// </summary>
        public event EventHandler Show;

        /// <summary>
        /// Raised when the axis should be hidden.
        /// </summary>
        public event EventHandler Hide;

        /// <summary>
        /// Raised when the axis moves.
        /// </summary>
        public event EventHandler Moved;

        /// <summary>
        /// Sets the visualization that controls axis
        /// endpoint #1.
        /// </summary>
        /// <param name="visualization"></param>
        public void SetVisualization1(TagVisualization visualization)
        {
            SetVisualization(visualization, ref visualization1);
        }

        /// <summary>
        /// Sets the visualization that controls axis
        /// endpoint #2.
        /// </summary>
        /// <param name="visualization"></param>
        public void SetVisualization2(TagVisualization visualization)
        {
            SetVisualization(visualization, ref visualization2);
        }

        /// <summary>
        /// Sets the minimum allowed length of the axis.
        /// </summary>
        /// <param name="minimum"></param>
        public void SetMinimumLength(double minimum)
        {
            minimumLength = minimum;
        }

        /// <summary>
        /// Gets whether the axis is "active" (i.e. the graphic associated with
        /// it should be displayed).
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (visualization1 != null)
                    && (visualization2 != null)
                    && (visualization1.TrackedTouch != null)
                    && (visualization2.TrackedTouch != null);
            }
        }


        /// <summary>
        /// Gets the length of the axis.
        /// </summary>
        /// <remarks>
        /// NaN if VisualizationAxis.IsActive is false.
        /// </remarks>
        public double Length
        {
            get { return Math.Max(length, minimumLength); }
        }

        /// <summary>
        /// Gets the orientation of the axis.
        /// </summary>
        /// <remarks>
        /// This is an angle between -180 degrees and 180 degrees.
        /// NaN if VisualizationAxis.IsActive is false.
        /// </remarks>
        public double Orientation
        {
            get
            {
                return IsInverted
                    ? NormalizeOrientation(orientation + 180)
                    : orientation;
            }
        }

        /// <summary>
        /// Gets the center point of the axis.
        /// </summary>
        /// <remarks>
        /// This is the center point of the axis.
        /// (NaN,NaN) if VisualizationAxis.IsActive is false.
        /// </remarks>
        public Point Center
        {
            get { return center; }
        }

        /// <summary>
        /// Gets whether the items in the axis are inverted (visualization 1 is on the right)
        /// or not (visualization 1 is on the left). Left and Right are relative to the
        /// visualization axis.
        /// </summary>
        public bool IsInverted
        {
            get 
            {
                if ((visualization1 == null)
                    || (visualization2 == null)
                    || (visualization1.Visualizer == null)
                    || (visualization2.Visualizer == null))
                {
                    // not enough information
                    return false;
                }

                // get the angle of the visualization objects relative to the 
                // visualization axis orientation.
                double angle1 = visualization1.Orientation - orientation;
                double angle2 = visualization2.Orientation - orientation;

                const double degreesToRadiansFactor = (2 * Math.PI) / 360;

                // Now find if the addition of both components perpendicular to 
                // the visualization axis is positive or negative to determine if 
                // the comparison table should be inverted or not.
                double dominantDirection = Math.Cos(angle1 * degreesToRadiansFactor) +
                                           Math.Cos(angle2 * degreesToRadiansFactor);

                return dominantDirection < 0;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>The container within which manipulations take place.</remarks>
        public VisualizationAxis(UIElement container)
        {
            ClearPosition();

            manipulationContainer = container;
            manipulationContainer.IsManipulationEnabled = true;
            manipulationContainer.ManipulationDelta += OnManipulationDelta;
            manipulationContainer.TouchDown += OnTouchDown;
        }

        /// <summary>
        /// Sets one of our endpoints to the specified visualization.
        /// </summary>
        /// <param name="newVisualization"></param>
        /// <param name="currentVisualization"></param>
        private void SetVisualization(
            TagVisualization newVisualization,
            ref TagVisualization currentVisualization)
        {
            // Unhook events from the previous visualization,
            // and hook up the new one.
            if (currentVisualization != null)
            {
                Unregister(currentVisualization);
            }
            currentVisualization = newVisualization;
            if (currentVisualization != null)
            {
                Register(currentVisualization);
            }
            UpdateVisibility();
        }

        /// <summary>
        /// Hook up appropriate events on a visualization that
        /// controls one of our endpoints.
        /// </summary>
        /// <param name="visualization"></param>
        private void Register(TagVisualization visualization)
        {
            visualization.GotTag += OnVisualizationGotOrLostTag;
            visualization.LostTag += OnVisualizationGotOrLostTag;
            visualization.Unloaded += OnVisualizationUnloaded;
            visualization.Moved += OnVisualizationMoved;
        }

        /// <summary>
        /// Unhook events from a visualization that we're no longer
        /// using as an endpoint.
        /// </summary>
        /// <param name="visualization"></param>
        private void Unregister(TagVisualization visualization)
        {
            visualization.GotTag -= OnVisualizationGotOrLostTag;
            visualization.LostTag -= OnVisualizationGotOrLostTag;
            visualization.Unloaded -= OnVisualizationUnloaded;
            visualization.Moved -= OnVisualizationMoved;
        }

        /// <summary>
        /// Returns the orientation as a value between -180 degrees and 180 degrees.
        /// The equivalent of a mod function on a double.
        /// </summary>
        /// <param name="orientation">An orientation in degrees</param>
        /// <returns>The orientation as a value between -180 degrees and 180 degrees.</returns>
        private static double NormalizeOrientation(double orientation)
        {
            double normalizedOrientation = orientation;

            // While this will work for any angle, we don't really expect to
            // go through the while loop more than once because the orientation is
            // normalized every time it changes.
            while (normalizedOrientation < -180 || normalizedOrientation > 180)
            {
                if (normalizedOrientation > 180)
                {
                    normalizedOrientation -= 360;
                }
                else if (normalizedOrientation < -180)
                {
                    normalizedOrientation += 360;
                }
            }

            return normalizedOrientation;
        }

        /// <summary>
        /// Here when a visualization gets or loses a tag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisualizationGotOrLostTag(object sender, RoutedEventArgs e)
        {
            UpdateVisibility();
        }

        /// <summary>
        /// Updates the orientation of the comparison table if necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// We need to make sure the orientation of the comparison table is updated if 
        /// its orientation is supposed to change.
        /// The manipulator processor does not produce "deltas" if the tags are in the exact same
        /// position and just rotate. This is an infrequent scenario with an actual physical object, 
        /// since rotating it will almost always result in some small amount of translational motion.  
        /// </remarks>
        void OnVisualizationMoved(object sender, TagVisualizerEventArgs e)
        {
            bool isInverted = IsInverted;

            // Raise the Moved event only if VisualizationAxis.IsActive is true (otherwise some
            // properties are invalidated), and an update is required.
            if (Moved != null 
                && wasPreviouslyInverted != isInverted
                && IsActive)
            {
                wasPreviouslyInverted = isInverted;
                Moved(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Here when a visualization that we're tracking is unloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisualizationUnloaded(object sender, RoutedEventArgs e)
        {
            // The visualization has been unloaded from the visualizer, so
            // we should stop tracking it.
            Unregister((TagVisualization)sender);
        }

        /// <summary>
        /// Checks the "active" status of the axis and fires the Show and Hide events as needed.
        /// </summary>
        private void UpdateVisibility()
        {
            if (!wasPreviouslyActive && IsActive)
            {
                // We're becoming active.
                wasPreviouslyActive = true;
                SetPosition();

                Manipulation.AddManipulator(manipulationContainer, visualization1);
                Manipulation.AddManipulator(manipulationContainer, visualization2);

                if (Show != null)
                {
                    Show(this, EventArgs.Empty);
                }
            }
            else if (wasPreviouslyActive && !IsActive)
            {
                // We're becoming inactive.
                if (Hide != null)
                {
                    Hide(this, EventArgs.Empty);
                }

                Manipulation.CompleteManipulation(manipulationContainer);

                ClearPosition();
                wasPreviouslyActive = false;
            }
        }

        /// <summary>
        /// Initializes positioning information.
        /// </summary>
        private void SetPosition()
        {
            Vector direction = visualization2.Center - visualization1.Center;
            length = direction.Length;
            orientation = (length < 1) ? 0 : Vector.AngleBetween(new Vector(1, 0), direction);
            center.X = 0.5 * (visualization1.Center.X + visualization2.Center.X);
            center.Y = 0.5 * (visualization1.Center.Y + visualization2.Center.Y);
        }

        /// <summary>
        /// Clears positioning information.
        /// </summary>
        private void ClearPosition()
        {
            length = double.NaN;
            orientation = double.NaN;
            center.X = double.NaN;
            center.Y = double.NaN;
        }

        /// <summary>
        /// Here when we get a manipulation delta event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            center += e.DeltaManipulation.Translation;

            // e.RotationalDelta gives a good approximation of the rotation applied.
            // In this specific case an approximation is not sufficient since any angle variation
            // between the comparison table and the position of the tags is easily noticed by the user.
            // Since the two visualizations define the angle of the comparison table, we can calculate 
            // the exact rotation of the comparison table directly from the position of the visualizations.
            Vector direction = visualization2.Center - visualization1.Center;
            length = direction.Length;
            orientation = NormalizeOrientation((length < 1) ? 0 : Vector.AngleBetween(new Vector(1, 0), direction));

            if (Moved != null)
            {
                Moved(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Here when a new touch arrives.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            // If there's already a manipulation going, then there are already
            // two items being compared. In that case, don't allow another
            // touch to join in the manipulation.
            if (Manipulation.IsManipulationActive(manipulationContainer))
            {
                e.Handled = true;
            }
        }
    }
}
