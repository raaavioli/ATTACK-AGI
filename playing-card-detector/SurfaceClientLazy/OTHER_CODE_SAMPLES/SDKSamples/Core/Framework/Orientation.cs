using System;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Defines values that represent the orientation of the state machine. 
    /// </summary>
    /// <remarks>Enumeration values begin
    /// with <strong>Vertical</strong> = 0x1, <strong>Horizontal</strong>, <strong>Default</strong> (<strong>Vertical</strong>), 
    /// and <strong>Both</strong> (an OR combination of <strong>Vertical</strong> and <strong>Horizontal</strong>.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [Flags]
    public enum Orientation
    {
        /// <summary>
        /// The state machine is oriented in the vertical direction.
        /// </summary>
        Vertical = 0x1,

        /// <summary>
        /// The state machine is oriented in the horizontal direction.
        /// </summary>
        Horizontal,

        /// <summary>
        /// The default direction is <strong>Vertical</strong>.
        /// </summary>
        Default = Vertical,

        /// <summary>
        /// Scroll in both directions. This value is not a valid option for 
        /// <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> objects.
        /// </summary>
        Both = Vertical | Horizontal,
    }
}
