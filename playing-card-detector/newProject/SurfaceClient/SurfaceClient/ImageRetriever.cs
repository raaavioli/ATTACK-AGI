using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using System.Net.Sockets;
using System.Threading;
using System.IO;

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

        private readonly object syncObject = new object();

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