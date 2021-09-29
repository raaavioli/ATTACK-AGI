using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;

namespace XnaScatter
{
    static class Program
    {
        // Hold on to the game window.
        static GameWindow Window;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Disable the WinForms unhandled exception dialog.
            // SurfaceShell will notify the user.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

            // Apply Surface globalization settings
            GlobalizationSettings.ApplyToCurrentThread();

            using (App1 app = new App1())
            {
                app.Run();
            }
        }

        /// <summary>
        /// Gets the size of the main window.
        /// </summary>
        internal static Size WindowSize
        {
            get
            {
                return ((Form)Form.FromHandle(Window.Handle)).DesktopBounds.Size;
            }
        }

        /// <summary>
        /// Position and adorn the Window appropriately.
        /// </summary>
        /// <param name="window"></param>
        internal static void InitializeWindow(GameWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            Window = window;

            Form form = (Form)Form.FromHandle(Window.Handle);
            form.LocationChanged += OnFormLocationChanged;

            SetWindowStyle();
            SetWindowSize();
        }

        /// <summary>
        /// Respond to changes in the form location, adjust if necessary.
        /// </summary>
        private static void OnFormLocationChanged(object sender, EventArgs e)
        {
            if (SurfaceEnvironment.IsSurfaceEnvironmentAvailable)
            {
                Form form = (Form)Form.FromHandle(Window.Handle);
                form.LocationChanged -= OnFormLocationChanged;
                PositionWindow();
                form.LocationChanged += OnFormLocationChanged;
            }
        }

        /// <summary>
        /// Position and size the window to the primary device.
        /// </summary>
        internal static void PositionWindow()
        {
            int left = (InteractiveSurface.PrimarySurfaceDevice != null)
                            ? InteractiveSurface.PrimarySurfaceDevice.WorkingAreaLeft
                            : Screen.PrimaryScreen.WorkingArea.Left;
            int top = (InteractiveSurface.PrimarySurfaceDevice != null)
                            ? InteractiveSurface.PrimarySurfaceDevice.WorkingAreaTop
                            : Screen.PrimaryScreen.WorkingArea.Top;

            Form form = (Form)Form.FromHandle(Window.Handle);
            FormWindowState windowState = form.WindowState;
            form.WindowState = FormWindowState.Normal;
            form.Location = new System.Drawing.Point(left, top);
            form.WindowState = windowState;
        }

        /// <summary>
        /// Set the window's style based on the availability of the Surface environment.
        /// </summary>
        private static void SetWindowStyle()
        {
            Window.AllowUserResizing = true;
            Form form = (Form)Form.FromHandle(Window.Handle);
            form.FormBorderStyle = (SurfaceEnvironment.IsSurfaceEnvironmentAvailable)
                                    ? FormBorderStyle.None
                                    : FormBorderStyle.Sizable;
        }

        /// <summary>
        /// Size the window to the primary device.
        /// </summary>
        private static void SetWindowSize()
        {
            int width = (InteractiveSurface.PrimarySurfaceDevice != null)
                            ? InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth
                            : Screen.PrimaryScreen.WorkingArea.Width;
            int height = (InteractiveSurface.PrimarySurfaceDevice != null)
                            ? InteractiveSurface.PrimarySurfaceDevice.WorkingAreaHeight
                            : Screen.PrimaryScreen.WorkingArea.Height;

            Form form = (Form)Form.FromHandle(Window.Handle);
            form.ClientSize = new Size(width, height);
            form.WindowState = (SurfaceEnvironment.IsSurfaceEnvironmentAvailable)
                            ? FormWindowState.Normal
                            : FormWindowState.Maximized;
        }
    }
}

