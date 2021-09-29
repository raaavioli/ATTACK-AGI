namespace ItemCompare.Items
{
    /// <summary>
    /// Represents the definition of an item property.
    /// </summary>
    /// <remarks>
    /// Each defined item property corresponds to one column in the item
    /// comparison table in the UI.
    /// </remarks>
    public class ItemProperty
    {
        private readonly string brand;
        private readonly string name;
        private readonly ItemPropertyType propertyType;

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Brand
        {
            get { return brand; }
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public ItemPropertyType PropertyType
        {
            get { return propertyType; }
        }

        /// <summary>
        /// Constructor. Builds the item from raw XML data.
        /// </summary>
        /// <param name="xmlProperty"></param>
        internal ItemProperty(XmlItemSerialization.ItemProperty xmlProperty)
        {
            brand = xmlProperty.Brand;
            name = xmlProperty.Name;
            propertyType = xmlProperty.PropertyType;
        }
    }
}
