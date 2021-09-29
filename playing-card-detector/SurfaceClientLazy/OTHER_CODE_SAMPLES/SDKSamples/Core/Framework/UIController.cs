using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Represents the UI controller. 
    /// </summary>
    /// <remarks>A <strong>UIController</strong> object retrieves touches from the ordered 
    /// touch events buffer, hit tests the touches, and routes touches that were hit tested 
    /// successfully or were captured to the correct state machine.  </remarks>
    public class UIController
    {
        // Used to determine if update is currently being called.
        private bool isUpdating;

        // The maximum size of the orderedTouchEventsBackbuffer before an exception is thrown.
        private const int MaximumQueueSize = 200000; // About 52 touch events per frame at 60 FPS for 1 minute.

        // An ordered list of touchEvents that is updated by OnFrameReceived 
        internal System.Collections.Generic.Queue<TouchTargetEvent> orderedTouchEvents = new Queue<TouchTargetEvent>();
        internal System.Collections.Generic.Queue<TouchTargetEvent> orderedTouchEventsBackbuffer = new Queue<TouchTargetEvent>();

        // The list of touches that will be routed to UI elements. 
        internal System.Collections.Generic.Queue<System.Collections.Generic.Dictionary<TouchTargetEvent, IInputElementStateMachine>> touchEventsToRoute = new Queue<Dictionary<TouchTargetEvent, IInputElementStateMachine>>();

        internal System.Collections.Generic.Dictionary<TouchTargetEvent, IInputElementStateMachine> packagedTouches = new Dictionary<TouchTargetEvent, IInputElementStateMachine>();
        internal Dictionary<TouchTargetEvent, IInputElementStateMachine> unpackagedTouches = new Dictionary<TouchTargetEvent, IInputElementStateMachine>();


        // A paired list of touches to perform hitTesting on.  The IInputElementStateMachine parameter
        // will be null when passed to the HitTestCallback delegate such that
        // the client hitTesting will fill out the IInputElementStateMachine.
        private System.Collections.Generic.Dictionary<TouchTargetEvent, IInputElementStateMachine> hitTestingTouches = new Dictionary<TouchTargetEvent, IInputElementStateMachine>();

        // A dictionary of touches (touch ID) with are captured and the IInputElementStateMachine
        private System.Collections.Generic.Dictionary<int, IInputElementStateMachine> capturedTouches = new Dictionary<int, IInputElementStateMachine>();

        // A dictionary of touches (touch ID) with are captured and the IInputElementStateMachine
        private System.Collections.Generic.Dictionary<int, IInputElementStateMachine> touchesOver = new Dictionary<int, IInputElementStateMachine>();

        /// <summary>
        /// A lock to synchronize access to the swapLock.
        /// </summary>
        private readonly object swapLock = new object();

        private readonly TouchTarget touchTarget;
        private readonly HitTestCallback hitTestCallback;

        /// <summary>
        /// Occurs in <strong>Update</strong> before any touches are processed. 
        /// </summary>
        /// <remarks>The <strong>ResetState</strong> event gives
        /// listeners a chance to reset any state before <strong>Update</strong> is processed.</remarks>
        public event EventHandler ResetState;

        /// <summary>
        /// Raises the <strong><see cref="ResetState"/></strong> event to enable listeners to 
        /// reset their state before any touches are processed.
        /// </summary>
        private void OnResetState()
        {
            EventHandler temp = ResetState;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates a <strong><see cref="UIController"/></strong> instance with the specified parameters.
        /// </summary>
        /// <param name="touchTarget">A touch target to use for collecting touches.</param>
        /// <param name="hitTestCallback">A delegate that is used to do hit testing.</param>
        public UIController(TouchTarget touchTarget, HitTestCallback hitTestCallback)
        {
            if (touchTarget == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touchTarget");

            if (hitTestCallback == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("hitTestCallback");

            this.touchTarget = touchTarget;
            this.hitTestCallback = hitTestCallback;

            this.touchTarget.TouchDown += new EventHandler<TouchEventArgs>(OnTouchAdded);
            this.touchTarget.TouchMove += new EventHandler<TouchEventArgs>(OnTouchMoved);
            this.touchTarget.TouchUp += new EventHandler<TouchEventArgs>(OnTouchRemoved);
        }

        /// <summary>
        /// Gives exclusive access to events that are raised for a particular touch.  
        /// </summary>
        /// <remarks>All events raised on the specified touch are routed to the element that passed
        /// to the <strong>Capture</strong> method. The 
        /// <strong><see cref="M:CoreInteractionFramework.HitTestCallback"/></strong> delegate is not called while
        /// a touch is captured.
        /// </remarks>
        /// <param name="touch">The touch to capture.</param>
        /// <param name="element">The element to route the captured touch's event to.</param>
        public void Capture(TouchPoint touch, IInputElementStateMachine element)
        {
            if (touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            if (element == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("element");

            if (this.capturedTouches.ContainsKey(touch.Id))
            {
                this.capturedTouches.Remove(touch.Id);
            }

            this.capturedTouches.Add(touch.Id, element);

            element.OnGotTouchCapture(touch);
        }

        /// <summary>
        /// Gets the 
        /// <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> object
        /// that has captured the touch.  
        /// </summary>
        /// <param name="touch">The touch to check if it is captured by a 
        /// <strong>IInputElementStateMachine</strong> object.
        /// </param>
        /// <returns>The <strong>IInputElementStateMachine</strong> object that the touch was captured on as 
        /// <strong>IInputElementStateMachine</strong>. Returns null if the touch is not captured.</returns>
        public IInputElementStateMachine GetCapturingElement(TouchPoint touch)
        {
            if (touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            if (this.capturedTouches.ContainsKey(touch.Id))
            {

                IInputElementStateMachine statemachine;

                this.capturedTouches.TryGetValue(touch.Id, out statemachine);

                return statemachine;
            }


            return null;
        }

        /// <summary>
        /// Releases a captured touch. 
        /// </summary>
        /// <remarks>The <strong>Release</strong> method causes hit testing to be
        /// performed for the specified touch.</remarks>
        /// <param name="touch">The touch to release.</param>
        public void Release(TouchPoint touch)
        {
            if (touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            IInputElementStateMachine element = null;

            // Attempt to get the statemachine from the collection with the touch.Id as a key.
            if (this.capturedTouches.TryGetValue(touch.Id, out element))
            {
                this.capturedTouches.Remove(touch.Id);

                element.OnLostTouchCapture(touch);
            }
        }

        /// <summary>
        /// Specifies whether the touch hit tested to the capturing element.
        /// </summary>
        /// <param name="touch">The touch to test.</param>
        /// <returns><strong>true</strong> if hit tested to the captured element; otherwise, <strong>false</strong>.</returns>
        public bool DoesHitTestMatchCapture(TouchPoint touch)
        {
            if (touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            IInputElementStateMachine capturedModel = null, overModel = null;

            if (capturedTouches.TryGetValue(touch.Id, out capturedModel))
            {
                if (touchesOver.TryGetValue(touch.Id, out overModel))
                {
                    if (overModel == capturedModel)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes input that the <strong>TouchTarget</strong> receives and dispatches that input for 
        /// hit testing and 
        /// <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> updates.
        /// </summary>
        public void Update()
        {
            if (isUpdating)
                throw SurfaceCoreFrameworkExceptions.UpdateCannotBeCalledDuringUpdate();

            try
            {
                isUpdating = true;

                OnResetState();
                Swap();
                DispatchHitTesting();
                RoutePackagedTouches();
                ClearOrderedTouchEvents();
                ClearTouchOverState();
            }
            finally
            {
                isUpdating = false;
            }
        }

        /// <summary>
        /// Swaps the <strong>orderedTouchEvents</strong> and the <strong>orderedTouchEventsBackbuffer</strong>.  This is thread-safe.
        /// </summary>
        private void Swap()
        {
            // Only lock long enough to swap the collections.
            lock (swapLock)
            {
                Queue<TouchTargetEvent> newFrontBuffer = this.orderedTouchEventsBackbuffer;
                orderedTouchEventsBackbuffer = this.orderedTouchEvents;
                this.orderedTouchEvents = newFrontBuffer;
            }
        }

        /// <summary>
        /// Causes each Touch in orderedTouchEvents to be routed 
        /// to the IInputElementStateMachine associated with the Touch in Touches.
        /// </summary>
        private void RoutePackagedTouches()
        {
            Dictionary<IInputElementStateMachine, Queue<TouchTargetEvent>> modelsToRouteTouchesTo = new Dictionary<IInputElementStateMachine, Queue<TouchTargetEvent>>();

            foreach (KeyValuePair<TouchTargetEvent, IInputElementStateMachine> touchTargetWithModels in packagedTouches)
            {
                Debug.Assert(touchTargetWithModels.Value != null, "The model is null and so the touch can't be routed");

                if (modelsToRouteTouchesTo.ContainsKey(touchTargetWithModels.Value))
                {
                    Queue<TouchTargetEvent> modelQueue;

                    if (modelsToRouteTouchesTo.TryGetValue(touchTargetWithModels.Value, out modelQueue))
                    {
                        modelQueue.Enqueue(touchTargetWithModels.Key);
                    }
                    else
                    {
                        Debug.Fail("Unable to get the value from modelsToRouteTouchesTo even though the key was found with the ContainsKey method.");
                    }
                }
                else
                {
                    Queue<TouchTargetEvent> modelQueue = new Queue<TouchTargetEvent>();

                    modelQueue.Enqueue(touchTargetWithModels.Key);

                    modelsToRouteTouchesTo.Add(touchTargetWithModels.Value, modelQueue);
                }
            }

            foreach (KeyValuePair<IInputElementStateMachine, Queue<TouchTargetEvent>> models in modelsToRouteTouchesTo)
            {
                models.Key.Update(models.Value);
            }
        }

        /// <summary>
        /// Calls HitTestCallBack passing a dictionary (Dictionary&lt;Touch, IInputElementStateMachine\&gt;) which contains a single 
        /// touch for each touch id that was in orderedTouchEvents.  Captured touches are removed from
        /// this dictionary when it is constructed and placed in touchesToRoute so that they aren't hit tested. 
        /// When the call to HitTestCallback returns the pairs (KeyValuePair&lt;Touch, IInputElementStateMachine&gt;) which contain 
        /// non-null IInputElementStateMachine are added to touchesToRoute.
        /// </summary>
        private void DispatchHitTesting()
        {
            PackageCapturedTouches();
            HitTestNonCapturedTouches();
        }

        /// <summary>
        /// Packages touches which are captured and places them into the packagedTouch dictionary so they are 
        /// ready to be routed to the IInputElementStateMachine.       
        /// </summary>
        private void PackageCapturedTouches()
        {
            // Don't waste time going through the queue of orderedTouches if no Touches are captured.
            if (this.capturedTouches.Count == 0)
            {
                // We need to make sure that unpackedTouches is filled out.
                foreach (TouchTargetEvent unpackagedTouchEvent in orderedTouchEvents)
                {
                    unpackagedTouches.Add(unpackagedTouchEvent, null);
                }

                return;
            }

            foreach (TouchTargetEvent cte in orderedTouchEvents)
            {
                // Check if touches arriving on the orderedTouchEvents queue are in teh capturedTouches queue.
                if (this.capturedTouches.ContainsKey(cte.Touch.Id))
                {
                    IInputElementStateMachine model;

                    if (this.capturedTouches.TryGetValue(cte.Touch.Id, out model))
                    {
                        Debug.Assert(model != null, "A captured Touch has a null IInputElementStateMachine.");

                        if (model.Controller == this)
                        {
                            this.packagedTouches.Add(cte, model);
                        }
                        else
                        {
                            Debug.Fail("This shouldn't happen because setting the Controller of the IInputElementStateMachine should have released the touch from capture.");
                        }
                    }
                    else
                    {
                        // When we build the SDK in debug this won't matter, but in release we want to handle this gracefully.
                        unpackagedTouches.Add(cte, null);

                        Debug.Fail("Unable to retrieve IInputElementStateMachine from capturedTouches.");
                    }
                }
                else
                {
                    unpackagedTouches.Add(cte, null);
                }
            }

            Debug.Assert(unpackagedTouches.Count + packagedTouches.Count == orderedTouchEvents.Count, "The count of TouchTargetEvents in orderedTouchEvents wasn't equal to the sum of packaged and unpackaged TouchTargetEvents");
        }

        /// <summary>
        /// Calls the users HitTestCallback delegate passing the unpackaged touches.
        /// </summary>
        private void HitTestNonCapturedTouches()
        {
            // Don't try to do hit testing if there are no touches to test.
            if (this.unpackagedTouches.Count == 0 && this.packagedTouches.Count == 0)
            {
                return;
            }

            ReadOnlyHitTestResultCollection uncapturedHitTestResults = new ReadOnlyHitTestResultCollection(this.unpackagedTouches);
            ReadOnlyHitTestResultCollection capturedHitTestResults = new ReadOnlyHitTestResultCollection(this.packagedTouches);

            // We want to remove the packed touches such that all touches must be re-added.
            this.packagedTouches.Clear();

            Debug.Assert(hitTestCallback != null, "The HitTestCallBack is null");

            // Call the user HitTestCallback delegate.
            this.hitTestCallback(uncapturedHitTestResults, capturedHitTestResults);

            // Check the capturedTouchResults to see if they were hit Tested successfully
            foreach (HitTestResult capturedHitTestResult in capturedHitTestResults)
            {
                PostHitTestResult(capturedHitTestResult);
            }

            foreach (HitTestResult hitTestResult in uncapturedHitTestResults)
            {
                PostHitTestResult(hitTestResult);
            }
        }

        private void PostHitTestResult(HitTestResult hitTestResult)
        {
            if (hitTestResult.StateMachine == null)
            {   
                // If the model is null then the hit testing didn't find an IInputElementStateMachine
                // This means it's not over any models.
                HitTestFail(hitTestResult);
            }
            else
            { 
                HitTestSuccess(hitTestResult);
            }
        }

        private void HitTestFail(HitTestResult hitTestResult)
        {
            IInputElementStateMachine stateMachine;

            // Check to see if this touch is captured to an IInputElementStateMachine.
            if (capturedTouches.TryGetValue(hitTestResult.Touch.Id, out stateMachine))
            {
                if (hitTestResult.TouchTargetEvent.EventType == TouchEventType.Removed)
                {
                    // Send the up notification to the old statemachine.
                    packagedTouches.Add(hitTestResult.TouchTargetEvent, stateMachine);

                    // Send leave notification to the old statemachine.
                    TouchTargetEvent leaveEvent = new TouchTargetEvent(TouchEventType.Leave, hitTestResult.Touch);
                    packagedTouches.Add(leaveEvent, stateMachine);
                }
                else
                {
                    // Send the leave notification to the old statemachine.
                    packagedTouches.Add(hitTestResult.TouchTargetEvent, stateMachine);
                }

                // If this touch was over it shouldn't be any longer.
                if (touchesOver.ContainsKey(hitTestResult.Touch.Id))
                {
                    touchesOver.Remove(hitTestResult.Touch.Id);
                }

            }
            else
            {
                // Is this touch over an IInputElementStateMachine currently?
                if (touchesOver.TryGetValue(hitTestResult.Touch.Id, out stateMachine))
                {
                    // The touch just moved off the edge of the IInputElementStateMachine.
                    if (hitTestResult.TouchTargetEvent.EventType == TouchEventType.Changed)
                    {
                        hitTestResult.TouchTargetEvent.EventType = TouchEventType.Leave;
                    }

                    // Send the notification to the old statemachine.
                    packagedTouches.Add(hitTestResult.TouchTargetEvent, stateMachine);

                    touchesOver.Remove(hitTestResult.Touch.Id);
                }
            }

            // The touch isn't captured, isn't in the touches over 
            // and it didn't hit anything so there is no where to route it.
        }

        private void HitTestSuccess(HitTestResult hitTestResult)
        {

            if (hitTestResult.StateMachine.Controller != this)
            {
                throw SurfaceCoreFrameworkExceptions.ControllerSetToADifferentControllerException(hitTestResult.StateMachine);
            }

            // Check if the TouchId is in the touchesOver collection.
            IInputElementStateMachine overStateMachine;
            if (touchesOver.TryGetValue(hitTestResult.Touch.Id, out overStateMachine))
            {
                // Then check if the hitTestResult is over the sane statemachine
                if (hitTestResult.StateMachine == overStateMachine)
                {
                    // Just because the touchOver collection hasn't change doesn't mean this event is being
                    // sent to the correct statemachine.  Check if this should be sent to the captured statemachine.
                    IInputElementStateMachine capturedStateMachine;
                    if (capturedTouches.TryGetValue(hitTestResult.Touch.Id, out capturedStateMachine))
                    {
                        if (hitTestResult.TouchTargetEvent.EventType == TouchEventType.Removed)
                        {
                            // Send the up notification to the old statemachine.
                            packagedTouches.Add(hitTestResult.TouchTargetEvent, capturedStateMachine);

                            // Send leave notification to the old statemachine.
                            TouchTargetEvent leaveEvent = new TouchTargetEvent(TouchEventType.Leave,
                                                                                   hitTestResult.Touch);
                            packagedTouches.Add(leaveEvent, capturedStateMachine);
                        }
                        else
                        {
                            packagedTouches.Add(hitTestResult.TouchTargetEvent, capturedStateMachine);
                        }  

                    }
                    else
                    {
                        // Touch is not currently captured.
                        if (hitTestResult.TouchTargetEvent.EventType == TouchEventType.Removed)
                        {
                            // Send the up notification to the old statemachine.
                            packagedTouches.Add(hitTestResult.TouchTargetEvent, hitTestResult.StateMachine);

                            // Send leave notification to the old statemachine.
                            TouchTargetEvent leaveEvent = new TouchTargetEvent(TouchEventType.Leave,
                                                                                   hitTestResult.Touch);

                            packagedTouches.Add(leaveEvent, hitTestResult.StateMachine);
                        }
                        else
                        {
                            packagedTouches.Add(hitTestResult.TouchTargetEvent, hitTestResult.StateMachine);
                        }
                    }
                }
                else
                {
                    // It's over a different statemachine.

                    // Remove the old IInputElementStateMachine from the touchesOver collection.
                    touchesOver.Remove(hitTestResult.Touch.Id);

                    // Add the new IInputElementStateMachine to the touchesOver collection
                    touchesOver.Add(hitTestResult.Touch.Id, hitTestResult.StateMachine);

                    IInputElementStateMachine capturedStateMachine;
                    if (capturedTouches.TryGetValue(hitTestResult.Touch.Id, out capturedStateMachine))
                    {
                        // Touch is captured, but over a different statemachine.
                        // If the touch is captured then don't send enter leave events.

                        // Route this event to the capturedStateMachine.
                        packagedTouches.Add(hitTestResult.TouchTargetEvent, capturedStateMachine);
                    }
                    else
                    {
                        // Touch is not captured over a new statemachine.

                        // It's not over the same statemachine, so we need to add a leave TouchTargetEvent to tell 
                        // the statemachine its leaving. 
                        TouchTargetEvent leaveEvent = new TouchTargetEvent(TouchEventType.Leave,
                                                                               hitTestResult.Touch);

                        // We need to add the leave event or it will not get routed.
                        packagedTouches.Add(leaveEvent, overStateMachine);

                        // Then change the EventType to Enter so that the new statemachine
                        // will know a Touch just entered.
                        hitTestResult.TouchTargetEvent.EventType = TouchEventType.Enter;
                        packagedTouches.Add(hitTestResult.TouchTargetEvent, hitTestResult.StateMachine);

                    }
                }
            }
            else  
            {
                // Not in touchesOver.

                // This touch is just coming over a statemachine either change or add.

                // Check to see if this touch is captured to an IInputElementStateMachine.
                IInputElementStateMachine capturedStateMachine = null;
                if (capturedTouches.TryGetValue(hitTestResult.Touch.Id, out capturedStateMachine))
                {
                    // TouchesOver should reflect which element this touch is over, not which it's captured too.
                    touchesOver.Add(hitTestResult.Touch.Id, hitTestResult.StateMachine);

                    // We should send this event to the element that captured it.
                    packagedTouches.Add(hitTestResult.TouchTargetEvent, capturedStateMachine);
                }
                else
                {
                    // Not captured.

                    // We want to send an Enter event instead of a changed.
                    if (hitTestResult.TouchTargetEvent.EventType == TouchEventType.Changed)
                    {
                        hitTestResult.TouchTargetEvent.EventType = TouchEventType.Enter;
                    }

                    if (hitTestResult.TouchTargetEvent.EventType == TouchEventType.Added)
                    {
                        TouchTargetEvent enterEvent = new TouchTargetEvent(TouchEventType.Enter, hitTestResult.Touch);
                        packagedTouches.Add(enterEvent, hitTestResult.StateMachine);
                    }

                    // This touch is now over this IInputElementStateMachine.
                    switch (hitTestResult.TouchTargetEvent.EventType)
                    {
                        case TouchEventType.Enter:
                        case TouchEventType.Added:
                            touchesOver.Add(hitTestResult.Touch.Id, hitTestResult.StateMachine);
                            break;
                        case TouchEventType.Removed:
                        case TouchEventType.Leave:
                            Debug.Fail("If we get an removed or leave we missed adding it to an IInputElementStateMachine somewhere.");
                            break;
                    }

                    packagedTouches.Add(hitTestResult.TouchTargetEvent, hitTestResult.StateMachine);
                }
            }

            // Route touches and remove the added ones anytime a touch Enter, Add, Remove or Leaves.
            RoutePackagedTouches();
            packagedTouches.Clear();
        }

        /// <summary>
        /// This method clears out all the tracked touches over the UI elements.
        /// </summary>
        private void ClearTouchOverState()
        {
            ReadOnlyTouchPointCollection currentState = touchTarget.GetState();
            IEnumerable<int> remove = touchesOver.Keys.Where(touchId => !currentState.Contains(touchId));
            if (remove.Count() > 0)
            {
                foreach (int item in new List<int>(remove))
                {
                    touchesOver.Remove(item);
                }
            }
        }

        /// <summary>
        /// This method clears out all the collections that were used during this update loop.
        /// </summary>
        private void ClearOrderedTouchEvents()
        {
            orderedTouchEvents.Clear();
            touchEventsToRoute.Clear();
            packagedTouches.Clear();
            unpackagedTouches.Clear();
            hitTestingTouches.Clear();
        }

        /// <summary>
        /// Handles TouchAdded events from the TouchTarget
        /// </summary>
        private void OnTouchAdded(object sender, TouchEventArgs e)
        {
            TouchTargetEvent cte = new TouchTargetEvent(TouchEventType.Added, e.TouchPoint);

            lock (swapLock)
            {
                this.orderedTouchEventsBackbuffer.Enqueue(cte);

                if (this.orderedTouchEventsBackbuffer.Count > MaximumQueueSize)
                {
                    throw SurfaceCoreFrameworkExceptions.MaximumQueueSizeReached(MaximumQueueSize);
                }
            }
        }

        /// <summary>
        /// Handles TouchRemoved events from the TouchTarget
        /// </summary>
        private void OnTouchRemoved(object sender, TouchEventArgs e)
        {
            TouchTargetEvent cte = new TouchTargetEvent(TouchEventType.Removed, e.TouchPoint);

            lock (swapLock)
            {
                this.orderedTouchEventsBackbuffer.Enqueue(cte);

                if (this.orderedTouchEventsBackbuffer.Count > MaximumQueueSize)
                {
                    throw SurfaceCoreFrameworkExceptions.MaximumQueueSizeReached(MaximumQueueSize);
                }
            }
        }

        /// <summary>
        /// Handles TouchMoved events from the TouchTarget
        /// </summary>
        private void OnTouchMoved(object sender, TouchEventArgs e)
        {
            TouchTargetEvent cte = new TouchTargetEvent(TouchEventType.Changed, e.TouchPoint);

            lock (swapLock)
            {
                this.orderedTouchEventsBackbuffer.Enqueue(cte);

                if (this.orderedTouchEventsBackbuffer.Count > MaximumQueueSize)
                {
                    throw SurfaceCoreFrameworkExceptions.MaximumQueueSizeReached(MaximumQueueSize);
                }
            }
        }
    }
}
