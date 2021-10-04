namespace CoreInteractionFramework
{
    /// <summary>
    /// Provides more details about the touch hit on a list box, including 
    /// members of <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> 
    /// through its <strong><see cref="CoreInteractionFramework.ScrollViewerHitTestDetails"/></strong> base class. 
    /// </summary>
    /// <remarks>
    /// <note type="caution"> The <strong>ListBoxHitTestDetails</strong> class does not provide 
    /// any functionality beyond the 
    /// base class but simplifies API use if hit testing maps to the class that is being 
    /// tested.
    /// </note></remarks>
    public class ListBoxHitTestDetails : ScrollViewerHitTestDetails
    {
        /// <summary>
        /// Creates an <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> object for a 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> object.
        /// </summary>
        public ListBoxHitTestDetails() 
        {
        }

        /// <summary>
        /// Creates an <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> object for a 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> object with the specified parameters.
        /// </summary>
        /// <param name="horizontalPosition">The horizontal normalized coordinate 
        /// where a touch hit the scroll bar from 0 to 1. A value of 0 indicates a 
        /// left-most hit, and a value of 1 indicates a right-most hit of the scroll bar.</param>
        /// <param name="verticalPosition">The vertical normalized coordinate where a 
        /// touch hit the scroll bar from 0 to 1. A value of 0 indicates a 
        /// top-most hit, and a value of 1 indicates a bottom-most hit of the scroll bar.</param>
        public ListBoxHitTestDetails(float horizontalPosition, float verticalPosition) 
            : base (horizontalPosition, verticalPosition)
        {
           
        }
    }
}
