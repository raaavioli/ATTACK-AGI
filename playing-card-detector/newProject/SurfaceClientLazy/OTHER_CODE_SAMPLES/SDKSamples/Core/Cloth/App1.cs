using System;
using System.Collections.Generic;
using CoreInteractionFramework;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Cloth.UI;

namespace Cloth
{
    /// <summary>
    /// This is the main class for the cloth application.
    /// </summary>
    public class App1 : Game
    {
        private GraphicsDeviceManager graphics;

        private bool applicationLoadCompleteSignalled;

        // This orients batches of sprites.
        private UserOrientation currentOrientation = UserOrientation.Bottom;
        private Matrix screenTransform = Matrix.Identity;
        private Matrix inverted;

        // The UI controller.
        private UIController controller;
        private TouchTarget touchTarget;

        // Top-Level UIElements
        private MeshCanvas meshCanvas;       // Background ScrollViewer.
        private Textiles textiles;           // Contains the cloth simulation component.

        private UIContainer hudContainer;    // Container for Heads Up Display (HUD) elements.

        private UI.ListBox listbox;             // HUD Control. Consists of listbox, button, and scrollbar.

        // Captures touches that should be passed on to the cloth simulation component.
        private readonly Dictionary<int, Vector2> activeTouches = new Dictionary<int, Vector2>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1()
        {
            graphics = new GraphicsDeviceManager(this);
            System.Diagnostics.Debug.Assert(graphics.GraphicsDevice == GraphicsDevice);
        }

        #region Initialization

        /// <summary>
        /// Creates the visual elements of the game and adds them to game Components.
        /// </summary>
        private void PopulateGame()
        {
            int drawOrder = 0;

            // Create the background Canvas and add it to game Components
            meshCanvas = new MeshCanvas(
                this,
                controller,
                @"Content\ApplicationBackground.jpg",
                graphics.PreferredBackBufferWidth * 2,
                graphics.PreferredBackBufferHeight * 2);
            meshCanvas.Name = "meshCanvas";
            meshCanvas.DrawOrder = drawOrder++;
            meshCanvas.AutoScaleTexture = false;
            meshCanvas.SpriteBlendState = BlendState.NonPremultiplied;

            Components.Add(meshCanvas);

            // Created the UIElement that encapsulates the cloth simulation component.
            textiles = new Textiles(this, controller);

            // Once we created the textiles, we can enable the tap gesture.
            touchTarget.TouchTapGesture += OnTouchTapGesture;

            textiles.DrawOrder = drawOrder++;

            // Create the HUD Container and add it to game Components
            hudContainer = new UIContainer(
                this,
                controller,
                @"Content\Transparent256x256.png",
                null,
                256,
                256,
                null);
            hudContainer.Name = "hudContainer";
            hudContainer.Center = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2f,
                                    graphics.GraphicsDevice.Viewport.Height / 2f);
            hudContainer.DrawOrder = drawOrder++;
            hudContainer.LayerDepth = 1.0f;
            hudContainer.SpriteBlendState = BlendState.NonPremultiplied;
            hudContainer.Active = false;

            Components.Add(hudContainer);

