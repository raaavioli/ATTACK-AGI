using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Input.Manipulations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreInteractionFramework;

namespace XnaScatter
{
    /// <summary>
    /// The class that represents a scatterViewItem.
    /// </summary>
    /// <remarks>
    /// XnaScatterViewItem inherits from UIElementStateMachine, but does not fully implement
    /// a standard controller/view/model design pattern like most CoreInteractionFramework 
    /// controls. The purpose here is to demonstrate how to use the contact capture and 
    /// routing mechanisms in CIF without using the other aspects of the  framework. 
    /// </remarks>
    public class XnaScatterViewItem : UIElementStateMachine, ITouchableObject, IDisposable
    {
        private const float DefaultDpi = 96.0f;

        // deceleration: inches/second squared 
        private const float Deceleration = 10.0f * DefaultDpi / (1000.0f * 1000.0f);
        private const float ExpansionDeceleration = 16.0f * DefaultDpi / (1000.0f * 1000.0f);

        // angular deceleration: degrees/second squared
        private const float AngularDeceleration = 270.0f / 180.0f * (float)Math.PI / (1000.0f * 1000.0f);

        // minimum flick velocities
        private const float MinimumFlickVelocity = 2.0f * DefaultDpi / 1000.0f;                     // =2 inches per sec
        private const float MinimumAngularFlickVelocity = 45.0f / 180.0f * (float)Math.PI / 1000.0f; // =45 degrees per sec
        private const float MinimumExpansionFlickVelocity = 2.0f * DefaultDpi / 1000.0f;            // =2 inches per sec

        private readonly string textureSourceFile;
        private Texture2D content;
        private Vector2 transformedCenter;
        private float scaleFactor = 0.5f;
        private const float MinScaleFactor = 0.25f;
        private const float MaxScaleFactor = 2.0f;
        private float zoomFactor = 1.0f;
        private float orientation;

        private readonly XnaScatterView parent;

        public XnaScatterView Parent
        {
            get { return parent; }
        }

        private ManipulationProcessor2D manipulationProcessor;
        private InertiaProcessor2D inertiaProcessor;
        private bool manipulating;
        private bool extrapolating;
        private List<Manipulator2D> currentManipulators = new List<Manipulator2D>();

        private bool canTranslate = true;
        private bool canTranslateFlick = true;
        private bool canRotate = true;
        private bool canRotateFlick = true;
        private bool canScale = true;
        private bool canScaleFlick = true;
        private Matrix transform = Matrix.Identity;

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        #region IDisposable

        private bool disposed;

        public bool IsDisposed
        {
            get {return disposed;}
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose of managed resources.
                    content.Dispose();
                }

                // Clean up unmanaged resources.

