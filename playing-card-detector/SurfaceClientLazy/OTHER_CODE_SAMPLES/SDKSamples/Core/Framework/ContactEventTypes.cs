namespace CoreInteractionFramework
{
    /// <summary>
    /// The possible types of touch events.
    /// </summary>
    public enum TouchEventType
    {
        /// <summary>
        /// The touch was added to the Microsoft Surface unit.
        /// </summary>
        Added,

        /// <summary>
        /// The touch was removed from the Microsoft Surface unit.
        /// </summary>
        Removed,

        /// <summary>
        /// The touch was changed.
        /// </summary>
        Changed,

        /// <summary>
        /// The touch left the UI element.
        /// </summary>
        Leave,
        
        /// <summary>
        /// The touch entered a new UI element.
        /// </summary>
        Enter,
    }
}
