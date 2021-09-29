using System.IO;
using System.Xml.Serialization;

namespace ItemCompare.XmlItemSerialization
{
    /// <summary>
    /// Represents the definition of all properties and items.
    /// </summary>
    /// <remarks>
    /// This class, along with the other classes in this namespace, is designed
    /// to be deserialized in "raw" form directly from an XML file that accompanies
    /// the application at run time, using the .NET framework's built-in
    /// XmlSerializer class to do the file parsing. These XML-deserialized classes
    /// are then used as arguments to construct ItemCompare.Items.ItemData and related
    /// classes, which the application consumes to populate its UI.
    /// </remarks>
    public class ItemData
    {
        /// <summary>
        /// Properties that are found on the items.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        public ItemProperty[] Properties;

        /// <summary>
        /// The items.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Required to enable XML serialization")]
        public Item[] Items;

        /// <summary>
        /// Deserializes an ItemData from an XML file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ItemData Deserialize(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ItemData));
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return (ItemData)serializer.Deserialize(stream);
            }
        }
    }
}
