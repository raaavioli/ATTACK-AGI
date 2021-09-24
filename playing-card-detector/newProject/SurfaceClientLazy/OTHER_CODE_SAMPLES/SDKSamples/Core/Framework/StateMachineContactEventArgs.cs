using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>Represents details for events that relate to touches on an 
    /// <strong><see cref="T:CoreInteractionFramework.IInputElementStateMachine">IInputElementStateMachine</see></strong> object.</summary>
    public class StateMachineTouchEventArgs : TouchEventArgs
    {
        /// <summary>
        /// The state machine that raised this event.
        /// </summary>
        public IInputElementStateMachine StateMachine { get; private set; }

        internal StateMachineTouchEventArgs(TouchPoint touch, IInputElementStateMachine stateMachine) : base(touch)
        {
            StateMachine = stateMachine;
        }
    }
}
