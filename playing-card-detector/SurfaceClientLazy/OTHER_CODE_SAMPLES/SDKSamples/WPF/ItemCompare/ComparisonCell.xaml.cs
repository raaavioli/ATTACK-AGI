using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ItemCompare
{
    /// <summary>
    /// Represents a single cell within the comparison table, for displaying
    /// a property value of an item. Can display values for different property
    /// types (text, image, boolean).
    /// </summary>
    public partial class ComparisonCell : UserControl
    {
        // Graphics to use for true/false fields. We load these from
        // resources compiled in our assembly.
        private static readonly BitmapImage trueImage = new BitmapImage(new Uri("Resources/True.png", UriKind.Relative));
        private static readonly BitmapImage falseImage = new BitmapImage(new Uri("Resources/False.png", UriKind.Relative));

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ComparisonCell()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the value to display in the cell.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(object value, Items.ItemPropertyType type)
        {
            PriceValue.Visibility = Visibility.Collapsed;
            TextValue.Visibility = Visibility.Collapsed;
            ImageValue.Visibility = Visibility.Collapsed;
            BooleanValue.Visibility = Visibility.Collapsed;
                     
            // Is it a string value?
            string text = value as string;
            if (text != null)
            {
                if (type == Items.ItemPropertyType.Price)
                {
                    PriceValue.Text = text;
                    PriceValue.Visibility = Visibility.Visible;
                }
                else
                {
                    TextValue.Text = text;
                    TextValue.Visibility = Visibility.Visible;
                }                
                return;
            }

            // Is it an image?
            ImageSource imageSource = value as ImageSource;
            if (imageSource != null)
            {
                ImageValue.Source = imageSource;
                ImageValue.Visibility = Visibility.Visible;
                return;
            }

            // Is it a boolean value?
            if (value is bool)
            {
                bool flag = (bool)value;
                BooleanValue.Source = flag ? trueImage : falseImage;
                BooleanValue.Visibility = Visibility.Visible;
            }
        }
    }
}
