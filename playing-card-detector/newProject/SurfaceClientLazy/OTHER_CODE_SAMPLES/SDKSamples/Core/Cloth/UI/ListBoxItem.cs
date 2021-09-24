using System;
using CoreInteractionFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cloth.UI
{
    /// <summary>
    /// This class implements a view of a ListBoxStateMachineItem.
    /// It extends the base class to add a few properties and methods
    /// related to how the item is rendered.
    /// </summary>
    class ListBoxItem : ListBoxStateMachineItem
    {
        // The textile theme represented by this item.
        public Textiles.Theme Theme { get; private set; }

        // The icon texture associated with this items theme.
        public Texture2D Icon { get; private set; }

        // The current viewState of this item.
        private ViewStates viewState;

        // Glow and Grow animation parameters.

        // Desired length of the glow and grow animation in milliseconds.
        private const float animationDuration = 150;

        // The targetedLength of the game Update cycle (1/60 of a second)
        private const float targetElapsedMilliSeconds = 1000f/60f;

        // Number of update cycles the glow and grow animation should take.
        private readonly int animationSteps = Convert.ToInt32(animationDuration/targetElapsedMilliSeconds);

        private int animationStepCount;
        private Vector2 animationScale = Vector2.One;
        private Vector2 scaleStep;

        // Item dimensions in pixels for glow and grow animation
        private const int minItemWidth = 40;
        private const int maxItemWidth = 50;
        private const int minItemHeight = 40;
        private const int maxItemHeight = 50;

        /// <summary>
        /// Create a ListBoxItem with the specified parameters.
        /// </summary>
        /// <param name="icon">Icon texture for this item.</param>
        /// <param name="theme">Textile theme represented by this item.</param>
        /// <param name="horizontalSize">Item's horizontal size as a fraction of the listbox viewport width.</param>
        /// <param name="verticalSize">Item's vertical size as a fraction of the listbox viewport height./param>
        public ListBoxItem(Texture2D icon, Textiles.Theme theme, float horizontalSize, float verticalSize)
            : base(null, horizontalSize, verticalSize)
        {
            Icon = icon;
            Theme = theme;
            viewState = ViewStates.Minimized;

            // Default animations steps based on the default Game.TargetElapsedType of 1/60 of a second.
            animationSteps = Convert.ToInt32(animationDuration/targetElapsedMilliSeconds);

            scaleStep = new Vector2((float)(maxItemWidth - minItemWidth)/maxItemWidth ,
                                    (float)(maxItemHeight - minItemHeight)/maxItemHeight);

            scaleStep /= (float) animationSteps;
        }


        /// <summary>
        /// Read-only property that determines if the item is animating.
        /// </summary>
        public bool IsAnimating
        {
            get { return (viewState & ViewStates.Animating) != 0; }
        }


        /// <summary>
        /// Scale to apply for glow and grow animation.
        /// </summary>
        /// <remarks>
        /// We are animating the itemHit texture from the itemDefault size (40x40)
        /// to the itemHit size (50x50) so the scale goes from 0.80 to 1.0 (or 1.0 to 0.80).
        /// </remarks>
        public Vector2 AnimationScale
        {
            get { return animationScale; }
            private set { animationScale = value; }
        }

        /// <summary>
        /// Updates the item's state.
        /// </summary>
        internal void Update()
        {
            if (IsPressed)
            {
                // Item is pressed.
                switch (viewState)
                {
                    case ViewStates.Minimized:
                        // Begin expansion animation.
                        animationStepCount = 0;
                        animationScale = new Vector2((float)minItemWidth/maxItemWidth,
                                                     (float)minItemHeight/maxItemHeight);
                        viewState = ViewStates.Expanding;
                        break;
                    case ViewStates.Contracting:
                        // Reverse the animation.
                        viewState = ViewStates.Expanding;
                        // Reset the step count so that the number of steps remaining
                        // is equal to the number of steps taken.
                        animationStepCount = animationSteps - animationStepCount;
                        AnimationScale += scaleStep;
                        animationStepCount++;
                        if (animationStepCount >= animationSteps - 1)
                        {
                            viewState = ViewStates.Maximized;
                        }
                        break;
                    case ViewStates.Expanding:
                        // Continue expansion animation.
                        // Detect finish condition.
                        AnimationScale += scaleStep;
                        animationStepCount++;
                        if (animationStepCount >= animationSteps - 1)
                        {
                            viewState = ViewStates.Maximized;
                        }
                        break;
                }
            }
            else
            {
                switch (viewState)
                {
                    case ViewStates.Maximized:
                        // Start contraction animation.
                        animationStepCount = 0;
                        AnimationScale = Vector2.One;
                        viewState = ViewStates.Contracting;
                        break;
                    case ViewStates.Expanding:
                        // Reverse the animation.
                        viewState = ViewStates.Contracting;
                        // Reset the step count so that the number of steps remaining
                        // is equal to the number of steps taken.
                        animationStepCount = animationSteps - animationStepCount;
                        AnimationScale -= scaleStep;
                        animationStepCount++;
                        // Detect finish condition.
                        if (animationStepCount >= animationSteps - 1)
                        {
                            viewState = ViewStates.Minimized;
                        }
                        break;
                    case ViewStates.Contracting:
                        // Continue contraction animation.
                        AnimationScale -= scaleStep;
                        animationStepCount++;
                        // Detect finish condition.
                        if (animationStepCount >= animationSteps - 1)
                        {
                            viewState = ViewStates.Minimized;
                        }
                        break;
                }
            }
        }
    }
}