                disposed = true;
            }
        }

        ~XnaScatterViewItem()
        {
            Dispose(false);
        }

        #endregion

        #region Layout and Hit Testing Properties
        //==========================================================//
        /// <summary>
        /// The screen transform of all of the objects. Contacts need to be hit test based on this
        /// transformation. The center of this is also updated when this property is updated.
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return transform;
            }
            set
            {
                Vector2 center = Center;

                transform = value;

                // Transform the center into the screen coordinate system.
                transformedCenter = Vector2.Transform(center, Transform);
            }
        }

        //==========================================================//
        /// <summary>
        /// The transformed center of the object. All layout is based off of this property.
        /// </summary>
        public Vector2 TransformedCenter
        {
            get
            {
                return transformedCenter;
            }
            set
            {
                transformedCenter = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// The center of the object. All layout is based off of this property.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return Vector2.Transform(transformedCenter, Matrix.Invert(Transform));
            }
            set
            {
                // Transform the center into the screen coordinate system.
                transformedCenter = Vector2.Transform(value, Transform);
            }
        }

        //==========================================================//
        /// <summary>
        /// The scaled hight of the object. Does not reflect changes to
        /// height based on orientation.
        /// </summary>
        public float Height
        {
            get
            {
                return (float)content.Height * zoomFactor * scaleFactor;
            }
        }

        //==========================================================//
        /// <summary>
        /// The scaled width of the object. Does not reflect changes to
        /// width based on orientation.
        /// </summary>
        public float Width
        {
            get
            {
                return (float)content.Width * zoomFactor * scaleFactor;
            }
        }

        //==========================================================//
        /// <summary>
        /// The scale of the object based on the parent's scale.
        /// </summary>
        public float ZoomFactor
        {
            get
            {
                return zoomFactor;
            }
            set
            {
                zoomFactor = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// The drawing origin for the item. Drawing origin is loosely equivilant
        /// to WPF's transform origin, but it is specified in pixels from the top left,
        /// not as a value between 0 and 1. When XNA draws, it uses the DrawingOrigin
        /// before it applies scale or orientation, so use _unmodified_ values here.
        /// </summary>
        private Vector2 DrawingOrigin
        {
            get
            {
                return new Vector2(content.Width / 2, content.Height / 2);
            }
        }

        //==========================================================//
        /// <summary>
        /// The bounding rectangle for the item if its orientation were set to 0.
        /// </summary>
        private Rectangle AxisAlignedBoundingRectangle
        {
            get { return new Rectangle((int)(transformedCenter.X - (Width / 2)), (int)(transformedCenter.Y - (Height / 2)), (int)Width, (int)Height); }
        }

        #endregion

        #region Manipulation Properties

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be moved via direct manipulation.
        /// </summary>
        public bool CanTranslate
        {
            get
            {
                return canTranslate;
            }
            set
            {
                canTranslate = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be moved via flick.
        /// </summary>
        public bool CanTranslateFlick
        {
            get
            {
                return canTranslateFlick;
            }
            set
            {
                canTranslateFlick = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be rotated via direct manipulation.
        /// </summary>
        public bool CanRotate
        {
            get
            {
                return canRotate;
            }
            set
            {
                canRotate = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be rotated via flick.
        /// </summary>
        public bool CanRotateFlick
        {
            get
            {
                return canRotateFlick;
            }
            set
            {
                canRotateFlick = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be scaled via direct manipulation.
        /// </summary>
        public bool CanScale
        {
            get
            {
                return canScale;
            }
            set
            {
                canScale = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be scaled via flick.
        /// </summary>
        public bool CanScaleFlick
        {
            get
            {
                return canScaleFlick;
            }
            set
            {
                canScaleFlick = value;
            }
        }

        #endregion

        //==========================================================//
        /// <summary>
        /// Gets the type of HitTestDetails used to store the results of a hit test against this class.
        /// </summary>
        public override Type TypeOfHitTestDetails
        {
            get
            {
                return typeof(XnaScatterHitTestDetails);
            }
        }

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="passedColor">The color of the item.</param>
        /// <param name="passedParent">The item's parent XnaScatterView.</param>
        public XnaScatterViewItem(UIController controller, string contentImage, XnaScatterView passedParent)
            :base(controller, 1, 1 )
        {
            textureSourceFile = contentImage;
            parent = passedParent;
        }

        //==========================================================//
        /// <summary>
        /// Creates manipulation and inertis processors that support only this item's allowed manipulations.
        /// </summary>
        private void SetAllowedManipulations()
        {
            Manipulations2D supportedManipulations =
                ((canTranslate || canTranslateFlick) ? Manipulations2D.TranslateX | Manipulations2D.TranslateY : Manipulations2D.None) |
                ((canScale || canScaleFlick) ? Manipulations2D.Scale : Manipulations2D.None) |
                ((canRotate || canRotateFlick) ? Manipulations2D.Rotate : Manipulations2D.None);

            manipulationProcessor = new ManipulationProcessor2D(supportedManipulations);
            manipulationProcessor.Started += OnAffine2DManipulationStarted;
            manipulationProcessor.Delta += OnAffine2DDelta;
            manipulationProcessor.Completed += OnAffine2DManipulationCompleted;

            inertiaProcessor = new InertiaProcessor2D();
            inertiaProcessor.Completed += OnAffine2DInertiaCompleted;
            inertiaProcessor.Delta += OnAffine2DDelta;
        }

        //==========================================================//
        /// <summary>
        /// Load the image that is used to draw the XNAScatterViewItem.
        /// </summary>
        /// <param name="device">The graphics device that is being for the application.</param>
        /// <param name="contentPath">The path to the content directory.</param>
        public void LoadContent(GraphicsDevice device, String contentPath)
        {
            using (Stream textureFileStream = File.OpenRead(Path.Combine(contentPath, textureSourceFile)))
            {
                content = Texture2D.FromStream(device, textureFileStream);
            }
        }

        #endregion

        #region Manipulation and Inertia Processor Events

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation processor's delta event. 
        /// Occurs whenever the first time that the manipulation processor processes a 
        /// group of manipulators.
        /// </summary>
        /// <param name="sender">The manipulation processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DManipulationStarted(object sender, Manipulation2DStartedEventArgs e)
        {
            Debug.Assert(!extrapolating);
            manipulating = true;

            if (canRotate || canRotateFlick)
            {
                manipulationProcessor.Pivot = new ManipulationPivot2D
                {
                    X = transformedCenter.X,
                    Y = transformedCenter.Y,
                    Radius = Math.Max(Width, Height)/2.0f
                };
            }
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation and inertia processor's delta events. 
        /// Occurs whenever the manipulation or inertia processors processes or extrapolate 
        /// manipulator data.
        /// </summary>
        /// <param name="sender">The manipulation or inertia processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            Debug.Assert(manipulating && sender is ManipulationProcessor2D ||
                extrapolating && sender is InertiaProcessor2D);

            Vector2 manipulationOrigin = new Vector2(e.OriginX, e.OriginY);
            Vector2 manipulationDelta = new Vector2(e.Delta.TranslationX, e.Delta.TranslationY);
            Vector2 previousOrigin = new Vector2(manipulationOrigin.X - manipulationDelta.X, manipulationOrigin.Y - manipulationDelta.Y);
            float restrictedOrientation = RestrictOrientation(e.Delta.Rotation);
            float restrictedScale = RestrictScale(e.Delta.ScaleX);

            // Adjust the position of the item based on change in rotation
            if (restrictedOrientation != 0.0f)
            {
                Vector2 manipulationOffset = transformedCenter - previousOrigin;
                Vector2 rotatedOffset = GeometryHelper.RotatePointVector(manipulationOffset, restrictedOrientation);
                Vector2 compensation = rotatedOffset - manipulationOffset;
                transformedCenter += compensation;
            }

            // Adjust the position of the item based on change in scale
            if (restrictedScale != 1.0f)
            {
                Vector2 manipulationOffset = manipulationOrigin - transformedCenter;
                Vector2 scaledOffset = manipulationOffset * restrictedScale;
                Vector2 compensation = manipulationOffset - scaledOffset;
                transformedCenter += compensation;
            }

            // Rotate the item if it is allowed
            if (canRotate || canRotateFlick)
            {
                orientation += restrictedOrientation;
            }

            // Scale the item if it is allowed
            if (canScale || canScaleFlick)
            {
                scaleFactor *= restrictedScale;
            }

            // Translate the item if it is allowed
            if (canTranslate || canTranslateFlick)
            {
                transformedCenter += new Vector2(e.Delta.TranslationX, e.Delta.TranslationY);
            }

            RestrictCenter();

            if (canRotate || canRotateFlick)
            {
                manipulationProcessor.Pivot = new ManipulationPivot2D
                {
                    X = transformedCenter.X,
                    Y = transformedCenter.Y,
                    Radius = Math.Max(Width, Height) / 2.0f
                };
            }
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation processor's completed event. 
        /// Occurs whenever the manipulation processor processes manipulator 
        /// data where all remaining contacts have been removed.
        /// Check final deltas and start the inertia processor if they are high enough.
        /// </summary>
        /// <param name="sender">The manipulation processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e)
        {
            // manipulation completed
            manipulating = false;

            // Get the inital inertia values
            Vector2 initialVelocity = new Vector2(e.Velocities.LinearVelocityX, e.Velocities.LinearVelocityY);
            float angularVelocity = e.Velocities.AngularVelocity;
            float expansionVelocity = e.Velocities.ExpansionVelocityX;

            bool startFlick = false;
           
            // Rotate and scale around the center of the item
            inertiaProcessor.InitialOriginX = TransformedCenter.X;
            inertiaProcessor.InitialOriginY = TransformedCenter.Y;

            // set initial velocity if translate flicks are allowed
            if (canTranslateFlick && initialVelocity.LengthSquared() > MinimumFlickVelocity * MinimumFlickVelocity)
            {
                startFlick = true;
                inertiaProcessor.TranslationBehavior.InitialVelocityX = initialVelocity.X;
                inertiaProcessor.TranslationBehavior.InitialVelocityY = initialVelocity.Y;
                inertiaProcessor.TranslationBehavior.DesiredDeceleration = Deceleration;
            }
            else
            {
                inertiaProcessor.TranslationBehavior.InitialVelocityX = 0.0f;
                inertiaProcessor.TranslationBehavior.InitialVelocityY = 0.0f;
                inertiaProcessor.TranslationBehavior.DesiredDeceleration = 0.0f;
            }

            // set angular velocity if rotation flicks are allowed
            if (canRotateFlick && Math.Abs(angularVelocity) >= MinimumAngularFlickVelocity)
            {
                startFlick = true;
                inertiaProcessor.RotationBehavior.InitialVelocity = angularVelocity;
                inertiaProcessor.RotationBehavior.DesiredDeceleration = AngularDeceleration;
            }
            else
            {
                inertiaProcessor.RotationBehavior.InitialVelocity = 0.0f;
                inertiaProcessor.RotationBehavior.DesiredDeceleration = 0.0f;
            }

            // set expansion velocity if scale flicks are allowed
            if (canScaleFlick && Math.Abs(expansionVelocity) >= MinimumExpansionFlickVelocity)
            {
                startFlick = true;
                inertiaProcessor.ExpansionBehavior.InitialVelocityX = expansionVelocity;
                inertiaProcessor.ExpansionBehavior.InitialVelocityY = expansionVelocity;
                inertiaProcessor.ExpansionBehavior.InitialRadius = 
                    0.25f * (AxisAlignedBoundingRectangle.Width + AxisAlignedBoundingRectangle.Height);
                inertiaProcessor.ExpansionBehavior.DesiredDeceleration = ExpansionDeceleration;
            }
            else
            {
                inertiaProcessor.ExpansionBehavior.InitialVelocityX = 0.0f;
                inertiaProcessor.ExpansionBehavior.InitialVelocityY = 0.0f;
                inertiaProcessor.ExpansionBehavior.InitialRadius = 1.0f;
                inertiaProcessor.ExpansionBehavior.DesiredDeceleration = 0.0f;
            }

            if (startFlick)
            {
                extrapolating = true;
            }
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the inertia processor's complete event.
        /// Occurs whenever the item comes to rest after being flicked.
        /// </summary>
        /// <param name="sender">The inertia processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DInertiaCompleted(object sender, Manipulation2DCompletedEventArgs e)
        {
            extrapolating = false;
        }

        #endregion

        #region Contact Tracking

        //==========================================================//
        /// <summary>
        /// Handles contact down events from the UIController.
        /// </summary>
        /// <param name="touchEvent">Event data.</param>
        protected override void OnTouchDown(TouchTargetEvent touchEvent)
        {
            base.OnTouchDown(touchEvent);

            // Capture the contact
            Controller.Capture(touchEvent.Touch, this);

            // Raise the Activated event if this is the first contact
            if (TouchesCaptured.Count == 1)
            {
                OnActivated();
            }

            // Transform the contact into the screen coordinate system.
            Vector2 temp = Vector2.Transform(new Vector2(touchEvent.Touch.X, touchEvent.Touch.Y), Transform);
            currentManipulators.Add(new Manipulator2D(touchEvent.Touch.Id, temp.X, temp.Y));
        }

        //==========================================================//
        /// <summary>
        /// Handles contact changed events from the UIController.
        /// </summary>
        /// <param name="touchEvent">Event data.</param>
        protected override void OnTouchMoved(TouchTargetEvent touchEvent)
        {
            // Transform the contact into the screen coordinate system.
            Vector2 temp = Vector2.Transform(new Vector2(touchEvent.Touch.X, touchEvent.Touch.Y), Transform);
            // Remove manipulator if the Id already exists in the list. Can't update Manipulator2D
            // in-place as X and Y are read-only.
            // Using an anonymous predicate delegate to locate matching ManipulatorIds. 
            currentManipulators.RemoveAll(delegate(Manipulator2D m)
                    { return m.Id == touchEvent.Touch.Id; });
            // Add it to the currentManipulators list with updated values.
            currentManipulators.Add(new Manipulator2D(touchEvent.Touch.Id, temp.X, temp.Y));
        }

        //==========================================================//
        /// <summary>
        /// Handles contact up events from the UIController.
        /// </summary>
        /// <param name="touchEvent">Event data.</param>
        protected override void OnTouchUp(TouchTargetEvent touchEvent)
        {
            // Release the contact
            Controller.Release(touchEvent.Touch);
            base.OnTouchUp(touchEvent);

            // A manipulator can't be current and removed at the same time.
            // Remove it from the current list before adding to removed list.
            currentManipulators.RemoveAll(delegate(Manipulator2D m)
                    { return m.Id == touchEvent.Touch.Id; });
            
            // Raise the Deactivated event if this is the last contact
            if (TouchesCaptured.Count < 1)
            {
                OnDeactivated();
            }
        }

        //==========================================================//
        /// <summary>
        /// Let the manipulation processor process touch information.
        /// </summary>
        public void ProcessTouches()
        {
            if (manipulationProcessor == null || inertiaProcessor == null)
            {
                SetAllowedManipulations();
            }

            Int64 timestamp = StopwatchHelper.Get100NanosecondsTimestamp();

            if (extrapolating)
            {
                if (currentManipulators.Count == 0)
                {
                    // process inertia
                    inertiaProcessor.Process(timestamp);
                }
                else
                {
                    // stop inertia
                    inertiaProcessor.Complete(timestamp);
                    manipulationProcessor.ProcessManipulators(timestamp, currentManipulators);
                }
            }

            // update the manipulation
            else
            {
                manipulationProcessor.ProcessManipulators(timestamp, currentManipulators);
            }
        }

        //==========================================================//
        /// <summary>
        /// Raise the Activated event.
        /// </summary>
        protected virtual void OnActivated()
        {
            if (Activated != null)
            {
                Activated(this, new EventArgs());
            }
        }

        //==========================================================//
        /// <summary>
        /// Raise the Deactivated event.
        /// </summary>
        protected virtual void OnDeactivated()
        {
            if (Deactivated != null)
            {
                Deactivated(this, new EventArgs());
            }
        }

        #endregion

        //==========================================================//
        /// <summary>
        /// Draw the XNAScatterViewItem to the screen.
        /// </summary>
        /// <param name="batch">The SpriteBatch to which content will be drawn.</param>
        /// <itemZOrder>The z order to use when drawing the item.</itemZOrder>
        public void Draw(SpriteBatch batch, float itemZOrder)
        {
            batch.Draw(content, transformedCenter, null, Color.White, orientation, DrawingOrigin, zoomFactor * scaleFactor, SpriteEffects.None, itemZOrder);
        }

        //==========================================================//
        /// <summary>
        /// Checks center against boundaries and moved center inside 
        /// of boundaries if it is found to be out of bounds.
        /// </summary>
        private void RestrictCenter()
        {
            float leftEdge, rightEdge, topEdge, bottomEdge;
            GetBoundingRect(out leftEdge, out rightEdge, out topEdge, out bottomEdge);

            if (transformedCenter.X < leftEdge)
            {
                transformedCenter.X = leftEdge;
            }
            else if (transformedCenter.X > rightEdge)
            {
                transformedCenter.X = rightEdge;
            }

            if (transformedCenter.Y < topEdge)
            {
                transformedCenter.Y = topEdge;
            }
            else if (transformedCenter.Y > bottomEdge)
            {
                transformedCenter.Y = bottomEdge;
            }
        }

        /// <summary>
        /// To ensure that the item doesn't go off completely into a corner from where the user
        /// can't bring it back, we restrict the item's center within a deflated bounding box.
        /// </summary>
        /// <param name="leftBoundingEdge">Left edge of the bounding rect.</param>
        /// <param name="rightBoundingEdge">Right edge of the bounding rect.</param>
        /// <param name="topBoundingEdge">Top edge of the bounding rect.</param>
        /// <param name="bottomBoundingEdge">Bottom edge of the bounding rect.</param>
        private void GetBoundingRect(out float leftBoundingEdge, out float rightBoundingEdge, out float topBoundingEdge, out float bottomBoundingEdge)
        {
            leftBoundingEdge = parent.Left + parent.BoundaryThreshold;
            rightBoundingEdge = parent.Right - parent.BoundaryThreshold;
            topBoundingEdge = parent.Top + parent.BoundaryThreshold;
            bottomBoundingEdge = parent.Bottom - parent.BoundaryThreshold;
        }

        //==========================================================//
        /// <summary>
        /// Determines if the item contains a point (represented by a vector).
        /// </summary>
        /// <param name="point">The vector that is being checked against the item.</param>
        /// <returns>True if the item contains the vector, false otherwise.</returns>
        public bool Contains(Vector2 point)
        {
            // Create an axis aligned bounding rect and rotate the vector so its values are relative to the new rectangle
            Vector2 rotated = GeometryHelper.RotatePointVectorAroundOrigin(transformedCenter, point, -orientation);
            Rectangle bounds = AxisAlignedBoundingRectangle;

            // see if the adjusted vector is in the axis aligned rectangle
            return GeometryHelper.RectangleContainsPointVector(bounds, rotated);
        }

        //==========================================================//
        /// <summary>
        /// Restrict scale to prevent the scatter view from being scaled past its maximum or minimum scale values.
        /// </summary>
        /// <param name="scaleDelta">The proposed change in scale.</param>
        /// <returns>The restricted change in scale</returns>
        private float RestrictScale(float scaleDelta)
        {
            float modifiedScale = scaleDelta * scaleFactor;

            if (modifiedScale > MaxScaleFactor)
            {
                return MaxScaleFactor / scaleFactor;
            }

            if (modifiedScale < MinScaleFactor)
            {
                return MinScaleFactor / scaleFactor;
            }

            return scaleDelta;
        }

        //==========================================================//
        /// <summary>
        /// Restrict rotation to keep values between 0 and 360 degrees.
        /// </summary>
        /// <param name="desiredRotation">The proposed change in rotation.</param>
        /// <returns>The restricted change in rotation.</returns>
        private static float RestrictOrientation(float desiredRotation)
        {
            float constrainedRotation = desiredRotation % 360.0f;

            return constrainedRotation;
        }
    }
}
