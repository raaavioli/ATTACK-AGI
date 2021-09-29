using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Input;

namespace ItemCompare
{
    /// <summary>
    /// Interaction logic for ItemVisualization.xaml
    /// </summary>
    public partial class ItemVisualization : TagVisualization
    {
        private Items.ItemData itemData;

        public Items.ItemData ItemData
        {
            get { return itemData; }
            set { itemData = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemVisualization()
        {
            InitializeComponent();

            TagRemovedBehavior = TagRemovedBehavior.Wait;
            LostTagTimeout = double.PositiveInfinity;
        }

        /// <summary>
        /// Sets the item and properties to display.
        /// </summary>
        /// <param name="properties">An array of properties to display.</param>
        /// <param name="item">The item to display.</param>
        public void SetItem(
            Items.ItemProperty[] properties,
            Items.Item item)
        {
            // clear out any prior contents
            RowHost.Children.Clear();
            RowHost.RowDefinitions.Clear();

            // set up our row definitions
            for (int index = 0; index < properties.Length; ++index)
            {
                RowHost.RowDefinitions.Add(new RowDefinition());
            }

            // add our rows
            for (int index = 0; index < properties.Length; ++index)
            {
                InformationPanelRow row = new InformationPanelRow();
                Grid.SetRow(row, index);
                RowHost.Children.Add(row);              
                row.HeadingLabel.Text = properties[index].Name;                
                row.Cell.SetValue(item.Values[index], properties[index].PropertyType);
            }

            ItemBrandPanel.Text = item.Brand;
            ItemNamePanel.Text = item.Name;
        }

        /// <summary>
        /// Determines whether this visualization matches the specified input device.
        /// </summary>
        /// <param name="inputDevice"></param>
        /// <returns></returns>
        public override bool Matches(InputDevice inputDevice)
        {
            // We match a given InputDevice if it's tag value is present
            // in our item data.

            if (itemData == null)
            {
                return false;
            }

            // Has to be a valid tag
            if (!IsTagValid(inputDevice.GetTagData()))
            {
                return false;
            }

            // Look for a match in our item data
            Items.Item matchingItem = itemData.Find((byte)inputDevice.GetTagData().Value);

            return matchingItem != null;
        }

        /// <summary>
        /// Refresh the item visualization properties when a tag is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnGotTag(RoutedEventArgs e)
        {
            TagVisualization tv = (TagVisualization)e.Source;

            TagData tag = tv.VisualizedTag;

            // Has to be a valid tag
            if (IsTagValid(tag))
            {
                // Look for a match in our item data
                Items.Item matchingItem = itemData.Find((byte)tag.Value);

                if (matchingItem != null)
                {
                    SetItem(itemData.Properties, matchingItem);
                }
            }
        }

        /// <summary>
        /// Helper method to validate tag data.
        /// </summary>
        /// <param name="tagData">the tag data to validate</param>
        /// <returns>true if this tag is valid for this app</returns>
        private static bool IsTagValid(TagData tagData)
        {
            return tagData.Schema == 0
                && tagData.Series == 0
                && tagData.ExtendedValue == 0
                && tagData.Value >= 0
                && tagData.Value < 256;
        }
    }
}
