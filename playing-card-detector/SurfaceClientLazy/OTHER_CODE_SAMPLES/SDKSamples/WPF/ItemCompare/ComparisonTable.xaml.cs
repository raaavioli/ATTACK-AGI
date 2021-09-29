using System.Windows;
using System.Windows.Controls;

namespace ItemCompare
{
    /// <summary>
    /// This class represents the comparison graphic that displays a table
    /// of item names and properties.
    /// </summary>
    public partial class ComparisonTable : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ComparisonTable()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the items to be used in the comparison.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        public void SetItems(
            Items.ItemProperty[] properties,
            Items.Item item1,
            Items.Item item2)
        {
            // clear out any prior contents
            RowHost.Children.Clear();
            RowHost.RowDefinitions.Clear();

            // set up our row/column definitions
            for (int index = 0; index < properties.Length; ++index)
            {
                RowHost.RowDefinitions.Add(new RowDefinition());
            }

            // add our columns
            for (int index = 0; index < properties.Length; ++index)
            {
                ComparisonRow row = new ComparisonRow();
                Grid.SetRow(row, index);
                RowHost.Children.Add(row);                
                row.HeadingLabel.Text = properties[index].Name;
                row.Cell1.SetValue(item1.Values[index], properties[index].PropertyType);
                row.Cell2.SetValue(item2.Values[index], properties[index].PropertyType);
            }

            ItemBrand1.Text = item1.Brand;
            ItemBrand2.Text = item2.Brand;
            ItemName1.Text = item1.Name;
            ItemName2.Text = item2.Name;
        }
    }
}
