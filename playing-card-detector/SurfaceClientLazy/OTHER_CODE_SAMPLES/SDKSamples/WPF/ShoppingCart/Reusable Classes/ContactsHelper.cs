using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Input;

namespace ShoppingCart
{

    /// <summary>
    /// A helper class that provides extra data for a given input device:
    ///  - delta between the current position,
    ///  - an element the intput device was placed at.
    /// </summary>
    public static class InputDeviceHelper
    {
        private static readonly Vector ZeroVector = new Vector(0, 0);
        private static Dictionary<InputDevice, DragState> deviceStateDictionary = new Dictionary<InputDevice, DragState>();

        // internal state for the input down
        private struct DragState
        {
            public DragState(Point position, DependencyObject source)
            {
                this.Position = position;
                this.Source = source;
            }

            // position relative to the window where the input device was placed at
            public Point Position;

            // the element the input device was placed at
            public DependencyObject Source;
        }

        /// <summary>
        /// Returns delta between current input device position and where the input device was placed
        /// relative to the given elementToDrag.
        /// </summary>
        /// <param name="inputDevice"></param>
        /// <param name="relativeTo"></param>
        /// <returns></returns>
        public static Vector DraggedDelta(InputDevice inputDevice, UIElement relativeTo)
        {
            if (inputDevice == null)
            {
                throw new ArgumentNullException("inputDevice");
            }

            if (relativeTo == null)
            {
                throw new ArgumentNullException("relativeTo");
            }

            Window window = inputDevice.ActiveSource.RootVisual as Window;
            if (window != null)
            {
                return DraggedDelta(inputDevice, window, relativeTo);
            }

            return ZeroVector;
        }


        /// <summary>
        /// Returns the Source the input device was placed at.
        /// </summary>
        public static DependencyObject GetDragSource(InputDevice inputDevice)
        {
            if (inputDevice == null)
            {
                throw new ArgumentNullException("inputDevice");
            }

            if (deviceStateDictionary.ContainsKey(inputDevice))
            {
                return deviceStateDictionary[inputDevice].Source;
            }

            return null;
        }

        private static Vector DraggedDelta(InputDevice inputDevice, Window window, UIElement relativeTo)
        {
            // get the current position
            Point currentPosition = inputDevice.GetPosition(window);

            // get the down state
            if (!deviceStateDictionary.ContainsKey(inputDevice))
            {
                return ZeroVector;
            }

            // translate to the relativeTo elementToDrag
            Point downPosition = deviceStateDictionary[inputDevice].Position;

            if (relativeTo != window)
            {
                currentPosition = window.TranslatePoint(currentPosition, relativeTo);
                downPosition = window.TranslatePoint(downPosition, relativeTo);
            }

            return (currentPosition - downPosition);
        }

        public static void InitializeDeviceState(InputDevice inputDevice)
        {
            if (inputDevice == null || deviceStateDictionary.ContainsKey(inputDevice))
            {
                return;
            }

            Window window = inputDevice.ActiveSource.RootVisual as Window;
            if (window == null)
            {
                return;
            }

            Point position = inputDevice.GetPosition(window);

            DependencyObject directlyOver = inputDevice.GetDirectlyOver() as DependencyObject;

            deviceStateDictionary.Add(inputDevice, new DragState(position, directlyOver));
        }

        public static void ClearDeviceState(InputDevice device)
        {
            if (device == null)
            {
                return;
            }

            deviceStateDictionary.Remove(device);
        }
    }
}
