using System.Diagnostics.CodeAnalysis;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Provides more information about the hit test. </summary>
    /// <remarks>Not all state machines require 
    /// more details. If additional details are required about a hit test, there will 
    /// be a class named <em>(ControlName)</em><strong>HitTestDetails</strong>. For example, 
    /// the <strong><see cref="CoreInteractionFramework.ScrollBarHitTestDetails"/></strong> 
    /// class provides more details about a hit test for a 
    /// <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> state machine. 
    /// Classes that do not require addition details do not have 
    /// a corresponding <strong>HitTestDetails</strong> class. For example, 
    /// <strong><see cref="CoreInteractionFramework.ButtonStateMachine"/></strong> does not 
    /// require more details and so there is no <strong>ButtonHitTestDetails</strong> class.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IHitTestDetails
    {
    }
}
