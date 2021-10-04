using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CoreInteractionFramework;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cloth.UI
{
    /// <summary>
    /// An abstract class that represents a View of a UIElement in the Model-View-Controller pattern.
    /// </summary>
    /// <remarks>
    /// Because this class is supplied as source and not in a library, we can allow derived
    /// classes to have access to our member fields without having to worry about versioning
    /// problems.  There are currently 7 protected member fields.
    /// Originally this class was designed for more generality than is required by the Cloth
    /// Sample. Some of that functionality has been preserved.  For example, the ActualRotation
    /// property is still used, even though UIElements are not rotatable at this time.
    /// </remarks>
    public abstract class UIElement : DrawableGameComponent
    {
        private static int instanceCount;

        // Containing UIElement.
        private readonly UIElement parent;

        private readonly UIController controller;

        // Determines if a UIElement responds to touches.
        private bool active = true;

        private string textureSourceFile;
        private Texture2D texture;
        private bool autoScaleTexture = true;

        protected bool initialized;

        private SpriteBatch spriteBatch;

        // SpriteBatch.Begin() Parameters.
        private BlendState spriteBlendState = BlendState.Opaque;
        private SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;

        // SpriteBatch.Draw() Parameters.
        internal Color spriteColor = Color.White;
        internal float layerDepth = 1f;                                   // 0.0 (front) .. 1.0 (back)

        private UserOrientation currentOrientation = UserOrientation.Bottom;
        private  Matrix screenTransform = Matrix.Identity;

        private Vector2 transformedCenter;

        // Normalized position relative to parent's Center
        // expressed as a two fractions between -0.5 and 0.5.
        private Vector2 relativePosition;

        // Center of element is restricted to boundary of its container.
        private bool restrictCenter;

        // Rotation
        // Keep track of our relative rotation.
        protected float rotation;

        private Vector2 scale = new Vector2(1.0f, 1.0f);
        private readonly Vector2 minScale = new Vector2(0.001f, 0.001f);
        private readonly Vector2 maxScale = new Vector2(1000.0f, 1000.0f);

        // The width and height requested in constructor.
        // Once the texture is loaded a suitable scale factor is computed.
        private int width;
        private int height;

        protected readonly List<UIElement> children = new List<UIElement>();

        public void SortChildrenByLayerDepth()
        {
            // Sort children by LayerDepth.
            children.Sort((left, right) => (int) (left.LayerDepth - right.layerDepth));
        }

        /// <summary>
        /// Adds a child to this UIElement.
        /// </summary>
        /// <remarks>Maintain children in LayerDepth order to ensure proper hit testing.</remarks>
        /// <param name="child"></param>
        public void AddChild(UIElement child)
        {
            // Require that the child have been created with this element as its parent.
            Debug.Assert(child.Parent == this);
            if (!children.Contains(child))
            {
                children.Add(child);
                SortChildrenByLayerDepth();
            }
        }

        /// <summary>
        /// Removes a child from this UIElements list of children.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(UIElement child)
        {
            children.Remove(child);
            // Assumption: removing an element from a list does not alter the order of other elements.
        }

        #region Constructors

        /// <summary>
        /// Base constructor for UIElement class.  Only derived classes can call the base
        /// constructor.
        /// </summary>
        /// <param name="game">Game to which the UIElement belongs.</param>
        /// <param name="controller">UIController associated with this UIElements state machine.</param>
        /// <param name="textureFile">The name of a source file used to create a Texture2D
        /// used to render thisUIElement</param>
        /// <param name="position">
        /// Nullable Vector2 containing the desired position of this element.
        /// Position is in screen coordinates for top-level elements.  Otherwise,
        /// it is relative to the center of the parent.
        /// </param>
        /// <param name="width">Requested render width of UIElement in pixels</param>
        /// <param name="height">Requested render height of UIElement in pixels</param>
        /// <param name="parent">UIElement containing this element.</param>
        protected UIElement(Game game,
                            UIController controller,
                            string textureFile,
                            Vector2? position,
                            int width, int height,
                            UIElement parent)
            : this(game, controller, textureFile, null, position, width, height, parent)
        {
            // Empty.
        }


        /// <summary>
        /// Base constructor for UIElement.  Only derived classes may call the base constructor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="controller"></param>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="parent"></param>
        protected UIElement(Game game,
                            UIController controller,
                            Vector2? position,
                            int width, int height,
                            UIElement parent)
            : this(game, controller, null, null, position, width, height, parent)
        {
            // Empty.
        }

        /// <summary>
        /// Base constructor for UIElement.  Only derived classes may call the base constructor.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected UIElement(UIElement parent, Vector2 position, int width, int height)
            : this(parent.Game, parent.Controller, (string) null, (Texture2D) null,
                   position, width, height, parent)
        {
            // Empty.
        }

        /// <summary>
        /// Private contructor invoked by overloaded base contructors to build the UIElement.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="controller"></param>
        /// <param name="textureFile"></param>
        /// <param name="texture"></param>
        /// <param name="position">Position of the UIElement (relative or screen coordinates)</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="parent"></param>
        private UIElement(Game game,
                          UIController controller,
                          string textureFile, Texture2D texture,
                          Vector2? position,
                          int width, int height,
                          UIElement parent)
            : base(game)
        {
            // Should only set one or the other, not both.
            Debug.Assert(textureFile == null || texture == null);

            this.controller = controller;
            this.textureSourceFile = textureFile;
            this.texture = texture;
            this.width = width;
            this.height = height;
            this.parent = parent;

            instanceCount++;
            Name = "UIElement " + instanceCount.ToString(CultureInfo.InvariantCulture);

            if (parent != null)
            {
                // Inherit some properties from the parent (containing) UIElement.
                currentOrientation = parent.currentOrientation;
                screenTransform = parent.ScreenTransform;
                spriteBlendState = parent.SpriteBlendState;
                spriteSortMode = parent.spriteSortMode;

                // Draw children in front of parents.
                layerDepth = parent.layerDepth * 0.9f;

                // Position is a relative to parent's Center.
                if (position != null)
                {
                    relativePosition = Vector2.Clamp(position.Value,
                                                     new Vector2(-0.5f, -0.5f),
                                                     new Vector2(0.5f, 0.5f));
                    Vector2 center = parent.Center;
                    float x = center.X + relativePosition.X * parent.Width;
                    float y = center.Y + relativePosition.Y * parent.Height;
                    Center = new Vector2(x, y);
                }

                // Add this to parent's liest of children.
                parent.AddChild(this);
            }
            else
            {
                // Position is in screen coordinates.
                if (position != null)
                {
                    Center = position.Value;
                }
            }
        }

        #endregion Constructors

        #region Basic Properties

        public UIElement Parent
        {
            get { return parent; }
        }

        public string Name { get; set; }

        public UIController Controller
        {
            get { return controller; }
        }

        /// <summary>
        /// The state machine associated with this UIElement.
        /// </summary>
        /// <remarks>
        /// The constructors for concrete instances of this class
        /// should instantiate a state machine with the UIController above,
        /// and set the Tag property of the statemachine to the containing UIElement
        /// (i.e. StateMachine.Tag = this).
        /// </remarks>
        public UIElementStateMachine StateMachine { get; protected set; }

        /// <summary>
        /// Read-write property that determines if this UIElement should respond to touches.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                // Only update if state is changing.
                if (active != value)
                {
                    active = value;
                }
            }
        }


        /// <summary>
        /// Read-write property that gets or set the BlendState used to initialize this
        /// UIElement's spriteBatch.
        /// </summary>
        public BlendState SpriteBlendState
        {
            get { return spriteBlendState; }
            set { spriteBlendState = value; }
        }

        /// <summary>
        /// Read-write property that gets or set the SpriteSortMode used to initialize this
        /// UIElement's spriteBatch.
        /// </summary>
        public SpriteSortMode SpriteSortMode
        {
            get { return spriteSortMode; }
            set { spriteSortMode = value; }
        }

        /// <summary>
        /// Read-write property that gets or sets the SpriteColor used when this
        /// element is drawn.
        /// </summary>
        public Color SpriteColor
        {
            get { return spriteColor; }
            set { spriteColor = value; }
        }

        /// <summary>
        /// Read-write property that gets or sets the SpriteEffects used when this
        /// UIElement is drawn.
        /// </summary>
        public SpriteEffects SpriteEffects { get; set; }

        /// <summary>
        /// Read-write property that gets or sets the LayerDepth used when this
        /// UIElement is drawn.
        /// </summary>
        public float LayerDepth
        {
            get { return layerDepth; }
            set
            {
                if (layerDepth != value)
                {
                    layerDepth = value;
                    if (parent != null)
                    {
                        parent.SortChildrenByLayerDepth();
                    }
                }
            }
        }


        /// <summary>
        /// Returns the top-level containing UIElement of this UIElement.
        /// </summary>
        public UIElement Root
        {
            get
            {
                return Parent == null ? this : Parent.Root;
            }
        }

        /// <summary>
        /// Finds the layerDepth of the front-most element starting at node.
        /// </summary>
        /// <param name="node">UIElement node to begin search.</param>
        /// <returns>The minimum layerDepth for all elements below node.</returns>
        public float MinimumLayerDepth(UIElement node)
        {
            float minimum = node.LayerDepth;
            foreach (UIElement child in node.children)
            {
                float newMininum = MinimumLayerDepth(child);
                if (newMininum < minimum) {
                    minimum = newMininum;
                }
            }
            return minimum;
        }

        /// <summary>
        /// Finds the the layer depth of the front-most element in this
        /// UIElement's container hierarchy.
        /// </summary>
        /// <returns></returns>
        public float MinimumLayerDepth()
        {
            return MinimumLayerDepth(Root);
        }

        /// <summary>
        /// Gets or sets the file name associated with the texture for this UIElement.
        /// </summary>
        public string TextureSourceFile
        {
            get { return textureSourceFile; }
            set
            {
                textureSourceFile = value;
                if (GraphicsDevice != null)
                {
                    Texture = TextureFromFile(value);
                }
            }
        }

        /// <summary>
        /// Read-write property that determines if the UIElements texture should automatically
        /// scale to the size of the UIElement.
        /// </summary>
        public bool AutoScaleTexture
        {
            get { return autoScaleTexture; }
            set { autoScaleTexture = value; }
        }

        /// <summary>
        /// Read-write property that gets or sets the default texture associated with this UIElement.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set
            {
                texture = value;
                ResetScale();
            }
        }

        /// <summary>
        /// Read-write property that gets or sets the SpriteBatch in which this UIElement should
        /// be drawn.
        /// </summary>
        protected SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
            set { spriteBatch = value; }
        }

        #endregion

        #region Layout and Hit Testing Properties

        /// <summary>
        /// Gets or sets the screenTransform that is applied to SpriteBatch used to
        /// draw this UIElement and its children.
        /// </summary>
        public Matrix ScreenTransform
        {
            get { return screenTransform; }
            set
            {
                // We only want to adjust the positon of top-level elements.
                // Child elements will be adjusted when drawn.
                // The screen transform will be applied to their SpriteBatch,
                // and their TransformedCenter will be calculated relative to
                // the parent element.
                if (parent == null)
                {
                    // Take the original untransformed Center
                    // and apply the new transform.
                    Vector2 center = Center;
                    TransformedCenter = Vector2.Transform(center, value);
                }
                screenTransform = value;
            }
        }

        /// <summary>
        /// Updates UIElements when Shell orientation changes.
        /// </summary>
        /// <param name="newOrientation"></param>
        /// <param name="transform"></param>
        public void ResetOrientation(UserOrientation newOrientation, Matrix transform)
        {
            if (newOrientation == currentOrientation) return;

            currentOrientation = newOrientation;
            ScreenTransform = transform;

            OnOrientationReset(newOrientation, transform);

            foreach (UIElement child in children)
            {
                child.ResetOrientation(newOrientation, transform);
            }
        }

        /// <summary>
        /// Allow derived classes to perform additional operations
        /// when orientation changes.  This would be necessary for items not
        /// affected by the SpriteBatch transform.
        /// </summary>
        /// <param name="newOrientation">The new user orientation.</param>
        /// <param name="transform">The transform associated with the new orientation.</param>
        protected virtual void OnOrientationReset(UserOrientation newOrientation, Matrix transform)
        {
            // Empty.
        }

        /// <summary>
        /// The transformed center of the object.
        /// All layout is based off of this property.
        /// </summary>
        public Vector2 TransformedCenter
        {
            get
            {
                if (parent == null)
                {
                    return transformedCenter;
                }
                else
                {
                    float x = parent.TransformedCenter.X + relativePosition.X * parent.Width;
                    float y = parent.TransformedCenter.Y + relativePosition.Y * parent.Height;
                    return new Vector2(x, y);
                }
            }
            set
            {
                value = ConstrainedCenter(value);
                if (parent != null)
                {
                    float x = ((value.X - Parent.TransformedCenter.X) / parent.Width);
                    float y = ((value.Y - Parent.TransformedCenter.Y) / parent.Height);
                    relativePosition = new Vector2(x, y);
                }
                transformedCenter = value;
            }
        }

        /// <summary>
        /// The center of the object.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                // Apply inverse Transform to transformedCenter
                return Vector2.Transform(TransformedCenter, Matrix.Invert(screenTransform));
            }
            set
            {
                // Apply Transform to Center
                TransformedCenter = Vector2.Transform(value, screenTransform);
            }
        }

        /// <summary>
        /// The height of the object. Does not reflect changes to
        /// height based on rotation.
        /// </summary>
        public float Height
        {
            get
            {
                if (texture != null && !IgnoreTextureSize)
                {
                    return (float)texture.Height * ActualScale.Y;
                }
                else
                {
                    return height;
                }
            }
            set
            {
                height = (int)Math.Round(value);
                ResetScale();
            }

        }

        /// <summary>
        /// The width of the object. Does not reflect changes to
        /// width based on rotation.
        /// </summary>
        public float Width
        {
            get
            {
                if (texture != null && !IgnoreTextureSize)
                {
                    return (float)Texture.Width * ActualScale.X;
                }
                else
                {
                    return width;
                }
            }
            set
            {
                width = (int)Math.Round(value);
                ResetScale();
            }
        }

        /// <summary>
        /// Don't respect the size of texture if this value is true.
        /// By setting true to this, it always uses the size that you passed to the constructor.
        /// This is typically used when your derived class renders the texture by its own logic and the actual rendered size is not the same as texture size.
        /// e.g. You are rendering with tiling.
        /// </summary>
        protected bool IgnoreTextureSize { get; set; }

        /// <summary>
        /// The X position of the left side of the object after scaling is applied.
        /// </summary>
        public float Left
        {
            get
            {
                return TransformedCenter.X - Width / 2;
            }
            set
            {
                TransformedCenter = new Vector2(value + Width / 2, TransformedCenter.Y);
            }
        }

        /// <summary>
        /// The X position of the right side of the object after scaling is applied.
        /// </summary>
        public float Right
        {
            get
            {
                return TransformedCenter.X + Width / 2;
            }
        }

        /// <summary>
        /// The Y position of the top of the element after scaling is applied.
        /// </summary>
        public float Top
        {
            get
            {
                return TransformedCenter.Y - Height / 2;
            }
            set
            {
                TransformedCenter = new Vector2(TransformedCenter.X, value + Height / 2);
            }
        }

        /// <summary>
        /// The Y position of the bottom of the element after scaling is applied.
        /// </summary>
        public float Bottom
        {
            get
            {
                return TransformedCenter.Y + Height / 2;
            }
        }

        /// <summary>
        /// Gets or sets a UIElement's position relative to its parent container.
        /// </summary>
        public Vector2 RelativePosition
        {
            get
            {
                if (Parent != null)
                {
                    return relativePosition;
                }
                throw new InvalidOperationException("Relative positioning requires a parent.");
            }
            set
            {
                if (Parent != null)
                {
                    float x = Parent.TransformedCenter.X + Parent.Width * MathHelper.Clamp(value.X, -0.5f, 0.5F);
                    float y = Parent.TransformedCenter.Y + Parent.Height * MathHelper.Clamp(value.Y, -0.5f, 0.5F);
                    TransformedCenter = new Vector2(x, y);
                }
                else
                {
                    throw new InvalidOperationException("Relative positioning requires a parent.");
                }
            }
        }

        /// <summary>
        /// Gets the UIElement's drawing rectangle (i.e. where it would render on the screen).
        /// </summary>
        public Rectangle DrawingRectangle
        {
            get
            {
                Vector2 topLeft = new Vector2(Left, Top);
                Vector2 bottomRight = new Vector2(Right, Bottom);

                Matrix rotationMatrix = Matrix.CreateRotationZ(ActualRotation);

                topLeft = Vector2.Transform(topLeft, rotationMatrix);
                bottomRight = Vector2.Transform(bottomRight, rotationMatrix);

                Rectangle result = new Rectangle((int)Math.Round(topLeft.X),
                                                 (int)Math.Round(topLeft.Y),
                                                 (int)Math.Round(bottomRight.X - topLeft.X),
                                                 (int)Math.Round(bottomRight.Y - topLeft.Y));
                return result;
            }
        }

        /// <summary>
        /// The drawing origin for the item. Drawing origin is loosely equivilant
        /// to WPF's transform origin, but it is specified in pixels from the top left,
        /// not as a value between 0 and 1. When XNA draws, it uses the DrawingOrigin
        /// before it applies scale or rotation, so use _unmodified_ values here.
        /// </summary>
        public Vector2 DrawingOrigin
        {
            get
            {
                if (texture != null && !IgnoreTextureSize)
                {
                    return new Vector2(texture.Width/2.0f, texture.Height/2.0f);
                }

                return new Vector2(width, height);
            }
        }

        /// <summary>
        /// Read-only property that computes an element's actual scale base
        /// on the product of scales of all its ancestors.
        /// </summary>
        public Vector2 ActualScale
        {
            get
            {
                if (parent != null)
                {
                    return scale * parent.ActualScale;
                }
                else
                {
                    return scale;
                }
            }
        }

        /// <summary>
        /// Read-only property that computes an element's actual rotaion based
        /// on the sum of rotations of all it's ancestors.
        /// </summary>
        public float ActualRotation
        {
            get
            {
                if (parent != null)
                {
                    return rotation + parent.ActualRotation;
                }
                else
                {
                    return rotation;
                }
            }
        }

        /// <summary>
        /// Read-write property that determines if an element's center is restricted to the
        /// boundaries of its containing element.
        /// </summary>
        public bool RestrictCenter
        {
            get { return restrictCenter; }
            set { restrictCenter = value; }
        }

        #endregion Layout and Hit Testing Properties

        #region Overrriden virtual methods for DrawableGameComponent

        /// <summary>
        /// Initialize this UIElement and all of its children.
        /// </summary>
        /// <remarks>
        /// Extends the Initialze method from DrawableGameComponent.
        /// </remarks>
        public override void Initialize()
        {
            // A UIElement can be a GameComponent as well as a member of UIElement
            // container hierarchy.  Avoid calling Initialize (and LoadContent) twice.
            if (initialized) { return; }

            base.Initialize();

            foreach (UIElement child in children)
            {
                child.Initialize();
            }

            initialized = true;
        }

        /// <summary>
        /// Loads the content for this UIElement.
        /// </summary>
        /// <remarks>
        /// Extends the LoadContent method from DrawableGameComponent.
        /// </remarks>
        protected override void LoadContent()
        {
           base.LoadContent();

           if (textureSourceFile != null)
            {
                if (AutoScaleTexture)
                {
                      texture = TextureFromFile(textureSourceFile, width, height);
                }
                else
                {
                      texture = TextureFromFile(textureSourceFile);
                }
            }

            ResetScale();

            if (parent == null)
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            }
            else
            {
                spriteBatch = parent.SpriteBatch;
            }

            // Do not need to call children's LoadContent().
            // LoadContent() is call from base.Initialize().

        }

        /// <summary>
        /// Resets the texturing scaling so that the UIElement texture fills
        /// the area assigned to the UIElement, if AutoScaleTexture is true.
        /// </summary>
        private void ResetScale()
        {
            if (texture == null)
            {
                scale = Vector2.One;
                return;
            }

            // Use Texture size if width or height no specified.
            if (width == 0) { width = texture.Width; }
            if (height == 0) { height = texture.Height; }

            // Compute scaling based on requested width and height.
            if (AutoScaleTexture)
            {
                scale.X = (float)width / texture.Width;
                scale.Y = (float)height / texture.Height;
                scale = Vector2.Clamp(scale, minScale, maxScale);
            }
        }

        /// <summary>
        /// Unload content for this UIElement and all of its children.
        /// </summary>
        /// <remarks>
        /// Extends UnloadContent method from DrawableGameComponent.
        /// UnloadContent gets called from DrawableGameComponent.Dispose(disposing)
        /// </remarks>
        protected override void UnloadContent()
        {
            base.UnloadContent();

            // Do not call UnloadContent() on this element's children.
            // The Dispose(disposing) method calls Dispose() for each child
            // and UnLoadContent() gets called from the base.Dispose(disposing) method.

        }

        /// <summary>
        /// Updates the UIElement and all of its children.
        /// </summary>
        /// <remarks>
        /// Exteneds Update method from GameComponent.
        /// </remarks>
        /// <param name="gameTime">Snapshot of game timing state.</param>
        public override void Update(GameTime gameTime)
        {
            foreach (UIElement child in children)
            {
                child.Update(gameTime);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the UIElement and all of its children in a SpriteBatch.
        /// </summary>
        /// <remarks>
        /// Overrides Draw method from DrawableGameComponent.
        /// </remarks>
        /// <param name="gameTime">Snapshot of game timing state.</param>
        public override void Draw(GameTime gameTime)
        {
            if (parent == null)
            {
                // Top-Level Containers are resposible for calling spriteBatch.Begin().
                spriteBatch.Begin(spriteSortMode, spriteBlendState, null, null, null, null, screenTransform);
            }

            // Draw this element.
            if (Visible)
            {
                Draw(spriteBatch, gameTime);
            }

            // Draw all the children.
            foreach (UIElement child in children)
            {
                child.Draw(gameTime);
            }

            if (parent == null)
            {
                // Top-Level Containers are resposible for calling spriteBatch.End().
                spriteBatch.End();
            }
        }

        /// <summary>
        /// Peforms UIElement-specific drawing.
        /// </summary>
        /// <remarks>
        /// Most UIElements should override this method and not the Draw() method
        /// from DrawableGameComponent.
        /// </remarks>
        /// <param name="batch">SpriteBatch for this UIElement container hierarchy.</param>
        /// <param name="gameTime">Snapshot of game timing state.</param>
        public virtual void Draw(SpriteBatch batch, GameTime gameTime)
        {
            if (texture != null)
            {
                batch.Draw(Texture, TransformedCenter, null, SpriteColor, ActualRotation, DrawingOrigin,
                              ActualScale, SpriteEffects, LayerDepth);
            }
        }

        #endregion

        #region Hit Testing

        /// <summary>
        /// Return HitTestDetails for the state machine associated with this UIElement.
        /// </summary>
        /// <remarks>
        /// A derived class must override this method if it encapsulates a type of state machine that
        /// supplies some type of IHitTestDetails.
        /// <para>
        /// HitTestDetails() is called in three scenarios:
        /// <list>
        /// <item>An uncaptured touch over the element.</item>
        /// <item>A previously captured touch still over the element.</item>
        /// <item>A previously captured touch no longer over the element.</item>
        /// </list>
        /// Determining what the HitTestDetails should be in this last case may require
        /// some thought.
        /// </para>
        /// </remarks>
        /// <param name="touch">The touch for which hit test details should be supplied.</param>
        /// <param name="captured">boolean indicating if the touch was previously captured.</param>
        /// <returns></returns>
        public virtual IHitTestDetails HitTestDetails(TouchPoint touch, bool captured)
        {
            return null;
        }

        /// <summary>
        /// Returns true if the touch is contained within this element.
        /// </summary>
        /// <param name="touch">The touch to hit test.</param>
        /// <param name="captured">Boolean indicating if the touch was previously captured.</param>
        /// <returns></returns>
        public virtual bool HitTest(TouchPoint touch, bool captured)
        {
            // Controls must be Active and Visible in order to respond to touches.
            if (!Active || !Visible) { return false; }

            Vector2 touchVector = Vector2.Transform(new Vector2(touch.CenterX, touch.CenterY), screenTransform);

            return DrawingRectangle.Contains(Convert.ToInt32(touchVector.X),
                                             Convert.ToInt32(touchVector.Y));
        }

        /// <summary>
        /// Performs hit testing for this UIElement and all its children.
        /// </summary>
        /// <remarks>Recursively test all child elements, and then test this element.</remarks>
        /// <param name="touch"></param>
        /// <param name="captured">Boolean indicating if the touch was previously captured.</param>
        /// <returns>The UIElement hit by a touch or null if no hit is found.</returns>
        public UIElement HitTesting(TouchPoint touch, bool captured)
        {
            // Test each of this elements children first from front to back.
            // Maintain children in LayerDepth order.
            foreach (UIElement child in children)
            {
                UIElement hit = child.HitTesting(touch, captured);
                if (hit != null)
                {
                    return hit;
                }
            }

            // Now test this element.
            if (HitTest(touch, captured))
            {
                return this;
            }

            return null;

        }

        /// <summary>
        /// Used by the UIController to determine what UIElement is being touched by a given touch.
        /// </summary>
        /// <param name="uncapturedTouches">All touches touching the app that have not been captured.</param>
        /// <param name="capturedTouches">All touches touching the app that have already been captured.</param>
        public virtual void HitTestCallback(ReadOnlyHitTestResultCollection uncapturedTouches,
                                     ReadOnlyHitTestResultCollection capturedTouches)
        {
            HitTestCapturedTouches(capturedTouches);
            HitTestUncapturedTouches(uncapturedTouches);
        }

        /// <summary>
        /// Performs hit testing on all captured touches and updates the hit test information.
        /// </summary>
        /// <remarks>
        /// This method does not require an instance of UIElement to work.
        /// </remarks>
        /// <param name="capturedTouches">Read-only collection of captured touches to test.</param>
        public static void HitTestCapturedTouches(ReadOnlyHitTestResultCollection capturedTouches)
        {
            // Hit test all previously captured touches.
            foreach (HitTestResult result in capturedTouches)
            {
                // Only hit test touches that have not already been released.
                if (result.StateMachine != null)
                {
                    // Determine if touch is still touching the same element.
                    UIElement element = result.StateMachine.Tag as UIElement;
                    if (element != null)
                    {
                        // Hit test the captured element directly.
                        // This makes overlayed child elements "transparent."
                        if (element.HitTest(result.Touch, true))
                        {
                            // The touch is still touching the same element.
                            // Update the hit test information.
                            result.SetCapturedHitTestInformation(true, element.HitTestDetails(result.Touch, true));
                        }
                        else
                        {
                            // Touch is no longer over the capturing element.
                            result.SetCapturedHitTestInformation(false, element.HitTestDetails(result.Touch, true));
                       }
                    }
                    else
                    {
                        throw new InvalidOperationException(Properties.Resources.StateMachineTagShouldBeUIElement);
                    }
                }
            }
        }

        /// <summary>
        /// Performs hit testing on all uncaptured touches and updates the hit test information.
        /// </summary>
        /// <param name="uncapturedTouches">Read-only collection of uncaptured touches to test.</param>
        public virtual void HitTestUncapturedTouches(ReadOnlyHitTestResultCollection uncapturedTouches)
        {
            // Hit test and assign all new touches.
            foreach (HitTestResult result in uncapturedTouches)
            {
                // Make sure that touch hasn't been assigned already.
                if (result.StateMachine != null) { continue; }

                // Hit test the touch to determine which object it is touching
                UIElement touched = HitTesting(result.Touch, false);
                if (touched != null)
                {
                    IHitTestDetails details = touched.HitTestDetails(result.Touch, false);
                    result.SetUncapturedHitTestInformation(touched.StateMachine, details);
                }
                else
                {
                    // This touch has not been captured.
                    result.SetUncapturedHitTestInformation(null, null);
                }
            }
        }

        #endregion

        /// <summary>
        /// Checks center against boundaries and moved center inside
        /// of boundaries if it is found to be out of bounds.
        /// </summary>
        private Vector2 ConstrainedCenter(Vector2 center)
        {
            if (!RestrictCenter) return center;

            Vector2 topLeft, bottomRight;

            if (parent != null)
            {
                topLeft = new Vector2(parent.Left, parent.Top);
                bottomRight = new Vector2(parent.Right, parent.Bottom);
            }
            else
            {
                Viewport viewport = Game.GraphicsDevice.Viewport;
                topLeft = new Vector2(viewport.X, viewport.Y);
                bottomRight = new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height);
            }

            return Vector2.Clamp(center, topLeft, bottomRight);
        }

        /// <summary>
        /// Returns a Texture2D object loaded from the specified file.
        /// </summary>
        protected Texture2D TextureFromFile(string path)
        {
            using (Stream textureFileStream = File.OpenRead(path))
            {
                return Texture2D.FromStream(Game.GraphicsDevice, textureFileStream);
            }
        }

        /// <summary>
        /// Returns a Texture2D object loaded from the specified file.
        /// </summary>
        protected Texture2D TextureFromFile(string path, int textureWidth, int textureHeight)
        {
            using (Stream textureFileStream = File.OpenRead(path))
            {
                return Texture2D.FromStream(Game.GraphicsDevice, textureFileStream, textureWidth, textureHeight, true);
            }
        }

        #region Static Utility Methods

        /// <summary>
        /// Finds the center of a Rectangle.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns>A Vector2 containing the center of the rectangle</returns>
        public static Vector2 CenterOf(Rectangle rectangle)
        {
            return new Vector2(rectangle.Left + rectangle.Width / 2f, rectangle.Top + rectangle.Height / 2f);
        }

        /// <summary>
        /// Finds the center of a Texture2D.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>A Vector2 containing the center of the rectangle.</returns>
        public static Vector2 CenterOf(Texture2D texture)
        {
            return new Vector2(texture.Width / 2f, texture.Height / 2f);
        }


        /// <summary>
        /// Computes a relative position for a child element.
        /// </summary>
        /// <remarks>
        /// The child will be centered horizontally and the top edge of the child
        /// will be atthe specified offset from the top edge of the parent.
        /// </remarks>
        /// <param name="verticalOffset">Desired offset from top edge of the parent in pixels.</param>
        /// <param name="childHeight">Height of child in pixels.</param>
        /// <param name="parent">The containing UIElement.</param>
        /// <returns>A Vector2 containing the computed center relative position.</returns>
        public static Vector2 CenterHorizontal(int verticalOffset, float childHeight, UIElement parent)
        {
            float y = (verticalOffset + (childHeight / 2f)) / parent.Height - 0.5f;

            return new Vector2(0.0f, y);
        }

        /// <summary>
        /// Compute the offset from left (or top) edge of a container
        /// to the left (or top) edge of the contained item so that the
        /// contained item will be centered along the horizontal (or vertical)
        /// axis of the containing element.
        /// </summary>
        /// <param name="itemSize">size of the item(s)0 to be centered along
        /// the axis on which to center the item(s).</param>
        /// <param name="start"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        public static float CenterOffset(float itemSize, float start, float totalSize)
        {
            return start + (totalSize - itemSize) / 2f;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (parent == null && spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }
                foreach (UIElement child in children)
                {
                    child.Dispose();
                }
                children.Clear();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }

}
