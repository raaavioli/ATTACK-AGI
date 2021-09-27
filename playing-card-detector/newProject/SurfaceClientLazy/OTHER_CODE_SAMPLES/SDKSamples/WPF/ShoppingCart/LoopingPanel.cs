using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Controls.Primitives;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Input;

namespace ShoppingCart
{
    /// <summary>
    /// This class provides "looping list" functionality for a SurfaceListBox or a SurfaceScrollViewer.
    /// 
    /// Typical list scrolling is implemented by moving a viewport left and right (or up and down)
    /// against an unmoving list of child items (usually ListBoxItems.) all contained in an extent
    /// that is sized to the sum of the sizes of the children items
    /// 
    /// Looping scrolling expands on the standard scrolling technique. The extent that contains the 
    /// items is very large, more than large enough to contain all the items many times over. The
    /// viewport and the items begin centered in the extent. As the viewport is moved around the 
    /// extent, it's position is constantly compared to the edges of the secion of the extent where
    /// the items are laid out. As the viewport approaches the edge of the items, items from the far
    /// edge are moved to the near edge so there are always items on both sides of the viewport.
    /// 
    /// </summary>
    public class LoopingPanel: Canvas, ISurfaceScrollInfo
    {
        #region Private Members

        private ScrollViewer owner;
        private Size extent = new Size(0, 0);
        private Size viewport = new Size(0, 0);
        private Point viewportOffset = new Point();
        private bool viewportPositionDirty;
        private bool horizontalScrollAllowed;
        private TranslateTransform transform = new TranslateTransform();
        private int firstItem;
        private double firstItemOffset;
        private double previousOffset = double.NaN;
        private double totalContentWidth;
        private bool isFlicking;

        #endregion

        #region Initalization

        /// <summary>
        /// Constructor
        /// </summary>
        public LoopingPanel()
        {

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            // Set the render transform that will be used to position the viewport
            this.RenderTransform = transform;
        }

        #endregion

        #region Input

        /// <summary>
        /// Called when an input device is removed.
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="args">The arguments for the event</param>
        private void LostInputCapture(object sender, RoutedEventArgs args)
        {
            UIElement scrollViewer = sender as UIElement;

            if (scrollViewer != null && !scrollViewer.GetAreAnyInputDevicesCaptured())
            {
                previousOffset = double.NaN;
            }
        }

        #endregion

        #region IScrollInfo Members

        /// <summary>
        /// Gets or sets a value that determines if the content can be scrolled horizontally
        /// </summary>
        public bool CanHorizontallyScroll
        {
            get
            {
                return horizontalScrollAllowed;
            }
            set
            {
                horizontalScrollAllowed = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if the content can be scrolled vertically
        /// </summary>
        public bool CanVerticallyScroll
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    throw new NotImplementedException(ShoppingCart.Properties.Resources.ArgumentExceptionMessage);
                }
            }
        }

        /// <summary>
        /// Gets a value that describes the height of the area in which child items can be positioned
        /// </summary>
        public double ExtentHeight
        {
            get { return extent.Height; }
        }

        /// <summary>
        /// Gets a value that describes the width of the area in which child items can be positioned
        /// </summary>
        public double ExtentWidth
        {
            get { return extent.Width; }
        }

        /// <summary>
        /// Gets a value that describes the horizontal offset of the viewbox from the left of the extent
        /// </summary>
        public double HorizontalOffset
        {
            get { return viewportOffset.X; }
        }

        /// <summary>
        /// Gets or sets the owner of the LoopingPanel
        /// </summary>
        public ScrollViewer ScrollOwner
        {
            get
            {
                return owner;
            }
            set
            {
                // Unhook the event from the last owner (if there is one)
                if (owner != null)
                {
                    owner.RemoveHandler(UIElement.LostTouchCaptureEvent, new RoutedEventHandler(LostInputCapture));
                    owner.RemoveHandler(LostMouseCaptureEvent, new RoutedEventHandler(LostInputCapture));
                }

                // Use the new owner
                this.owner = value;

                // Hook the event on the new owner
                if (owner != null)
                {
                    owner.AddHandler(UIElement.LostTouchCaptureEvent, new EventHandler<TouchEventArgs>(LostInputCapture), true);
                    owner.AddHandler(LostMouseCaptureEvent, new RoutedEventHandler(LostInputCapture), true);
                }
            }
        }

