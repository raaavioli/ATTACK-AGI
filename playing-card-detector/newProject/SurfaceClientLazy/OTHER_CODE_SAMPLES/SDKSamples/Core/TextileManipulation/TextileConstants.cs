using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TextileManipulation
{
    internal static class TextileConstants
    {
        public const float LengthPerStitch = 16;

        public const float VelocityLimit = 24;
        public const float VelocityDecay = 0.99f;
        public const float RelationalUpdateRatio = 1.0f;
        public const float RestoreUpdateRatio = 0.04F;

        public const float CaptureAffectRatio = 2.0f;

        public const float Amplitude = 0.65f;

        public const float PullAmplitudeWeight = 0.5f;
        public const float PullRange = 2.00f * LengthPerStitch;

        public const float RestoreAmplitudeWeight = 0.7f;
        public const float RestoreRange = 2.0F;

        public const float RippleAmplitudeWeight = 0.3f;
        public const float RippleRange = 20;

        public const float RippleDiameter = 180.0f;
    }
}
