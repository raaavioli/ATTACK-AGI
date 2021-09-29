using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FingerFountain
{
    /// <summary>
    /// This sample demonstrates a very simple drawing technique.
    /// </summary>
    public class App1 : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private TouchTarget touchTarget;
        private bool applicationLoadCompleteSignalled;
        private const int millisecondsToDisappear = 3000;
        private SpriteBatch foregroundBatch;
        private Texture2D touchSprite;
        private Vector2 spriteOrigin;
        private LinkedList<SpriteData> sprites = new LinkedList<SpriteData>();

        #region FingerFountain Sprites

        /// <summary>
        /// Reduces the scale of each sprite in the sprites list by
        /// the specified amount.
        /// </summary>
        /// <param name="shrinkBy">amount to shrink by</param>
        private void ShrinkSprites(float shrinkBy)
        {
            // go through the whole list and decrement the scale value
            LinkedListNode<SpriteData> currentNode = sprites.First;
            while (currentNode != null)
            {
                currentNode.Value.Scale -= shrinkBy;
                currentNode = currentNode.Next;
            }
        }

        /// <summary>
        /// Removes any sprites with a scale of zero or lower.
        /// </summary>
        private void RemoveInvisibleSprites()
        {
            // Since the sprites are always added to the end,
            // removals will always come from the beginning.
            while (sprites.First != null && sprites.First.Value.Scale <= 0.0f)
            {
                sprites.RemoveFirst();
            }
        }

        /// <summary>
        /// Creates a new sprite at each touch location.
        /// </summary>
        private int InsertSpritesAtTouchPositions(ReadOnlyTouchPointCollection touches)
        {
            int count = 0;
            foreach (TouchPoint touch in touches)
            {
                // Create a sprite for each touch that has been recognized as a finger, 
                // or for any touch if finger recognition is not supported.
                if (touch.IsFingerRecognized || InteractiveSurface.PrimarySurfaceDevice.IsFingerRecognitionSupported == false)
                {
                    SpriteData sprite = new SpriteData(new Vector2(touch.X, touch.Y),
                        touch.Orientation,
                        1.0f);
                    sprites.AddLast(sprite); // always add to the end
                    count++;
                }
            }
            return count;
        }

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            IsFixedTimeStep = false; // we will update based on time
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            base.Initialize();
        }

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;

            // Get the window sized right.
            Program.InitializeWindow(Window);
            // Set the graphics device buffers.
            graphics.PreferredBackBufferWidth = Program.WindowSize.Width;
            graphics.PreferredBackBufferHeight = Program.WindowSize.Height;
            graphics.ApplyChanges();
            // Make sure the window is in the right location.
            Program.PositionWindow();
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(touchTarget == null,
                "Surface input already initialized");
            if (touchTarget != null)
                return;

            // Create a target for surface input.
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            string filename = System.Windows.Forms.Application.ExecutablePath;
            string path = System.IO.Path.GetDirectoryName(filename) + "\\FingerFountainContent\\";

            foregroundBatch = new SpriteBatch(graphics.GraphicsDevice);
            using (Stream textureFileStream = File.OpenRead(Path.Combine(path, "sprite.png")))
            {
                touchSprite = Texture2D.FromStream(graphics.GraphicsDevice, textureFileStream);
            }
            spriteOrigin = new Vector2((float)touchSprite.Width / 2.0f,
                (float)touchSprite.Height / 2.0f);
        }
        
        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }
        
        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable)
            {
                // get the current state
                ReadOnlyTouchPointCollection touches = touchTarget.GetState();

                // first update the state of any existing sprites
                ShrinkSprites((float)gameTime.ElapsedGameTime.Milliseconds /
                    (float)millisecondsToDisappear);

                // next update the sprites list with new additions
                InsertSpritesAtTouchPositions(touches);

                // finally remove any invisible sprites
                RemoveInvisibleSprites();
            }

            base.Update(gameTime);
        }
        
        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationServices.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            graphics.GraphicsDevice.Clear(Color.Black);

            foregroundBatch.Begin();
            // draw all the sprites in the list
            foreach (SpriteData sprite in sprites)
            {
                foregroundBatch.Draw(touchSprite, sprite.Location, null, Color.White,
                    sprite.Orientation, spriteOrigin, sprite.Scale, SpriteEffects.None, 0f);
            }
            foregroundBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.              
                touchSprite.Dispose();
                foregroundBatch.Dispose();
                touchTarget.Dispose();

                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
            }

            // Release unmanaged Resources.

            base.Dispose(disposing);
        }

        #endregion       
    }

}
