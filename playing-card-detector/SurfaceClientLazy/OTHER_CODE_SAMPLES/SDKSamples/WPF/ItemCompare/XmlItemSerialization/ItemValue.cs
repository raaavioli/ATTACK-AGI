using System.Xml.Serialization;

namespace ItemCompare.XmlItemSerialization
{
    /// <summary>
    /// Represents a property value for an item.
    /// </summary>
    /// <remarks>
    /// See detailed comments on ItemCompare.XmlItemSerialization.ItemData
    /// for a discussion of how this class is used.
    /// </remarks>
    public class ItemValue
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        [XmlAttribute]
        public string Name;

        /// <summary>
        /// The value of the property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        [XmlAttribute]
        public string Value;
    }
}
