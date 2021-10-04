using System;

namespace Cloth.UI
{
    /// <summary>
    /// A useful Enum that represents whether a UIElement is in its
    /// Minimized or Maximized state (whatever that may be), or
    /// is animating between the the Minimized and Maximized states.
    /// </summary>
    [Flags]
    public enum ViewStates
    {
        None = 0,
        Minimized = 1,
        Maximized = 2,
        Animating = 4,
        Expanding = Maximized|Animating,
        Contracting = Minimized|Animating
    }
}
