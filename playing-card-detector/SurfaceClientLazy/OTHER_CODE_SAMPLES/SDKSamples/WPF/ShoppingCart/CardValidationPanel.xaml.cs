using System;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace ShoppingCart
{
    /// <summary>
    /// Interaction logic for CardValidationPanel.xaml
    /// </summary>
    public partial class CardValidationPanel: TagVisualization
    {
        /// <summary>
        /// Occurs when the user successfully validates their identity
        /// </summary>
        public event EventHandler<IdentityValidatedEventArgs> IdentityValidated;

        /// <summary>
        /// Constructor
        /// </summary>
        public CardValidationPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for the Validate button's click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateButtonClick(object sender, EventArgs e)
        {
            if(PinTextBox.Text == "1234") // Ultra Secure
            {
                OnIdentityValidated();
            }
        }

        /// <summary>
        /// Raises the IdentityValidatedEvent
        /// </summary>
        protected virtual void OnIdentityValidated()
        {
            if(IdentityValidated != null)
                IdentityValidated(this, new IdentityValidatedEventArgs(Center, Orientation));
            if (Visualizer.ActiveVisualizations.Contains(this))
            {
                Visualizer.RemoveVisualization(this);
            }
        }
    }
}