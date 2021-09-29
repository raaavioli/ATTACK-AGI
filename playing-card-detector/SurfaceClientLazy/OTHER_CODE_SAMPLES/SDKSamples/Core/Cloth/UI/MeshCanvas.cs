using System;
using System.Diagnostics;
using CoreInteractionFramework;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cloth.UI
{
    /// <summary>
    /// This class provides the view for the background canvas for the Cloth sample.
    /// It demonstrates the use of the ScrollViewerStateMachine class.
    /// </summary>
    public class MeshCanvas : UIElement
    {
        // The ScrollViewerStateMachine encapsulated by this UIElement.
        private readonly ScrollViewerStateMachine viewPort;

        // Keeps track of the drawing position of the MeshCanvas.
        private Vector2 currentPosition;

        /// <summary>
        /// A parameterized public constructor for the MeshCanvas class.
        /// </summary>
        /// <param name="game">The game that to which this UIElement belongs.</param>
        /// <param name="controller">The UIController that will route touch events this UIElement's state machine.</param>
        /// <param name="texture">The name of a file containing the texture to use for this UIElement.</param>
        /// <param name="width">The desired width of this UIElement in pixels.</param>
        /// <param name="height">The desired height of this UIElemnt in pixels.</param>
        public MeshCanvas(Game game, UIController controller, string texture, int width, int height)
            : base(game, controller, texture, null, width, height, null)
        {
            IgnoreTextureSize = true;

            viewPort = new ScrollViewerStateMachine(controller, width, height);

            // Add a little elasticity so it's apparent when boundary of
            // the ScrollViewer is reached.
            viewPort.HorizontalElasticity = 0.03f;
            viewPort.VerticalElasticity = 0.03f;

            StateMachine = viewPort;
            StateMachine.Tag = this;

            viewPort.TouchDown += OnContactDown;
        }

        /// <summary>
        /// Get or set the background texture for the MeshCanvas.
        /// </summary>
        /// <remarks>Renames the Texture property of the base class.</remarks>
        public Texture2D Background
        {
            get { return Texture; }
            set { Texture = value; }
        }

        /// <summary>
        /// Read-only property that describes the relative change in the position of the MeshCanvas
        /// since the last update.
        /// </summary>
        public Vector2 Delta { get; private set; }


        /// <summary>
        /// Returns the current positon of the MeshCanvas in the viewport.
        /// </summary>
        /// <returns></returns>
        private Vector2 GetPosition()
        {
            float x = -(Width * viewPort.HorizontalViewportStartPosition);
            float y = -(Height * viewPort.VerticalViewportStartPosition);

            if (float.IsNaN(x)) { x = 0.0f; }
            if (float.IsNaN(y)) { y = 0.0f; }

            return new Vector2(x, y);
        }

        #region Overriden UIElement Methods.

        /// <summary>
        /// Extends the LoadContent method from UIElement.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void LoadContent()
        {
            base.LoadContent();

            // We can't determine our view port size until the texture has been loaded and scaled.
            // The view port size is the ratio of the GraphicsDevice viewport size and the extent
            // of the background image.
            viewPort.VerticalViewportSize = Math.Min(Game.GraphicsDevice.Viewport.Height / Height, 1.0f);
            viewPort.HorizontalViewportSize = Math.Min(Game.GraphicsDevice.Viewport.Width / Width, 1.0f);

            // Position the viewport in the top left quadrant of the canvas.
            viewPort.HorizontalViewportStartPosition = 0f;
            viewPort.VerticalViewportStartPosition = 0f;

            currentPosition = GetPosition();
        }

        /// <summary>
        /// Updates the MeshCanvas positon.
        /// </summary>
        /// <remarks>Extends the base class Update method from UIElement.</remarks>
        /// <param name="gameTime">Snapshot of game timing state.</param>
        public override void Update(GameTime gameTime)
        {
            Delta = GetPosition() - currentPosition;
            currentPosition += Delta;
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the MeshCanvas background in the specified SpriteBatch.
        /// </summary>
        /// <param name="gameTime">Snapshot of game timing state.</param>
        public override void Draw(GameTime gameTime)
        {
            if (Texture != null && SpriteBatch != null)
            {
                // Render the canvas background before we start descendants enumeration.
                // Because we want to use the different SampleState to render the background with tiling.
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap,
                                    DepthStencilState.Default, RasterizerState.CullNone);
                SpriteBatch.Draw(Texture, currentPosition, DrawingRectangle, Color.White, 0, Vector2.Zero, ActualScale, SpriteEffects, 0);
                SpriteBatch.End();
            }

            // Make sure we call base so that it renders the descendants.
            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the MeshCanvas in the specified SpriteBatch.
        /// </summary>
        /// <remarks>Overrides the base class Draw(batch) methdod.</remarks>
        /// <param name="batch">SpriteBatch for this UIElement container hierarchy.</param>
        /// <param name="gameTime">Snapshot of game timing state.</param>
        public override void Draw(SpriteBatch batch, GameTime gameTime)
        {
            // No-op, we render the background before start recursion of elements, to use specific SamplerState for tiling.
        }

        /// <summary>
        /// Returns hit test details for the ScrollViewerStateMachine.
        /// </summary>
        /// <remarks>
        /// Should return X and Y values between 0.0 and 1.0 that are proportional to where the
        /// touch is in the scrolling region, in this case the entire viewport/window.
        /// </remarks>
        /// <param name="touch">A surface touch.</param>
        /// <param name="captured">Boolean indicating that the touch was previously captured.</param>
        /// <returns>ScollViewerHitTestDetails for the touch.</returns>
        public override IHitTestDetails HitTestDetails(TouchPoint touch, bool captured)
        {
            Vector2 transformed = Vector2.Transform(new Vector2(touch.CenterX, touch.CenterY), ScreenTransform);

            float x = MathHelper.Clamp(transformed.X / (float) GraphicsDevice.Viewport.Width, 0f, 1f);
            float y = MathHelper.Clamp(transformed.Y / (float) GraphicsDevice.Viewport.Height, 0f, 1f);

            return new ScrollViewerHitTestDetails(x, y);
        }

        #endregion

        #region Event Handlers

        private void OnContactDown(object sender, StateMachineTouchEventArgs e)
        {
            Debug.Assert(e.StateMachine == viewPort);
            Controller.Capture(e.TouchPoint, e.StateMachine);
        }

        #endregion

    }
}
