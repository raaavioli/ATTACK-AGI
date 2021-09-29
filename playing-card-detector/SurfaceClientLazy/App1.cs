using System;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using SurfaceClient;

namespace RawImageVisualizer {
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class App1 : Microsoft.Xna.Framework.Game {
        private readonly GraphicsDeviceManager graphics;
        private TouchTarget touchTarget;
        private bool applicationLoadCompleteSignalled;
        private SpriteBatch foregroundBatch;
        private Texture2D touchSprite;
        private readonly Vector2 spriteOrigin = new Vector2(0f, 0f);
        private Client surfaceClient = new Client();

        // normalizedImageUpdated is accessed from differet threads. Mark it
        // volatile to make sure that every read gets the latest value.
        private volatile bool normalizedImageUpdated;

        // For Normalized Raw Image
        volatile public static byte[] normalizedImage;
        volatile public static byte[] normalizedImage_compressed = null;
        private long totalTime = 0;
        private int totalFrames = 0;
        volatile private bool compressing = false;
        private ImageMetrics normalizedMetrics;


        // For Scaling the RawImage back to full screen.
        private float scale;

        // Something to lock to deal with asynchronous frame updates
        private readonly object syncObject = new object();

        // Blend state used when drawing the raw image data on the screen.  Results
        // in a black background with image data shown in the color used to clear the display.
        private readonly BlendState textureBlendState = new BlendState {
            AlphaDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.SourceAlpha,
            ColorSourceBlend = Blend.Zero
        };

        /// <summary>
        /// Default constructor.
        /// </summary>
        public App1() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            SocketThreadManager.CreateServer("169.254.185.113", 666);
            Console.WriteLine("HELLO");

        }

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it calls to loads amd Initializes SurfaceInput TouchTarget.  
        /// Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
            graphics.IsFullScreen = false;
            base.Initialize();
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e) {
            // Turn raw image back on again
            touchTarget.EnableImage(ImageType.Normalized);
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e) {
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e) {
            // If the app isn't active, there's no need to keep the raw image enabled
            touchTarget.DisableImage(ImageType.Normalized);
        }

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface() {
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
        private void InitializeSurfaceInput() {
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

            // Enable the normalized raw-image.
            touchTarget.EnableImage(ImageType.Normalized);

            // Hook up a callback to get notified when there is a new frame available
            touchTarget.FrameReceived += OnTouchTargetFrameReceived;
        }

        /// <summary>
        /// Handler for the FrameReceived event. 
        /// Here we get the rawimage data from FrameReceivedEventArgs object.
        /// </summary>
        /// <remarks>
        /// When a frame is received, this event handler gets the normalized image 
        /// from the TouchTarget. The image is copied into the touch sprite in the 
        /// Update method. The reason for this separation is that this event handler is 
        /// called from a background thread and Update is called from the main thread. It 
        /// is not safe to get the normalized image from the TouchTarget in the Update 
        /// method because TouchTarget and Update run on different threads. It is 
        /// possible for TouchTarget to change the image while Update is accessing it.
        /// 
        /// To address the threading issue, the raw image is retrieved here, on the same 
        /// thread that updates and uses it on the main thread. It is stored in a variable 
        /// that is available to both threads, and access to the variable is controlled 
        /// through lock statements so that only one thread can use the image at a time.
        /// </remarks>
        /// <param name="sender">TouchTarget that received the frame</param>
        /// <param name="e">Object containing information about the current frame</param>
        Stopwatch frameTimer = new Stopwatch();
        private void OnTouchTargetFrameReceived(object sender, FrameReceivedEventArgs e) {
            // Lock the syncObject object so normalizedImage isn't changed while the Update method is using it

            lock (syncObject) {
                if (normalizedImage == null) {
                    Console.WriteLine("iamm null");
                    // get rawimage data for a specific area
                    if (e.TryGetRawImage(
                            ImageType.Normalized,
                            0, 0,
                           1920, 1080, //InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth,
                                       // InteractiveSurface.PrimarySurfaceDevice.WorkingAreaHeight,
                            out normalizedImage,
                            out normalizedMetrics)) {
                        scale = (InteractiveSurface.PrimarySurfaceDevice == null)
                                    ? 1.0f
                                    : (float)InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth / normalizedMetrics.Width;
                    }
                } else {
                    // get the updated rawimage data for the specified area
                    e.UpdateRawImage(
                        ImageType.Normalized,
                        normalizedImage,
                        0, 0,
                        1920, 1080);// InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth,
                                    //InteractiveSurface.PrimarySurfaceDevice.WorkingAreaHeight);

                    // --------------------- HERE WE DO OUR STUFF ----------------------------

                    surfaceClient.Process(normalizedImage);

                    // --------------------- HERE WE STOP DOING OUR STUFF --------------------
                }
                frameTimer.Stop();
                if (!compressing)//ensures we don't create a backlog of threads
                {
                    compressing = true;
                    new Thread(delegate () {
                        //Stopwatch stopwatch = Stopwatch.StartNew();
                        normalizedImage_compressed = StaticNetworkUtilities.compressFrame_assumeNoZero(normalizedImage);
                        compressing = false;
                        //stopwatch.Stop();
                        //Interlocked.Increment(ref totalFrames);
                        //Interlocked.Add(ref totalTime, stopwatch.ElapsedMilliseconds);
                        //RawImageVisualizer.Program.sw.WriteLine("Compression took: " + stopwatch.ElapsedMilliseconds+", average: " + ((double)totalTime)/(double)totalFrames);
                    }).Start();
                } else {
                    RawImageVisualizer.Program.sw.WriteLine("A frame was not compressed because the previous thread failed to complete in " + frameTimer.ElapsedMilliseconds + " ms");
                }
                frameTimer.Restart();

                normalizedImageUpdated = true;
            }

        }


        void compressRawimage() {
            normalizedImage_compressed = StaticNetworkUtilities.compressFrame_assumeNoZero(normalizedImage);
        }
        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent() {
            foregroundBatch = new SpriteBatch(graphics.GraphicsDevice);
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent() {
            Content.Unload();
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            // Lock the syncObject object so the normalized image and metrics aren't updated while this method is using them
            lock (syncObject) {
                // Don't bother if the app isn't visible, or if the image hasn't been updates since the last update
                if (normalizedImageUpdated &&
                    (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable)) {
                    if (normalizedMetrics != null) {
                        if (touchSprite == null) {
                            // Creating a Sprite from the rawimage metrics data
                            touchSprite = new Texture2D(graphics.GraphicsDevice,
                                                          normalizedMetrics.Width,
                                                          normalizedMetrics.Height,
                                                          true,
                                                          SurfaceFormat.Alpha8);
                        }

                        // Texture2D requires that the texture is not set on the device when updating it, so set it null               
                        graphics.GraphicsDevice.Textures[0] = null;

                        // Setting the Texture2D with normalized rawimage data.
                        touchSprite.SetData<Byte>(normalizedImage,
                                                  0,
                                                  normalizedMetrics.Width * normalizedMetrics.Height);
                    }
                }
                // reset the flag
                normalizedImageUpdated = false;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            if (!applicationLoadCompleteSignalled) {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationServices.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            // This controls the color of the image data.
            graphics.GraphicsDevice.Clear(Color.White);

            foregroundBatch.Begin(SpriteSortMode.Immediate, textureBlendState);
            // draw the sprite of RawImage
            if (touchSprite != null) {
                // Adds the rawimage sprite to Spritebatch for drawing.
                foregroundBatch.Draw(touchSprite, spriteOrigin, null, Color.White,
                   0f, spriteOrigin, scale, SpriteEffects.FlipVertically, 0f);
            }
            foregroundBatch.End();

            base.Draw(gameTime);
        }

        #region IDisposable

        protected override void Dispose(bool disposing) {
            if (disposing) {
                SocketThreadManager.Alive = false;
                Thread.Sleep(500);
                // Release  managed resources.
                if (touchSprite != null) {
                    touchSprite.Dispose();
                }
                if (foregroundBatch != null) {
                    foregroundBatch.Dispose();
                }
                if (touchTarget != null) {
                    touchTarget.Dispose();
                }

                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null) {
                    graphicsDispose.Dispose();
                }
            }

            // Release unmanaged Resources.

            base.Dispose(disposing);
        }

        #endregion
    }

}
