namespace CoreInteractionFramework
{
    /// <summary>
    /// Identifies the touch state that determines 
    /// when a click occurs. The default mode is <strong>Release</strong>.
    /// </summary>
    public enum ClickMode
    {
        /// <summary>
        /// A click occurs when a touch is added and released.
        /// </summary>
        Release,

        /// <summary>
        /// A click occurs when the button is pressed.
        /// </summary>
        Press,

        /// <summary>
        /// A click occurs when a touch enters or is added to the button.
        /// </summary>
        Hover,
    }
}
