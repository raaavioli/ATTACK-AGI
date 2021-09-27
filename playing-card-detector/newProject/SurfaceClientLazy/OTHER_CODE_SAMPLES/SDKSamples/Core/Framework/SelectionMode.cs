namespace CoreInteractionFramework
{
    /// <summary>
    /// The selection mode for items.
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>
        /// Only one item can be selected.
        /// </summary>
        Single,

        /// <summary>
        /// Multiple items can be selected.
        /// </summary>
        Multiple,

        /// <summary>
        /// Item selection defaults to <strong>Single</strong>.
        /// </summary>
        Default = Single,
    }
}
