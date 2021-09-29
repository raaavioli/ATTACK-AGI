using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Represents a specific event that has occurred for a touch. 
    /// </summary>
    /// <remarks>
    /// <para>The <strong><see cref="CoreInteractionFramework.TouchEventType"/></strong>
    /// enumeration defines the following touch events:
    /// <strong>Added</strong>, <strong>Changed</strong>, <strong>Removed</strong>, 
    /// <strong>Enter</strong>, and <strong>Leave</strong>. <strong>TouchTargetEvent</strong> 
    /// objects have state set to <strong>Leave</strong> or <strong>Enter</strong>,
    /// depending on <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> route 
    /// destination and the capture state.</para>
    /// <note type="caution"> Touches that are processed on 
    /// <strong>Added</strong>, <strong>Changed</strong>, <strong>Removed</strong>, 
    /// <strong>Enter</strong>, and <strong>Leave</strong>
    /// events are immediately sent to the state machine, so the touches in a 
    /// frame update are split into separate update calls for the state machines. This action is 
    /// important because a state machine often changes the capture state of a touch 
    /// when it receives one of these four events.  
    /// </note>
    /// </remarks>
    public class TouchTargetEvent
    {
        private TouchPoint touch;
        private TouchEventType eventType;
        private IHitTestDetails hitTestDetails;

        /// <summary>
        /// Parameterized class constructor that creates a TouchTargetEvent.
        /// </summary>
        /// <param name="eventType">Event types include touch Added, Changed, Removed, 
        /// Enter and Leave.</param>
        /// <param name="touch">The subject touch of this TouchTargetEvent</param>
        internal TouchTargetEvent(TouchEventType eventType, TouchPoint touch)
        {
            this.EventType = eventType;
            this.touch = touch;
        }
        
        /// <summary>
        /// Gets a value that represents the <strong>Touch</strong> source 
        /// of this event.
        /// </summary>
        public TouchPoint Touch
        {
            get
            {
                return touch;
            }
            internal set
            {
                touch = value;
            }
        }

        /// <summary>
        /// Gets a value that represents the <strong><see cref="CoreInteractionFramework.TouchEventType"/></strong> 
        /// that is associated with this <strong>TouchTargetEvent</strong> event.  
        /// </summary>
        /// <remarks>The possible values include <strong>Added</strong>, <strong>Changed</strong>, <strong>Removed</strong>, 
        /// <strong>Enter</strong>, and <strong>Leave</strong>.</remarks>
        public TouchEventType EventType
        {
            get
            {
                return eventType;
            }
            internal set
            {
                eventType = value;
            }
        }

        /// <summary>
        /// Gets a value that represents details about a hit test for 
        /// certain state machines.  
        /// </summary>
        /// <remarks>For more information
        /// about which state machines require hit test details, see <strong><see cref="CoreInteractionFramework.IHitTestDetails">IHitTestDetails</see></strong>.
        /// </remarks>
        public IHitTestDetails HitTestDetails
        {
            get { return hitTestDetails; }
            internal set { hitTestDetails = value; }
        }
    }
}
