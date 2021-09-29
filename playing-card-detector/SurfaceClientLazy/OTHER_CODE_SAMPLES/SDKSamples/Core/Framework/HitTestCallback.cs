namespace CoreInteractionFramework
{
    /// <summary>
    /// Defines a delegate for the method that is called to determine if touches collide with UI elements.  
    /// </summary>
    /// <remarks>The parameters include two collections:
    /// <list type="bullet">
    /// <item>The first parameter is a 
    /// <strong>ReadOnlyHitTestResultCollection</strong> collection of elements that contain 
    /// touches that are not 
    /// captured by an <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> state machine. 
    /// The delegate implementation should hit 
    /// test each touch element of the <strong>ReadOnlyHitTestResultCollection</strong> collection and assign the 
    /// <strong>IInputElementStateMachine</strong> instance that the touch hit. If the touch 
    /// did not hit anything, the <strong>IInputElementStateMachine</strong> instance should be set to null.</item>
    /// <item>The second parameter is a <strong>ReadOnlyHitTestResultCollection</strong> collection of elements 
    /// that contain touches that an <strong>IInputElementStateMachine</strong> captured. The <strong>Touch</strong> property 
    /// of the <strong><see cref="CoreInteractionFramework.HitTestResult"/></strong> is initialized with the 
    /// <strong>IInputElementStateMachine</strong> instance that
    /// captured it. The delegate implementation should test each touch to determine if 
    /// it hit. Those touches that did not hit should be set to null.</item>
    /// </list>
    /// 
    /// 
    /// 
    /// </remarks>
    /// <param name="uncapturedTouchEventsToHitTest">A paired list of which touches hit 
    /// which <strong>IInputElementStateMachine</strong> instances.</param>
    /// <param name="capturedTouchEventsToHitTest">A paired list of which captured touches 
    /// hit which <strong>IInputElementStateMachine</strong> instances.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public delegate void HitTestCallback(ReadOnlyHitTestResultCollection uncapturedTouchEventsToHitTest, ReadOnlyHitTestResultCollection capturedTouchEventsToHitTest);
}
