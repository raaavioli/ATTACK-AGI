using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input.Manipulations;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Maintains and manages the state that is associated with an application-defined UI scroll bar 
    /// object. 
    /// </summary>
    /// <remarks>
    /// <para>The<strong>ScrollBarStateMachine</strong> 
    /// maintains the following types of state elements:</para>
    /// <list type="bullet">
    /// <item>The direction that the scroll bar is oriented toward 
    /// (<strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.Orientation"/></strong>).</item>
    /// <item>If scrolling is occurring or not 
    /// (<strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.IsScrolling"/></strong>).</item>
    /// <item>Position monitoring 
    /// (<strong><see cref="E:CoreInteractionFramework.ScrollBarStateMachine.ValueChanged"/></strong> and 
    /// <strong><see cref="E:CoreInteractionFramework.ScrollBarStateMachine.ThumbChanged"/></strong>).</item>
    /// </list>
    /// <note type="caution"> The Core Interaction Framework and API use the 
    /// Model-View-Controller (MVC) design pattern. The API state machines 
    /// represent the Model component of the MVC design pattern. </note>
    /// </remarks>    
    public class ScrollBarStateMachine : UIElementStateMachine
    {
        bool processInertia;

        // represents the number of pixels in the current axis
        // this is determined by the Orientation, NumberOfPixelsInHorizontalAxis and NumberOfPixelsInVerticalAxis properties.
        private int numberOfPixelsInAxis;
        // 0...1
        private float scrollBarValue;
        // 0...1
        private readonly Stopwatch stopwatch;
        private float viewportSize;
        // Scrolls in the opposite direction of normal.
        private bool isReversed;
        private float minThumbSize;
        private bool gotValueChanged;
        private bool gotThumbChanged;
        private bool isScrolling;
        private float thumbStartPosition;
        private Queue<int> trackQueue = new Queue<int>();
        private List<int> thumbCapturedTouchesList = new List<int>();
        private Dictionary<int, ScrollBarPart> capturedCollectionLookup = new Dictionary<int, ScrollBarPart>();
        private Animation scrollAnimation;
        private Dictionary<int, ScrollBarHitTestDetails> captureTouchesHitTestDetails = new Dictionary<int, ScrollBarHitTestDetails>();

        private readonly TimeSpan ScrollBarAnimationDuration = TimeSpan.FromSeconds(0.5);

        private Dictionary<int, float> distanceOffset = new Dictionary<int, float>();

        private InertiaProcessor2D inertiaProcessor;
        private ManipulationProcessor2D manipulationProcessor;

        private float maximumFlickVelocity = FlickUtilities.MaximumFlickVelocity;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private ScrollBarPart selectedPart = ScrollBarPart.None;
        delegate void PageDelegate();
        PageDelegate PageAgain;

        Orientation orientation = Orientation.Default;

        /// <summary>
        /// Creates an initialized instance of a
        /// <strong><see cref="ScrollBarStateMachine"/></strong> object with the specified parameters.
        /// </summary>
        /// <param name="controller">The <strong>UIController</strong> object that dispatches hit testing.</param>
        /// <param name="numberOfPixelsInHorizontalAxis">
        /// The number of pixels that this control occupies horizontally. 
        /// For more information, see 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInHorizontalAxis">NumberOfPixelsInHorizontalAxis</see></strong>.
        /// </param>
        /// <param name="numberOfPixelsInVerticalAxis">
        /// The number of pixels that this control occupies vertically. 
        /// For more information, see 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInVerticalAxis">NumberOfPixelsInVerticalAxis</see></strong>.
        /// </param>
        public ScrollBarStateMachine(UIController controller, int numberOfPixelsInHorizontalAxis, int numberOfPixelsInVerticalAxis)
            : base(controller, numberOfPixelsInHorizontalAxis, numberOfPixelsInVerticalAxis)
        {
            stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets or sets the direction that the 
        /// <strong><see cref="ScrollBarStateMachine"/></strong> object 
        /// is oriented toward.  
        /// </summary>
        /// <returns>The current enumeration value that identifies <strong>ScrollBarStateMachine</strong> 
        /// orientation.</returns>
        /// <remarks>
        /// Orientation zero-based enumeration identifiers include <strong>Vertical</strong> (0x0), 
        /// <strong>Horizontal</strong> (0x1), <strong>Default</strong> (0x0), or <strong>Both</strong> 
        /// (an OR combination of <strong>Vertical</strong> and <strong>Horizontal</strong>). 
        /// The <strong>Orientation</strong> property affects if 
        /// <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.NumberOfPixelsInHorizontalAxis"/></strong>
        /// or <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.NumberOfPixelsInVerticalAxis"/></strong>
        /// is used when flicking the <strong>ScrollBarStateMachine</strong> object.  
        /// </remarks>
        public Orientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (orientation == Orientation.Both)
                    throw SurfaceCoreFrameworkExceptions.InvalidOrientationArgumentException("Orientation", Orientation.Both);

                orientation = value;

                if (orientation == Orientation.Vertical)
                {
                    numberOfPixelsInAxis = NumberOfPixelsInVerticalAxis;
                }
                else
                {
                    numberOfPixelsInAxis = NumberOfPixelsInHorizontalAxis;
                }
            }
        }

        /// <summary>
        /// Gets or sets the scroll bar width (horizontal axis), in pixels.
        /// </summary>
        /// <returns>The current horizontal pixel width of the scroll bar.</returns>
        public override int NumberOfPixelsInHorizontalAxis
        {
            get
            {
                return base.NumberOfPixelsInHorizontalAxis;
            }
            set
            {
                if (orientation == Orientation.Horizontal)
                {
                    numberOfPixelsInAxis = value;
                }
                base.NumberOfPixelsInHorizontalAxis = value;
            }
        }

        /// <summary>
        /// Gets or sets the scroll bar height (vertical axis), in pixels.
        /// </summary>
        /// <returns>The current vertical pixel height of the scroll bar.</returns>
        public override int NumberOfPixelsInVerticalAxis
        {
            get
            {
                return base.NumberOfPixelsInVerticalAxis;
            }
            set
            {
                if (orientation == Orientation.Vertical)
                {
                    numberOfPixelsInAxis = value;
                }
                base.NumberOfPixelsInVerticalAxis = value;
            }
        }

        /// <summary>
        /// Gets the selected <strong><see cref="ScrollBarStateMachine"/></strong> part.  
        /// </summary>
        /// <returns>The currently selected <strong>ScrollBarStateMachine</strong> part.</returns>
        /// <remarks><strong>ScrollBarStateMachine</strong> parts include the thumb and the track.
        /// </remarks>
        public ScrollBarPart SelectedPart
        {
            get
            {
                return selectedPart;
            }
        }

        /// <summary>
        /// Gets or sets the position value of the scroll bar.   
        /// </summary>
        /// <remarks>The position 
        /// value is represented by a floating point number with a valid range from 0 through 1.</remarks>
        /// <returns>The current scroll bar value.</returns>
        public float Value
        {
            get { return scrollBarValue; }
            set
            {
                if (value < 0 || value > 1)
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("Value");

                // Update Value which will cause the thumbStartPosition to be recalculated.
                UpdatedValueInValueSpace(value);
            }

        }

        /// <summary>
        /// Gets or sets the ratio of the size of the scroll bar viewport versus the 
        /// extent. 
        /// <remarks>Extent is always 1, so the viewport size must be 1 or less. If the viewport 
        /// is 1, there is no scrolling. If the viewport size is 0.5, the viewport is half 
        /// the size of the extent and the scroll bar is half the size of the scroll bar. 
        /// </remarks>
        /// </summary>
        /// <returns>The current scroll bar viewport size.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ViewportSize",
            Justification = "The intent is to use two words")]
        public float ViewportSize
        {
            get { return viewportSize; }
            set
            {
                if (value < 0 || value > 1)
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("ViewportSize");

                viewportSize = value;

                // Update Value which will cause the thumbStartPosition to be recalculated.
                UpdatedValueInValueSpace(Value);
            }
        }

        /// <summary>
        /// Gets or sets a Boolean value that identifies whether the scroll bar is scrolling in the opposite 
        /// direction.
        /// </summary>
        /// <returns><strong>true</strong> if scrolling is occurring in the opposite direction.</returns>
        public bool IsReversed
        {
            get { return isReversed; }
            set { isReversed = value; }
        }

        /// <summary>
        /// Gets or sets the minimum size of the scroll bar's thumb. 
        /// </summary>
        /// <returns>The current minimum size of the scroll bar's thumb.</returns>
        /// <remarks>The thumb 
        /// size is represented as a floating point number with a valid range from 0 through 1.  </remarks>
        public float MinThumbSize
        {
            get { return minThumbSize; }
            set
            {
                if (value < 0 || value > 1)
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("MinThumbSize");

                minThumbSize = value;
            }
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the scroll bar 
        /// <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.Value"/></strong>
        /// property has changed.  
        /// </summary>
        /// <returns><strong>true</strong> if the <strong>Value</strong> property changed since the controller was 
        /// updated.</returns>
        public bool GotValueChanged
        {
            get { return gotValueChanged; }
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the scroll bar 
        /// <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.ThumbSize"/></strong>
        ///  property has changed 
        /// since the controller was last updated.  
        /// </summary>
        /// <returns><strong>true</strong> if the <strong>ThumbSize</strong> property changed.</returns>
        public bool GotThumbChanged
        {
            get { return gotThumbChanged; }
        }

        /// <summary>
        /// Gets a Boolean value that indicates if scrolling is presently occurring on the scroll 
        /// bar.
        /// </summary>
        /// <returns><strong>true</strong> if the scroll bar is currently scrolling.</returns>
        public bool IsScrolling
        {
            get { return isScrolling; }
        }

        /// <summary>
        /// Gets the current normalized size of the scroll bar's thumb. 
        /// </summary>
        /// <remarks>The thumb size is represented as a floating point number with a valid range 
        /// from 0 through 1. 
        /// </remarks>
        /// <returns>The current normalized thumb size.</returns>
        public float ThumbSize
        {
            get
            {
                float thumbSize = viewportSize * viewportSize;

                // Stay above the minimum. 
                thumbSize = Math.Max(thumbSize, minThumbSize);

                return thumbSize;
            }
        }

        /// <summary>
        /// Gets the start position of the scroll bar's thumb. 
        /// </summary>
        /// <remarks>The thumb 
        /// start position is represented as a floating point number with a valid range from 0 through 1. </remarks>
        /// <returns>The thumb's start position.</returns>
        public float ThumbStartPosition
        {
            get { return thumbStartPosition; }
        }

        /// <summary>
        /// Scrolls the thumb 1 page forward over 0.5 seconds.
        /// </summary>
        public void PageForward()
        {
            // Assign the PageForward method to the PageAgain delegate so that the thumb
            // keeps moving in the correct direction.
            PageAgain = PageForward;

            // If the thumb animation is playing adjust the location it will move to
            // based on where it currently is animating to.
            if (scrollAnimation != null && scrollAnimation.IsPlaying)
            {
                AnimateTo(scrollAnimation.To + ViewportSize);
            }
            else
            {
                AnimateTo(Value + ViewportSize);
            }
        }

        /// <summary>
        /// Scrolls the thumb 1 page back over 0.5 seconds.
        /// </summary>
        public void PageBack()
        {
            // Assign the PageBack method to the PageAgain delegate so that the thumb
            // keeps moving in the correct direction.
            PageAgain = PageBack;

            // If the thumb animation is playing adjust the location it will move to 
            // based on where it currently is animating too.
            if (scrollAnimation != null && scrollAnimation.IsPlaying)
            {
                AnimateTo(scrollAnimation.To - ViewportSize);
            }
            else
            {
                AnimateTo(Value - ViewportSize);
            }
        }

        /// <summary>
        /// Scrolls the thumb to the specified value. 
        /// </summary>
        /// <param name="value">The value where the thumb should scroll. Valid values are from 0 through 1.</param>
        public void ScrollTo(float value)
        {
            if (value < 0 || value > 1)
                throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("value");

            AnimateTo(value);
        }

        /// <summary>
        /// Stops the thumb from scrolling if it is currently being flicked.
        /// </summary>
        public void StopFlick()
        {
            // Stop manipulating.
            if (manipulationProcessor != null)
            {
                manipulationProcessor.CompleteManipulation(stopwatch.Elapsed100Nanoseconds());
            }
            // Stop inertia.
            if (inertiaProcessor != null)
            {
                inertiaProcessor.Complete(stopwatch.Elapsed100Nanoseconds());
            }

            PageAgain = null;
            isScrolling = false;
        }

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.Value"/></strong> property changes.
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.ThumbSize"/></strong>
        /// property or position changes.
        /// </summary>
        public event EventHandler ThumbChanged;

        /// <summary>
        /// Called when the <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.Value"/></strong>
        /// property changes.
        /// </summary>
        protected virtual void OnValueChanged()
        {
            gotValueChanged = true;

            EventHandler temp = ValueChanged;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the 
        /// <strong><see cref="P:CoreInteractionFramework.ScrollBarStateMachine.ThumbSize"/></strong>
        ///  property or position changes.
        /// </summary>
        protected virtual void OnThumbChanged()
        {
            gotThumbChanged = true;

            EventHandler temp = ThumbChanged;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the type of <strong><see cref="CoreInteractionFramework.ScrollBarHitTestDetails"/></strong>.
        /// </summary>
        /// <returns>Type as <strong>typeof(ScrollBarHitTestDetails)</strong>.</returns>
        public override Type TypeOfHitTestDetails
        {
            get
            {
                // The type of IHitTestDetails we expect.
                return typeof(ScrollBarHitTestDetails);
            }
        }

        /// <summary>
        /// Gets or sets the maximum flick velocity for the 
        /// scroll bar's thumb, in device-independent units per millisecond.
        /// </summary>
        public float MaximumFlickVelocity
        {
            get { return maximumFlickVelocity; }
            set
            {
                maximumFlickVelocity = FlickUtilities.Clamp(value,
                                                            FlickUtilities.MinimumFlickVelocity,
                                                            FlickUtilities.MaximumFlickVelocity);
            }
        }

        /// <summary>
        /// Handles the <strong>TouchMoved</strong> event.
        /// </summary>
        /// <param name="touchEvent">The container for the touch that the event is about.</param>
        protected override void OnTouchMoved(TouchTargetEvent touchEvent)
        {
            base.OnTouchMoved(touchEvent);

            // Figure out if this touch is in the list or queue.
            if (capturedCollectionLookup.ContainsKey(touchEvent.Touch.Id))
            {
                ScrollBarPart part = capturedCollectionLookup[touchEvent.Touch.Id];

                // Update the hitTestDetails for this touch id.
                ScrollBarHitTestDetails details = touchEvent.HitTestDetails as ScrollBarHitTestDetails;
                if (details != null)
                {
                    captureTouchesHitTestDetails[touchEvent.Touch.Id] = details;
                }

                // We only care about changes in the thumb.
                if (part == ScrollBarPart.Thumb)
                {
                    if (!captureTouchesHitTestDetails.ContainsKey(touchEvent.Touch.Id))
                    {
                        Debug.Fail("A touch changed occured on the thumb, but the touch wasn't captured");
                    }

                    // Get the average position over the thumb.
                    float averagePoint = AverageCapturedTouchesInThumbList();

                    // Update the value and thumb properties.  Convert into Value space.
                    UpdatedValueInValueSpace(averagePoint / (1 - viewportSize));
                }
            }
            isScrolling = true;
        }

        /// <summary>
        /// Handles the <strong>TouchDown</strong> event.
        /// </summary>
        /// <param name="touchEvent">The container for the touch that the event is about.</param>
        protected override void OnTouchDown(TouchTargetEvent touchEvent)
        {
            base.OnTouchDown(touchEvent);

            // Did the touch hit the thumb or the track.
            ScrollBarPart partHit = GetScrollBarPartHit(touchEvent.HitTestDetails as ScrollBarHitTestDetails);

            // The touch went down on the ScrollBar so capture it.
            Controller.Capture(touchEvent.Touch, this);

            ScrollBarHitTestDetails details = touchEvent.HitTestDetails as ScrollBarHitTestDetails;

            // Check to make sure the details are of the correct type. 
            // We assert because this should already be caught by HitTestResult.SetHitTestInformation.
            if (details == null)
            {
                Debug.Fail("touchEvent.HitTestDetails should be of type ScrollBarHitTestDetails.");
                return;
            }

            // Stop any flicking behavior.
            StopFlick();

            switch (partHit)
            {
                // The Thumb was hit.
                case ScrollBarPart.Thumb:

                    // Pause the animation if we are playing.
                    if (scrollAnimation != null && scrollAnimation.IsPlaying)
                    {
                        scrollAnimation.Pause();
                    }

                    // Set the part hit to the thumb.
                    selectedPart = ScrollBarPart.Thumb;

                    // Add this touch to the thumb list so that it can be used for averaging. 
                    thumbCapturedTouchesList.Add(touchEvent.Touch.Id);

                    // Tie this touch id to the Thumb.
                    capturedCollectionLookup.Add(touchEvent.Touch.Id, ScrollBarPart.Thumb);

                    // We need to be able to look up the hit test details later so save that off.
                    captureTouchesHitTestDetails.Add(touchEvent.Touch.Id, details);

                    // We want to move the thumb from the point at which it was hit not the top, so store the offset in scrollbar space.
                    distanceOffset.Add(touchEvent.Touch.Id, details.Position - (Value * (1 - ViewportSize)));

                    break;

                // The Track was hit.
                case ScrollBarPart.Track:

                    // Place the touch at the end of the track queue.
                    trackQueue.Enqueue(touchEvent.Touch.Id);

                    // Tie this touch to the Track.
                    capturedCollectionLookup.Add(touchEvent.Touch.Id, ScrollBarPart.Track);

                    // We need to be able to look up the hit test details later so save that off.
                    captureTouchesHitTestDetails.Add(touchEvent.Touch.Id, details);

                    // The thumb has touches captured so don't proceed.
                    if (thumbCapturedTouchesList.Count != 0)
                        return;

                    int id = GetFrontOfTrackQueue();

                    // If the id isn't the same as touch id which went down don't proceed.
                    if (id != touchEvent.Touch.Id)
                        return;

                    selectedPart = ScrollBarPart.Track;

                    // The touch hit before the thumb.
                    if (details.Position < ThumbStartPosition)
                    {
                        if (isReversed)
                        {
                            PageForward();
                        }
                        else
                        {
                            PageBack();
                        }
                    }
                    else // The touch hit after the thumb
                    {
                        if (isReversed)
                        {
                            PageBack();
                        }
                        else
                        {
                            PageForward();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Start the flicking behavior. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAffine2DManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e)
        {
            if (inertiaProcessor != null)
            {
                inertiaProcessor.Delta -= OnAffine2DInertiaDelta;
                inertiaProcessor.Completed -= OnAffine2DInertiaCompleted;
                inertiaProcessor = null;
            }

            // The Manipulations should all run in screen space so don't convert between spaces.
            VectorF initialVelocity;
            VectorF maxViewPortSize;
            if (Orientation == Orientation.Horizontal)
            {
                initialVelocity = new VectorF(e.Velocities.LinearVelocityX, 0);
                maxViewPortSize = new VectorF(numberOfPixelsInAxis * viewportSize, 0);
            }
            else
            {
                initialVelocity = new VectorF(0, e.Velocities.LinearVelocityY);
                maxViewPortSize = new VectorF(0, numberOfPixelsInAxis * viewportSize);
            }

            // Check velocity.
            if (initialVelocity.Length < FlickUtilities.MinimumFlickVelocity)
            {
                return;
            }

            if (initialVelocity.Length >= MaximumFlickVelocity)
            {
                // If velocity is too large, reduce it to a reasonable value.
                initialVelocity.Normalize();
                initialVelocity = MaximumFlickVelocity * initialVelocity;
            }

            VectorF flickDistance;

            if (FlickUtilities.TryGetFlickDistance(initialVelocity, out flickDistance, maxViewPortSize))
            {
                processInertia = true;
                inertiaProcessor = new InertiaProcessor2D();

                inertiaProcessor.Delta += OnAffine2DInertiaDelta;
                inertiaProcessor.Completed += OnAffine2DInertiaCompleted;

                float displacement = 0;

                if (flickDistance.X == 0)
                {
                    displacement = flickDistance.Y;
                }
                else if (flickDistance.Y == 0)
                {
                    displacement = flickDistance.X;
                }
                else
                {
                    displacement = Math.Min(flickDistance.X, flickDistance.Y);
                }

                inertiaProcessor.TranslationBehavior = new InertiaTranslationBehavior2D
                {
                    DesiredDisplacement = float.IsInfinity(displacement) ? 0 : Math.Abs(displacement),
                    InitialVelocityX = initialVelocity.X,
                    InitialVelocityY = initialVelocity.Y
                };

                inertiaProcessor.InitialOriginX = ToScreenSpace(Value);
                inertiaProcessor.InitialOriginY = ToScreenSpace(Value);
            }
        }

        /// <summary>
        /// Set the flick to completed so that it stops moving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnAffine2DInertiaCompleted(object sender, Manipulation2DCompletedEventArgs e)
        {
            processInertia = false;
            manipulationProcessor = null;

            if (inertiaProcessor != null)
            {
                inertiaProcessor.Delta -= OnAffine2DInertiaDelta;
                inertiaProcessor.Completed -= OnAffine2DInertiaCompleted;
                inertiaProcessor = null;
            }

        }

        /// <summary>
        /// Update the position of the Value and the thumb each delta. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnAffine2DInertiaDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            float valueX = Value + ToValueSpace(e.Delta.TranslationX);

            valueX = FlickUtilities.Clamp(valueX, 0, 1);

            UpdatedValueInValueSpace(valueX);
        }

        private float ToScreenSpace(float value)
        {
            return numberOfPixelsInAxis * value;
        }

        private float ToValueSpace(float screenValue)
        {
            return screenValue / numberOfPixelsInAxis;
        }

        /// <summary>
        /// Handles the <strong>TouchUp</strong> event.
        /// </summary>
        /// <param name="touchEvent">The container for the touch that the event is about.</param>
        protected override void OnTouchUp(TouchTargetEvent touchEvent)
        {
            base.OnTouchUp(touchEvent);

            int id = touchEvent.Touch.Id;

            // Depending on state, the touch will be in some of these dictionaries.
            // we don't need to track their state after this point for the given id
            // so just call remove since remove doesn't throw exceptions for items not the collection.
            thumbCapturedTouchesList.Remove(id);
            capturedCollectionLookup.Remove(id);

            // If there aren't any touches on the thumb stop manipulating. 
            if (thumbCapturedTouchesList.Count == 0 && captureTouchesHitTestDetails.Count != 0)
            {
                if (manipulationProcessor != null)
                {
                    // No more touches
                    manipulationProcessor.ProcessManipulators(stopwatch.Elapsed100Nanoseconds(), null);
                }

                // The thumb is no longer captured so check to see what the selected part should be.
                if (GetFrontOfTrackQueue() == -1)
                    selectedPart = ScrollBarPart.None;
                else
                    selectedPart = ScrollBarPart.Track;
            }

            distanceOffset.Remove(id);
            captureTouchesHitTestDetails.Remove(id);
        }



        /// <summary>
        /// Handles the reset state event.
        /// </summary>
        /// <param name="sender">A <strong>UIController</strong> object.</param>
        /// <param name="e">Empty.</param>
        protected override void OnResetState(object sender, EventArgs e)
        {
            // Reset state transition properties.
            gotValueChanged = false;
            gotThumbChanged = false;

            // Update the position of the thumb if it is animating. 
            if (scrollAnimation != null && scrollAnimation.IsPlaying)
            {
                CheckForTrackCaptureAndChange();

                UpdatedValueInValueSpace(scrollAnimation.Current);
            }
            // Update the position of the thumb if it was flicked.
            else if (processInertia)
            {
                CheckForTrackCaptureAndChange();
                if (inertiaProcessor != null)
                {
                    inertiaProcessor.Process(stopwatch.Elapsed100Nanoseconds());
                }
            }
            // The thumb is no longer animating or being flicked, 
            // but we may need to Page again if it was tapped more then once. 
            else
            {
                // If the track is still selected then we want to page in the same direction as before.
                if (selectedPart == ScrollBarPart.Track)
                {
                    if (PageAgain != null)
                    {
                        PageAgain();
                    }
                }
                else
                {
                    isScrolling = false;
                }
            }
        }

        /// <summary>
        /// Checks each touch to see if the thumb has moved under it.
        /// </summary>
        private void CheckForTrackCaptureAndChange()
        {
            foreach (KeyValuePair<int, ScrollBarHitTestDetails> details in this.captureTouchesHitTestDetails)
            {
                ScrollBarPart part = GetScrollBarPartHit(details.Value);

                // If they aren't the same and the part is now a thumb then we capture this touch to the Thumb.
                if (this.capturedCollectionLookup[details.Key] != part && part == ScrollBarPart.Thumb)
                {
                    // Recapture this touch to the thumb.
                    this.capturedCollectionLookup.Remove(details.Key);
                    this.capturedCollectionLookup.Add(details.Key, ScrollBarPart.Thumb);
                    this.thumbCapturedTouchesList.Add(details.Key);

                    // We want to move the thumb from the point at which it was hit not the top, so store the offset in scrollbar space.
                    distanceOffset.Add(details.Key, details.Value.Position - (Value * (1 - ViewportSize)));

                    StopFlick();

                    // Pause the animation if we are playing.
                    if (scrollAnimation != null && scrollAnimation.IsPlaying)
                    {
                        scrollAnimation.Pause();
                    }

                    selectedPart = ScrollBarPart.Thumb;
                }
            }
        }

        /// <summary>
        /// Captures the touch and calculates which part the touch hit: Thumb or Track. 
        /// </summary>
        /// <param name="details">Details about the hit test</param>
        /// <returns>The part which was hit.</returns>
        private ScrollBarPart GetScrollBarPartHit(ScrollBarHitTestDetails details)
        {
            if (null == details)
            {
                Debug.Fail("touchEvent.HitTestDetails should be of type ScrollBarHitTestDetails.");
                return ScrollBarPart.Track;
            }

            // Check if the detail is in the thumb line segment. 
            if (details.Position >= thumbStartPosition && details.Position <= thumbStartPosition + ThumbSize / ViewportSize)
            {
                return ScrollBarPart.Thumb;
            }

            return ScrollBarPart.Track;
        }

        /// <summary>
        /// Returns the average of the captured touches in the ThumbList.
        /// </summary>
        /// <returns></returns>
        private float AverageCapturedTouchesInThumbList()
        {
            float average = 0;
            int count = 0;

            if (manipulationProcessor == null)
            {
                manipulationProcessor = new ManipulationProcessor2D(Manipulations2D.TranslateX);  // The coordinate doesn't matter we always deal in 1 dimension.
                manipulationProcessor.Completed += OnAffine2DManipulationCompleted;
            }

            List<Manipulator2D> currentManipulators = new List<Manipulator2D>();

            // Go through the touches which are captured on the thumb and average them.
            for (int i = 0; i < thumbCapturedTouchesList.Count; i++)
            {
                int id = thumbCapturedTouchesList[i];

                // Make sure the touch is captured.
                if (TouchesCaptured.Contains(id))
                {
                    //  Make sure we have hit test details for this touch.
                    if (captureTouchesHitTestDetails.ContainsKey(id))
                    {
                        Debug.Assert(distanceOffset.ContainsKey(id), "Offset wasn't calculated for this touch.");

                        float offset = distanceOffset[id];

                        ScrollBarHitTestDetails details = captureTouchesHitTestDetails[id];

                        // The Manipulations should all run in screen space.
                        Manipulator2D manipulator = new Manipulator2D(id, ToScreenSpace(details.Position - offset), 0);

                        // Make sure the value of each touch accounts for offset.
                        average += captureTouchesHitTestDetails[id].Position - offset;
                        count++;

                        currentManipulators.Add(manipulator);
                    }
                    else
                    {
                        Debug.Fail("The touch was captured, but wasn't in the hit test details dictionary.");
                    }
                }
                else
                {
                    // The touch was released so we need to remove it.
                    thumbCapturedTouchesList.Remove(id);
                    i--;
                }
            }

            manipulationProcessor.ProcessManipulators(stopwatch.Elapsed100Nanoseconds(), currentManipulators);

            // Don't divide by zero.
            if (count != 0)
                average = average / count;

            return average;
        }

        /// <summary>
        /// Updates the position of the thumb and other appropriate properties.
        /// </summary>
        private void UpdatedValueInValueSpace(float position)
        {
            float oldValue = scrollBarValue;

            if (position > 1)
            {
                position = 1;

                if (scrollAnimation != null)
                    scrollAnimation.Pause();
            }
            else if (position < 0)
            {
                position = 0;

                if (scrollAnimation != null)
                    scrollAnimation.Pause();
            }

            scrollBarValue = position;

            float oldThumbStartPosition = thumbStartPosition;

            thumbStartPosition = position * (1 - ThumbSize / viewportSize);

            // Check to see if the value property changes before calling OnValueChanged.  This is possible
            // if the viewport size changed, but not he value.
            if (oldValue != scrollBarValue)
                OnValueChanged();

            if (oldThumbStartPosition != thumbStartPosition)
                OnThumbChanged();
        }

        /// <summary>
        /// Returns the first captured touch at the front of the TrackQueue.
        /// </summary>
        /// <returns></returns>
        private int GetFrontOfTrackQueue()
        {
            int id = -1;

            // Until we find a captured touch dequeue touches. 
            while (true)
            {
                if (trackQueue.Count != 0)
                {
                    int frontID = trackQueue.Peek();

                    // If the front id is captured and it's still captured to the track.
                    if (TouchesCaptured.Contains(frontID) && capturedCollectionLookup[frontID] == ScrollBarPart.Track)
                    {
                        return frontID;
                    }
                    else
                    {
                        trackQueue.Dequeue();
                    }
                }
                else
                {
                    return id;
                }
            }
        }

        /// <summary>
        /// Animates from the position of the thumb to the the to parameter over the appropriate amount of time.
        /// </summary>
        /// <param name="to"></param>
        private void AnimateTo(float to)
        {
            isScrolling = true;

            scrollAnimation = new Animation(Value, to, ScrollBarAnimationDuration);
            scrollAnimation.Play();
        }
    }
}
