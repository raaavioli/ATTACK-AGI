using System;
using System.Diagnostics;
using CoreInteractionFramework.Properties;

namespace CoreInteractionFramework
{
    /// <summary>
    /// SizeF is basically a partial duplicate of System.Drawing.SizeF. It is internal
    /// and meant for our own use (for convenience instead of carrying around two floats
    /// everywhere.)
    /// </summary>
    internal struct SizeF
    {
        private float width;
        private float height;

        /// <summary>
        /// Simple constructor - specify the width and height
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public SizeF(float width, float height)
        {
            Debug.Assert(width >= 0.0f);
            Debug.Assert(height >= 0.0f);

            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Explicitly converts an instance of SizeF to an instance of PointF.
        /// </summary>
        /// <param name="size">The SizeF value to be converted.</param>
        /// <returns>A PointF equal in value to this instance of SizeF.</returns>
        public static explicit operator PointF(SizeF size)
        {
            return new PointF(size.width, size.height);
        }

        /// <summary>
        /// Explicitly converts an instance of SizeF to an instance of VectorF.
        /// </summary>
        /// <param name="size">The SizeF value to be converted.</param>
        /// <returns>A VectorF equal in value to this instance of SizeF.</returns>
        public static explicit operator VectorF(SizeF size)
        {
            return new VectorF(size.width, size.height);
        }

        /// <summary>
        /// Get or set the width
        /// </summary>
        public float Width
        {
            get { return width; }
            set 
            {
                Debug.Assert(value >= 0.0f);
                width = value; 
            }
        }

        /// <summary>
        /// Get or set the height
        /// </summary>
        public float Height
        {
            get { return height; }
            set
            {
                Debug.Assert(value >= 0.0f);
                height = value;
            }
        }

        /// <summary>
        /// == operator definition
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator ==(SizeF s1, SizeF s2)
        {
            return s1.Width == s2.Width && s1.Height == s2.Height;
        }

        /// <summary>
        /// != operator definition
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator !=(SizeF s1, SizeF s2)
        {
            return !(s1 == s2);
        }

        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is SizeF)
                return this == (SizeF)obj;

            return false;
        }

        /// <summary>
        /// GetHashCode override (does nothing)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (width.GetHashCode() ^ height.GetHashCode());
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.CurrentCulture,
                Resources.SizeFToStringFormat, Width, Height);
        }
    }
}