        /// <summary>
        /// Gets a value that describes the vertical offset of the viewbox from the top of the extent
        /// </summary>
        public double VerticalOffset
        {
            get { return viewportOffset.Y; }
        }

        /// <summary>
        /// Gets a value that describes the height of the viewport
        /// </summary>
        public double ViewportHeight
        {
            get { return viewport.Width; }
        }

        /// <summary>
        /// Gets a value that describes the width of the viewport
        /// </summary>
        public double ViewportWidth
        {
            get { return viewport.Width; }
        }

        /// <summary>
        /// Not implemented. Cannot scroll vertically.
        /// </summary>
        public void LineDown()
        {

        }

        /// <summary>
        /// Scrolls the content left by one line. Called when the user clicks the left arrow button on 
        /// the left of the horizontal scroll bar
        /// </summary>
        public void LineLeft()
        {
            ScrollContent(-1);
        }

        /// <summary>
        /// Scrolls the content right by one line. Called when the user clicks the right arrow button on 
        /// the right of the horizontal scroll bar
        /// </summary>
        public void LineRight()
        {
            ScrollContent(1);
        }

        /// <summary>
        /// Not implemented. Cannot scroll vertically.
        /// </summary>
        public void LineUp()
        {

        }

        /// <summary>
        /// Ensures that the contents of the LoopingPanel are scrolled such that a specified visual
        /// element is visible.
        /// </summary>
        /// <param name="visual">A Visual that becomes visible.</param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        /// <returns>A Rect that is visible.</returns>
        public Rect MakeVisible(System.Windows.Media.Visual visual, System.Windows.Rect rectangle)
        {
            int itemIndex = Children.IndexOf((UIElement)visual);
            int index = firstItem;
            double itemOffset = firstItemOffset;

            // Get the offset for the item that should be visible
            while (index != itemIndex)
            {
                itemOffset += Children[index].DesiredSize.Width;
                index++;
                if (index >= Children.Count)
                {
                    index = 0;
                }
            }

            // If the item is not fully in view on the left side, then adjust the offset to bring it into view
            if (itemOffset < viewportOffset.X)
            {
                ScrollContent(viewportOffset.X - itemOffset);
            }

            // If the item is not fully in view on the right side, then adjust the offset to bring it into view
            if (itemOffset + rectangle.Width > viewportOffset.X + ViewportWidth)
            {
                ScrollContent((viewportOffset.X + ViewportWidth) - (itemOffset + rectangle.Width));
            }

            return rectangle;
        }

        /// <summary>
        /// Scrolls the content left by ten lines. Called when the user moves the mouse wheel down.
        /// </summary>
        public void MouseWheelDown()
        {
            ScrollContent(-10);
        }

        /// <summary>
        /// Scrolls the content left by ten lines. Called when the user moves the mouse wheel to the left.
        /// </summary>
        public void MouseWheelLeft()
        {
            ScrollContent(-10);
        }

        /// <summary>
        /// Scrolls the content right by ten lines. Called when the user moves the mouse wheel to the right.
        /// </summary>
        public void MouseWheelRight()
        {
            ScrollContent(10);
        }

        /// <summary>
        /// Scrolls the content right by ten lines. Called when the user moves the mouse wheel up.
        /// </summary>
        public void MouseWheelUp()
        {
            ScrollContent(10);
        }

        /// <summary>
        /// Not implemented. Cannot scroll vertically.
        /// </summary>
        public void PageDown()
        {

        }

        /// <summary>
        /// Scrolls the content right by the width of the viewport. Called when the user preses the page left key (if they have one.)
        /// </summary>
        public void PageLeft()
        {
            ScrollContent(viewport.Width);
        }

        /// <summary>
        /// Scrolls the content left by the width of the viewport. Called when the user preses the page right key (if they have one.)
        /// </summary>
        public void PageRight()
        {
            ScrollContent(-viewport.Width);
        }

        /// <summary>
        /// Not implemented. Cannot scroll vertically.
        /// </summary>
        public void PageUp()
        {

        }

