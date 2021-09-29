using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Specifies the base class for all <strong>UIElementStateMachine</strong> classes such as 
    /// <strong><see cref="CoreInteractionFramework.ButtonStateMachine"/></strong>.  
    /// </summary>
    /// <remarks>
    /// <note type="caution"> The Core Interaction Framework and API use the 
    /// Model-View-Controller (MVC) design pattern. State machines 
    /// represents the Model component of the MVC design pattern. </note>
    /// </remarks>    
    public abstract class UIElementStateMachine : IInputElementStateMachine
    {
        private ReadOnlyTouchCollectionCache touchesOver = new ReadOnlyTouchCollectionCache();
        private UIController controller;
        private ReadOnlyTouchCollectionCache touchesCaptured = new ReadOnlyTouchCollectionCache();
        private object tag;
        private int numberOfPixelsInHorizontalAxis;
        private int numberOfPixelsInVerticalAxis;

        /// <summary>
        /// Manages a cached TouchCollection so that an editable version of the
        /// collection maybe accessed by internal components and a readonly version
        /// maybe returned to public callers.
        /// </summary>
        private class ReadOnlyTouchCollectionCache
        {
            /// <summary>
            /// Used for caching. This should be used other then by 
            /// </summary>
            private List<TouchPoint> actualTouchCollection = new List<TouchPoint>();
            internal bool IsStale = true;
            private ReadOnlyTouchPointCollection cachedTouchCollection;

            /// <summary>
            /// Gets the collection of touches which are over this state machine.
            /// </summary>
            internal List<TouchPoint> EditableTouchCollection
            {
                get
                {
                    IsStale = true;
                    return actualTouchCollection;
                }
            }

            /// <summary>
            /// Gets a cached version of the ReadOnlyTouchCollection.
            /// </summary>
            internal ReadOnlyTouchPointCollection CachedTouchCollection
            {
                get
                {
                    if (IsStale)
                    {
                        IsStale = false;
                        cachedTouchCollection = new ReadOnlyTouchPointCollection(new ReadOnlyCollection<TouchPoint>(actualTouchCollection));
                    }

                    return cachedTouchCollection;
                }
            }
        }

        /// <summary>
        /// Represents the number of pixels that this control occupies horizontally. 
        /// </summary>
        /// <returns>The horizontal dimension (width) of this control.</returns>
        /// <remarks>
        /// <para>
        /// Many Microsoft Surface controls require data about how much of a change has occurred in 
        /// physical screen space. The <strong>NumberOfPixelsInHorizontalAxis</strong> property provides mapping for this control from 
        /// normal space to screen space. For controls that occupy only 2-D screen spaces, 
        /// you can set this property as the height of the control, regardless of how it is 
        /// rotated in 2-D space. You should update this property when the control changes size.
        /// </para>
        /// <note type="caution"> If this control occupies 3-D space, set <strong>NumberOfPixelsInHorizontalAxis</strong> 
        /// to the number of pixels in screen space that the control projects into. You can update this 
        /// value as needed, but it is taken into account only when 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Update"/></strong>
        /// is called.</note>
        /// </remarks>
        public virtual int NumberOfPixelsInHorizontalAxis 
        {
            get
            {
                return numberOfPixelsInHorizontalAxis;
            }
            set
            {
                // Only update and notify if there has been a change.
                if (value == numberOfPixelsInHorizontalAxis)
                {
                    return;
                }

                numberOfPixelsInHorizontalAxis = value;

                OnNumberOfPixelsInHorizontalAxisChanged();
            }
        }

        /// <summary>
        /// Called when the number of pixels in the horizontal axis changes.
        /// </summary>
        protected virtual void OnNumberOfPixelsInHorizontalAxisChanged()
        {
            EventHandler temp = NumberOfPixelsInHorizontalAxisChanged;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the <strong><see cref="NumberOfPixelsInHorizontalAxis"/></strong> property is updated to a different 
        /// value.
        /// </summary>
        public event EventHandler NumberOfPixelsInHorizontalAxisChanged;

        /// <summary>
        /// Represents the number of pixels that this control occupies vertically. 
        /// </summary>
        /// <returns>The vertical dimension (height) of this control.</returns>
        /// <remarks>
        /// <para>
        /// Many Microsoft Surface controls require data about how much of a change has occurred in 
        /// physical screen space. The <strong>NumberOfPixelsInVerticalAxis</strong> property provides mapping for this control from 
        /// normal space to screen space. For controls that occupy only 2-D screen spaces, 
        /// you can set this property as the height of the control, regardless of how it is 
        /// rotated in 2-D space. You should update it when the control changes size.
        /// </para>
        /// <note type="caution"> If this control occupies 3-D space, set <strong>NumberOfPixelsInVerticalAxis</strong> 
        /// to the number of pixels in screen space that the control projects into. You can update 
        /// this value as needed, but it is taken into account only when 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Update"/></strong> 
        /// is called.</note>
        /// </remarks>
        public virtual int NumberOfPixelsInVerticalAxis
        {
            get
            {
                return numberOfPixelsInVerticalAxis;
            }
            set
            {
                // Only update and notify if there has been a change.
                if (value == numberOfPixelsInVerticalAxis)
                {
                    return;
                }

                numberOfPixelsInVerticalAxis = value;

                OnNumberOfPixelsInVerticalAxisChanged();
            }
        }

        /// <summary>
        /// Called when the number of pixels in the vertical axis changes.
        /// </summary>
        protected virtual void OnNumberOfPixelsInVerticalAxisChanged()
        {
            EventHandler temp = NumberOfPixelsInVerticalAxisChanged;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInVerticalAxis"/></strong>
        /// property is updated to a 
        /// different value.
        /// </summary>
        public event EventHandler NumberOfPixelsInVerticalAxisChanged;

        /// <summary>
        /// Occurs when a touch that is routed to this state machine changes.
        /// </summary>
        public event EventHandler<StateMachineTouchEventArgs> TouchMoved;

        /// <summary>
        /// Occurs when a touch that is routed to this state machine goes down.
        /// </summary>
        public event EventHandler<StateMachineTouchEventArgs> TouchDown;

        /// <summary>
        /// Occurs when a touch that is routed to this state machine enters the state machine.
        /// </summary>
        public event EventHandler<StateMachineTouchEventArgs> TouchEnter;

        /// <summary>
        /// Occurs when a touch that is routed to this state machine leaves the state machine.
        /// </summary>
        public event EventHandler<StateMachineTouchEventArgs> TouchLeave;

        /// <summary>
        /// Occurs when a touch that is routed to this state machine is removed from the state machine.
        /// </summary>
        public event EventHandler<StateMachineTouchEventArgs> TouchUp;

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Capture"/></strong> 
        /// method is called for a touch and this state machine.
        /// </summary>
        public event EventHandler<StateMachineTouchEventArgs> GotTouchCapture;

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Release"/></strong>
        /// method is called for a touch that this state machine captured.
        /// </summary>
        public event EventHandler<StateMachineTouchEventArgs> LostTouchCapture;


        /// <summary>
        /// Gets the touches that this state machine captured.
        /// </summary>
        /// <returns>
        /// A cached touch collection.
        /// </returns>
        public ReadOnlyTouchPointCollection TouchesCaptured
        {
            get
            {
                return touchesCaptured.CachedTouchCollection;
            }
        }

        /// <summary>
        /// Gets the touches over this state machine.
        /// </summary>
        /// <returns>A collection of touches over this state machine.</returns>
        public ReadOnlyTouchPointCollection TouchesOver
        {
            get
            {
                return touchesOver.CachedTouchCollection;
            }
        }

        /// <summary>
        /// Initializes the <strong><see cref="UIElementStateMachine"/></strong> objects.
        /// </summary>
        /// <param name="controller">The controller for this <strong>UIElementStateMachine</strong> object.</param>
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
        protected UIElementStateMachine(UIController controller, int numberOfPixelsInHorizontalAxis, int numberOfPixelsInVerticalAxis)
        {
            if (controller == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("controller");

            this.numberOfPixelsInHorizontalAxis = numberOfPixelsInHorizontalAxis;
            this.numberOfPixelsInVerticalAxis = numberOfPixelsInVerticalAxis;

            this.controller = controller;
            
            this.controller.ResetState += new EventHandler(OnResetState);
        }

        /// <summary>
        /// Overrides <strong>OnResetState</strong> to handle the 
        /// <strong><see cref="E:CoreInteractionFramework.UIController.ResetState"/></strong> event.
        /// Performs actions necessary to reset the state machine state at the beginning
        /// of each update cycle.
        /// </summary>
        /// <param name="sender">The controller that raised the 
        /// <strong>UIController.ResetState</strong> event.</param>
        /// <param name="e">Empty.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Security", 
            "CA2109:ReviewVisibleEventHandlers",
            Justification=@"This method is meant to be overridden by subclasses to support event routing.
                            The event handler code doesn't do anything that makes it dangerous or exploitable
                            as no permissions are being asserted in the code.
                            Hence the concerns of this security rule do not apply.")]
        protected virtual void OnResetState(object sender, EventArgs e) {}

        /// <summary>
        /// Gets or sets the <strong><see cref="CoreInteractionFramework.UIController"/></strong>
        /// object for this state machine.
        /// </summary>
        /// <returns>The <strong>UIController</strong> object for this state machine.</returns>
        public virtual UIController Controller
        {
            get
            {
                return controller;
            }
            set
            {
                if (value == null && controller == null)
                {
                    return;
                }

                if (value == null && controller != null)
                {
                    // Release any touch still captured by this control.
                    foreach (TouchPoint touch in this.TouchesCaptured)
                    {
                        controller.Release(touch);
                    }

                    controller.ResetState -= new EventHandler(OnResetState);
                }
                else if (value != controller)
                {
                    if (controller != null)
                    {
                        // Release any touch still captured by this control.
                        foreach (TouchPoint touch in this.TouchesCaptured)
                        {
                            controller.Release(touch);
                        }

                        controller.ResetState -= new EventHandler(OnResetState);
                    }

                    value.ResetState += new EventHandler(OnResetState);
                }
                
                controller = value;
            }
        }

        /// <summary>
        /// Gets or sets a data storage object for this state machine.
        /// </summary>
        /// <returns>The tag data object.</returns>
        public virtual object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }

        /// <summary>
        /// Gets the type that implements 
        /// <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> for this state machine. 
        /// </summary>
        /// <remarks>If <strong>TypeOfHitTestDetails</strong> returns null, the second parameter should be null 
        /// when your application calls 
        /// the <strong><see cref="M:CoreInteractionFramework.HitTestResult.SetCapturedHitTestInformation"/></strong>
        /// or <strong><see cref="M:CoreInteractionFramework.HitTestResult.SetUncapturedHitTestInformation"/></strong> 
        /// methods, when the first parameter is this type of state machine.
        /// </remarks>
        /// <returns>The type that implements <strong>IHitTestDetails</strong> for this state machine.</returns>
        public virtual Type TypeOfHitTestDetails
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Called when a touch that is routed to this state machine changed.
        /// </summary>
        /// <param name="touchEvent">The container object for the touch that changes.</param>
        protected virtual void OnTouchMoved(TouchTargetEvent touchEvent)
        {
            if (touchEvent == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            if (!touchesOver.CachedTouchCollection.Contains(touchEvent.Touch.Id) && !touchesCaptured.CachedTouchCollection.Contains(touchEvent.Touch.Id))
            {
                return;
            }

            EventHandler<StateMachineTouchEventArgs> temp = TouchMoved;

            if (temp != null)
            {
                temp(this, new StateMachineTouchEventArgs(touchEvent.Touch, this));
            }
        }

        /// <summary>
        /// Called when a touch that is routed to this state machine goes down.
        /// </summary>
        /// <param name="touchEvent">The container object for the touch that is down.</param>
        protected virtual void OnTouchDown(TouchTargetEvent touchEvent)
        {
            if (touchEvent.Touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            EventHandler<StateMachineTouchEventArgs> temp = TouchDown;

            if (temp != null)
            {
                temp(this, new StateMachineTouchEventArgs(touchEvent.Touch, this));
            }
        }

        /// <summary>
        /// Called when a touch that is routed to this state machine enters the state machine.
        /// </summary>
        /// <param name="touchEvent">The container for the touch that entered this state 
        /// machine.</param>
        protected virtual void OnTouchEnter(TouchTargetEvent touchEvent)
        {
            if (touchEvent.Touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            bool isTouchOverAlready = false;
            if (touchesOver.EditableTouchCollection.Contains(touchEvent.Touch.Id))
            {
                isTouchOverAlready = true;
                this.touchesOver.EditableTouchCollection.Remove(touchEvent.Touch.Id);
            }

            this.touchesOver.EditableTouchCollection.Add(touchEvent.Touch);

            if (isTouchOverAlready)
            {
                // We already have the touch over the element, no event to raise.
                return;
            }

            EventHandler<StateMachineTouchEventArgs> temp = TouchEnter;

            if (temp != null)
            {
                temp(this, new StateMachineTouchEventArgs(touchEvent.Touch, this));
            }
        }

        /// <summary>
        /// Called when a touch that is routed to this state machine leaves the state machine.
        /// </summary>
        /// <param name="touchEvent">The container object for the departed touch.</param>
        protected virtual void OnTouchLeave(TouchTargetEvent touchEvent)
        {
            if (touchEvent.Touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            if (touchesOver.EditableTouchCollection.Contains(touchEvent.Touch.Id))
                this.touchesOver.EditableTouchCollection.Remove(touchEvent.Touch.Id);

            EventHandler<StateMachineTouchEventArgs> temp = TouchLeave;

            if (temp != null)
            {
                temp(this, new StateMachineTouchEventArgs(touchEvent.Touch, this));
            }
        }

        /// <summary>
        /// Called when a touch that is routed to this state machine is removed.
        /// </summary>
        /// <param name="touchEvent">The container for the removed touch.</param>
        protected virtual void OnTouchUp(TouchTargetEvent touchEvent)
        {
            if (touchEvent.Touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            if (!touchesOver.CachedTouchCollection.Contains(touchEvent.Touch.Id) && !touchesCaptured.CachedTouchCollection.Contains(touchEvent.Touch.Id))
            {
                return;
            }

            if (touchesOver.EditableTouchCollection.Contains(touchEvent.Touch.Id))
                this.touchesOver.EditableTouchCollection.Remove(touchEvent.Touch.Id);

            EventHandler<StateMachineTouchEventArgs> temp = TouchUp;

            if (temp != null)
            {
                temp(this, new StateMachineTouchEventArgs(touchEvent.Touch, this));
            }

            if (touchesCaptured.CachedTouchCollection.Contains(touchEvent.Touch.Id))
            {
                Controller.Release(touchEvent.Touch);
            }
        }

        /// <summary>
        /// Called when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Capture"/></strong> method is called for a touch and this 
        /// state machine.
        /// </summary>
        /// <param name="touch">The container for the touch that is captured.</param>
        protected virtual void OnGotTouchCapture(TouchPoint touch)
        {
            if (touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            bool isTouchCapturedAlready = false;
            if (touchesCaptured.EditableTouchCollection.Contains(touch.Id))
            {
                isTouchCapturedAlready = true;
                touchesCaptured.EditableTouchCollection.Remove(touch.Id);
            }

            touchesCaptured.EditableTouchCollection.Add(touch);

            if (isTouchCapturedAlready)
            {
                // We already have the touch captured by the element, no event to raise.
                return;
            }

            EventHandler<StateMachineTouchEventArgs> temp = GotTouchCapture;

            // Use a temporary delegate for thread-safety.
            if (temp != null)
            {
                temp(this, new StateMachineTouchEventArgs(touch, this));
            }
        }

        /// <summary>
        /// Called when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Release"/></strong>
        ///  method is called for a touch that this state machine captured.
        /// </summary>
        /// <param name="touch">The touch that is released.</param>
        protected virtual void OnLostTouchCapture(TouchPoint touch)
        {
            if (touch == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("touch");

            if (!touchesCaptured.EditableTouchCollection.Contains(touch.Id))
            {
                return;
            }

            touchesCaptured.EditableTouchCollection.Remove(touch.Id);

            EventHandler<StateMachineTouchEventArgs> temp = LostTouchCapture;

            if (temp != null)
            {
                temp(this, new StateMachineTouchEventArgs(touch, this));
            }
        }

        /// <summary>
        /// Called when all of the touch events have been routed to this state machine.
        /// </summary>
        /// <param name="orderTouches">An ordered list of touches for each touch event.</param>
        protected virtual void OnUpdated(System.Collections.Generic.Queue<TouchTargetEvent> orderTouches)
        {
            foreach (TouchTargetEvent cte in orderTouches)
            {
                switch (cte.EventType)
                {
                    case TouchEventType.Added:
                        OnTouchDown(cte);
                        break;
                    case TouchEventType.Removed:
                        OnTouchUp(cte);
                        break;
                    case TouchEventType.Changed:
                        OnTouchMoved(cte);
                        break;
                    case TouchEventType.Enter:
                        OnTouchEnter(cte);
                        break;
                    case TouchEventType.Leave:
                        OnTouchLeave(cte);
                        break;
                    default:
                        Debug.Fail("TouchEventType is unknown at time of UIElementStateMachine OnUpdate");
                        break;
                }
            }
        }

        #region IInputElementStateMachine Members

        //Review: Does this SuppressMessage make sense? Why does it warn about this method, but not the next two?
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IInputElementStateMachine.Update(Queue<TouchTargetEvent> touches)
        {
            OnUpdated(touches);
        }

        void IInputElementStateMachine.OnGotTouchCapture(TouchPoint touch)
        {
            OnGotTouchCapture(touch);
        }

        void IInputElementStateMachine.OnLostTouchCapture(TouchPoint touch)
        {
            OnLostTouchCapture(touch);
        }

        #endregion
    }
}
