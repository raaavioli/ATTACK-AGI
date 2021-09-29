using System;
using System.Collections.Generic;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Represents a scroll viewer state machine as part of the Model-View-Controller (MVC) pattern. 
    /// The basic purpose of the class is to provide a wrapper for the <strong>ScrollAdapter</strong> class.  
    /// </summary>
    /// <remarks><note type="caution"> In this API, state machine represents the Model of part 
    /// of the MVC pattern.</note>
    /// </remarks>
    public class ScrollViewerStateMachine : UIElementStateMachine
    {
        // This class is just a pass through to ScrollAdapter, which is also used in 
        // StateMachines like ListBox, ComboBox, etc.  This was done because it is 
        // cumbersome for a developer consuming this API to implement both hitTesting and
        // visuals for ScrollViewer, ScrollBar and one of these other classes like ListBox, 
        // when all they want is ListBoxStateMachine behavior.

        private readonly ScrollAdapter scrollAdapter;
        
        /// <summary>
        /// Creates a new <strong><see cref="ScrollViewerStateMachine"/></strong> instance
        /// with the specified parameters.
        /// </summary>
        /// <param name="controller">The <strong>UIController</strong> object that dispatches hit testing.</param>
        /// <param name="numberOfPixelsInHorizontalAxis">
        /// The number of pixels that this control occupies horizontally. 
        /// For more information, see 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInHorizontalAxis">NumberOfPixelsInHorizontalAxis</see></strong>.
        /// </param>
        /// <param name="numberOfPixelsInVerticalAxis">
        /// The number of pixels that this control occupies vertically. 
        /// For more information, see 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInVerticalAxis">NumberOfPixelsInVerticalAxis</see></strong>.
        /// </param>
        public ScrollViewerStateMachine(UIController controller, int numberOfPixelsInHorizontalAxis, int numberOfPixelsInVerticalAxis)
            : base(controller, numberOfPixelsInHorizontalAxis, numberOfPixelsInVerticalAxis)
        {
            scrollAdapter = new ScrollAdapter(controller, this);
            scrollAdapter.ViewportChanged += new EventHandler(OnViewportChanged);
        }

        /// <summary>
        /// Gets the type of <strong><see cref="CoreInteractionFramework.ScrollViewerHitTestDetails"/></strong>.
        /// </summary>
        /// <returns>
        /// Type as <strong>typeof(ScrollViewerHitTestDetails)</strong> of this 
        /// <strong><see cref="ScrollViewerStateMachine"/></strong> object.
        /// </returns>
        public override Type TypeOfHitTestDetails
        {
            get
            {
                // The type of IHitTestDetails for ScrollViewerStateMachine.
                return typeof(ScrollViewerHitTestDetails);
            }
        }

        /// <summary>
        /// Gets or sets the horizontal 
        /// <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// A <strong>ScrollBarStateMachine</strong> object as this <strong>scrollAdapter.HorizontalScrollBarStateMachine</strong>.
        /// </returns>
        public ScrollBarStateMachine HorizontalScrollBarStateMachine
        {
            get { return scrollAdapter.HorizontalScrollBarStateMachine; }
            set { scrollAdapter.HorizontalScrollBarStateMachine = value; }
        }

        /// <summary>
        /// Gets or sets the vertical <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// A <strong>ScrollBarStateMachine</strong> as this <strong>scrollAdapter.VerticalScrollBarStateMachine</strong>.
        /// </returns>
        public ScrollBarStateMachine VerticalScrollBarStateMachine
        {
            get { return scrollAdapter.VerticalScrollBarStateMachine; }
            set { scrollAdapter.VerticalScrollBarStateMachine = value; }
        }


        /// <summary>
        /// Gets or sets the vertical normalized height of the viewport 
        /// that is associated with this 
        /// <strong><see cref="ScrollViewerStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// The vertical viewport size of this <strong>ScrollViewerStateMachine</strong> object.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ViewportSize",
            Justification = "The intent is to use two words")]
        public float VerticalViewportSize
        {
            get { return scrollAdapter.VerticalViewportSize; }
            set { scrollAdapter.VerticalViewportSize = value; }
        }

        /// <summary>
        /// Gets or sets the horizontal normalized width of the viewport 
        /// that is associated with this <strong><see cref="ScrollViewerStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// The horizontal viewport size of this <strong>ScrollViewerStateMachine</strong> object.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ViewportSize",
            Justification = "The intent is to use two words")]
        public float HorizontalViewportSize
        {
            get { return scrollAdapter.HorizontalViewportSize; }
            set { scrollAdapter.HorizontalViewportSize = value; }
        }


        /// <summary>
        /// Gets or sets the starting normalized vertical coordinate 
        /// of the viewport that is associated with this 
        /// <strong><see cref="ScrollViewerStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// The vertical viewport start position of this <strong>ScrollViewerStateMachine</strong> object.
        /// </returns>
        public float VerticalViewportStartPosition
        {
            get { return scrollAdapter.VerticalViewportStartPosition; }
            set { scrollAdapter.VerticalViewportStartPosition = value; }
        }

        /// <summary>
        /// Gets or sets the starting normalized horizontal coordinate 
        /// of the viewport that is associated with this 
        /// <strong><see cref="ScrollViewerStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// The horizontal viewport start position of this <strong>ScrollViewerStateMachine</strong> object.
        /// </returns>
        public float HorizontalViewportStartPosition
        {
            get { return scrollAdapter.HorizontalViewportStartPosition; }
            set { scrollAdapter.HorizontalViewportStartPosition = value; }
        }

        /// <summary>
        /// Gets or sets the vertical elasticity (left elastic margin) 
        /// of this <strong><see cref="ScrollViewerStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// The vertical elasticity of this <strong>ScrollViewerStateMachine</strong> object.
        /// </returns>
        public float VerticalElasticity
        {
            get { return scrollAdapter.VerticalElasticity; }
            set { scrollAdapter.VerticalElasticity = value; }
        }

        /// <summary>
        /// Gets or sets the horizontal elasticity (right elastic margin) 
        /// of this <strong><see cref="ScrollViewerStateMachine"/></strong> object.
        /// </summary>
        /// <returns>
        /// The horizontal elasticity of this <strong>ScrollViewerStateMachine</strong> object.
        /// </returns>
        public float HorizontalElasticity
        {
            get { return scrollAdapter.HorizontalElasticity; }
            set { scrollAdapter.HorizontalElasticity = value; }
        }

        /// <summary>
        /// Gets a value that determines whether the viewport has changed. 
        /// </summary>
        /// <returns><strong>true</strong> if any of the viewport properties changed since updating the 
        /// controller; otherwise, <strong>false</strong>.
        /// </returns>
        public bool GotViewportChange
        {
            get { return scrollAdapter.GotViewportChange; }
        }

 
        /// <summary>
        /// Gets a value that determines if scrolling is 
        /// occurring. 
        /// </summary>
        /// <returns>
        /// <strong>true</strong> if currently scrolling; otherwise, <strong>false</strong>.
        /// </returns>
        public bool IsScrolling
        {
            get { return scrollAdapter.IsScrolling; }
        }

        /// <summary>
        /// Occurs when any of the viewport properties change.
        /// </summary>
        public event EventHandler ViewportChanged;

        /// <summary>
        /// Called when any of the viewport properties change.
        /// </summary>
        private void OnViewportChanged(object sender, EventArgs e)
        {
            EventHandler temp = ViewportChanged;

            if (temp != null)
            {
                temp(this, e);
            }
        }

        /// <summary>
        /// Scrolls the viewport 1 page towards the top.
        /// </summary>
        public void PageUp()
        {
            scrollAdapter.PageUp();
        }

        /// <summary>
        /// Scrolls the viewport 1 page towards the bottom.
        /// </summary>
        public void PageDown()
        {
            scrollAdapter.PageDown();
        }

        /// <summary>
        /// Scrolls the viewport 1 page towards the left.
        /// </summary>
        public void PageLeft()
        {
            scrollAdapter.PageLeft();
        }

        /// <summary>
        /// Scrolls the viewport 1 page towards the right.
        /// </summary>
        public void PageRight()
        {
            scrollAdapter.PageRight();
        }

        /// <summary>
        /// Scrolls the viewport to the specified values. 
        /// </summary>
        /// <param name="viewportTopPosition">The y-coordinate to scroll to.</param>
        /// <param name="viewportLeftPosition">The x-coordinate to scroll to.</param>
        public void ScrollTo(float viewportTopPosition, float viewportLeftPosition)
        {
            scrollAdapter.ScrollTo(viewportTopPosition, viewportLeftPosition);
        }

        /// <summary>
        /// Stops the thumb from scrolling if it is currently being flicked.
        /// </summary>
        public void StopFlick()
        {
            scrollAdapter.StopFlick();
        }

        /// <summary>
        /// Handles the <strong>TouchDown</strong> event.
        /// </summary>
        /// <param name="touchEvent">The container for the touch that the event is about.</param>
        protected override void OnTouchDown(TouchTargetEvent touchEvent)
        {
            base.OnTouchDown(touchEvent);

            scrollAdapter.ProcessTouchDown(touchEvent);
        }

        /// <summary>
        /// Handles the <strong>TouchMoved</strong> event.
        /// </summary>
        /// <param name="touchEvent">The container for the touch that the event is about.</param>
        protected override void OnTouchMoved(TouchTargetEvent touchEvent)
        {
            base.OnTouchMoved(touchEvent);

            scrollAdapter.ProcessTouchMoved(touchEvent);
        }

        /// <summary>
        /// Processes all the manipulator changes in one badge.
        /// </summary>
        protected override void  OnUpdated(Queue<TouchTargetEvent> orderTouches)
        {
            base.OnUpdated(orderTouches);

            // Process the manipulator changes in a single batch.
            scrollAdapter.ProcessManipulators();
        }

        /// <summary>
        /// Handles the <strong>TouchUp</strong> event.
        /// </summary>
        /// <param name="touchEvent">The container for the touch that the event is about.</param>
        protected override void OnTouchUp(TouchTargetEvent touchEvent)
        {
            base.OnTouchUp(touchEvent);

            scrollAdapter.ProcessTouchUp(touchEvent);
        }

        /// <summary>
        /// Handles the <strong>ResetState</strong> event.
        /// </summary>
        /// <param name="sender">A <strong>UIController</strong> object.</param>
        /// <param name="e">Empty.</param>
        protected override void OnResetState(object sender, EventArgs e)
        {
            scrollAdapter.ProcessResetState(sender, e);
        }
    }
}
