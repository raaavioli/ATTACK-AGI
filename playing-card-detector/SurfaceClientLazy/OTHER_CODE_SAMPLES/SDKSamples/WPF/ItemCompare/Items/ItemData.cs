namespace ItemCompare.Items
{
    /// <summary>
    /// Represents the data for all items.
    /// </summary>
    /// <remarks>
    /// The ItemCompare application reads its "item definition" data at run time
    /// from an XML file accompanying the app. This file defines which tag values
    /// the app is interested in, and relevant information for each tag value (e.g.
    /// the item's name, and the values for its various properties). This class serves
    /// as the collection point for all the item information that's read from the file.
    /// </remarks>
    public class ItemData
    {
        private readonly ItemProperty[] properties;
        private readonly Item[] items;

        /// <summary>
        /// Gets the properties for the items.
        /// </summary>
        /// <remarks>
        /// This information will serve as the "schema" for the comparison graphic
        /// table; there will be one column in the table for each property in this
        /// array. The properties inform the table how to display the information,
        /// by providing a title and data type for each column in the table.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Required to enable XML serialization")]
        public ItemProperty[] Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Constructor. Deserializes the data from the specified file.
        /// </summary>
        /// <param name="filePath"></param>
        public ItemData(string filePath)
        {
            XmlItemSerialization.ItemData xmlData = XmlItemSerialization.ItemData.Deserialize(filePath);

            int numProperties = xmlData.Properties.Length;
            properties = new ItemProperty[numProperties];
            for (int index = 0; index < numProperties; ++index)
            {
                properties[index] = new ItemProperty(xmlData.Properties[index]);
            }

            int numItems = xmlData.Items.Length;
            items = new Item[numItems];
            for (int index = 0; index < numItems; ++index)
            {
                items[index] = new Item(properties, xmlData.Items[index]);
            }
        }

        /// <summary>
        /// Looks for an item whose tag value matches the specified value.
        /// </summary>
        /// <param name="tagValue">The tag value to search for.</param>
        /// <returns>If found, an item whose tag value matches. Returns null if no match found.</returns>
        public Item Find(byte tagValue)
        {
            foreach (Item item in items)
            {
                if (item.TagValue == tagValue)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
