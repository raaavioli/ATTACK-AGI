using System;
using System.Windows;

namespace ShoppingCart
{
    /// <summary>
    /// Arguments for the IdentityValidated event
    /// </summary>
    public class IdentityValidatedEventArgs : EventArgs
    {
        private Point validationCenter;
        private double validationOrientation;

        /// <summary>
        /// The center of the card/tag that was just validated
        /// </summary>
        public Point ValidationCenter
        {
            get
            {
                return validationCenter;
            }
        }

        /// <summary>
        /// The orientation of the card/tag that was just validated
        /// </summary>
        public double ValidationOrientation
        {
            get
            {
                return validationOrientation;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center">The center of the card/tag that was just validated</param>
        /// <param name="orientation">The orientation of the card/tag that was just validated</param>
        public IdentityValidatedEventArgs(Point center, double orientation)
            : base()
        {
            validationCenter = center;
            validationOrientation = orientation;
        }
    }
}
