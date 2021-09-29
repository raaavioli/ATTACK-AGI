using CoreInteractionFramework;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;

namespace Cloth.UI
{
    /// <summary>
    /// Provides a concrete instance of UIElementStateMachine to be encapsulated
    /// by the Textiles UIElement. 
    /// </summary>
    /// <remarks>
    /// The only functionality required here is to handle ContactDown and ContactUp to 
    /// capture and release contacts.  All other manipulations are handled in the 
    /// TextileManipulationComponent.
    /// </remarks>
    public class TextilesStateMachine : UIElementStateMachine
    {

        // The UIElement containing this statemachine.
        // Also contains the TextileManipulationComponent.
        private readonly Textiles textiles;

        /// <summary>
        /// Creates a TextilesStatemachine.
        /// </summary>
        /// <param name="controller">The UIController for this state machine.</param>
        /// <param name="textiles">The UIElement encapsulating this state machine.</param>
        public TextilesStateMachine(UIController controller, Textiles textiles)
            : base(controller, 0, 0)
        {
            this.textiles = textiles;
        }


        /// <summary>
        /// Handles the ContactDown event.
        /// </summary>
        /// <param name="touchEvent">The contact that hit element.</param>
        protected override void OnTouchDown(TouchTargetEvent touchEvent)
        {
            if (textiles.ActiveTouches == null) 
            { 
                return; 
            }

            TouchPoint contact = touchEvent.Touch;
            Controller.Capture(contact, this);

            Vector2 worldVector;
            if (textiles.ActiveTouches.TryGetValue(contact.Id, out worldVector))
            {
                 textiles.TextileComponent.TouchAdd(contact.Id, worldVector); 
            }

        }


        /// <summary>
        /// A contact was removed from the element.
        /// </summary>
        /// <param name="touchEvent">The contact that was removed.</param>
        protected override void OnTouchUp(TouchTargetEvent touchEvent)
        {
            if (textiles.ActiveTouches == null)
            {
                return;
            }

            TouchPoint contact = touchEvent.Touch;

            // Attempt to remove the touch regardless the touch is captured by a textile or not 
            // since the capture must go away as a touch up event happens.
            textiles.TextileComponent.TryTouchRemove(contact.Id);
 
            if (TouchesCaptured.Contains(contact.Id))
            {
                Controller.Release(contact);
            }
        }


        /// <summary>
        /// Handles the ContactChanged event.
        /// </summary>
        /// <param name="touchEvent">The Contact that changed.</param>
        protected override void OnTouchMoved(TouchTargetEvent touchEvent)
        {
            // Suppress OnContactChanged events.
            // These will be handled in the textileComponent.
        }


        /// <summary>
        /// A contact has entered the element.
        /// </summary>
        /// <param name="touchEvent">The contact that entered.</param>
        protected override void OnTouchEnter(TouchTargetEvent touchEvent)
        {
            // Suppress OnContactEnter.
        }

        /// <summary>
        /// A contact has left the element.
        /// </summary>
        /// <param name="touchEvent">The contact that left.</param>
        protected override void OnTouchLeave(TouchTargetEvent touchEvent)
        {
            // Suppress OnContactLeave.
        }

    }


}
