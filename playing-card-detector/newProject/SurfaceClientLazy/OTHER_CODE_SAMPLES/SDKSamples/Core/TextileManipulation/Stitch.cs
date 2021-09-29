using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TextileManipulation
{
	internal class Stitch
	{
        private readonly Textile owner;
        private readonly Vector2 homePosition;
        private readonly Color color;

        private IList<Stitch> referencingStitches = new List<Stitch>();
        private IList<Stitch> referencedStitches = new List<Stitch>();

        public Stitch(Textile owner, Vector2 position, Vector2 coordinate, Color color)
		{
            this.owner = owner;
            this.Position = owner.Transform(position);
            this.homePosition = position;
            this.Coordinate = coordinate;
            this.color = color;
		}


        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; }

        internal Vector2 Coordinate;

        public IList<Stitch> ReferencingStitches { get { return referencingStitches; } }
        public IList<Stitch> ReferencedStitches { get { return referencedStitches; } }

        public Textile Owner
        {
            get { return owner; }
        }

        public Color Color
        {
            get { return color; }
        }

        private Vector2 HomePosition
        {
            get { return owner.Transform(homePosition); }
        }

        public void Move(Stitch source, float affect)
        {
            if (affect <= 0.1)
            {
                return;
            }

            this.Velocity -= this.Velocity * affect;
            affect *= 0.6f;

            if (this != source)
            {
                Vector2 relative = source.Position - this.Position;
                this.Velocity += relative * affect;
            }

            foreach (Stitch stitch in ReferencingStitches)
            {
                if (stitch != source)
                {
                    stitch.Move(this, affect);
                }
            }
            foreach (Stitch stitch in ReferencedStitches)
            {
                if (stitch != source)
                {
                    stitch.Move(this, affect);
                }
            }
        }

        public void UpdateAcceleration()
        {
            this.Velocity *= TextileConstants.VelocityDecay;
            this.Position += this.Velocity;
            if (this.Velocity.Length() > TextileConstants.VelocityLimit)
            {
                this.Velocity *= TextileConstants.VelocityLimit / this.Velocity.Length();
            }
        }

        public void Update()
        {
            Vector2 relativePos = this.Position - this.HomePosition;

            float distance = relativePos.Length();
            if (distance > TextileConstants.RestoreRange)
            {
                relativePos *= TextileConstants.RestoreRange / distance;
            }

            float pull = AverageElementLength(relativePos * Velocity);
            float force = WeightingAdd(distance - TextileConstants.LengthPerStitch, pull / TextileConstants.LengthPerStitch,
                          TextileConstants.RestoreAmplitudeWeight) * TextileConstants.Amplitude;

            if (force > 0 && distance > 0)
            {
                this.Velocity -= relativePos * force * TextileConstants.RestoreUpdateRatio / distance;
            }
        }

        public void UpdateRelationalVelocity()
        {
            foreach (Stitch stitch in ReferencedStitches)
            {
                Vector2 affect = GetRelationalVelocity(stitch);
                affect *= TextileConstants.RelationalUpdateRatio;

                this.Velocity -= affect;
                stitch.Velocity += affect;
            }
        }

        private Vector2 GetRelationalVelocity(Stitch target)
        {
            Vector2 relativePos = this.Position - target.Position;

            float length = relativePos.Length();
            if (length < TextileConstants.LengthPerStitch)
            {
                return Vector2.Zero;
            }

            if (length > TextileConstants.PullRange)
            {
                relativePos *= TextileConstants.PullRange / length;
            }

            Vector2 relativeVelocity = this.Velocity - target.Velocity;

            float pull = AverageElementLength(relativePos * relativeVelocity);
            float force = WeightingAdd(length - TextileConstants.LengthPerStitch, pull / TextileConstants.LengthPerStitch,
                          TextileConstants.PullAmplitudeWeight) * TextileConstants.Amplitude;

            return length > 0f ? (relativePos * force / length) : Vector2.Zero;
        }

        public void Ripple(Vector2 epicenter, float diameter, float update)
        {
            Vector2 relativePos = this.Position - epicenter;

            float distance = relativePos.Length();
            if (distance < (diameter * 0.5F))
            {
                if (distance > TextileConstants.RippleRange)
                {
                    relativePos *= TextileConstants.RippleRange / distance;
                }

                float pull = AverageElementLength(relativePos * Velocity);
                float power = WeightingAdd(distance - diameter, pull / diameter,
                              TextileConstants.RippleAmplitudeWeight) * TextileConstants.Amplitude;

                distance = MathHelper.Max(distance, 0.01f);
                this.Velocity = relativePos * power * update / distance;
            }
        }

        private static float WeightingAdd(float valueA, float valueB, float weight)
        {
            return (valueA * weight) + (valueB * (1.0f - weight));
        }

        private static float AverageElementLength(Vector2 vector)
        {
            return (vector.X + vector.Y) / 2.0f;
        }
	}
}
