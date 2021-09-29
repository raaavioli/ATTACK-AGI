using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input.Manipulations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreInteractionFramework;
using Microsoft.Surface.Core;

namespace XnaScatter
{
    /// <summary>
    /// The class that represents a scalable ScatterView, the container for ScatterViewItems.
    /// </summary>
    /// <remarks>
    /// XnaScatterView inherits from UIElementStateMachine, but does not fully implement a 
    /// standard controller/view/model design pattern like most CoreInteractionFramework 
    /// controls. The purpose here is to demonstrate how to use the contact capture and 
    /// routing mechanisms in CIF without using the other aspects of the framework. 
    /// </remarks>
    public class XnaScatterView : UIElementStateMachine, ITouchableObject, IDisposable
    {
        private readonly string textureSourceFile;
        private Texture2D content;

        private float zoomFactor = 1.0f;
        private const float minZoomFactor = 1.0f;
        private const float maxZoomFactor = 3.0f;
        private const float boundaryThreshold = 30f;

        private Vector2 transformedCenter;

        private readonly LinkedList<XnaScatterViewItem> activeChildren = new LinkedList<XnaScatterViewItem>();
        private readonly LinkedList<XnaScatterViewItem> inactiveChildren = new LinkedList<XnaScatterViewItem>();

        private ManipulationProcessor2D manipulationProcessor;

        private List<Manipulator2D> currentManipulators = new List<Manipulator2D>();

        private readonly int topBoundary;
        private readonly int bottomBoundary;
        private readonly int leftBoundary;
        private readonly int rightBoundary;

        private Matrix transform = Matrix.Identity;

        #region IDisposable
        // Implement the IDisposable interface.

        private bool disposed;

        public bool IsDisposed
        {
            get { return disposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Release managed resources.
                    content.Dispose();
                }

                // Release unmanaged resources.

                disposed = true;
            }

        }

        ~XnaScatterView()
        {
            Dispose(false);
        }

        #endregion

        #region Layout Properties
        
        //==========================================================//
        /// <summary>
        /// The screen transform of all of the objects. Contacts need to be hit test based on this
        /// transformation. The center of this and the transform of each item is also updated when this 
        /// property is updated.
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

                foreach (XnaScatterViewItem item in activeChildren)
                {
                    item.Transform = transform;
                }

                foreach (XnaScatterViewItem item in inactiveChildren)
                {
                    item.Transform = transform;
                }
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
        /// The center of the object.
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
        /// The X position of the left side of the object after scaling is applied.
        /// </summary>
        public float Left
        {
            get
            {
                return transformedCenter.X - Width / 2;
            }
        }

        //==========================================================//
        /// <summary>
        /// The X position of the right side of the object after scaling is applied.
        /// </summary>
        public float Right
        {
            get
            {
                return transformedCenter.X + Width / 2;
            }
        }

        //==========================================================//
        /// <summary>
        /// The Y position of the top of the object after scaling is applied.
        /// </summary>
        public float Top
        {
            get
            {
                return transformedCenter.Y - Height / 2;
            }
        }

        //==========================================================//
        /// <summary>
        /// The Y position of the bottom of the object after scaling is applied.
        /// </summary>
        public float Bottom
        {
            get
            {
                return transformedCenter.Y + Height / 2;
            }
        }

        //==========================================================//
        /// <summary>
        /// The minimum distance by which an item must be inside the XNAScatterView boundary
        /// to be considered within bounds. There is some inside padding on ScatterView to ensure
        /// that an item center does not go too far out.
        /// </summary>
        public float BoundaryThreshold
        {
            get
            {
                return boundaryThreshold * zoomFactor;
            }
        }

        //==========================================================//
        /// <summary>
        /// The height of the object after scaling is applied.
        /// </summary>
        public float Height
        {
            get { return OriginalHeight * zoomFactor; }
        }

        //==========================================================//
        /// <summary>
        /// The height of the object after scaling is applied.
        /// </summary>
        public float Width
        {
            get { return OriginalWidth * zoomFactor; }
        }

        /// <summary>
        /// The height of the object before scaling is applied.
        /// </summary>
        private float OriginalHeight
        {
            get { return (float)(bottomBoundary - topBoundary); }
        }

