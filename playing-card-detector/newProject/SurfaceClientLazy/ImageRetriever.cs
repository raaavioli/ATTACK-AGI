using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace SurfaceClient {
    public class ImageRetriever {
        volatile private static byte[] normalizedImage;
        volatile private static byte[] normalizedImage_compressed = null;
        private long totalTime = 0;
        private int totalFrames = 0;
        volatile private bool compressing = false;
        private ImageMetrics normalizedMetrics;
        private float scale;
        private bool imageUpdated;
        private TouchTarget touchTarget;

        private readonly object syncObject = new object();

        public ImageRetriever() {
            Debug.Assert(touchTarget == null,
                "Surface input already initialized");
            if (touchTarget != null)
                return;

            //var mar = LoadLibrary("user32.dll");
            //s_MouseHookHandle = SetWindowsHookEx(
            //    WH_MOUSE_LL,
            //    s_MouseDelegate,
            //    mar,
            //    0);

            // Create a target for surface input.
            touchTarget = new TouchTarget(Process.GetCurrentProcess().MainWindowHandle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();

            // Enable the normalized raw-image.
            touchTarget.EnableImage(ImageType.Normalized);

            // Hook up a callback to get notified when there is a new frame available
            touchTarget.FrameReceived += OnTouchTargetFrameReceived;
        }

        private void OnTouchTargetFrameReceived(object sender, FrameReceivedEventArgs e) {
            lock (syncObject) {
                if (normalizedImage == null) {
                    if (e.TryGetRawImage(
                            ImageType.Normalized,
                            0, 0,
                            1920, 1080,
                            out normalizedImage,
                            out normalizedMetrics)) {
                        scale = (InteractiveSurface.PrimarySurfaceDevice == null)
                                    ? 1.0f
                                    : (float)InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth / normalizedMetrics.Width;
                    }
                } else {
                    e.UpdateRawImage(
                        ImageType.Normalized,
                        normalizedImage,
                        0, 0,
                        1920, 1080);
                }
            }
            imageUpdated = true;
        }

        public byte[] GetImage() { return normalizedImage; }
    }
}