        /// <summary>
        /// Change the horizontal offset of the content to represent the current scroll value. Typically
        /// called many times over the course of a scroll operation as the value of the scroll changes.
        /// </summary>
        /// <remarks>
        /// Typically, content scrolls to an absolute position. For example, if the value passed to 
        /// SetHorizontalOffset is 236, the viewport should be moved to 236 pixels from the left of the 
        /// extent. The concept of an absolute position is not helpful in a scrolling panel. For example, 
        /// a position of 0 should mean "put the viewport at the begining of the list." There is no 
        /// "begining of the list" in LoopingPanel.
        /// 
        /// What is more important to LoopingPanel is the change in scroll values between scrolls. For 
        /// example, if this is called with an offset value of 110, and called again later in the scroll 
        /// operation with a value of 120, the content should scroll ten pixels to the right because the
        /// input device has moved by ten pixels. It could be possible that the values match up.  For example,
        /// if the items started with an offset of 110, they would be scrolled to an offset of 120. That 
        /// is usually not the case however. In the above case, items that start with an offset of 15
        /// should be scrolled to an offset of 25, because the input device moved 10 pixels
        /// </remarks>
        /// <param name="offset">The offset to which content can be scrolled</param>
        public void SetHorizontalOffset(double offset)
        {
            if (CanHorizontallyScroll)
            {
                // If a user stops the flick, reset the offset
                if (isFlicking && ((SurfaceScrollViewer)owner).GetAreAnyInputDevicesCaptured())
                {
                    previousOffset = double.NaN;
                    isFlicking = false;
                }

                // Make sure the offset is set for the current inputs
                if (double.IsNaN(previousOffset))
                {
                    previousOffset = offset;
                }

                // Calculate the movement delta
                double difference = previousOffset - offset;

                // Scroll the content
                ScrollContent(difference);

                // Update the offset for next time
                previousOffset = offset;
            }
        }

        /// <summary>
        /// Not implemented. Cannot scroll vertically.
        /// </summary>
        /// <param name="offset">The new scroll value to which the offset should be set.</param>
        public void SetVerticalOffset(double offset)
        {
        }

        #endregion

        #region ISurfaceScrollInfo Members

        /// <summary>
        /// Convert to pixels from viewport units.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Vector ConvertFromViewportUnits(Point origin, Vector offset)
        {
            return offset;
        }

        /// <summary>
        /// Convert to viewport units from pixels.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Vector ConvertToViewportUnits(Point origin, Vector offset)
        {
            return offset;
        }

