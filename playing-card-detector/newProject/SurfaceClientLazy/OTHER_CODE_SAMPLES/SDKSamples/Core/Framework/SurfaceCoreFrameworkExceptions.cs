using System;
using System.Globalization;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    internal static class SurfaceCoreFrameworkExceptions
    {
        #region State Machine Exceptions

        internal static Exception HitTestDetailsMustBeTypeof(Type hitTestDetailsType, Type stateMachineType, CoreInteractionFramework.IHitTestDetails hitTestDetails)
        {
            //The IHitTestDetails supplied: {0} were not of type {1} as is required by {2}.
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.HitTestDetailsMustBeTypeofException, hitTestDetails, hitTestDetailsType, stateMachineType));  
        }

        #endregion

        internal static Exception UpdateCannotBeCalledDuringUpdate()
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.UpdateCannotBeCalledDuringUpdate));
        }

        internal static Exception MaximumQueueSizeReached(int maxQueueSize)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.MaximumQueueSizeReached, maxQueueSize));
        }

        internal static Exception CalledCapturedHitTestInformationForReleasedElement()
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.CalledCapturedHitTestInformationForReleasedElement));
        }

        internal static Exception CalledReleaseHitTestInformationForCapturedElement()
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.CalledReleaseHitTestInformationForCapturedElement));
        }

        internal static Exception ControllerSetToADifferentControllerException(CoreInteractionFramework.IInputElementStateMachine IInputElementStateMachine)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ControllerSetToADifferentController, IInputElementStateMachine));
        }

        internal static Exception ArgumentNullException(string argument)
        {
            return new ArgumentNullException(argument);
        }

        internal static Exception ArgumentOutOfRangeException(string argument)
        {
            return new ArgumentOutOfRangeException(argument);
        }

        internal static Exception InvalidOrientationArgumentException(string argument, Orientation orientation)
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ControllerSetToADifferentController, orientation, argument));
        }

        internal static Exception ItemIsAlreadyInCollection()
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ItemIsAlreadyInCollection));
        }

        internal static Exception ItemIsNotInListBoxItemsCollection(string collectionName)
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ItemNotInCollection, collectionName));
        }
    }
}
