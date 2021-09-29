using CoreInteractionFramework;

namespace Cloth.UI
{
    /// <summary>
    /// This class provides a realization of the abstract UIElementStateMachine that adds
    /// no additional functionality.
    /// </summary>
    class NullStateMachine : UIElementStateMachine
    {

        /// <summary>
        /// Creates a NullStateMachine.
        /// </summary>
        /// <param name="controller">The UIcontroller assocatiated with this state machine.</param>
        /// <param name="width">Number of pixels in horizontal axis.</param>
        /// <param name="height">Number of pixels in vertical axis</param>
        public NullStateMachine(UIController controller, int width, int height)
            : base(controller, width, height)
        {
            // Empty.
        }

    }
}