        //==========================================================//
        /// <summary>
        /// The height of the object before scaling is applied.
        /// </summary>
        private float OriginalWidth
        {
            get { return (float)(rightBoundary - leftBoundary); }
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
        /// Constructor. Sets allowed manipulations and creates the manipulation processor.
        /// </summary>
        public XnaScatterView(UIController controller, string backgroundImage, int top, int bottom, int left, int right)
            : base(controller, right - left, bottom - top)
        {
            textureSourceFile = backgroundImage;
            transformedCenter = new Vector2(0, 0);

            topBoundary = top;
            bottomBoundary = bottom;
            leftBoundary = left;
            rightBoundary = right;

            Manipulations2D supportedManipulations =
                Manipulations2D.TranslateX | Manipulations2D.TranslateY | Manipulations2D.Scale;

            manipulationProcessor = new ManipulationProcessor2D(supportedManipulations);
            manipulationProcessor.Delta += OnAffine2DDelta;
        }

        #endregion

        #region Manipulation Processor Events

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation processor's delta event.
        /// Occurs whenever the manipulation processor processes manipulator data.
        /// </summary>
        /// <param name="sender">The manipulation processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            // Check the raw manipulation values
            float restrictedScale = RestrictScale(e.Delta.ScaleX);

            Vector2 restrictedTranslation = RestrictTranslation(e.Delta.TranslationX, e.Delta.TranslationY, restrictedScale);

            // Get the original and scaled offset from the manipulation origin to the center of the scatterview.
            Vector2 originalManipulationOffset = new Vector2 ( e.OriginX, e.OriginY ) - transformedCenter;
            Vector2 scaledManipulationOffset = originalManipulationOffset * restrictedScale;

            // Remember the original center point
            Vector2 originalCenter = transformedCenter;

            // Scale the item
            zoomFactor *= restrictedScale;

            // Move the center of the scatter view towards the manipulation origin
            transformedCenter += originalManipulationOffset - scaledManipulationOffset;

            // Apply the translation
            transformedCenter += restrictedTranslation;

            // Children will have to be updated too
            foreach (XnaScatterViewItem item in inactiveChildren)
            {
                Vector2 offsetToOriginalCenter = item.TransformedCenter - originalCenter;
                offsetToOriginalCenter *= restrictedScale;
                item.TransformedCenter = transformedCenter + offsetToOriginalCenter;

                item.ZoomFactor *= restrictedScale;
            }
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
        }

        //==========================================================//
        /// <summary>
        /// Process all touch data that has changed since the last time data was processed. 
        /// This includes added, moved, and removed touches. Also causes all children to
        /// process contacts.
        /// </summary>
        public void ProcessTouches()
        {
            manipulationProcessor.ProcessManipulators(StopwatchHelper.Get100NanosecondsTimestamp(), currentManipulators);

            foreach (XnaScatterViewItem child in inactiveChildren)
                child.ProcessTouches();

            foreach (XnaScatterViewItem child in activeChildren)
                child.ProcessTouches();
        }

        #endregion

        //==========================================================//
        /// <summary>
        /// Load the image that is used to draw the XNAScatterView. Also causes children 
        /// to load their content also.
        /// </summary>
        /// <param name="device">The graphics device that is being for the application.</param>
        /// <param name="contentPath">The path to the content directory.</param>
        public void LoadContent(GraphicsDevice device, String contentPath)
        {
            using (Stream textureFileStream = File.OpenRead(Path.Combine(contentPath, textureSourceFile)))
            {
                content = Texture2D.FromStream(device, textureFileStream);
            }

            foreach (XnaScatterViewItem child in inactiveChildren)
            {
                child.LoadContent(device, contentPath);
            }
            foreach (XnaScatterViewItem child in activeChildren)
            {
                child.LoadContent(device, contentPath);
            }
        }

        //==========================================================//
        /// <summary>
        /// Draw the XNAScatterView and all children to the screen.
        /// </summary>
        /// <param name="batch">The SpriteBatch to which content will be drawn.</param>
        public void Draw(SpriteBatch batch, Matrix screenTransform)
        {
            // Draw the scatter view itself first because it is on the bottom. 
            // If it is drawn first, everything else will be drawn on top of it.
            // Pass in the screenTransform to orient the display correctly.
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearWrap,
                    DepthStencilState.Default, RasterizerState.CullNone, null, screenTransform);
            batch.Draw(content, transformedCenter, new Rectangle(0, 0, (int)OriginalWidth, (int)OriginalHeight), Color.White, 0, new Vector2(OriginalWidth, OriginalHeight) / 2f, zoomFactor, SpriteEffects.None, 0);
            batch.End();

            // Calculate the zorder for items. XNA uses a Z order where 1.0f is the bottom element, and 0.0f is the top element.
            // First calculate the increment such that each item can be drawn with a different z-order. Then use a z-order that 
            // starts at 1.0f and decrecases by the increment value to 0.0f over the course of drawing the items.
            float zOrderIncrement = 1.0f / (float)(inactiveChildren.Count + activeChildren.Count);
            float zOrder = 1 - zOrderIncrement;

