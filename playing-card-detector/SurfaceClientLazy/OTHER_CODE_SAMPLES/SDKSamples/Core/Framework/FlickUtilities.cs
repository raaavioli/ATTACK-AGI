using System;
using System.Diagnostics;

namespace CoreInteractionFramework
{
    internal static class FlickUtilities
    {
        /// <summary>
        /// Minimum flick velocities measured in device independent 
        /// units per millisecond.
        /// </summary>
        public const float MinimumFlickVelocity = 2 * 96.0f / 1000.0f;

        /// <summary>
        /// Maximum flick velocities measured in device independent 
        /// units per millisecond.
        /// </summary>
        public const float MaximumFlickVelocity = 24 * 96.0f / 1000.0f;

        /// <summary>
        /// Maximum duration of the flick measured in milliseconds. 
        /// </summary>
        public const float MaximumFlickDuration = 1500;

        /// <summary>
        /// Restricts the value passed in to the range min..max (inclusive).
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        /// <returns>
        /// The value passed in, if it falls between min and max.
        /// Otherwise returns the boundary value that is closest to the clamped value.
        /// If value is NaN, then NaN is returned.
        /// </returns>
        public static float Clamp(float value, float min, float max)
        {
            Debug.Assert(min <= max);
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// Calculates deceleration.
        /// </summary>
        /// <param name="distance">The distance the object travels while decelerating.</param>
        /// <param name="duration">The time it takes for the object to stop moving.</param>
        /// <returns></returns>
        public static float GetDecelerationGivenDistanceAndDuration(float distance, float duration)
        {
            Debug.Assert(!float.IsNaN(distance) && !float.IsInfinity(distance));
            Debug.Assert(!float.IsNaN(duration) && !float.IsInfinity(duration) && duration > float.Epsilon);
            return 2.0f * distance / (duration * duration);
        }

        /// <summary>
        /// Calculates duration.
        /// </summary>
        /// <remarks>
        /// <strong>NOTE:</strong> this method ignores signs of velocity and deceleration 
        /// parameters.
        /// </remarks>
        /// <param name="velocity">Current rate of position change of the object in motion.</param>
        /// <param name="deceleration">Rate of deceleration of the object in motion.</param>
        /// <returns></returns>
        public static float GetDurationGivenVelocityAndDeceleration(float velocity, float deceleration)
        {
            Debug.Assert(!float.IsNaN(velocity) && !float.IsInfinity(velocity));
            Debug.Assert(Math.Abs(deceleration) > float.Epsilon);
            return Math.Abs(velocity / deceleration);
        }

        /// <summary>
        /// Calculates duration.
        /// </summary>
        /// <remarks>
        /// <strong>NOTE:</strong> this method ignores signs of velocity and distance parameters.
        /// </remarks>
        /// <param name="distance">Distance traveled by the object in question.</param>
        /// <param name="velocity">Object rate of position change.</param>
        /// <returns></returns>
        public static float GetDurationGivenDistanceAndVelocity(float distance, float velocity)
        {
            Debug.Assert(!float.IsNaN(distance) && !float.IsInfinity(distance));
            Debug.Assert(Math.Abs(velocity) > float.Epsilon);
            return 2.0f * Math.Abs(distance / velocity);
        }

        /// <summary>
        /// Calculates distance.
        /// </summary>
        /// <param name="velocity">Object rate of position change.</param>
        /// <param name="duration">The time traveled by the object.</param>
        /// <returns></returns>
        private static float GetDistanceGivenVelocityAndDuration(float velocity, float duration)
        {
            Debug.Assert(!float.IsNaN(velocity) && !float.IsInfinity(velocity));
            Debug.Assert(!float.IsNaN(duration) && !float.IsInfinity(duration) && duration > float.Epsilon);
            return 0.5f * duration * velocity;
        }

        /// <summary>
        /// Calculates the page size for a flick with a given velocity.
        /// The method chooses vertical or horizontal direction depending on the
        /// given initial velocity.
        /// </summary>
        /// <param name="velocity">flick velocity in device independent units per msec.</param>
        /// <param name="flickDistance">flick page size in device independent units.</param>
        /// <param name="maxViewportSize">The maximum size of the viewport.</param>
        /// <returns></returns>
        public static bool TryGetFlickDistance(VectorF velocity, out VectorF flickDistance, VectorF maxViewportSize)
        {
            Debug.Assert(!float.IsNaN(velocity.X) && !float.IsInfinity(velocity.X), "Cannot start flick with invalid velocity.X");
            Debug.Assert(!float.IsNaN(velocity.Y) && !float.IsInfinity(velocity.Y), "Cannot start flick with invalid velocity.Y");

            // Set the flickDistance to the maximum viewport size.
            flickDistance = maxViewportSize;

            // choose the main direction of the flick based on duration
            float durationX = float.PositiveInfinity;
            float durationY = float.PositiveInfinity;

            if (Math.Abs(velocity.X) > float.Epsilon && !float.IsInfinity(flickDistance.X))
            {
                durationX = GetDurationGivenDistanceAndVelocity(flickDistance.X, velocity.X);
            }

            if (Math.Abs(velocity.Y) > float.Epsilon && !float.IsInfinity(flickDistance.Y))
            {
                durationY = GetDurationGivenDistanceAndVelocity(flickDistance.Y, velocity.Y);
            }

            // choose the minimum duration
            float duration = Math.Min(durationX, durationY);
            if (float.IsInfinity(duration))
            {
                // cannot calculate flick page size
                return false;
            }

            // make sure that duration doesn't exceed MaximumFlickDuration,
            if (duration > MaximumFlickDuration)
            {
                // recalculate duration for the 'flick' direction
                if (durationX <= durationY)
                {
                    // calculate deceleration based on X page size and MaximumFlickDuration
                    Debug.Assert(flickDistance.X > 0);
                    float deceleration = GetDecelerationGivenDistanceAndDuration(flickDistance.X, MaximumFlickDuration);
                    duration = GetDurationGivenVelocityAndDeceleration(velocity.X, deceleration);
                }
                else
                {
                    // calculate deceleration based on Y page size and MaximumFlickDuration
                    Debug.Assert(flickDistance.Y > 0);
                    float deceleration = GetDecelerationGivenDistanceAndDuration(flickDistance.Y, MaximumFlickDuration);
                    duration = GetDurationGivenVelocityAndDeceleration(velocity.Y, deceleration);
                }
                Debug.Assert(duration <= MaximumFlickDuration, "Duration is supposed to be less than MaximumFlickDuration.");
            }

            // adjust the flick distance
            if (duration < durationX)
            {
                flickDistance.X = Math.Abs(GetDistanceGivenVelocityAndDuration(velocity.X, duration));
            }

            if (duration < durationY)
            {
                flickDistance.Y = Math.Abs(GetDistanceGivenVelocityAndDuration(velocity.Y, duration));
            }

            return true;
        }
    }
}
