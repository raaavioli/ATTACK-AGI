using System;
using CoreInteractionFramework;

namespace XnaScatter
{
    /// <summary>
    /// Details about where the Touch hit the ITouchableObject.
    /// </summary>
    public class XnaScatterHitTestDetails : IHitTestDetails
    {
        private float horizontalPosition;
        private float verticalPosition;

        /// <summary>
        /// Creates an IHitTestDetails object for an ITouchableObject.
        /// </summary>
        public XnaScatterHitTestDetails()
        {

        }

        /// <summary>
        /// Creates an IHitTestDetails object for an ITouchableObject.
        /// </summary>
        /// <param name="horizontalPosition">The horizontal component of the point where a touch hit the ITouchableObject.
        /// Zero is the left of the object, and one is the right.</param>
        /// <param name="verticalPosition">The vertical component of the point where a touch hit the ITouchableObject.
        /// Zero is the top of the object, and one is the bottom.</param>
        public XnaScatterHitTestDetails(float horizontalPosition, float verticalPosition)
        {
            if (horizontalPosition < 0 || horizontalPosition > 1)
            {
                throw new ArgumentOutOfRangeException("horizontalPosition");
            }

            if (verticalPosition < 0 || verticalPosition > 1)
            {
                throw new ArgumentOutOfRangeException("verticalPosition");
            }

            this.horizontalPosition = horizontalPosition;
            this.verticalPosition = verticalPosition;
        }

        /// <summary>
        /// The horizontal normalized coordinate where a touch hit the ITouchableObject from 0 to 1.  
        /// Where 0 is the very left and 1 is the very right of the object.
        /// </summary>
        public float HorizontalPosition
        {
            get 
            { 
                return horizontalPosition; 
            }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                horizontalPosition = value;
            }
        }

        /// <summary>
        /// The vertical normalized coordinate where a touch hit the ITouchableObject from 0 to 1.  
        /// Where 0 is the very top and 1 is the very bottom of the object.
        /// </summary>
        public float VerticalPosition
        {
            get 
            { 
                return verticalPosition; 
            }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                verticalPosition = value;
            }
        }
    }
}
