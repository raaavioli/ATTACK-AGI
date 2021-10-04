using System;
using Microsoft.Xna.Framework;

namespace XnaScatter
{
    class GeometryHelper
    {
        private GeometryHelper() { }
        //==========================================================//
        /// <summary>
        /// Rotates a point (represented by a vector) around another point (represented by a vector)
        /// </summary>
        /// <param name="origin">The point/vector to rotate around</param>
        /// <param name="rotateMe">The point/vector to be rotated</param>
        /// <param name="radians">The angle in radians to rotate the vector</param>
        /// <returns>The rotated vector</returns>
        public static Vector2 RotatePointVectorAroundOrigin(Vector2 origin, Vector2 rotateMe, float radians)
        {
            Vector2 translated = Vector2.Subtract(origin, rotateMe);
            Vector2 rotated = RotatePointVector(translated, radians);
            return Vector2.Add(origin, rotated);                              
        }

        //==========================================================//
        /// <summary>
        /// Rotates a point (represented by a vector) around the origin
        /// </summary>
        /// <param name="rotateMe">The point/vector to be rotated</param>
        /// <param name="radians">The angle to rotate the vector</param>
        /// <returns>The rotated vector</returns>
        public static Vector2 RotatePointVector(Vector2 rotateMe, float radians)
        {
            return new Vector2((float)(rotateMe.X * Math.Cos(radians) - rotateMe.Y * Math.Sin(radians)),
                               (float)(rotateMe.X * Math.Sin(radians) + rotateMe.Y * Math.Cos(radians)));
        }

        //==========================================================//
        /// <summary>
        /// Checks to see if a point (represented by a vector) is inside a rectangle
        /// </summary>
        /// <param name="rect">The rectangle that may contain the point/vector</param>
        /// <param name="vector">The vector being checked</param>
        /// <returns>True if the rectangle contains the vector, false otherwise</returns>
        public static bool RectangleContainsPointVector ( Rectangle rect, Vector2 vector )
        {
            if (vector.X <= rect.Right && vector.X >= rect.Left &&
                vector.Y <= rect.Bottom && vector.Y >= rect.Top)
            {
                return true;
            }
            return false;
        }
    }
}