        /// <summary>
        /// Called at the begining of a flick operation to determine the endpoint of the flick.
        /// </summary>
        /// <param name="origin">The point where the flick started</param>
        /// <param name="offset">The suggested endpoint for the flick, as an offset from the origin</param>
        /// <returns>The final endpoint of the flick</returns>
        public Vector ConvertToViewportUnitsForFlick(Point origin, Vector offset)
        {
            isFlicking = true;
            if (CanHorizontallyScroll)
            {
                // Save the current first item index and offset values, and the viewport offset.
                int newFirstItem = 0;
                double newFirstItemOffset = 0.0;
                double newViewportOffset = 0.0;

                // Now calculate the state of the list if the suggested scrolling were to be applied.
                CalculatePostFlickState(offset.X, out newFirstItem, out newFirstItemOffset, out newViewportOffset);

                // Determine the direction of the flick
                bool leftFlick = offset.X < 0 ? true : false;

                double currentItemOffset = newFirstItemOffset;
                double currentBestAdjustment = leftFlick ? double.MinValue : double.MaxValue;

                // For a left flick, the relevant edge is the right edge, otherwise it's the left edge
                double relevantViewportEdge = leftFlick ? newViewportOffset + ViewportWidth : newViewportOffset;

                // Compare the positions of all newly moved items to the viewport bounds
                // Can't just do a foreach, because firstItemOffset points to a specific item. Instead,
                // do two for loops, the first looks at every item between firstItem and the last item 
                // in InternalChildren.
                for (int i = newFirstItem; i < InternalChildren.Count; i++)
                {
                    UIElement currentItem = InternalChildren[i];
                    double relevantItemEdge = leftFlick ? currentItemOffset + currentItem.DesiredSize.Width : currentItemOffset;

                    double difference = relevantViewportEdge - relevantItemEdge;

                    // For a left flick, the adjustment shoud be the negative adjustment with the smallest absolute value
                    if (leftFlick && difference < 0 && difference > currentBestAdjustment)
                    {
                        currentBestAdjustment = difference;
                    }

                    // For a right flick, the adjustment shoud be the negative adjustment with the smallest absolute value
                    if (!leftFlick && difference > 0 && difference < currentBestAdjustment)
                    {
                        currentBestAdjustment = difference;
                    }

                    // Update the right edge to get the right edge of the current item
                    currentItemOffset += currentItem.DesiredSize.Width;
                }

                // The second for loop does the same thing, but it does it for the items not covered
                // by the first for loop
                for (int i = 0; i < newFirstItem; i++)
                {
                    UIElement currentItem = InternalChildren[i];
                    double relevantItemEdge = leftFlick ? currentItemOffset + currentItem.DesiredSize.Width : currentItemOffset;

                    double difference = relevantViewportEdge - relevantItemEdge;

                    // For a left flick, the adjustment shoud be the negative adjustment with the smallest absolute value
                    if (leftFlick && difference < 0 && difference > currentBestAdjustment)
                    {
                        currentBestAdjustment = difference;
                    }

                    // For a right flick, the adjustment shoud be the negative adjustment with the smallest absolute value
                    if (!leftFlick && difference > 0 && difference < currentBestAdjustment)
                    {
                        currentBestAdjustment = difference;
                    }

                    // Update the right edge to get the right edge of the current item
                    currentItemOffset += currentItem.DesiredSize.Width;
                }

                Vector adjustment = new Vector(currentBestAdjustment, 0);

                previousOffset = origin.X;

                // Return the amount of scrolling needed for the flick to end at the point that was just calculated
                return offset + adjustment;
            }
            else
            {
                return offset;
            }
        }

        #endregion

        #region Layout Code

        /// <summary>
        /// Measure the InternalChildren of LoopingPanel in anticipation of arranging
        /// them during the ArrangeOverride pass.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // If there's nothing don't calculate anything, just take all the space we can
            if (this.InternalChildren.Count == 0)
            {
                return availableSize;
            }

            // First, measure each of the child items, keep a running sum of the child widths
            // Used later to measure the viewport and the extent
            totalContentWidth = 0;
            foreach (UIElement child in this.InternalChildren)
            {
                child.Measure(availableSize);
                totalContentWidth += child.DesiredSize.Width;
            }

            // The viewport should be as large as it can be
            viewport = availableSize;

            // Remeasuring could invalidate the current viewport position, mark it as dirty
            viewportPositionDirty = true;


