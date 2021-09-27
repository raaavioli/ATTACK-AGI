using CoreInteractionFramework;
using Microsoft.Xna.Framework;

namespace Cloth.UI
{
    /// <summary>
    /// This class provides a realization of the abstract UIElement class that adds
    /// no additional functionality.  It serves as a container for other UIElements
    /// to aid in layout.
    /// </summary>
    public class UIContainer : UIElement
    {
        readonly NullStateMachine nullStateMachine;

        /// <summary>
        /// Create a UIContainer with the specified parameters.
        /// </summary>
        /// <param name="game">Game to which the element belongs.</param>
        /// <param name="controller">UIController associated with this element.</param>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="parent"></param>
        public UIContainer(Game game,
                           UIController controller,
                           string texture,
                           Vector2? position, int width, int height, UIElement parent)
            : base(game, controller, texture, position, width, height, parent)
        {
            nullStateMachine = new NullStateMachine(controller, width, height);
            StateMachine = nullStateMachine;
            StateMachine.Tag = this;
        }

    }
}
