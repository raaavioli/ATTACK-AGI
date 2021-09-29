using System;
using System.Collections.Generic;
using CoreInteractionFramework;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XnaScatter
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class App1: Microsoft.Xna.Framework.Game
    {
        private UIController controller;

        private Random r = new Random();
        private readonly GraphicsDeviceManager graphics;
        private TouchTarget touchTarget;
        private bool applicationLoadCompleteSignalled;
        private Matrix screenTransform = Matrix.Identity;
        private UserOrientation currentOrientation;

        private XnaScatterView scatterView;

        private SpriteBatch spriteBatch;

        private readonly List<XnaScatterView> gameObjects = new List<XnaScatterView>();

        /// <summary>
        /// The graphics device manager for the application.
        /// </summary>
        protected GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }

        /// <summary>
        /// The target receiving all input for the application.
        /// </summary>
        protected TouchTarget TouchTarget
        {
            get { return touchTarget; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        #region Initialization

        /// <summary>
        /// Populates the game.  Creates an XNAScatterView and five XNAScatterViewItems.
        /// Places the items randomly in the XNAScatterView
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification="Not applicable here.")]
        private void PopulateGameWorld()
        {
            int maxHeight = graphics.PreferredBackBufferHeight;
            int maxWidth = graphics.PreferredBackBufferWidth;

            scatterView = new XnaScatterView(controller, "Canvas.jpg", 0, maxHeight, 0, maxWidth);
            scatterView.Center = new Vector2(maxWidth / 2, maxHeight / 2);

            // Item 1 - Translate, Rotate
            XnaScatterViewItem item1 = new XnaScatterViewItem(controller, "Card01.png", scatterView);
            item1.CanTranslateFlick = false;
            item1.CanRotateFlick = false;
            item1.CanScale = false;
            item1.CanScaleFlick = false;
            item1.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item1);

            // Item 2 
            XnaScatterViewItem item2 = new XnaScatterViewItem(controller, "Card02.png", scatterView);
            item2.CanRotate = false;
            item2.CanRotateFlick = false;
            item2.CanScaleFlick = false;
            item2.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item2);

            // Item 3
            XnaScatterViewItem item3 = new XnaScatterViewItem(controller, "Card04.png", scatterView);
            item3.CanRotate = false;
            item3.CanRotateFlick = false;
            item3.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item3);

            // Item 4
            XnaScatterViewItem item4 = new XnaScatterViewItem(controller, "Card03.png", scatterView);
            item4.CanScale = false;
            item4.CanScaleFlick = false;
            item4.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item4);

            // Item 5
            XnaScatterViewItem item5 = new XnaScatterViewItem(controller, "Card05.png", scatterView);
            item5.Center = new Vector2(r.Next(maxWidth), r.Next(maxHeight));
            scatterView.AddItem(item5);

            gameObjects.Add(scatterView);
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

        #endregion

        #region Overridden Game Methods

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();

            controller = new UIController(touchTarget, HitTestCallback);

            // Set the application's orientation based on the orientation at launch
            currentOrientation = ApplicationServices.InitialOrientation;

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            // Create a rotation matrix to orient the screen so it is viewed correctly,
            // when the user orientation is 180 degress different.
            Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(180));
            Matrix translation = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                          graphics.GraphicsDevice.Viewport.Height, 0);
            Matrix inverted = rotation * translation;

            PopulateGameWorld();

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }
            scatterView.Transform = screenTransform;

            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            string filename = System.Windows.Forms.Application.ExecutablePath;
            string path = System.IO.Path.GetDirectoryName(filename) + "\\Resources\\";

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            foreach (XnaScatterView gameObject in gameObjects)
            {
                gameObject.LoadContent(graphics.GraphicsDevice, path);
            }
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
            base.Update(gameTime);

            if (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable)
            {
                // Let the UIController process new touches and changes to old touches
                controller.Update();

                // All touches have been processed, have all children process their manipulations
                foreach (XnaScatterView child in gameObjects)
                {
                    child.ProcessTouches();
                }
            }
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
                applicationLoadCompleteSignalled = true;
                ApplicationServices.SignalApplicationLoadComplete();
            }

            graphics.GraphicsDevice.Clear(Color.Black);

            // draw all the shapes in the list
            foreach (XnaScatterView gameObject in gameObjects)
            {
                gameObject.Draw(spriteBatch, screenTransform);
            }

            base.Draw(gameTime);
        }

        #endregion

        #region Hit Testing

        /// <summary>
        /// Used by the UIController to determine what ITouchableObject is being touched by a given touch.
        /// </summary>
        /// <param name="uncapturedTouches">All touches touching the app that have not been captured.</param>
        /// <param name="capturedTouches">All touches touching the app that have already been captured.</param>
        private void HitTestCallback(ReadOnlyHitTestResultCollection uncapturedTouches, 
                                     ReadOnlyHitTestResultCollection capturedTouches)
        {
            // Hit test and assign all new touches
            foreach (HitTestResult result in uncapturedTouches)
            {
                // Hit test the touch to determine which object it is touching
                UIElementStateMachine touched = HitTest(result.Touch);

                if (touched != null)
                {
                    // Only using state machines to do capture, not to track touch position inside the object, 
                    // so just use (0, 0)
                    XnaScatterHitTestDetails details = new XnaScatterHitTestDetails(0, 0);

                    // Set the hit test details
                    result.SetUncapturedHitTestInformation(touched, details);
                }
                else
                {
                    // Must call SetUncapturedHitTestInformation, but since nothing was touched, pass null
                    result.SetUncapturedHitTestInformation(null, null);
                }
            }

            // Hit test all previously captured touches
            foreach (HitTestResult result in capturedTouches)
            {
                // Only using state machines to do capture, not to track touch position inside the object, 
                // so just use (0, 0)
                XnaScatterHitTestDetails details = new XnaScatterHitTestDetails(0, 0);

                // Set the hit test details
                result.SetCapturedHitTestInformation(true, details);
            }
        }

        /// <summary>
        /// Compare the touch's location to the XNAScatterView to see if the touch
        /// is touching an XNAScatterView item or the scatter view itself
        /// </summary>
        /// <param name="c">The touch to be hit tested</param>
        private UIElementStateMachine HitTest(TouchPoint touch)
        {
            // Hit test against scatterView, it will test against its children
            return scatterView.HitTest(touch);
        }

        #endregion

        #region Application Event Handlers

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

        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing) 
            {
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }
                if (touchTarget != null)
                {
                    touchTarget.Dispose();
                    touchTarget = null;
                }
                if (scatterView != null)
                {
                    scatterView.Dispose();
                    scatterView = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}