            return availableSize;
        }

        /// <summary>
        /// Position the content in the LoopingPanel. 
        /// </summary>
        /// <param name="finalSize">
        /// The size of the viewport. Content can be arranged outside of these limits, but it will not be visible.
        /// </param>
        /// <returns>The size the control uses to display itself.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // If there's nothing don't calculate anything, just take all the space we can
            if (this.InternalChildren.Count == 0)
            {
                return finalSize;
            }

            // Arrange the viewport relative to the extent
            if (viewportPositionDirty)
            {
                PositionViewportAndExtent();
            }

            double nextDrawingPosition = firstItemOffset;
            Rect arrangeInMe;

            // InternalChildren[FirstItem] -> InternalChildren[InternalChildren.Count-1]
            for (int i = firstItem;i < InternalChildren.Count;i++)
            {
                UIElement item = InternalChildren[i];
                arrangeInMe = new Rect(nextDrawingPosition, 0, item.DesiredSize.Width, item.DesiredSize.Height);
                item.Arrange(arrangeInMe);
                nextDrawingPosition += item.DesiredSize.Width;
            }

            // InternalChildren[0] -> InternalChildren[FirstItem - 1]
            for (int i = 0;i < firstItem;i++)
            {
                UIElement item = InternalChildren[i];
                arrangeInMe = new Rect(nextDrawingPosition, 0, item.DesiredSize.Width, item.DesiredSize.Height);
                item.Arrange(arrangeInMe);
                nextDrawingPosition += item.DesiredSize.Width;
            }


            return finalSize;
        }

        /// <summary>
        /// Set the size of the extent, and adjust the viewport offset to position the viewport in the extent
        /// </summary>
        private void PositionViewportAndExtent()
        {
            // If the items will all fit in the viewport at the same time, disable scrolling and center the items
            if (totalContentWidth < viewport.Width)
            {
                extent = new Size(totalContentWidth, viewport.Height);
                SetViewport(0);
                firstItemOffset = (viewport.Width - totalContentWidth) / 2;

                CanHorizontallyScroll = false;
            }

            // Otherwise, extend the extent past both ends of the viewport so there will be plenty of space in
            // which to scroll the items
            else
            {
                // Make sure the extent is plenty large. This list loops, but the parent does not know that.
                // In a standard SurfaceListBox, scrolls or flicks that move the viewport past the end of the
                // extent are cut off at the extent border. The LoopingPanel's viewport doesn't move, but the
                // scroll/flick operations will still be cut off if they scroll the content by more than the
                // viewport offset. Make the extent super large to make sure that doesn't happen
                extent = new Size(Math.Max(1000000, totalContentWidth * 15), viewport.Height);
                SetViewport((extent.Width - viewport.Width) / 2);
                firstItemOffset = (extent.Width - totalContentWidth) / 2;

                CanHorizontallyScroll = true;
            }

            viewportPositionDirty = false;

            // Since the offset was changed, the current scroll info isn't valid anymore
            if (owner != null)
            {
                owner.InvalidateScrollInfo();
            }
        }

        #endregion

        #region Scrolling Code

        /// <summary>
        /// Positions the viewport relative to the extent
        /// </summary>
        /// <param name="newOffset">The offset from the left of the extent</param>
        private void SetViewport(double newOffset)
        {
            // Validate the input
            if (newOffset < 0 || viewport.Width >= extent.Width)
            {
                newOffset = 0;
            }

            if (newOffset + viewport.Width >= extent.Width)
            {
                newOffset = extent.Width - viewport.Width;
            }

            // Value is validated, use it
            viewportOffset.X = newOffset;

            // Adjust the transform to display based on the new offset
            transform.X = -newOffset;


            // Balance the content around the viewport.
            double firstItemRight = firstItemOffset + InternalChildren[firstItem].DesiredSize.Width;
            int lastItemIndex = firstItem <= 0 ? InternalChildren.Count - 1 : firstItem - 1;
            double lastItemLeft = firstItemOffset + totalContentWidth - InternalChildren[lastItemIndex].DesiredSize.Width;
 
            if (viewportOffset.X < firstItemRight)
            {
                MoveItemsFromRightToLeft();
            }

            if (viewportOffset.X + ViewportWidth > lastItemLeft)
            {
                MoveItemsFromLeftToRight();
            }
        }

        /// <summary>
        /// Move items on the left side of the viewport to the right side so that the
        /// distribution of items around the viewport is more balanced.
        /// </summary>
        private void MoveItemsFromRightToLeft()
        {
            // Move from the left to the right until the first item that is not in the viewport
            int index = firstItem;
            double offset = firstItemOffset;
            int itemsInViewport = 0;

            // Step through the items from the left, and count how many items are in the viewport
            while (offset < viewportOffset.X + ViewportWidth)
            {
                itemsInViewport++;
                offset += InternalChildren[index].DesiredSize.Width;
                index = index >= InternalChildren.Count - 1 ? 0 : index + 1;
            }

            // The items that are not in the viewport should be distributed so there are some on either side
            // of the viewport. It's likely that the viewport will continue to be moved in the direction it's 
            // currently being moved, so it makes sense to put more items in front of the viewport than behind.
            // Move 2/3 of the items, and leave 1/3 where they are.
            int itemsNotInViewport = InternalChildren.Count - itemsInViewport;
            int itemsToRemainInPlace = itemsNotInViewport / 3;
            
            // Skip past the items that will remain in place
            for (int i = 0; i < itemsToRemainInPlace; i++)           
            {                
                offset += InternalChildren[index].DesiredSize.Width;
                index = index >= InternalChildren.Count - 1 ? 0 : index + 1;
            }

            // Find the amount by which firstItemOffset will need to be adjusted
            double movedWidth = firstItemOffset + totalContentWidth - offset;

            // Change the first item index, and asjust the offset to match
            firstItemOffset -= movedWidth;
            firstItem = index;

            // Need to redraw the items
            InvalidateVisual();
        }

        /// <summary>
        /// Move items on the left side of the viewport to the right side so that the
        /// distribution of items around the viewport is more balanced.
        /// </summary>
        private void MoveItemsFromLeftToRight()
        {
            // Move from the left to the right until the first item that is not in the viewport
            int index = firstItem <= 0 ? InternalChildren.Count - 1 : firstItem - 1;
            double itemRight = firstItemOffset + totalContentWidth;
            int itemsInViewport = 0;

            // Step through the items from the right, and count how many items are in the viewport
            while (itemRight > viewportOffset.X)
            {
                itemsInViewport++;
                itemRight -= InternalChildren[index].DesiredSize.Width;
                index = index <= 0 ? InternalChildren.Count - 1 : index - 1;
            }

            // The items that are not in the viewport should be distributed so there are some on either side
            // of the viewport. It's likely that the viewport will continue to be moved in the direction it's 
            // currently being moved, so it makes sense to put more items in front of the viewport than behind.
            // Move 2/3 of the items, and leave 1/3 where they are.
            int itemsNotInViewport = InternalChildren.Count - itemsInViewport;
            int itemsToRemain = itemsNotInViewport / 3;

            // Skip past the items that will remain in place
            for (int i = 0; i < itemsToRemain; i++)
            {
                itemRight -= InternalChildren[index].DesiredSize.Width;
                index = index <= 0 ? InternalChildren.Count - 1 : index - 1;
            }

            // Find the amount by which firstItemOffset will need to be adjusted
            double movedWidth = itemRight - firstItemOffset;

            // Change the first item index, and adjust the offset to match
            firstItemOffset += movedWidth;
            firstItem = index >= InternalChildren.Count - 1 ? 0 : index + 1;

            // Need to redraw the items
            InvalidateVisual();
        }

        /// <summary>
        /// Moves the content of the LoopingPanel.
        /// </summary>
        /// <param name="adjustment">The distance of the scroll.</param>
        private void ScrollContent(double adjustment)
        {
            SetViewport(viewportOffset.X - adjustment);
        }

        /// <summary>
        /// Calculates the state of the content and viewport if the content were to scroll by 
        /// the suggested adjustment. No content is actually moved as a result of this operation.
        /// </summary>
        /// <param name="adjustment">The amount of scrolling to apply to the content.</param>
        /// <param name="newFirstItem">The item that would be the first item if the scrolling were applied.</param>
        /// <param name="newFirstItemOffset">The value of firstItemOffset if the scrolling were to be applied.</param>
        /// <param name="newViewportOffset">The viewport offset if the scrolling were to be applied.</param>
        private void CalculatePostFlickState (double adjustment, out int newFirstItem, out double newFirstItemOffset, out double newViewportOffset)
        {
            // Use negative adjustment. Content scrolling right means viewport scrolls left and vice versa.
            double suggestedViewportLeft = viewportOffset.X - adjustment;
            double suggestedViewportRight = suggestedViewportLeft + viewport.Width;

            newViewportOffset = suggestedViewportLeft;
            newFirstItem = firstItem;
            newFirstItemOffset = firstItemOffset;
            
            // Need to make sure there will be content on both sides of the viewport after it is 
            // moved, but MoveItemsFromLeftToRight (or right to left) actually moves the items and 
            // triggers a new render pass. Just use a simple approximation here.

            // Move items from the right to the left until the content extends past the left edge of the viewport
            while (newFirstItemOffset > suggestedViewportLeft)
            {
                newFirstItem = newFirstItem <= 0 ? InternalChildren.Count - 1 : newFirstItem - 1;
                newFirstItemOffset -= InternalChildren[newFirstItem].DesiredSize.Width;
            }

            // Move items from the left to the right until the content extends past the right edge of the viewport
            while (newFirstItemOffset + totalContentWidth < suggestedViewportRight)
            {
                newFirstItemOffset += InternalChildren[newFirstItem].DesiredSize.Width;
                newFirstItem = newFirstItem >= InternalChildren.Count - 1 ? 0 : newFirstItem + 1;
            }
        }

        #endregion
    }
}