            // Create and position listbox and add it to the hudCanvas.
            Vector2 position = new Vector2(0f, 0f);
            listbox = new UI.ListBox(
                hudContainer,
                position,
                3 * UI.ListBox.ItemWidth,
                UI.ListBox.ItemHeight,
                textiles);
            listbox.Name = "listbox";
            listbox.AutoScaleTexture = false;
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
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnCurrentThread);
            touchTarget.EnableInput();
        }

        /// <summary>
        /// Updates the items that depend on the viewport size.
        /// </summary>
        private void UpdateViewportSettings()
        {
            // Create a rotation matrix to orient the screen so it is viewed correctly,
            // when the user orientation is 180 degress different.
            Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(180));
            Matrix translation = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                          graphics.GraphicsDevice.Viewport.Height, 0);
            inverted = rotation * translation;

            // Make sure the HUD stays in the center of the window.
            if (hudContainer != null)
            {
                hudContainer.Center = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2f,
                                                  graphics.GraphicsDevice.Viewport.Height / 2f);
            }
        }

        /// <summary>
        /// Resets the application's orientation and screen transform
        /// based on the current launcher orientation.
        /// </summary>
        private void ResetOrientation(UserOrientation newOrientation)
        {
            if (newOrientation == currentOrientation)
            {
                return;
            }

            currentOrientation = newOrientation;

            switch (currentOrientation)
            {
                case UserOrientation.Bottom:
                    screenTransform = Matrix.Identity;
                    break;
                case UserOrientation.Top:
                    screenTransform = inverted;
                    break;
            }

            // Re-orient top-level components.
            textiles.ScreenTransform = screenTransform;
            foreach (GameComponent component in Components)
            {
                UIElement element = component as UIElement;
                if (element != null)
                {
                   if (element == meshCanvas) continue;
                   element.ResetOrientation(newOrientation, screenTransform);
                }
            }
        }

        #endregion

        #region Hit Testing

        /// <summary>
        /// Performs hit testing for all game components.
        /// </summary>
        private void HitTestCallback(ReadOnlyHitTestResultCollection uncapturedTouches,
                             ReadOnlyHitTestResultCollection capturedTouches)
        {
            // First check captured touches.
            UIElement.HitTestCapturedTouches(capturedTouches);

            // Now test the uncaptured touches.
            // The order is important for overlapping elements.
            // Test from front to back (reverse of DrawOrder).

            // Test the HUD elements first
            hudContainer.HitTestUncapturedTouches(uncapturedTouches);

            // Now the textiles.
            textiles.HitTestUncapturedTouches(uncapturedTouches);

            SetActiveTouches();

            // And finally the background canvas.
            meshCanvas.HitTestUncapturedTouches(uncapturedTouches);
        }

        /// <summary>
        /// Captures active touches on the Surface and bundles them into a dictionary
        /// to be sent to the textile manipulation component.
        /// </summary>
        /// <remarks>
        /// This routine is call in the middle of hit testing, after the HUD elements and textiles,
        /// but before the meshCanvas.
        /// The touches are transformed using the current WorldMatrix of the textile component,
        /// before the next translation is applied
        /// </remarks>
        private void SetActiveTouches()
        {
            IInputElementStateMachine capturingElement;
            activeTouches.Clear();

            foreach (TouchPoint touch in touchTarget.GetState())
            {
                capturingElement = controller.GetCapturingElement(touch);
                if (capturingElement == null || capturingElement == textiles.StateMachine)
                {
                    activeTouches.Add(touch.Id, textiles.WorldVector(touch.X, touch.Y));
                }
            }
        }

        #endregion

        #region Overridden Game Methods.

        //
        // The following virtual methods from the Game class have been overridden:
        //
        //    Initialize, LoadContent, Update, Draw, UnLoadContent
        //
        // The following methods are also available for override, but are not overridden
        // at this time:
        //
        //    BeginRun, BeginDraw, EndDraw, EndRun, OnActivated, OnDeactivated, OnExiting
        //

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

            // Make sure things are aware of the window size.
            UpdateViewportSettings();
            Window.ClientSizeChanged += OnClientSizeChanged;

            // Create the UIController for the StateMachines.
            controller = new UIController(touchTarget, HitTestCallback);

            PopulateGame();

            // Initialize UIElements which are not in Game.Components
            // and are not children of another UIElement.
            textiles.Initialize();

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Set the application's orientation based on the orientation launch
            ResetOrientation(ApplicationServices.InitialOrientation);

            base.LoadContent();
        }

        /// <summary>
        /// Unload your graphics content here.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ApplicationServices.WindowAvailability == WindowAvailability.Unavailable)
            {
                return;
            }

            controller.Update();

            textiles.SetActiveTouches(activeTouches);
            textiles.Translate(meshCanvas.Delta);

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
                // Dismiss the loading screen now that we are starting to draw.
                applicationLoadCompleteSignalled = true;
                ApplicationServices.SignalApplicationLoadComplete();
            }

            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Black, 0, 0);

            base.Draw(gameTime);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// This is called when the touch target receives a tap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTouchTapGesture(object sender, TouchEventArgs args)
        {
            if (hudContainer != null)
            {
                // Only pass taps to the textiles if the HUD container is not the one being tapped.
                UIElement touched = hudContainer.HitTesting(args.TouchPoint, false);
                if (touched == null && textiles != null)
                {
                    // forward the event to the textiles UI element.
                    textiles.OnTouchTapGesture(sender, args);
                }
            }
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

        /// <summary>
        /// This is called when the application's window is resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewportSettings();
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IDisposable graphicsDispose = graphics as IDisposable;
                    if (graphicsDispose != null)
                    {
                        graphicsDispose.Dispose();
                    }

                    if (touchTarget != null)
                    {
                        touchTarget.Dispose();
                        touchTarget = null;
                    }
                    if (textiles != null)
                    {
                        textiles.Dispose();
                        textiles = null;
                    }
                    if (hudContainer != null)
                    {
                        hudContainer.Dispose();
                        hudContainer = null;
                    }
                    if (listbox != null)
                    {
                        listbox.Dispose();
                        listbox = null;
                    }
                    if (meshCanvas != null)
                    {
                        meshCanvas.Dispose();
                        meshCanvas = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}
