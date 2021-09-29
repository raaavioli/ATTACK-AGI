namespace CoreInteractionFramework
{
    /// <summary>
    /// Provides more details about the touch hit on the scroll bar, including 
    /// members of its <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> base class.
    /// </summary>
    public class ScrollBarHitTestDetails : IHitTestDetails
    {
        private float position;

        /// <summary>
        /// Creates an <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> object for a 
        /// <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> object.
        /// </summary>
        public ScrollBarHitTestDetails()
        {
        }

        /// <summary>
        /// Creates an <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> object for a 
        /// <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> object with the
        /// specified parameters.
        /// </summary>
        /// <param name="position">The point where a touch hit the scroll bar. Valid values
        /// are from 0 through 1.  
        /// </param>
        public ScrollBarHitTestDetails(float position)
        {
            if (position < 0 || position > 1)
                throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("position");

            this.position = position;
        }

        /// <summary>
        /// Gets or sets the position value where a touch hit 
        /// the scroll bar.  
        /// </summary>
        /// <remarks>The valid values for <strong>Position</strong> range from 0 through 1. A value of 
        /// 0 indicates a topmost hit, and a value of 1 indicates a bottommost hit of the scroll bar.</remarks>
        /// <returns>The point where a touch hit the scroll bar.</returns>
        public float Position
        {
            get { return position; }
            set 
            {
                if (value < 0 || value > 1)
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("Position");

                position = value; 
            }
        }
    }
}
