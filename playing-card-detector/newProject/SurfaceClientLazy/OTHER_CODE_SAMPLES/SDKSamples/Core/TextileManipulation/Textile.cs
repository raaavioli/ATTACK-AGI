using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TextileManipulation
{
    [Flags]
    public enum RenderStyles
    {
        Wire = 0x01,
        Texture = 0x02,
    }

    public sealed class Textile
    {
        private readonly IList<Stitch> stitches = new List<Stitch>();

        private readonly ManipulationAdapter manipulation;
        private readonly Dictionary<int, List<Stitch>> capturingStitches = new Dictionary<int, List<Stitch>>();

        private static readonly Random random = new Random();

        private static int nextID;

        private Texture2D texture;
        private readonly VertexPositionColor[] linesVertices;
        private readonly VertexPositionTexture[] textureVertices;
        private short[] textureIndices;

        private readonly int columnCount;
        private readonly int rowCount;

        private static readonly BlendState LinesBlendState;
        private static readonly BlendState TextureBlendState;

        public int Id { get; private set; }

        /// <summary>
        /// Static constructor
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Textile()
        {
            LinesBlendState = new BlendState();
            LinesBlendState.AlphaSourceBlend = Blend.SourceAlpha;
            LinesBlendState.AlphaDestinationBlend = Blend.DestinationAlpha;
            LinesBlendState.ColorSourceBlend = Blend.SourceAlpha;
            LinesBlendState.ColorDestinationBlend = Blend.DestinationAlpha;

            TextureBlendState = new BlendState();
            TextureBlendState.AlphaSourceBlend = Blend.SourceColor;
            TextureBlendState.AlphaDestinationBlend = Blend.InverseDestinationAlpha;
            TextureBlendState.ColorSourceBlend = Blend.SourceColor;
            TextureBlendState.ColorDestinationBlend = Blend.InverseDestinationAlpha;
        }

        public Textile(int cols,
                       int rows,
                       Vector2 center,
                       float scale,
                       float orientation,
                       Vector3 startColor,
                       Vector3 endColor,
                       Vector2 screenSize)
        {
            Id = nextID++;
            columnCount = cols;
            rowCount = rows;

            Vector2 clothSize = new Vector2(
                cols * TextileConstants.LengthPerStitch,
                rows * TextileConstants.LengthPerStitch);

            Vector2 centerOffset = clothSize / 2;

            manipulation = new ManipulationAdapter(clothSize, new BoundingRect(screenSize / 2f, screenSize));
            manipulation.Scale = scale;
            manipulation.Center = center;
            manipulation.Orientation = orientation;

            int jointCount = 0;
            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    Color color = GetColor(col, row, cols, rows, startColor, endColor);
                    Stitch stitch = new Stitch(
                        this,
                        new Vector2(
                            (col * TextileConstants.LengthPerStitch) - centerOffset.X,
                            (row * TextileConstants.LengthPerStitch) - centerOffset.Y),
                            new Vector2((float)col / cols, (float)row /rows),
                            color);

                    if (row > 0)
                    {
                        Stitch upperStitch = stitches[stitches.Count - 1];
                        upperStitch.ReferencedStitches.Add(stitch);
                        stitch.ReferencingStitches.Add(upperStitch);
                        jointCount++;
                    }
                    if (col > 0)
                    {
                        Stitch leftStitch = stitches[stitches.Count - rows];
                        leftStitch.ReferencedStitches.Add(stitch);
                        stitch.ReferencingStitches.Add(leftStitch);
                        jointCount++;
                   }
                    stitches.Add(stitch);
                }
            }

            linesVertices = new VertexPositionColor[jointCount * 2];
            textureVertices = new VertexPositionTexture[stitches.Count];

            RenderStyle = RenderStyles.Wire | RenderStyles.Texture;

            Ripple(manipulation.Transform(Vector2.Zero), clothSize.Length() * (float)random.NextDouble(), -0.2f * (float)random.NextDouble());
        }

        private static Color GetColor(int col, int row, int columns, int rows, Vector3 startColor, Vector3 endColor)
        {
            float colFactor = (float)col / columns;
            float rowFactor = (float)row / rows;

            Vector3 diff = endColor - startColor;

            return new Color(new Vector3(
                startColor.X + (diff.X * rowFactor),
                startColor.Y + (diff.Y * colFactor),
                startColor.Z + (diff.Z * (colFactor + rowFactor) / 2.0f)));
        }

        public RenderStyles RenderStyle { get; set; }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        public float Scale
        {
            get { return manipulation.Scale; }
        }

        public float Orientation
        {
            get { return manipulation.Orientation; }
        }

        public Vector2 Center
        {
            get { return manipulation.Center; }
        }

        internal Vector2 Transform(Vector2 position)
        {
            return manipulation.Transform(position);
        }

        /// <summary>
        /// Returns true if touch position hits close to this textile.
        /// </summary>
        /// <param name="position">Touch position in screen coordinates.</param>
        /// <returns></returns>
        internal bool HitTest(Vector2 position)
        {
            const float closestDistance = TextileConstants.LengthPerStitch * 0.65f;

            foreach (Stitch stitch in stitches)
            {
                Vector2 distance = stitch.Position - position;
                if (distance.Length() < closestDistance)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool TryAddTouch(int touchId, Vector2 position)
        {
            float closestDistance = TextileConstants.LengthPerStitch * 0.65f;
            Stitch closestStitch = null;

            foreach (Stitch stitch in stitches)
            {
                Vector2 distance = stitch.Position - position;
                if (distance.Length() < closestDistance)
                {
                    closestDistance = distance.Length();
                    closestStitch = stitch;
                }
            }
            if (closestStitch != null)
            {
                if (capturingStitches.ContainsKey(touchId) == false)
                {
                    capturingStitches.Add(touchId, new List<Stitch>());
                }
                capturingStitches[touchId].Add(closestStitch);
                manipulation.AddTouch(touchId, position);

                return true;
            }

            return false;
        }

        internal bool IsCapturing(int touchId)
        {
            return capturingStitches.ContainsKey(touchId);
        }

        internal bool TryRemoveTouch(int touchId)
        {
            if (capturingStitches.ContainsKey(touchId))
            {
                foreach (Stitch stitch in capturingStitches[touchId])
                {
                    if (stitch.Owner == this)
                    {
                        manipulation.RemoveTouch(touchId);
                    }
                }
                capturingStitches[touchId].Clear();
                capturingStitches.Remove(touchId);
                return true;
            }

            return false;
        }

        public void Ripple(Vector2 epicenter, float diameter, float update)
        {
            foreach (Stitch stitch in stitches)
            {
                stitch.Ripple(epicenter, diameter, update);
            }
        }

        internal int CapturedTouchCount
        {
            get
            {
                return capturingStitches.Keys.Count;
            }
        }

        internal void Update(Dictionary<int, Vector2> touchItems)
        {
            foreach (Stitch stitch in stitches)
            {
                stitch.UpdateAcceleration();
            }

            foreach (int touchId in touchItems.Keys)
            {
                if (capturingStitches.ContainsKey(touchId))
                {
                    foreach (Stitch stitch in capturingStitches[touchId])
                    {
                        if (stitch.Owner == this)
                        {
                            Vector2 touchPosition = touchItems[touchId];
                            manipulation.UpdateTouch(touchId, touchPosition);
                            stitch.Position = touchPosition;
                            stitch.Move(stitch, 1.0f);
                        }
                    }
                }
            }

            foreach (Stitch stitch in stitches)
            {
                stitch.Update();
            }

            foreach (Stitch stitch in stitches)
            {
                stitch.UpdateRelationalVelocity();
            }

            manipulation.ProcessTouches();
        }

        internal void Draw(GraphicsDevice device, Effect effectLines, BasicEffect effectTexture)
        {
            if ((RenderStyle & RenderStyles.Texture) == RenderStyles.Texture && texture != null)
            {
                effectTexture.Texture = texture;
                DrawTexture(device, effectTexture);
            }

            if ((RenderStyle & RenderStyles.Wire) == RenderStyles.Wire || texture == null)
            {
                DrawLines(device, effectLines);
            }
        }

        private void DrawLines(GraphicsDevice device, Effect effect)
        {
            UpdateColoredLinesVertices();

            device.BlendState = LinesBlendState;
            device.RasterizerState = RasterizerState.CullClockwise;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.LineList, linesVertices, 0, linesVertices.Length / 2);
            }
        }

        private void DrawTexture(GraphicsDevice device, Effect effect)
        {
            UpdateTextureVertices();
            short[] indices = GetTextureIndices(columnCount, rowCount);

            device.BlendState = TextureBlendState;
            device.RasterizerState = RasterizerState.CullNone;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList, textureVertices, 0, textureVertices.Length,
                        indices, 0, indices.Length / 3);
            }
        }

        private void UpdateColoredLinesVertices()
        {
            int index = 0;
            foreach (Stitch stitch1 in stitches)
            {
                foreach (Stitch stitch2 in stitch1.ReferencedStitches)
                {
                    linesVertices[index].Color = stitch1.Color;
                    linesVertices[index].Position = new Vector3(stitch1.Position, 0);
                    index++;
                    linesVertices[index].Color = stitch2.Color;
                    linesVertices[index].Position = new Vector3(stitch2.Position, 0);
                    index++;
                }
            }
        }

        private void UpdateTextureVertices()
        {
            int index = 0;
            foreach (Stitch stitch in stitches)
            {
                textureVertices[index].Position = new Vector3(stitch.Position, 0f);
                textureVertices[index++].TextureCoordinate = stitch.Coordinate;
            }
        }

        private short[] GetTextureIndices(int cols, int rows)
        {
            if (textureIndices == null)
            {
                int indicesCount = (cols - 1) * (rows - 1) * 2 * 3;
                textureIndices = new short[indicesCount];
                int index = 0;
                for (int col = 0; col < cols - 1; col++)
                {
                    for (int row = 0; row < rows - 1; row++)
                    {
                        int basePosition = (col * rows);
                        textureIndices[index++] = (short)(basePosition + row);
                        textureIndices[index++] = (short)(basePosition + row + rows);
                        textureIndices[index++] = (short)(basePosition + row + 1);

                        textureIndices[index++] = (short)(basePosition + row + rows);
                        textureIndices[index++] = (short)(basePosition + row + rows + 1);
                        textureIndices[index++] = (short)(basePosition + row + 1);
                    }
                }
            }
            return textureIndices;
        }
    }
}
