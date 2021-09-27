using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

namespace ItemCompare.XmlItemSerialization
{
    /// <summary>
    /// Represents the definition of an item.
    /// </summary>
    /// <remarks>
    /// See detailed comments on ItemCompare.XmlItemSerialization.ItemData
    /// for a discussion of how this class is used.
    /// </remarks>
    public class Item
    {
        private byte tagValue;

        /// <summary>
        /// The tag value associated with the item.
        /// </summary>
        public byte TagValue
        {
            get
            {
                return tagValue;
            }
        }


        /// <summary>
        /// The string representation of the tag value associated with the item.
        /// </summary>
        [XmlAttribute]
        public string Tag
        {
            set
            {
                Debug.Assert(value != null);
                string tag = value;

                tag = tag.Replace(" ", "");

                // Convert from either decimal or hexadecimal format.
                int startOfHexValue = value.IndexOf("0x", StringComparison.Ordinal);
                bool isHex = startOfHexValue >= 0;

                if (isHex)
                {
                    // Get rid of the "0x" at the beginning of the string with no spaces.
                    tag = tag.Substring(startOfHexValue + 2);
                }

                tagValue = byte.Parse(tag, isHex ? NumberStyles.HexNumber : NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            get
            {
                return tagValue.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// The brand of the item.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        [XmlAttribute]
        public string Brand;
        
        /// <summary>
        /// The name of the item.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        [XmlAttribute]
        public string Name;

        /// <summary>
        /// The set of property values associated with the item.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        public ItemValue[] ItemValues;
    }
}
