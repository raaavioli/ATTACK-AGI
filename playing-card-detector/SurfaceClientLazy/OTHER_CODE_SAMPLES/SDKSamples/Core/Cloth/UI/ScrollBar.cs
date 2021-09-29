using System;
using CoreInteractionFramework;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cloth.UI
{
    /// <summary>
    /// This class implements the view of the scrollbar element for the heads up display
    /// in the cloth sample.  This implmentation is specific to the cloth sample and
    /// not intended as the basis for a generalized scrollbar view.
    /// </summary>
    internal class ScrollBar : UIElement
    {
        // UIElementStateMachine encapsulated by this ScrollBar.
        private ScrollBarStateMachine scrollBarStateMachine;

        // The ListBoxStateMachine associated with this ScrollBar
        private readonly ListBoxStateMachine listBox;

        // Scrollbar scissor rectangle.
        // Used in hide and reveal animation.
        private Rectangle scissorRectangle;

        private static readonly RasterizerState ScrollBarRasterizerState;

        // Textures used to render the scrollbar.
        private Texture2D scrollBarBackground;
        private Texture2D thumbDefaultBackground;
        private Texture2D thumbDefaultLeft;
        private Texture2D thumbDefaultMiddle;
        private Texture2D thumbDefaultRight;
        private Texture2D thumbExpandedBackground;
        private Texture2D thumbExpandedLeft;
        private Texture2D thumbExpandedMiddle;
        private Texture2D thumbExpandedRight;

        private readonly float scrollBarLength;
        private Vector2 thumbCenter;
        private float thumbSize;

        private int maxWidth;    // Maximim width of a scrollbar element in pixels.
        private int maxHeight;   // Maximum height of a scrollbar element in pixels.

        // Scrollbar Thumb animation.
        private readonly TimeSpan thumbAnimationDuration = TimeSpan.FromMilliseconds(125);
        private TimeSpan thumbAnimationStart;
        private ViewStates thumbViewState = ViewStates.Minimized;

        // Adjust LayerDepth by this amount to raise or lower sprites.
        private const float layerOffset = 0.002f;

        // Maximum Flick Velocity (device independent units per millisecond)
        private const float MaximumFlickVelocity = 12 * 96.0f / 1000.0f;


        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ScrollBar()
        {
            ScrollBarRasterizerState = new RasterizerState();
            ScrollBarRasterizerState.ScissorTestEnable = true;
        }

        /// <summary>
        /// Creates a ScrollBar HUD element with the specified parent UIElement.
        /// </summary>
        public ScrollBar(UIElement parent,  Vector2? position, float length,
                         ListBoxStateMachine listBox, ScrollBarStateMachine scrollBar)
            : this(parent.Game, parent.Controller, position, length, listBox, scrollBar, parent)
        {
            // Empty.
        }


        /// <summary>
        /// Creates a ScrollBar HUD element with the specified parameters.
        /// </summary>
        public ScrollBar(Game game, UIController controller, Vector2? position, float length,
                         ListBoxStateMachine listBox,
                         ScrollBarStateMachine scrollBar, UIElement parent)
            : base(game, controller, null, position,
                   scrollBar.NumberOfPixelsInHorizontalAxis,
                   scrollBar.NumberOfPixelsInVerticalAxis,
                   parent)
        {
            if (scrollBar.Orientation != Orientation.Horizontal)
            {
                throw new InvalidOperationException(Properties.Resources.ScrollBarStateMachineShouldBeHorizontal);
            }
            this.listBox = listBox;
            StateMachine = scrollBar;
            StateMachine.Tag = this;
            StateMachine.MaximumFlickVelocity = MaximumFlickVelocity;
            scrollBarLength = length;
        }

        #endregion Constructors

        /// <summary>
        /// Gets the StateMachine for this ScrollBar.
        /// </summary>
        /// <remarks>Hides the base property that returns a UIElementStateMachine.</remarks>
        public new ScrollBarStateMachine StateMachine
        {
            get
            {
                return scrollBarStateMachine;
            }
            protected set
            {
                scrollBarStateMachine = value;
                base.StateMachine = value;
            }
        }

        /// <summary>
        /// Loads all of the content for this ScrollBar.
        /// </summary>
        /// <remarks>Extends LoadContent method from UIElement.</remarks>
        protected override void LoadContent()
        {
            scrollBarBackground = TextureFromFile(@"Content\ScrollBarBackground.png");
            thumbDefaultBackground = TextureFromFile(@"Content\ScrollBarThumbDefaultBackground.png");
            thumbDefaultLeft = TextureFromFile(@"Content\ScrollBarThumbDefaultLeft.png");
            thumbDefaultMiddle = TextureFromFile(@"Content\ScrollBarThumbDefaultMiddletile.png");
            thumbDefaultRight = TextureFromFile(@"Content\ScrollBarThumbDefaultRight.png");
            thumbExpandedBackground = TextureFromFile(@"Content\ScrollBarThumbExpandedBackground.png");
            thumbExpandedLeft = TextureFromFile(@"Content\ScrollBarThumbExpandedLeft.png");
            thumbExpandedMiddle = TextureFromFile(@"Content\ScrollBarThumbExpandedMiddletile.png");
            thumbExpandedRight = TextureFromFile(@"Content\ScrollBarThumbExpandedRight.png");

            // Default thumbSize in pixels.
            thumbSize = scrollBarLength * (StateMachine.ThumbSize / StateMachine.ViewportSize);

            Texture = scrollBarBackground;

            maxWidth = scrollBarBackground.Width;
            maxHeight = thumbExpandedMiddle.Height;

            scissorRectangle = new Rectangle(Convert.ToInt32(Left), Convert.ToInt32(Top), maxWidth, maxHeight);

            base.LoadContent();
        }

        /// <summary>
        /// Re-orients items not affected by SpriteBatch screenTransform.
        /// </summary>
        /// <remarks>Overrides OnOrientationReset method from UIElement.</remarks>
        /// <param name="newOrientation">The new user orientation.</param>
        /// <param name="transform">The transform associated with the new orientation.</param>
        protected override void OnOrientationReset(UserOrientation newOrientation, Matrix transform)
        {
            // Compute new scissorRectange based on newOrientation.
            if (newOrientation == UserOrientation.Top)
            {
                 Vector2 topLeft = new Vector2(scissorRectangle.X, scissorRectangle.Y);
                 topLeft = Vector2.Transform(topLeft, transform);
                 scissorRectangle = new Rectangle(Convert.ToInt32(topLeft.X) - maxWidth,
                                                  Convert.ToInt32(topLeft.Y) - maxHeight,
                                                  maxWidth, maxHeight);
            }
            else
            {
                 scissorRectangle = new Rectangle(Convert.ToInt32(Left), Convert.ToInt32(Top), maxWidth, maxHeight);
            }
        }

        /// <summary>
        /// Updates ScrollBar state.
        /// </summary>
        /// <remarks>Extends Update method from UIElement.</remarks>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Thumb animation state.
            if (scrollBarStateMachine.SelectedPart == ScrollBarPart.None)
            {
                switch(thumbViewState)
                {
                    case ViewStates.Maximized:
                        // Start contraction animation.
                        thumbAnimationStart = gameTime.TotalGameTime;
                        thumbViewState = ViewStates.Contracting;
                        break;
                    case ViewStates.Expanding:
                        // Reverse the animation.
                        // Adjust the animationStart time so that the time remaining
                        // is equal the time the animation has run in the opposing direction.
                        TimeSpan remaining = thumbAnimationStart + thumbAnimationDuration - gameTime.TotalGameTime;
                        thumbAnimationStart = gameTime.TotalGameTime - remaining;
                        thumbViewState = ViewStates.Contracting;
                        break;
                    case ViewStates.Contracting:
                        // Continue contraction animation.
                        // Detect finish condition.
                        if (gameTime.TotalGameTime > thumbAnimationStart + thumbAnimationDuration)
                        {
                            thumbViewState = ViewStates.Minimized;
                        }
                        break;
                }
            }
            else
            {
                // Scrollbar is Selected.
                switch (thumbViewState)
                {
                    case ViewStates.Minimized:
                        // Begin expansion animation.
                        thumbAnimationStart = gameTime.TotalGameTime;
                        thumbViewState = ViewStates.Expanding;
                        break;
                     case ViewStates.Contracting:
                        // Reverse the animation.
                        // Adjust the animationStart time so that the time remaining
                        // is equal the time the animation has run in the opposing direction.
                        TimeSpan remaining = thumbAnimationStart + thumbAnimationDuration - gameTime.TotalGameTime;
                        thumbAnimationStart = gameTime.TotalGameTime - remaining;
                        thumbViewState = ViewStates.Expanding;
                        break;
                     case ViewStates.Expanding:
                        // Continue expansion animation.
                        // Detect finish condition.
                        if (gameTime.TotalGameTime > thumbAnimationStart + thumbAnimationDuration)
                        {
                            thumbViewState = ViewStates.Maximized;
                        }
                        break;
                }
            }

            // Hide and reveal animation.

            if (scrollBarViewState == ViewStates.Contracting)
            {
                alphaValue = MathHelper.Clamp(alphaValue - alphaStep, 0f, 1f);
                translate -= translateStep;
                if (alphaValue == 0f)
                {
                    // Animation complete.
                    scrollBarViewState = ViewStates.Minimized;
                    Visible = false;
                    Active = false;
                }
            }

            if (scrollBarViewState == ViewStates.Expanding)
            {
                alphaValue = MathHelper.Clamp(alphaValue + alphaStep, 0f, 1f);
                translate += translateStep;
                if (alphaValue == 1f)
                {
                    // Animation complete.
                    scrollBarViewState = ViewStates.Maximized;
                    translate = 0f;
                    Visible = true;
                    Active = true;
                }
            }

            // Set the spriteColor for hide and reveal animation.
            Vector4 color = spriteColor.ToVector4();
            color.W = alphaValue;
            spriteColor = new Color(color);

            base.Update(gameTime);
        }

        #region Drawing

        /// <summary>
        /// Draws the ScrollBar UIElement.
        /// </summary>
        /// <remarks>Overrides Draw method from UIElement.</remarks>
        /// <param name="batch">SpriteBatch for this UIElement container hierarchy.</param>
        /// <param name="gameTime">Snapshot of game timing state.</param>
        public override void Draw(SpriteBatch batch, GameTime gameTime)
        {
            // End the current SpriteBatch because we are going to make
            // changes to the RenderState.
            batch.End();

            // Set the ScissorRectangle and enable scissor test.
            GraphicsDevice.ScissorRectangle = scissorRectangle;
            GraphicsDevice.RasterizerState = ScrollBarRasterizerState;

            batch.Begin(SpriteSortMode, SpriteBlendState, null, null, null, null, ScreenTransform);

            switch (thumbViewState)
            {
                case ViewStates.Minimized:
                case ViewStates.Maximized:
                    DrawThumbStatic(batch);
                    break;
                case ViewStates.Contracting:
                case ViewStates.Expanding:
                    TimeSpan duration = gameTime.TotalGameTime - thumbAnimationStart;
                    float value = (float)(duration.TotalMilliseconds / thumbAnimationDuration.TotalMilliseconds);
                    if (thumbViewState == ViewStates.Contracting)
                    {
                        value = 1f - value;
                    }
                    DrawThumbAnimating(batch, value);
                    break;
            }

            batch.End();

            // Restart the SpriteBatch using our parent's batch parameters.
            batch.Begin(Parent.SpriteSortMode, Parent.SpriteBlendState, null, null, null, null, Parent.ScreenTransform);

        }

        /// <summary>
        /// Draws the ScrollBar with the thumb in the default or expanded state.
        /// </summary>
        /// <param name="batch">SpriteBatch for this UIElement container hierarchy.</param>
        private void DrawThumbStatic(SpriteBatch batch)
        {
            Vector2 origin;
            Vector2 position;
            Texture2D thumbBackground, thumbLeft, thumbMiddle, thumbRight;

            if (scrollBarStateMachine.SelectedPart == ScrollBarPart.None)
            {
                thumbBackground = thumbDefaultBackground;
                thumbLeft = thumbDefaultLeft;
                thumbMiddle = thumbDefaultMiddle;
                thumbRight = thumbDefaultRight;
            }
            else
            {
                thumbBackground = thumbExpandedBackground;
                thumbLeft = thumbExpandedLeft;
                thumbMiddle = thumbExpandedMiddle;
                thumbRight = thumbExpandedRight;
            }

            // Adjust drawing center for hide and reveal animation.
            Vector2 drawingCenter = TransformedCenter;
            drawingCenter.Y += translate;

            // The drawing center for the scrollbar thumb.
            if (scrollBarViewState == ViewStates.Maximized)
            {
                thumbCenter = new Vector2(
                    drawingCenter.X + (StateMachine.ThumbStartPosition - 0.5f) * scrollBarLength + thumbSize / 2f,
                    drawingCenter.Y);
            }
            else
            {
                // Pin the thumb center while scrollbar hide/reveal animation is running.
                thumbCenter = new Vector2(thumbCenter.X, drawingCenter.Y);
            }

            float elasticMargin = GetElasticMargin();
            thumbCenter.X += elasticMargin;

            // Amount to scale the thumb middle texture.  This is the same as the display width of
            // middle texture because the source texture is 1 pixel wide
            Vector2 thumbScale = GetThumbScale(elasticMargin, Math.Max(thumbLeft.Width, thumbRight.Width));

            // Draw the scrollbar background.
            origin = CenterOf(scrollBarBackground);
            batch.Draw(scrollBarBackground, drawingCenter, null, SpriteColor, ActualRotation, origin,
              ActualScale, SpriteEffects, LayerDepth);

            // Draw the thumb background
            origin = CenterOf(thumbBackground);
            batch.Draw(thumbBackground, drawingCenter, null, SpriteColor, ActualRotation, origin,
                ActualScale, SpriteEffects, LayerDepth);

            // Draw the left thumb tile.
            origin = CenterOf(thumbLeft);
            position = thumbCenter;
            position.X -= thumbScale.X / 2f + thumbLeft.Width / 2f;
            batch.Draw(thumbLeft, position, null, SpriteColor, ActualRotation, origin,
                       ActualScale, SpriteEffects, LayerDepth);
    
            // Draw the middle thumb tile.
            origin = CenterOf(thumbMiddle);
            batch.Draw(thumbMiddle, thumbCenter, null, SpriteColor, ActualRotation, origin,
                thumbScale, SpriteEffects, LayerDepth);

            // Draw the right thumb tile.
            origin = CenterOf(thumbRight);
            position = thumbCenter;
            position.X += thumbScale.X / 2f + thumbRight.Width / 2f;
            batch.Draw(thumbRight, position, null, SpriteColor, ActualRotation, origin,
                       ActualScale, SpriteEffects, LayerDepth);
       
        }

        /// <summary>
        /// Draw the ScrollBar during the thumb animation.
        /// </summary>
        /// <param name="batch">SpriteBatch for this UIElement container hierarchy.</param>
        /// <param name="animationValue">A float value between 0f and 1f that represents the current
        /// state of the animation.  It varies from 0 to 1 as the thumb expands and from 1 to 0 as
        /// the it contracts.</param>
        private void DrawThumbAnimating(SpriteBatch batch, float animationValue)
        {
            Vector2 origin;
            Vector2 position;
            Texture2D thumbBackground, thumbLeft, thumbMiddle, thumbRight;

            Vector2 thumbAnimation = Vector2.One;

            // Compute the scaling need for the thumb animation.
            float D = thumbDefaultMiddle.Height;
            float E = thumbExpandedMiddle.Height;
            thumbAnimation.Y = (D + (E - D) * animationValue) / E;   // Animates from D/E to E/E.

            thumbLeft = thumbExpandedLeft;
            thumbMiddle = thumbExpandedMiddle;
            thumbRight = thumbExpandedRight;

            thumbBackground = thumbExpandedBackground;
            if (scrollBarStateMachine.SelectedPart == ScrollBarPart.None)
            {
                thumbBackground = thumbDefaultBackground;
            }

            // Adjust drawing center for hide and reveal animation.
            Vector2 drawingCenter = TransformedCenter;
            drawingCenter.Y += translate;

            // The drawing center for the scrollbar thumb.
            if (scrollBarViewState == ViewStates.Maximized)
            {
                thumbCenter = new Vector2(
                    drawingCenter.X + (StateMachine.ThumbStartPosition - 0.5f) * scrollBarLength + thumbSize / 2f,
                    drawingCenter.Y);
            }
            else
            {
                // Pin the thumb center while scrollbar hide/reveal animation is running.
                thumbCenter = new Vector2(thumbCenter.X, drawingCenter.Y);
            }

            float elasticMargin = GetElasticMargin();
            thumbCenter.X += elasticMargin;

            // Amount to scale the thumb middle texture.  This is the same as the display width of
            // middle texture because the source texture is 1 pixel wide
            Vector2 thumbScale = GetThumbScale(elasticMargin, Math.Max(thumbLeft.Width, thumbRight.Width));

            // Draw the scrollbar background.
            origin = CenterOf(scrollBarBackground);
            batch.Draw(scrollBarBackground, drawingCenter, null, SpriteColor, ActualRotation, origin,
                       ActualScale, SpriteEffects, LayerDepth);

            // Draw the thumb background
            origin = CenterOf(thumbBackground);
            batch.Draw(thumbBackground, drawingCenter, null, SpriteColor, ActualRotation, origin,
                       ActualScale, SpriteEffects, LayerDepth);

            float zOrder = MathHelper.Clamp(LayerDepth - layerOffset, 0f, 0f);

            // Draw the left thumb tile.
            origin = CenterOf(thumbLeft);
            position = thumbCenter;
            position.X -= thumbScale.X / 2f + thumbLeft.Width / 2f;
            batch.Draw(thumbLeft, position, null, SpriteColor, ActualRotation, origin,
                       thumbAnimation * ActualScale, SpriteEffects, zOrder);

            // Draw the middle thumb tile.
            origin = CenterOf(thumbMiddle);
            batch.Draw(thumbMiddle, thumbCenter, null, SpriteColor, ActualRotation, origin,
                       thumbAnimation * thumbScale, SpriteEffects, zOrder);

            // Draw the right thumb tile.
            origin = CenterOf(thumbRight);
            position = thumbCenter;
            position.X += thumbScale.X / 2f + thumbRight.Width / 2f;
            batch.Draw(thumbRight, position, null, SpriteColor, ActualRotation, origin,
                       thumbAnimation * ActualScale, SpriteEffects, zOrder);
            
        }
        /// <summary>
        /// Computes the scale factor for the thumb middle texture.  
        /// This is based on the display width of the thumb.
        /// </summary>
        /// <param name="elasticMargin"></param>
        /// <param name="endWidth">Width in pixels one of the end tiles that make up the thumb.</param>
        /// <returns></returns>
        private Vector2 GetThumbScale(float elasticMargin, int endWidth)
        {
            float thumbMiddle = thumbSize - 2 * endWidth - Math.Abs(elasticMargin);
            
            return ((thumbMiddle > 1) ? new Vector2(thumbMiddle, 1.0f) : Vector2.One) * ActualScale;
        }

        /// <summary>
        /// Returns the number of pixels and direction that the scrollbar thumb
        /// size and position must be adjusted by to account for the elastic margin
        /// of the scrollbar's list box.
        /// </summary>
        /// <returns></returns>
        private float GetElasticMargin()
        {
            float elasticMargin = 0.0f;
            if (listBox.HorizontalViewportStartPosition < 0)
            {
                elasticMargin = listBox.HorizontalViewportStartPosition;
            }
            float maxRight = 1 - listBox.HorizontalViewportSize;
            if (listBox.HorizontalViewportStartPosition > maxRight)
            {
                elasticMargin = listBox.HorizontalViewportStartPosition - maxRight;

            }
            elasticMargin *= scrollBarLength;
            return elasticMargin;
        }

        #endregion Drawing

        /// <summary>
        /// Computes hit test details for the scrollbar.
        /// </summary>
        /// <param name="touch"></param>
        /// <param name="captured"></param>
        /// <returns>ScrollBarHitTestDetails</returns>
        public override IHitTestDetails HitTestDetails(TouchPoint touch, bool captured)
        {
            Vector2 transformed = Vector2.Transform(new Vector2(touch.CenterX, touch.CenterY), ScreenTransform);
            int x = Convert.ToInt32(transformed.X);
            int y = Convert.ToInt32(transformed.Y);

            float position;

            if (scrollBarStateMachine.Orientation == Orientation.Horizontal)
            {
                position = (x - DrawingRectangle.Left) / (float)DrawingRectangle.Width;
            }
            else
            {
                position = (y - DrawingRectangle.Top) / (float)DrawingRectangle.Height;
            }

            position = MathHelper.Clamp(position, 0f, 1f);

            return new ScrollBarHitTestDetails(position);
        }

        #region Hide and Reveal Animation

        private ViewStates scrollBarViewState = ViewStates.Maximized;
        private float alphaValue = 1.0f;
        private float alphaStep;
        private float translate;
        private float translateStep;

        /// <summary>
        /// Begin scroll bar hide animation.
        /// </summary>
        /// <param name="animationSteps">Number of update cycles the animation should take.</param>
        public void Hide(float animationSteps)
        {
            System.Diagnostics.Debug.Assert(scrollBarViewState == ViewStates.Maximized || scrollBarViewState == ViewStates.Expanding);
            alphaStep = 1f / animationSteps;
            translateStep = scrollBarBackground.Height / animationSteps;
            scrollBarViewState = ViewStates.Contracting;
        }

        /// <summary>
        /// Begin scroll bar reveal animation.
        /// </summary>
        /// <param name="animationSteps">Number of update cycles the animation should take.</param>
        public void Reveal(float animationSteps)
        {
            alphaStep = 1f / animationSteps;
            translateStep = scrollBarBackground.Height / animationSteps;
            scrollBarViewState = ViewStates.Expanding;
            Visible = true;
        }

        #endregion

    }
}
