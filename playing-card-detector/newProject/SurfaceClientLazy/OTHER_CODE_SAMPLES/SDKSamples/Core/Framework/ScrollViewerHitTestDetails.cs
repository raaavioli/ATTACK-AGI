using System;
using System.Collections.Generic;
using System.Text;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Provides more details about the touch hit on the <strong>ScrollViewer</strong> object, including 
    /// access to inherited data through <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong>.    
    /// </summary>
    public class ScrollViewerHitTestDetails : IHitTestDetails
    {
        private float horizontalPosition;
        private float verticalPosition;

        /// <summary>
        /// Creates a 
        /// <strong><see cref="ScrollViewerStateMachine"/></strong> instance.
        /// </summary>
        public ScrollViewerHitTestDetails()
        {
        }

        /// <summary>
        /// Creates a 
        /// <strong><see cref="ScrollViewerStateMachine"/></strong> instance with the specified parameters. 
        /// </summary>
        /// <param name="horizontalPosition">The horizontal normalized coordinate 
        /// where a touch hit the scroll bar from 0 to 1. A value of 0 indicates a 
        /// left-most hit, and a value of 1 indicates a right-most hit on the scroll bar.</param>
        /// <param name="verticalPosition">The vertical normalized coordinate where a 
        /// touch hit the scroll bar from 0 to 1. A value of 0 indicates a 
        /// top-most hit, and value of 1 indicates a bottom-most hit of the scroll bar.</param>
        public ScrollViewerHitTestDetails(float horizontalPosition, float verticalPosition)
        {
            if (horizontalPosition < 0 || horizontalPosition > 1)
                throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("horizontalPosition");

            if (verticalPosition < 0 || verticalPosition > 1)
                throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("verticalPosition");

            this.horizontalPosition = horizontalPosition;
            this.verticalPosition = verticalPosition;
        }

        /// <summary>
        /// Gets or sets the horizontal normalized coordinate where a touch hit 
        /// the scroll bar. 
        /// </summary>
        /// <remarks>Valid float values are within the range from 0 to 1. A value of 
        /// 0 indicates a left-most hit, and a value of 1 indicates a right-most hit on the scroll bar.</remarks>
        public float HorizontalPosition
        {
            get { return horizontalPosition; }
            set 
            {
                if (value < 0 || value > 1)
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("HorizontalPosition");

                horizontalPosition = value; 
            }
        }

        /// <summary>
        /// Gets or sets the vertical normalized coordinate where a touch hit 
        /// the scroll bar. 
        /// </summary>
        /// <remarks>Valid float values are within the range from 0 to 1. A value 
        /// of 0 indicates a top-most hit, and a value of 1 indicates a bottom-most hit on the scroll bar.</remarks>
        public float VerticalPosition
        {
            get { return verticalPosition; }
            set 
            {
                if (value < 0 || value > 1)
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("VerticalPosition");

                verticalPosition = value; 
            }
        }
    }
}
