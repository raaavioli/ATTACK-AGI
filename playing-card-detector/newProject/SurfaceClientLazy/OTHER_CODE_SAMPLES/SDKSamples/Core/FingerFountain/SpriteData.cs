using Microsoft.Xna.Framework;

namespace FingerFountain
{
    /// <summary>
    /// Describes the location, orientation, and scale of a sprite.
    /// </summary>
    public class SpriteData
    {
        private Vector2 location;
        private float orientation;
        private float scale;

        /// <summary>
        /// The location of the sprite.
        /// </summary>
        public Vector2 Location
        {
            get { return location; }
            set { location = value; }
        }

        /// <summary>
        /// The orientation of the sprite.
        /// </summary>
        public float Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        /// <summary>
        /// The scale of the sprite.
        /// </summary>
        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="location">initial location</param>
        /// <param name="orientation">initial orientation</param>
        /// <param name="scale">initial scale</param>
        public SpriteData(Vector2 location, float orientation, float scale)
        {
            this.location = location;
            this.orientation = orientation;
            this.scale = scale;
        }
    }
}
