using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Represents a list box item state. 
    /// </summary>
    /// <remarks><para>
    /// The <strong>ListBoxStateMachineItem</strong> class does not derive from the 
    /// <strong><see cref="CoreInteractionFramework.UIElementStateMachine"/></strong> and consequently 
    /// cannot be hit tested. All hit testing is provided by the 
    /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> 
    /// object that instantiates this class. The main purpose of this class is to provide state 
    /// information such as whether a list box item is selected or not.
    /// </para></remarks>
    public class ListBoxStateMachineItem
    {
        private ListBoxStateMachine parent; 

        private bool gotItemStateChanged;
        private bool isSelected;

        private float horizontalSize;
        private float verticalSize;
        private float verticalExtentStartPosition;
        private float horizontalExtentStartPosition;
        private float verticalStartPosition;
        private float horizontalStartPosition;

        private bool isInItemStateChanged;


        internal List<int> capturedTouches = new List<int>();

        /// <summary>
        /// Triggered when a ListBoxItemModel ItemStateChange occurs. Events in the Core are
        /// triggered only after the UIController.Update method is called and before the 
        /// method call returns.
        /// </summary>
        public event EventHandler<ListBoxStateMachineItemEventArgs> ItemStateChanged;
        
        /// <summary>
        /// Creates an element for containment in a 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> object
        /// with the specified parameters.
        /// </summary>
        /// <param name="horizontalSize">The width of the item in list box space. 
        /// The size can be larger than 1 if the item size is larger than the list box's size.
        /// </param>
        /// <param name="verticalSize">The height of the item in list box space. The size can 
        /// be larger than 1 if the item size is larger than the list box's size.
        /// </param>
        public ListBoxStateMachineItem(float horizontalSize, float verticalSize)
        {
            if (!IsFloatValidAndPositive(horizontalSize))
            {
                throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("horizontalSize");
            }

            if (!IsFloatValidAndPositive(verticalSize))
            {
                throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("verticalSize");
            }

            HorizontalSize = horizontalSize;
            VerticalSize = verticalSize;
        }

        private static bool IsFloatValid(float value)
        {
            return !(float.IsInfinity(value) || float.IsNaN(value));
        }

        private static bool IsFloatValidAndPositive(float value)
        {
            return !(float.IsInfinity(value) || float.IsNaN(value) || value < 0);
        }

        /// <summary>
        /// Creates an element for containment in a 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> object
        /// with the specified parameters.
        /// </summary>
        /// <param name="tag">An object bound to this item.</param>
        /// <param name="horizontalSize">The width of the item in list box space. 
        /// The size can be larger than 1 if the item size is larger than the list box's size.
        /// </param>
        /// <param name="verticalSize">The height of the item in list box space. 
        /// The size can be larger than 1 if the item size is larger than the list box's size.
        /// </param>
        public ListBoxStateMachineItem(object tag, float horizontalSize, float verticalSize)
            : this(horizontalSize, verticalSize)
        {
            Tag = tag;
        }

        /// <summary>
        /// Gets or sets a system object that is used to hold data for this <strong><see cref="ListBoxStateMachineItem"/></strong> object.
        /// </summary>
        /// <returns>The data object that is stored with this <strong>ListBoxStateMachineItem</strong> object.</returns>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the value that indicates visibility of this 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object. </summary>
        /// <remarks>An item is not visible if it is outside 
        /// of the viewport. This visibility changes if the item is scrolled into view.
        /// </remarks>
        /// <returns><strong>true</strong> if this item is currently in viewport of the list box.
        /// </returns>
        public bool IsVisible { get; internal set; }

        /// <summary>
        /// Gets a Boolean value that indicates if this <strong><see cref="ListBoxStateMachineItem"/></strong> 
        /// object is pressed.
        /// </summary>
        /// <returns><strong>true</strong> if this <strong>ListBoxStateMachineItem</strong> object is currently pressed; otherwise, <strong>false</strong>.</returns>
        public bool IsPressed { get; internal set; }

        /// <summary>
        /// Gets a value that determines if this <strong><see cref="ListBoxStateMachineItem"/></strong> object is 
        /// currently scrolling (based on if the <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> parent is scrolling).
        /// </summary>
        /// <returns><strong>true</strong> if the parent <strong>ListBoxStateMachine</strong> object is scrolling.</returns>
        public bool IsScrolling
        {
            get
            {
                if (parent == null)
                {
                    throw SurfaceCoreFrameworkExceptions.ItemIsNotInListBoxItemsCollection("Items");
                }

                return parent.IsScrolling;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether this 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object is selected.
        /// </summary>
        /// <returns><strong>true</strong> if this <strong>ListBoxStateMachineItem</strong> object is currently selected.</returns>
        public bool IsSelected
        {
            get
            {
                if (parent == null)
                {
                    throw SurfaceCoreFrameworkExceptions.ItemIsNotInListBoxItemsCollection("Items");
                }

                return isSelected;
            }
            set
            {
                if (parent == null)
                {
                    throw SurfaceCoreFrameworkExceptions.ItemIsNotInListBoxItemsCollection("Items");
                }

                // If the value is the same then don't send updates.
                if (isSelected == value)
                    return;

                isSelected = value;

                if (isSelected)
                {
                    if (!parent.SelectedItems.Contains(this))
                    {
                        parent.SelectedItems.Add(this);
                    }
                }
                else
                {
                    if (parent.SelectedItems.Contains(this))
                    {
                        parent.SelectedItems.Remove(this);
                    }
                }

                OnItemStateChanged();
            }

        }

        /// <summary>
        /// Gets or sets a value that indicates whether an <strong><see cref="ItemStateChanged"/></strong> event
        /// occurred in this update cycle. 
        /// </summary>
        /// <remarks>If the application is not using the <strong>ItemStateChanged</strong> 
        /// event, the application should check the <strong>GotItemStateChanged</strong> property after each call to the 
        /// <strong>Update</strong> method of <strong>UIController</strong>.</remarks>
        /// <return><strong>true</strong> if state changed for <strong><see cref="ListBoxStateMachineItem"/></strong> in the update cycle.</return>
        /// <example>
        /// <para>
        ///  The following code example demonstrates the use of various <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> properties
        ///  including <strong>GotItemStateChanged</strong>.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs" 
        ///  region="Listbox Item State" title="Listbox Item State" lang="cs" />
        /// </example>
        public bool GotItemStateChanged
        {
            get { return gotItemStateChanged; }
            internal set { gotItemStateChanged = value; }
        }

        /// <summary>
        /// Gets or sets the horizontal size value of this 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object, relative to the parent object viewport.
        /// </summary>
        /// <returns>The horizontal size of this <strong>ListBoxStateMachineItem</strong> object.</returns>
        public float HorizontalSize
        {
            get { return horizontalSize; }

            set
            {
                if (!IsFloatValidAndPositive(value))
                {
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("HorizontalSize");
                }

                horizontalSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical size value of this 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object, relative to the parent object viewport.
        /// </summary>
        /// <returns>The vertical size of this <strong>ListBoxStateMachineItem</strong> object.</returns>
        public float VerticalSize
        {
            get { return verticalSize; }
            set
            {
                if (!IsFloatValidAndPositive(value))
                {
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("VerticalSize");
                }

                verticalSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting vertical position of the 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object, relative to the extent.
        /// </summary>
        /// <returns>The starting vertical position of the <strong>ListBoxStateMachineItem</strong> object.</returns>
        internal float VerticalExtentStartPosition
        {
            get { return verticalExtentStartPosition; }
            set
            {
                Debug.Assert(IsFloatValid(value), "Argument out of range VerticalExtentStartPosition");
                verticalExtentStartPosition = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting horizontal position of the 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object, relative to the extent.
        /// </summary>
        /// <returns>The starting horizontal position of the <strong>ListBoxStateMachineItem</strong> object.</returns>
        internal float HorizontalExtentStartPosition
        {
            get { return horizontalExtentStartPosition; }
            set
            {
                Debug.Assert(IsFloatValid(value), "Argument out of range HorizontalExtentStartPosition");
                horizontalExtentStartPosition = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting vertical position of the 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object, relative to the parent list box.
        /// </summary>
        /// <remarks>The <strong>VerticalStartPosition</strong> property is valid only when the item is visible.</remarks>
        /// <returns>The starting vertical position of the <strong>ListBoxStateMachineItem</strong> object.
        /// </returns>
        /// <exception cref="InvalidOperationException">Invalid operation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Argument out of range.</exception>
        public float VerticalStartPosition
        {
            get
            {
                if (IsVisible)
                {
                     return verticalStartPosition;               
                }
                throw new InvalidOperationException("VerticalStartPosition is only valid when item is visible.");
            }
            internal set
            {
                if (!IsFloatValid(value))
                {
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("VerticalStartPosition");
                }

                verticalStartPosition = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting horizontal position of the 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object, relative to the parent list box.
        /// </summary>
        /// <remarks>The <strong>HorizontalStartPosition</strong> property is valid only when the item is visible.</remarks>
        /// <returns>The starting horizontal position of the <strong>ListBoxStateMachineItem</strong> object.
        /// </returns>
        /// <exception cref="InvalidOperationException">Invalid operation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Argument out of range.</exception>
        public float HorizontalStartPosition
        {
            get
            {
                if (IsVisible)
                {
                    return horizontalStartPosition;
                }
                throw new InvalidOperationException("HorizontalStartPosition is only valid when item is visible.");
            }
            internal set
            {
                if (!IsFloatValid(value))
                {
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("HorizontalStartPosition");
                }

                horizontalStartPosition = value;
            }
        }

        /// <summary>
        /// Clears an item.
        /// </summary>
        internal void ResetItem()
        {
            IsPressed = false;
            IsSelected = false;
            IsVisible = false;
            parent = null;
        }

        /// <summary>
        /// Gets the parent <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> object of this 
        /// <strong><see cref="ListBoxStateMachineItem"/></strong> object.
        /// </summary>
        /// <return>The parent <strong>ListBoxStateMachine</strong> object.</return>
        public ListBoxStateMachine Parent
        {
            get
            {
                return parent;
            }
            internal set
            {
                if (value == null && parent != null)
                {
                    ResetItem();
                    return;
                }
                parent = value;
            }
        }

        /// <summary>
        /// Called when this list box item's <strong><see cref="IsSelected"/></strong> state is changing.
        /// </summary>
        protected virtual void OnItemStateChanged()
        {
            // Don't allow the item to be updated when in a call to updating the item.
            if (isInItemStateChanged)
            {
                return;
            }

            isInItemStateChanged = true;

            gotItemStateChanged = true;
            EventHandler<ListBoxStateMachineItemEventArgs> temp = ItemStateChanged;

            if (temp != null)
            {
                temp(Parent, new ListBoxStateMachineItemEventArgs(this));
            }

            isInItemStateChanged = false;

        }

        /// <summary>
        /// The ListBoxStateMachine controls if it is scrolling and so it needs to inform
        /// the item that it has changed state.
        /// </summary>
        internal void ChangeToScrolling()
        {
            OnItemStateChanged();
        }

        /// <summary>
        /// Handles TouchDown events.
        /// </summary>
        internal void ProcessTouchDown(int id)
        {
            capturedTouches.Add(id);

            if (!IsPressed)
            {
                IsPressed = true;

                OnItemStateChanged();
            }
        }

        /// <summary>
        /// Handles TouchUp Events for touches which are captured.
        /// </summary>
        internal void ProcessCapturedTouchUp(int id)
        {
            capturedTouches.Remove(id);
            

            if (capturedTouches.Count == 0)
            {
                IsPressed = false;
                bool signalStateChange = true;

                if (IsSelected)
                {
                    if (parent.SelectionMode == SelectionMode.Multiple)
                    {
                        IsSelected = false;
                        signalStateChange = false;  // IsSelected will raise the event.
                    }
                }
                else
                {
                    IsSelected = true;
                    signalStateChange = false;  // IsSelected will raise the event.
                }

                if (signalStateChange)
                {
                    OnItemStateChanged();
                }
            }
        }

        /// <summary>
        /// Reset the GotItemStateChanged state.
        /// </summary>
        internal void ProcessResetState()
        {
            gotItemStateChanged = false;
        }
    }
}
