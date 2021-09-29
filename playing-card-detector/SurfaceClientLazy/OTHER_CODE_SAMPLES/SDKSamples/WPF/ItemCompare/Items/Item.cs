using System;
using System.Windows.Media.Imaging;

namespace ItemCompare.Items
{
    /// <summary>
    /// Represents the definition of an item.
    /// </summary>
    public class Item
    {
        private readonly byte tagValue;
        private readonly string name;
        private readonly string brand;
        private readonly object[] values;

        /// <summary>
        /// Gets the tag value associated with the item.
        /// </summary>
        public byte TagValue
        {
            get { return tagValue; }
        }

        /// <summary>
        /// Gets the brand of the item.
        /// </summary>
        public string Brand
        {
            get { return brand; }
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name
        {
            get { return name; }
        }     

        /// <summary>
        /// Gets the property values associated with the item.
        /// </summary>
        /// <remarks>
        /// The size of the array equals the number of defined properties.
        /// The values will have types determined by the corresponding properties.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Required to enable XML serialization")]
        public object[] Values
        {
            get { return values; }
        }

        /// <summary>
        /// Constructor. Builds the item from a list of properties and values.
        /// </summary>
        /// <param name="properties">Properties used to format the values.</param>
        /// <param name="values">Raw item as read from XML.</param>
        internal Item(
            ItemProperty[] properties,
            XmlItemSerialization.Item xmlItem)
        {
            tagValue = xmlItem.TagValue;
            name = xmlItem.Name;
            brand = xmlItem.Brand;

            int numProperties = properties.Length;
            values = new object[numProperties];

            if (xmlItem.ItemValues != null)
            {
                for (int index = 0; index < numProperties; ++index)
                {
                    foreach (XmlItemSerialization.ItemValue xmlValue in xmlItem.ItemValues)
                    {
                        if (xmlValue.Name == properties[index].Name)
                        {
                            values[index] = Parse(xmlValue.Value, properties[index].PropertyType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses a string value from an XML file to an object of the appropriate type.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object Parse(string stringValue, ItemPropertyType type)
        {
            switch (type)
            {
                case ItemPropertyType.Boolean:
                    return bool.Parse(stringValue);

                case ItemPropertyType.Text:
                    return stringValue;

                case ItemPropertyType.Image:
                    return new BitmapImage(new Uri("pack://siteOfOrigin:,,,/" + stringValue));

                case ItemPropertyType.Price:
                    return stringValue;

                default:
                    return null;
            }
        }
    }
}
