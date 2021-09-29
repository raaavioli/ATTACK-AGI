using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Cloth.UI
{
    /// <summary>
    /// This class extends to UIButton class to create an animated button
    /// that glows and grows.
    /// </summary>
    public class Button : UIButton
    {

        // Glow and grow animation parameters.
        private readonly float animationDuration = 150;                  // duration in milliseconds.
        private readonly float targetElapsedMilliSeconds = 1000f/60f;    // 1/60 of a second.
        private int animationStepCount;
        private Vector2 animationScale = Vector2.One;
        private Vector2 scaleStep;

        // Item dimensions in pixels for glow and grow animation
        private const int minItemWidth = 48;
        private const int maxItemWidth = 58;
        private const int minItemHeight = 25;
        private const int maxItemHeight = 35;

        /// <summary>
        /// Creates a Button element.
        /// </summary>
        /// <param name="parent">The containing UIElement for this button (required).</param>
        /// <param name="position">The position of this button's center relative to its parent center.</param>
        /// <param name="width">The render width of the button in pixels.</param>
        /// <param name="height">The render height of this button in pixels.</param>
        public Button(UIElement parent, Vector2 position, int width, int height)
            : base(parent, position, width, height)
        {
            ViewState = ViewStates.Minimized;

            // Default animations steps based on the default Game.TargetElapsedType of 1/60 of a second.
            AnimationSteps = Convert.ToInt32(animationDuration/targetElapsedMilliSeconds);

            scaleStep = new Vector2((float)(maxItemWidth - minItemWidth)/maxItemWidth ,
                                    (float)(maxItemHeight - minItemHeight)/maxItemHeight);

            scaleStep /= (float) AnimationSteps;
        }

        /// <summary>
        /// Read-write property that gets or sets the current viewState of this Button.
        /// </summary>
        protected ViewStates ViewState { get; private set; }


        /// <summary>
        /// Read-only property that determines if the item is animating.
        /// </summary>
        protected bool IsAnimating
        {
            get { return (ViewState & ViewStates.Animating) != 0; }
        }

        /// <summary>
        /// Number of update cycles the glow and grow animation should take.
        /// </summary>
        /// <remarks>
        /// animationSteps =  animationDuration / updateCycleTime.
        /// </remarks>
        protected int AnimationSteps { get; set; }

        /// <summary>
        /// Scale to apply for glow and grow animation.
        /// </summary>
        /// <remarks>
        /// We are animating the itemHit texture from the itemDefault size (48x25)
        /// to the itemHit size (58x35)
        /// </remarks>
        protected Vector2 AnimationScale
        {
            get { return animationScale; }
            private set { animationScale = value; }
        }

        /// <summary>
        /// Updates the button element.
        /// </summary>
        /// <remarks>Extends the Update method from UIButton.</remarks>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsPressed)
            {
                // Item is pressed.
                switch (ViewState)
                {
                    case ViewStates.Minimized:
                        // Begin expansion animation.
                        animationStepCount = 0;
                        animationScale = new Vector2((float)minItemWidth / maxItemWidth,
                                                     (float)minItemHeight / maxItemHeight);
                        ViewState = ViewStates.Expanding;
                        break;
                    case ViewStates.Contracting:
                        // Reverse the animation.
                        ViewState = ViewStates.Expanding;
                        // Reset the step count so that the number of steps remaining
                        // is equal to the number of steps taken.
                        animationStepCount = AnimationSteps - animationStepCount;
                        AnimationScale += scaleStep;
                        animationStepCount++;
                        if (animationStepCount >= AnimationSteps - 1)
                        {
                            ViewState = ViewStates.Maximized;
                        }
                        break;
                    case ViewStates.Expanding:
                        // Continue expansion animation.
                        // Detect finish condition.
                        AnimationScale += scaleStep;
                        animationStepCount++;
                        if (animationStepCount >= AnimationSteps - 1)
                        {
                            ViewState = ViewStates.Maximized;
                        }
                        break;
                }
            }
            else
            {
                switch (ViewState)
                {
                    case ViewStates.Maximized:
                        // Start contraction animation.
                        animationStepCount = 0;
                        AnimationScale = Vector2.One;
                        ViewState = ViewStates.Contracting;
                        break;
                    case ViewStates.Expanding:
                        // Reverse the animation.
                        ViewState = ViewStates.Contracting;
                        // Reset the step count so that the number of steps remaining
                        // is equal to the number of steps taken.
                        animationStepCount = AnimationSteps - animationStepCount;
                        AnimationScale -= scaleStep;
                        animationStepCount++;
                        // Detect finish condition.
                        if (animationStepCount >= AnimationSteps - 1)
                        {
                            ViewState = ViewStates.Minimized;
                            AnimationScale = Vector2.One;
                        }
                        break;
                    case ViewStates.Contracting:
                        // Continue contraction animation.
                        AnimationScale -= scaleStep;
                        animationStepCount++;
                        // Detect finish condition.
                        if (animationStepCount >= AnimationSteps - 1)
                        {
                            ViewState = ViewStates.Minimized;
                            AnimationScale = Vector2.One;
                        }
                        break;
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the button element.
        /// </summary>
        /// <param name="batch">A SpriteBatch to use for drawing.</param>
        /// <param name="texture">A Texture2D to draw.</param>
        private void Draw(SpriteBatch batch, Texture2D texture)
        {
            Vector2 origin = CenterOf(texture);
            batch.Draw(texture, TransformedCenter, null, SpriteColor, ActualRotation, origin,
                       AnimationScale * ActualScale, SpriteEffects, LayerDepth);
        }


        /// <summary>
        /// Draws the button.
        /// </summary>
        /// <remarks>Overrides Draw method from UIButton.</remarks>
        /// <param name="batch">A SpriteBatch to use for drawing.</param>
        /// <param name="gameTime">A snapshot of game timing state.</param>
        public override void Draw(SpriteBatch batch, GameTime gameTime)
        {
            if (IsPressed || IsAnimating)
            {
                Draw(batch, PressedImage);
            }
            else
            {
                Draw(batch, DefaultImage);
            }
        }
    }

}
