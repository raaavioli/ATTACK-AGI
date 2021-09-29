namespace ItemCompare.Items
{
    /// <summary>
    /// Represents the type of an item property.
    /// </summary>
    public enum ItemPropertyType
    {
        /// <summary>
        /// A boolean field.
        /// </summary>
        Boolean,

        /// <summary>
        /// A text field.
        /// </summary>
        Text,

        /// <summary>
        /// An image field. In the XML file, the value is the path to the image file.
        /// </summary>
        Image,

        /// <summary>
        /// A price field.
        /// </summary>
        Price
    }
}