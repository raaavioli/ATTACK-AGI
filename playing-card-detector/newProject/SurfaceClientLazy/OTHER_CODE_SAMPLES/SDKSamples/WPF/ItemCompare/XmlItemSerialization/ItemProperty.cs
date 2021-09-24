using System.Xml.Serialization;

namespace ItemCompare.XmlItemSerialization
{
    /// <summary>
    /// Represents a property defined for items.
    /// </summary>
    /// <remarks>
    /// See detailed comments on ItemCompare.XmlItemSerialization.ItemData
    /// for a discussion of how this class is used.
    /// </remarks>
    public class ItemProperty
    {
        /// <summary>
        /// The brand of the property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        [XmlAttribute]
        public string Brand;

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
        /// The type of the property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        [XmlAttribute]
        public ItemCompare.Items.ItemPropertyType PropertyType;
    }
}
