using System;
using System.Collections.Generic;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Defines an interface for working with state machines that update their state based on the 
    /// <strong>Update</strong>
    /// method being called from some outside controller.
    /// </summary>
    public interface IInputElementStateMachine
    {
        /// <summary>
        /// Changes the internal state based on the specified list of touch events.
        /// </summary>
        /// <param name="touches">The list of touches used to update the state for this state
        /// machine.</param>
        void Update(Queue<TouchTargetEvent> touches);

        /// <summary>
        /// Called when this state machine captures a new touch.
        /// </summary>
        /// <param name="touch">A touch that this state machine captures.</param>
        void OnGotTouchCapture(TouchPoint touch);

        /// <summary>
        /// Called when this state machine release a currently captured touch.
        /// </summary>
        /// <param name="touch">The touch that is no longer captured.</param>
        void OnLostTouchCapture(TouchPoint touch);

        /// <summary>
        /// Provides type information about a hit test.
        /// </summary>
        Type TypeOfHitTestDetails { get; }

        /// <summary>
        /// Gets or sets the controller to use with this state machine.
        /// </summary>
        UIController Controller { get; set; }

        /// <summary>
        /// Gets or sets the tag that is associated with this state machine.
        /// </summary>
        object Tag { get; set; }
    }
}
