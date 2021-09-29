using System;
using System.Collections.Generic;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Maintains and manages state that is associated with an application-defined UI button 
    /// object. 
    /// </summary>
    /// <remarks>
    /// <para>Some aspects of <strong>ButtonStateMachine</strong> state include:</para>
    /// <list type="bullet">
    /// <item>What represents a click (<strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.ClickMode" /></strong>).</item>
    /// <item>Whether the button is pressed or not (<strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.IsPressed" /></strong>).</item>
    /// <item>If the button is clicked within the current update cycle (<strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.GotClicked" /></strong>)</item>
    /// </list>
    /// <para>The <strong>ButtonStateMachine</strong> class is derived from the 
    /// <strong><see cref="T:CoreInteractionFramework.UIElementStateMachine" /></strong> class.  
    /// A user-defined button can use <strong>ButtonStateMachine</strong> as a base 
    /// class to inherit Core Interaction Framework functionality.
    /// </para>
    /// <para>When you implement hit testing for <strong>ButtonStateMachine</strong>, 
    /// <strong><see cref="T:CoreInteractionFramework.IHitTestDetails" /></strong> 
    /// is not required and consequently you should pass <strong>null</strong> as the second parameter 
    /// of the <strong><see cref="M:CoreInteractionFramework.HitTestResult.SetUncapturedHitTestInformation" /></strong> method.
    /// </para>
    /// <note type="caution"> The Core Interaction Framework and API use the 
    /// Model-View-Controller (MVC) design pattern. The API state machines 
    /// represent the Model component of the MVC design pattern. 
    /// </note>
    /// </remarks>
    public class ButtonStateMachine : UIElementStateMachine
    {
        private bool releaseModeTouchUp;
        private bool gotClicked;
        private bool isPressed;

        /// <summary>
        /// Occurs when a <strong><see cref="T:CoreInteractionFramework.ButtonStateMachine" /> Click</strong> occurs.  
        /// </summary>
        /// <remarks>A <strong>Click</strong> is determined by  
        /// the <strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.ClickMode" /></strong> property.  
        /// Click modes include <strong>Press</strong>, <strong>Hover</strong>, and <strong>Release</strong>. Core 
        /// events are triggered between the call to <strong>UIController.Update</strong> and its return. In 
        /// other words, core events are triggered after the call to <strong>UIController.Update</strong>, but 
        /// before the <strong>UIController.Update</strong> method completes. 
        /// </remarks>
        public event EventHandler Click;

        /// <summary>
        /// Creates an initialized instance of a
        /// <strong><see cref="T:CoreInteractionFramework.ButtonStateMachine" /></strong> object.
        /// </summary>
        /// <param name="controller">A <strong>UIController</strong> object with buffered touches and successfully
        /// hit testing touches.</param>
        /// <param name="numberOfPixelsInHorizontalAxis">
        /// The number of pixels that this control occupies horizontally. 
        /// For more information, see 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInHorizontalAxis">
        /// NumberOfPixelsInHorizontalAxis</see></strong>.
        /// </param>
        /// <param name="numberOfPixelsInVerticalAxis">
        /// The number of pixels that this control occupies vertically. 
        /// For more information, see <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInVerticalAxis">
        /// NumberOfPixelsInVerticalAxis</see></strong>.
        /// </param>
        /// <remarks>The <strong><see cref="T:CoreInteractionFramework.UIElementStateMachine" /></strong> base 
        /// class takes the same <strong>UIController</strong> object and pixel parameters as the 
        /// <strong>ButtonStateMachine</strong> object.
        /// </remarks>
        public ButtonStateMachine(UIController controller, int numberOfPixelsInHorizontalAxis, int numberOfPixelsInVerticalAxis)
            : base(controller, numberOfPixelsInHorizontalAxis, numberOfPixelsInVerticalAxis)
        {
        }

        /// <summary>
        /// Gets or sets value that determines what action causes a 
        /// click.  </summary>
        /// <remarks>The default action is <strong>Release</strong>.  In <strong>Release</strong> mode, the click event is triggered 
        /// when the touch is removed (button click up).  
        /// </remarks>
        /// <returns>The current enumeration value that identifies what causes a <strong><see cref="T:CoreInteractionFramework.ButtonStateMachine" /></strong> click.</returns>
        /// <example>
        /// <para>
        /// <strong>Release</strong> is the default <strong>ClickMode</strong> that the API sets.  <strong>ClickMode</strong> is read/write, 
        /// and you can reset it, as the following code example shows:
        /// </para>
        /// <code lang="cs">this.ClickMode = ClickMode.Press;</code>
        /// </example>
        public ClickMode ClickMode { get; set; }

        /// <summary>
        /// Called when a <strong><see cref="E:CoreInteractionFramework.ButtonStateMachine.Click" /></strong> 
        /// event occurs on this <strong><see cref="T:CoreInteractionFramework.ButtonStateMachine" /></strong> object.  
        /// </summary>
        /// <remarks>The <strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.ClickMode" /></strong> property
        /// defines a Microsoft Surface <em>click</em> for the Core Interaction Framework API.
        /// </remarks>
        /// <example>
        /// <para>
        /// Subscribe to the <strong>Click</strong> event the same as any Microsoft Windows event, as the following 
        /// code example shows:
        /// </para>
        /// <code lang="cs">myButtonStateMachine.Click += new EventHandler(OnButton_Click);</code>
        /// <para> In this example, <strong>OnButton_Click</strong> is the name of the method to call when 
        /// a <strong>myButtonStateMachine </strong> click event is triggered.
        /// </para>
        /// </example>
        protected virtual void OnClick()
        {
            gotClicked = true;

            EventHandler temp = Click;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets a value that verifies that 
        /// a <strong><see cref="E:CoreInteractionFramework.ButtonStateMachine.Click" /></strong> event 
        /// occurred in the current update.  
        /// </summary>
        /// <remarks>You can use the <strong>GotClicked</strong> property instead of using the 
        /// <strong>Click</strong> event by checking it after <strong><see cref="M:CoreInteractionFramework.UIController.Update" /></strong> is called.   </remarks>
        /// <returns><strong>true</strong> if the button is clicked within the current update cycle; otherwise, <strong>false</strong>.</returns>
        /// <example>
        /// <para>
        ///  In the following code example, the <strong>GotClicked</strong> property is used to discern the Button1 click 
        ///  state and take appropriate action.  
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs" 
        ///  region="Got Clicked Test" title="Got Clicked Test" lang="cs" />
        /// </example>
        public virtual bool GotClicked
        {
            get
            {
                return gotClicked;
            }
            internal set
            {
                gotClicked = value;
            }
        }

        /// <summary>
        /// Gets a value that verifies if a button is pressed.  
        /// </summary>
        /// <remarks>Your application should check a button 
        /// each time after <strong><see cref="M:CoreInteractionFramework.UIController.Update" /></strong> 
        /// is called.</remarks>
        /// <returns><strong>true</strong> if there is a touch currently within the x-boundary and y-boundary of the 
        /// associated UI element.</returns>
        /// <example>
        /// <para>
        ///  In this code example from <strong>StarshipArsenal</strong>, the <strong>IsPressed</strong> property is used to 
        ///  determine which button image to use.  
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\UI\ButtonControl.cs" 
        ///  region="Using IsPressed" title="Using IsPressed" lang="cs" />
        /// </example>
        public virtual bool IsPressed
        {
            get
            {
                return isPressed;
            }
            internal set
            {
                isPressed = value;
            }
        }


        /// <summary>
        /// Handles the <strong><see cref="E:CoreInteractionFramework.UIElementStateMachine.TouchMoved" /></strong> event.
        /// </summary>
        /// <param name="touchEvent">The touch that changed.</param>
        protected override void OnTouchMoved(TouchTargetEvent touchEvent)
        {
            base.OnTouchMoved(touchEvent);

            // If the touch is captured to this control then check to see if it or 
            // any other touch hit tested to the control.
            if (Controller.GetCapturingElement(touchEvent.Touch) == this)
            {
                foreach (TouchPoint capturedTouch in TouchesCaptured)
                {
                    if (Controller.DoesHitTestMatchCapture(capturedTouch))
                    {
                        isPressed = true;
                        return;
                    }
                }
                isPressed = false;
            }
        }

        /// <summary>
        /// Changes the state of the button based on the <strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.ClickMode" /></strong>
        /// property.
        /// </summary>
        /// <param name="touchEvent">The touch that hit the button.</param>
        protected override void OnTouchDown(TouchTargetEvent touchEvent)
        {
            this.IsPressed = true;

            switch (ClickMode)
            {
                case ClickMode.Press:
                    if (this.TouchesCaptured.Count == 0)
                        OnClick();
                    this.Controller.Capture(touchEvent.Touch, this);
                    break;

                case ClickMode.Release:
                    this.Controller.Capture(touchEvent.Touch, this);
                    break;

                case ClickMode.Hover:
                    if (this.TouchesOver.Count == 0)
                    {
                        OnClick();
                    }
                    break;
                default:
                    break;
            }

            base.OnTouchDown(touchEvent);
        }

        /// <summary>
        /// Calls <strong><see cref="M:CoreInteractionFramework.ButtonStateMachine.OnClick" /></strong> 
        /// if the touch is captured over the button and there are no more captured touches.
        /// </summary>
        /// <param name="touchEvent">The touch that was removed.</param>
        protected override void OnTouchUp(TouchTargetEvent touchEvent)
        {
            if (ClickMode == ClickMode.Release 
                && TouchesCaptured.Contains(touchEvent.Touch.Id) 
                && TouchesCaptured.Count == 1)
            {
                if (Controller.DoesHitTestMatchCapture(touchEvent.Touch))
                {
                    releaseModeTouchUp = true;
                }
            }

            base.OnTouchUp(touchEvent);

            switch (ClickMode)
            {
                case ClickMode.Release:
                case ClickMode.Press:
                    foreach (TouchPoint capturedTouch in TouchesCaptured)
                    {
                        if (Controller.DoesHitTestMatchCapture(capturedTouch))
                        {
                            isPressed = true;
                            return;
                        }
                    }
                    isPressed = false;
                    break;

                case ClickMode.Hover:
                    if (this.TouchesOver.Count == 0)
                        isPressed = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Calls <strong><see cref="M:CoreInteractionFramework.ButtonStateMachine.OnClick" /></strong> 
        /// if there are no touches over and 
        /// <strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.ClickMode" /></strong> is <strong>Hover</strong>.
        /// </summary>
        /// <param name="touchEvent">The touch that entered.</param>
        protected override void OnTouchEnter(TouchTargetEvent touchEvent)
        {
            switch (ClickMode)
            {
                case ClickMode.Hover:
                    if (this.TouchesOver.Count == 0)
                    {
                        isPressed = true;
                        OnClick();
                    }
                    break;
                case ClickMode.Press:
                case ClickMode.Release:
                default:
                    break;
            }
            base.OnTouchEnter(touchEvent);
        }

        /// <summary>
        /// Called when the Touch has left the button.
        /// </summary>
        /// <param name="touchEvent">The touch that departed.</param>
        protected override void OnTouchLeave(TouchTargetEvent touchEvent)
        {
            base.OnTouchLeave(touchEvent);

            switch (ClickMode)
            {
                case ClickMode.Hover:
                    if (this.TouchesOver.Count == 0)
                        isPressed = false;
                    break;
                case ClickMode.Press:
                case ClickMode.Release:
                default:
                    break;
            }
        }

        /// <summary>
        /// Resets the <strong><see cref="P:CoreInteractionFramework.ButtonStateMachine.GotClicked" /></strong> state.
        /// </summary>
        /// <param name="sender">A <strong>UIController</strong> object.</param>
        /// <param name="e">Empty.</param>
        protected override void OnResetState(object sender, EventArgs e)
        {
            // Reset Click
            GotClicked = false;
        }

        /// <summary>
        /// Calls <strong><see cref="M:CoreInteractionFramework.ButtonStateMachine.OnClick" /></strong> 
        /// when the last touch is released over the button.
        /// </summary>
        /// <param name="orderTouches">The queue of touches to process.</param>
        protected override void OnUpdated(Queue<TouchTargetEvent> orderTouches)
        {
            releaseModeTouchUp = false;

            base.OnUpdated(orderTouches);

            // We want to wait to call click until all the input has been processed.
            if (this.TouchesCaptured.Count == 0 && releaseModeTouchUp)
            {
                OnClick();
            }
        }
    }
}
