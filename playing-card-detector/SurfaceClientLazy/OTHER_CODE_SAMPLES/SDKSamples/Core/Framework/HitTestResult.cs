using System;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Defines a touch hit test result.
    /// </summary>
    /// <remarks>
    /// Uncaptured touches are placed into <strong>HitTestResult</strong> objects that are 
    /// placed into a <strong>ReadOnlyHitTestResultCollection</strong> collection. 
    /// Captured touches 
    /// are also placed into a <strong>ReadOnlyHitTestResultCollection</strong> collection. These 
    /// collections are passed to the <strong><see cref="CoreInteractionFramework.HitTestCallback"/></strong> 
    /// delegate implementation.  
    /// The application provides the delegate method name to the API when 
    /// the <strong>UIController</strong> object is declared.
    /// </remarks>
    /// <example>
    /// <para>The following code example shows the <strong>HitTestResult</strong> in use in a 
    /// <strong>HitTestCallback</strong>
    /// implementation.
    /// </para>
    ///  <code source="Core\Framework\StarshipArsenal\UI\ButtonControl.cs" 
    ///  region="Button Hit Test" title="Button Hit Test" lang="cs" />
    /// </example>
    public sealed class HitTestResult
    {
        private readonly TouchTargetEvent touch;
        private IInputElementStateMachine stateMachine;

        /// <summary>
        /// Parameterized internal class constructor.
        /// </summary>
        /// <param name="touch">
        /// An event that has occured for a Surface Touch.</param>
        /// <param name="model">
        /// Interface for upating a state machine.</param>
        internal HitTestResult(TouchTargetEvent touch, IInputElementStateMachine model)
        {
            this.touch = touch;
            this.stateMachine = model;
        }

        /// <summary>
        /// Gets the touch to hit test against.
        /// </summary>
        /// <returns>The touch to test.</returns>
        public TouchPoint Touch
        {
            get
            {
                return TouchTargetEvent.Touch;
            }
        }

        /// <summary>
        /// The TouchTargetEvent to hit test against.
        /// </summary>
        internal TouchTargetEvent TouchTargetEvent
        {
            get { return touch; }
        }

        /// <summary>
        /// Gets the interface to the state machine that is testing the 
        /// touch hit.
        /// </summary>
        /// <returns>The state machine that does the testing.</returns>
        public IInputElementStateMachine StateMachine
        {
            get { return stateMachine; }
        }

        /// <summary>
        /// Gets details about a hit test for certain state machines.
        /// </summary>
        /// <remarks>For more information about which state machines require 
        /// hit test details, see <strong><see cref="T:CoreInteractionFramework.IHitDetails">IHitTestDetails
        /// </see></strong>.
        /// </remarks>
        /// <returns>A touch hit test details object.</returns>
        public IHitTestDetails HitTestDetails
        {
            get { return touch.HitTestDetails; }
        }

        /// <summary>
        /// Sets information about what captured <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> 
        /// was hit.  
        /// </summary>
        /// <remarks>Your application should call the <strong>SetCapturedHitTestInformation</strong> method 
        /// on each item in the <strong>capturedTouchEventsToHitTest</strong> 
        /// collection that is represented by the second parameter of the 
        /// <strong><see cref="CoreInteractionFramework.HitTestCallback"/></strong> delegate.
        /// </remarks>
        /// <param name="hitCapturedInputElement"><strong>true</strong> if the captured <strong>IInputElement</strong> 
        /// object from 
        /// the collection was hit; otherwise, <strong>false</strong>.</param>
        /// <param name="hitTestDetails"> Details about the hit test that occurred.  
        /// This parameter is required, except in the case when the <strong><see cref="StateMachine"/></strong> 
        /// property  
        /// of a <strong><see cref="HitTestResult"/></strong> object is null. An exception is thrown in all other cases. 
        /// The type of <strong>hitTestDetails</strong> object must match the type that is returned from the 
        /// <strong>TypeOfHitTestDetails</strong> 
        /// property of the <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> state machine that is captured.
        /// </param>        
        public void SetCapturedHitTestInformation(bool hitCapturedInputElement, IHitTestDetails hitTestDetails)
        {
            // This will only be non null if this element is captured.
            if (stateMachine != null)
            {
                // The captured element was hit.
                if (hitCapturedInputElement)
                {
                    SetHitTestDetails(stateMachine, stateMachine.GetType(), hitTestDetails, stateMachine.TypeOfHitTestDetails);
                }
                // The captured element wasn't hit.
                else 
                {
                    SetHitTestDetails(null, stateMachine.GetType(), hitTestDetails, stateMachine.TypeOfHitTestDetails);
                }
            }
            else // stateMachine == null - This is an uncaptured hitTest so throw an exception.
            {
                throw SurfaceCoreFrameworkExceptions.CalledCapturedHitTestInformationForReleasedElement();
            }
        }

        /// <summary>
        /// Sets information about what uncaptured 
        /// <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> state machine was hit, 
        /// if any, and details about the hit. 
        /// </summary>
        /// <remarks>The <em>hitTestDetails</em> parameter can be null in two cases:
        /// <list type="bullet">
        /// <item>When the <strong>TypeOfHitTestDetails</strong> on an 
        /// <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> state machine
        /// returns null.</item>
        /// <item>When the <strong>StateMachine</strong> on a 
        /// <strong><see cref="CoreInteractionFramework.HitTestResult"/></strong> object returns 
        /// null.</item>
        /// </list>
        /// An exception is thrown in all other cases. The type of <strong>hitTestDetails</strong> must 
        /// match the type that is returned from the <strong>TypeOfHitTestDetails</strong> property on the <em>elementHit</em> 
        /// parameter.
        /// </remarks>
        /// <param name="elementHit">The element that a touch hit.</param>
        /// <param name="hitTestDetails">Details about that hit.</param>
        public void SetUncapturedHitTestInformation(IInputElementStateMachine elementHit, IHitTestDetails hitTestDetails)
        {
            // This will only be non null if this element is captured.
            if (stateMachine != null)
            {
                throw SurfaceCoreFrameworkExceptions.CalledReleaseHitTestInformationForCapturedElement();           
            }
            else // stateMachine == null - This is an uncaptured hitTest
            {
                if (elementHit == null)
                {
                    if (hitTestDetails == null)
                    {
                        stateMachine = null;
                        hitTestDetails = null;
                    }
                }
                else // elementHit != null.
                {
                    SetHitTestDetails(elementHit, elementHit.GetType(), hitTestDetails, elementHit.TypeOfHitTestDetails);
                }
            }
        }

        /// <summary>
        /// Sets the stateMachine and hitTestDetails. Throws an exception returned 
        /// from the exception parameter if the type of hitTestDetails and the 
        /// HitTestDetails paired with the stateMachine don't match.
        /// </summary>
        /// <param name="stateMachine">State Machine subject of the hit.</param>
        /// <param name="stateMachineType">Type of State Machine hit.</param>
        /// <param name="hitTestDetails">Details about the hit.</param>
        /// <param name="typeOfHitTestDetails">Type of hit test details about the hit.</param>
        private void SetHitTestDetails(IInputElementStateMachine stateMachine, Type stateMachineType, IHitTestDetails hitTestDetails, Type typeOfHitTestDetails)
        {
            this.stateMachine = stateMachine;

            Type actualType = (hitTestDetails == null) ? null : hitTestDetails.GetType();

            if (actualType != typeOfHitTestDetails)
            {
                throw SurfaceCoreFrameworkExceptions.HitTestDetailsMustBeTypeof(typeOfHitTestDetails, stateMachineType, hitTestDetails); 
            }

            TouchTargetEvent.HitTestDetails = hitTestDetails;

        }
    }
}
