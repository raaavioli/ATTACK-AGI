using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TextileManipulation
{
    public class TextileManipulationComponent : DrawableGameComponent
	{
        private readonly IList<Textile> textiles = new List<Textile>();
        private readonly IList<Textile> selectedTextiles = new List<Textile>();

        private Texture2D backgroundTexture;
        private SpriteBatch spriteBatch;
        private Rectangle screenRect;
        private BasicEffect effectLines;
        private BasicEffect effectTexture;

        private Dictionary<int, Vector2> activeTouches;
        private Matrix projectionMatrix;

        public TextileManipulationComponent(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            projectionMatrix = Matrix.CreateOrthographicOffCenter(0, screenRect.Width, screenRect.Height, 0, 1f, -1f);
            WorldMatrix = Matrix.Identity;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    DisposeGraphics();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override void LoadContent()
        {
            DisposeGraphics();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Game.Content.RootDirectory = "TextileManipulationContent";
            effectLines = new BasicEffect(GraphicsDevice);
            effectLines.VertexColorEnabled = true;
            effectTexture = new BasicEffect(GraphicsDevice);
            effectTexture.TextureEnabled = true;

            base.LoadContent();
        }

        private void DisposeGraphics()
        {
            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }

            if (effectLines != null)
            {
                effectLines.Dispose();
                effectLines = null;
            }

            if (effectTexture != null)
            {
                effectTexture.Dispose();
                effectTexture = null;
            }
        }

        public IList<Textile> Textiles
        {
            get { return textiles; }
        }

        public IList<Textile> SelectedTextiles
        {
            get { return selectedTextiles; }
        }

        /// <summary>
        /// Get a capturing Textile object for the given touchId
        /// </summary>
        /// <param name="touchId">Id of captured touch</param>
        /// <param name="capturingTextile">Textile which is capturing the touch</param>
        /// <returns></returns>
        public bool TryFindCapturingTextile(int touchId, out Textile capturingTextile)
        {
            foreach (Textile textile in textiles)
            {
                if (textile.IsCapturing(touchId))
                {
                    capturingTextile = textile;
                    return true;
                }
            }

            capturingTextile = null;
            return false;
        }

        /// <summary>
        /// Return true if touch position hits one of the contained textiles.
        /// </summary>
        /// <param name="position">Touch positon in World coordinates.</param>
        /// <returns></returns>
        public bool HitTest(Vector2 position)
        {
            for (int index = textiles.Count - 1; index >= 0; index--)
            {
                if (textiles[index].HitTest(position))
                {
                    return true;
                }
            }
            return false;
        }


        public void TouchAdd(int touchId, Vector2 position)
        {
            Textile targetCloth = null;
            for (int index = textiles.Count - 1; index >= 0; index--)
            {
                Textile textile = textiles[index];
                if (textile.TryAddTouch(touchId, position))
                {
                    targetCloth = textile;
                    selectedTextiles.Add(textile);
                    break;
                }
            }

            if (targetCloth != null && textiles.IndexOf(targetCloth) != textiles.Count - 1)
            {
                textiles.Remove(targetCloth);
                textiles.Add(targetCloth);
            }
        }

        public bool TryTouchRemove(int touchId)
        {
            bool removed = false;
            foreach (Textile textile in textiles)
            {
                removed |= textile.TryRemoveTouch(touchId);

                if (selectedTextiles.Contains(textile) && textile.CapturedTouchCount == 0)
                {
                    selectedTextiles.Remove(textile);
                }
            }

            return removed;
        }

        public void TouchTap(Vector2 position)
        {
            foreach (Textile textile in textiles)
            {
                textile.Ripple(position, TextileConstants.RippleDiameter, -1f);
            }
        }

        public void SetActiveTouches(Dictionary<int, Vector2> touches)
        {
            activeTouches = touches;
        }

        /// <summary>
        /// World Matrix for entire Textile canvas
        /// </summary>
        public Matrix WorldMatrix { get; set; }

        /// <summary>
        /// Background texture if there is any.
        /// Client is responsible to discard this
        /// </summary>
        public Texture2D BackgroundTexture
        {
            get { return backgroundTexture; }
            set { backgroundTexture = value; }
        }

        public override void Update(GameTime gameTime)
        {
            if (activeTouches != null)
            {
                foreach (Textile textile in textiles)
                {
                    textile.Update(activeTouches);
                }
            }

            effectLines.Projection = projectionMatrix;
            effectLines.World = WorldMatrix;
            effectTexture.Projection = projectionMatrix;
            effectTexture.World = WorldMatrix;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (backgroundTexture != null)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                spriteBatch.Draw(backgroundTexture, screenRect, new Color(255, 255, 255, 0));
                spriteBatch.End();
            }

            foreach (Textile textile in textiles)
            {
                textile.Draw(GraphicsDevice, effectLines, effectTexture);
            }

            base.Draw(gameTime);
        }
	}
}