            // Prepare the graphics device for rendering.
            // Pass in the screenTransform to orient the display correctly.
            batch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, null, null, null, null, screenTransform);

            // Next draw the inactive children. They should be on top of the scatter view but under the active items
            LinkedListNode<XnaScatterViewItem> node = inactiveChildren.Last;

            while (node != null)
            {
                node.Value.Draw(batch, zOrder);
                node = node.Previous;
                zOrder -= zOrderIncrement;
            }

            // Draw the active children last so they are drawn on top of everything
            node = activeChildren.Last;

            while (node != null)
            {
                node.Value.Draw(batch, zOrder);
                node = node.Previous;
                zOrder -= zOrderIncrement;
            }

            batch.End();
        }

        //==========================================================//
        /// <summary>
        /// Hit test against the scatter view and all of its children to see what a contact is 
        /// touching: a child item or the scatter view itself?
        /// </summary>
        /// <param name="c">The contact that is being hit tested.</param>
        public UIElementStateMachine HitTest(TouchPoint contact)
        {
            Vector2 contactVector = new Vector2(contact.X, contact.Y);

            contactVector = Vector2.Transform(contactVector, Transform);

            LinkedListNode<XnaScatterViewItem> node = activeChildren.First;

            while (node != null)
            {
                if (node.Value.Contains(contactVector))
                {
                    return node.Value;
                }
                node = node.Next;
            }

            node = inactiveChildren.First;

            while (node != null)
            {
                if (node.Value.Contains(contactVector))
                {
                    return node.Value;
                }
                node = node.Next;
            }

            return this;
        }

        //==========================================================//
        /// <summary>
        /// Add an XNAScatterViewItem to the XNAScatterView's children.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void AddItem(XnaScatterViewItem item)
        {
            System.Diagnostics.Debug.Assert(item.Parent == this, 
                                            "Should only add XnaScatterViewItem to its Parent");

            item.Activated += new EventHandler<EventArgs>(ChildActivated);
            item.Deactivated += new EventHandler<EventArgs>(ChildDeactivated);
                        
            inactiveChildren.AddFirst(item);
        }



        //==========================================================//
        /// <summary>
        /// Restrict translation values to prevent the scatter view from being moved off of the screen. 
        /// </summary>
        /// <param name="deltaX">The proposed change along the x axis.</param>
        /// <param name="deltaY">The proposed change along the y axis.</param>
        /// <param name="scaleDelta">The change in scale that will be applied to the scatter view.</param>
        /// <returns>A vector representing the maximum amount of translation that can be applied to the scatter view.</returns>
        private Vector2 RestrictTranslation(float deltaX, float deltaY, float scaleDelta)
        {
            float restrictedDeltaX = deltaX;
            float restrictedDeltaY = deltaY;

            // See where the translated boundaries would be
            float top = Top + deltaY * scaleDelta;
            float bottom = Bottom + deltaY * scaleDelta;

            float left = Left + deltaX * scaleDelta;
            float right = Right + deltaX * scaleDelta;

            // Compare the sides of the translated object to the bounds set by the parent
            if (top > topBoundary)
            {
                restrictedDeltaY += topBoundary - top;
            }
            if (bottom < bottomBoundary)
            {
                restrictedDeltaY -= bottom - bottomBoundary;
            }
            if (left > leftBoundary)
            {
                restrictedDeltaX += leftBoundary - left;
            }
            if (right < rightBoundary)
            {
                restrictedDeltaX -= right - rightBoundary;
            }

            return new Vector2 ( restrictedDeltaX, restrictedDeltaY );
        }

        //==========================================================//
        /// <summary>
        /// Restrict scale to prevent the scatter view from being scaled past its maximum or minimum scale values.
        /// </summary>
        /// <param name="scaleDelta">The proposed change in scale.</param>
        /// <returns>The restricted change in scale.</returns>
        private float RestrictScale(float scaleDelta)
        {
            float modifiedScale = scaleDelta * zoomFactor;

            if (modifiedScale > maxZoomFactor)
            {
                return maxZoomFactor / zoomFactor;
            }

            if (modifiedScale < minZoomFactor)
            {
                return minZoomFactor / zoomFactor;
            }
            
            return scaleDelta;
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the deactivated event on a child XNAScatterViewItem.
        /// </summary>
        /// <param name="sender">The XNAScatterViewItem that was deactivated.</param>
        /// <param name="e">The event args.</param>
        void ChildDeactivated(object sender, EventArgs e)
        {
            XnaScatterViewItem item = sender as XnaScatterViewItem;
            
            if (activeChildren.Contains(item))
            {
                activeChildren.Remove(item);
            }

            // A race condition between 2 manipulations could cause the item to be added twice
            if (!inactiveChildren.Contains(item))
            {
                inactiveChildren.AddFirst(item);
            }
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the activated event on a child XNAScatterViewItem.
        /// </summary>
        /// <param name="sender">The XNAScatterViewItem that was activated.</param>
        /// <param name="e">The event args.</param>
        void ChildActivated(object sender, EventArgs e)
        {
            XnaScatterViewItem item = sender as XnaScatterViewItem;

            if (inactiveChildren.Contains(item))
            {
                inactiveChildren.Remove(item);
            }

            // A race condition between 2 manipulations could cause the item to be added twice
            if (!activeChildren.Contains(item))
            {
                activeChildren.AddFirst(item);
            }
        }
    }
}
