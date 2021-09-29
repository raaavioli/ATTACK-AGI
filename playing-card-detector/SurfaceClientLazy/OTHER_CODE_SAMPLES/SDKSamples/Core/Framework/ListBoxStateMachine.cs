using System;
using System.Collections.Generic;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Maintains and manages state that is associated with an application-defined UI list box
    /// object. 
    /// </summary>
    /// <remarks>
    /// <para>Some aspects of <strong>ListBoxStateMachine</strong> state include:</para>
    /// <list type="bullet">
    /// <item>List box item state changes (<strong><see cref="GotItemStateChanged"/></strong>).</item>
    /// <item>Scroll bar state changes (<strong><see cref="IsScrolling"/></strong>).</item>
    /// <item>How list box items are selected (<strong><see cref="SelectionMode"/></strong>).</item>
    /// <item>The direction the scroll bar is oriented toward (<strong><see cref="Orientation"/></strong>).</item>
    /// </list>
    /// <para>Also state information from individual list box items is available from the
    /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItemCollection"/></strong> data member.</para>
    /// <note type="caution"> The Core Interaction Framework and API use the
    /// Model-View-Controller (MVC) design pattern. The API state machines
    /// represent the Model component of the MVC design pattern.</note>
    /// </remarks>
    public class ListBoxStateMachine : UIElementStateMachine
    {
        /// <summary>
        /// Keep track which touches have been added to the scroll adapter.
        /// </summary>
        private readonly List<int> scrollAdapterTouches = new List<int>();

        /// <summary>
        /// Used to track how much a touch has changed since it went down on the ListBox.
        /// </summary>
        private readonly Dictionary<int, float> touchChangeDeltaTouchIds = new Dictionary<int, float>();

        /// <summary>
        /// Tracks captured touch TouchTargetEvent id.
        /// This is use to track the position a touch is currently at.
        /// </summary>
        private readonly Dictionary<int, TouchTargetEvent> touchTargetEventTouchIds =
            new Dictionary<int, TouchTargetEvent>();

        /// <summary>
        /// Tracks captured touch ids to a particular item for which they are "captured".
        /// This is used because <strong>ListBoxStateMachineItem</strong> objects can't actually capture touches,
        /// but they track which touches are "logically" captured.
        /// </summary>
        private readonly Dictionary<int, ListBoxStateMachineItem> capturedItemTouchIds =
            new Dictionary<int, ListBoxStateMachineItem>();

        /// <summary>
        /// 1/8 of inch (based on 96 DPI), max drag distance to start scrolling and 'cancel' selection
        /// </summary>
        private const int DragDistanceLimit = 96 / 8;

        /// <summary>
        /// The current orientation of the ListBox
        /// </summary>
        private Orientation orientation = Orientation.Vertical;

        /// <summary>
        /// The current mode for selection of items.
        /// </summary>
        private SelectionMode selectionMode = SelectionMode.Default;

        /// <summary>
        /// Provides default scrolling behavior.
        /// </summary>
        private readonly ScrollAdapter scrollAdapter;

        /// <summary>
        /// The items which this ListBox currently contains.
        /// </summary>
        private readonly ListBoxStateMachineItemCollection items;

        /// <summary>
        /// The selected items in this ListBox.
        /// </summary>
        private readonly ListBoxStateMachineItemCollection selectedItems;

        /// <summary>
        /// Occurs when a list box item state has changed.
        /// </summary>
        /// <example>
        /// <para>
        ///  The following code example shows how to subscribe to the <strong>ItemStateChanged</strong> event.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs"
        ///  region="Initialize Listbox" title="Initialize Listbox" lang="cs" />
        /// </example>
        public event EventHandler<ListBoxStateMachineItemEventArgs> ItemStateChanged;

        /// <summary>
        /// Gets a Boolean value that indicates whether a
        /// <strong><see cref="ItemStateChanged"/></strong> event occurred in this update for a 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> in items.
        /// </summary>
        /// <remarks>Your application should check the <strong>GotItemStateChanged</strong> property 
        /// each time after <strong>UIController.Update</strong> is called if you are not using the
        /// <strong>ItemStateChanged</strong> event.
        /// </remarks>
        /// <returns><strong>true</strong> if a list box item state has changed within the current update cycle.
        /// </returns>
        /// <example>
        /// <para>
        ///  The following code example demonstrates <strong>GotItemStateChanged</strong> on the 
        ///  <strong><see cref="ListBoxStateMachine"/></strong>
        ///  and its related <strong>GotItemStateChanged</strong> properties.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs"
        ///  region="Listbox Item State" title="Listbox Item State" lang="cs" />
        /// </example>
        public bool GotItemStateChanged { get; internal set; }

        /// <summary>
        /// Occurs when any of the viewport properties change.
        /// </summary>
        public event EventHandler ViewportChanged;

        /// <summary>
        /// Gets a value that represents whether the viewport has changed.
        /// </summary>
        /// <returns><strong>true</strong> if any of the viewport properties have changed within the
        /// current update cycle (since the controller was last updated).</returns>
        /// <example>
        /// <para>
        ///  The following code example demonstrates the use of various 
        ///  <strong><see cref="ListBoxStateMachine"/></strong> properties,
        ///  including <strong>GotViewportChange</strong>.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs"
        ///  region="Listbox Item State" title="Listbox Item State" lang="cs" />
        /// </example>
        public bool GotViewportChange
        {
            get { return scrollAdapter.GotViewportChange; }
        }

        /// <summary>
        /// Creates an initialized instance of a
        /// <strong><see cref="ListBoxStateMachine"/></strong> object with the specified parameters.
        /// </summary>
        /// <param name="controller">The <strong>UIController</strong> object that dispatches hit testing.</param>
        /// <param name="numberOfPixelsInHorizontalAxis">
        /// The number of pixels that this control occupies horizontally.
        /// For more information, see <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInHorizontalAxis">
        /// NumberOfPixelsInHorizontalAxis</see></strong>.
        /// </param>
        /// <param name="numberOfPixelsInVerticalAxis">
        /// The number of pixels that this control occupies vertically.
        /// For more information, see <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInVerticalAxis">
        /// NumberOfPixelsInVerticalAxis</see></strong>.
        /// </param>
        public ListBoxStateMachine(UIController controller,
                                   int numberOfPixelsInHorizontalAxis,
                                   int numberOfPixelsInVerticalAxis)
            : base(controller, numberOfPixelsInHorizontalAxis, numberOfPixelsInVerticalAxis)
        {
            scrollAdapter = new ScrollAdapter(controller, this);
            scrollAdapter.ViewportChanged += OnScrollAdapterViewportChanged;

            ListBoxMode = ListBoxMode.Selection;

            // Create container objects.
            items = new ListBoxStateMachineItemCollection(this);
            selectedItems = new ListBoxStateMachineItemCollection(this);
            touchTargetEventTouchIds = new Dictionary<int, TouchTargetEvent>();
            capturedItemTouchIds = new Dictionary<int, ListBoxStateMachineItem>();

            items.ListBoxItemRemoved += OnListBoxItemRemoved;
            items.ListBoxItemAdded += OnListBoxItemAdded;

            selectedItems.ListBoxItemAdded += OnSelectedItemsListBoxItemAdded;
            selectedItems.ListBoxItemRemoved += OnSelectedItemsListBoxItemRemoved;
        }

        #region List Events

        /// <summary>
        /// Called when a ListBoxStateMachineItem is added to the selectedItems collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItemsListBoxItemAdded(object sender, ListBoxStateMachineItemEventArgs e)
        {
            if (items.Contains(e.Item))
            {
                if (selectionMode == SelectionMode.Single && selectedItems.Count > 1)
                {
                    if (!selectedItems.Remove(e.Item))
                    {
                        e.Item.IsSelected = false;
                    }
                }
                else
                {
                    e.Item.IsSelected = true;
                }
            }
            else
            {
                throw SurfaceCoreFrameworkExceptions.ItemIsNotInListBoxItemsCollection("Items");
            }
        }

        /// <summary>
        /// Called when a ListBoxStateMachineItem is removed to the selectedItems collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItemsListBoxItemRemoved(object sender, ListBoxStateMachineItemEventArgs e)
        {
            e.Item.IsSelected = false;
        }

        /// <summary>
        /// Called when a ListBoxStateMachineItem is added to the items collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListBoxItemAdded(object sender, ListBoxStateMachineItemEventArgs e)
        {
            if (e.Item != null)
            {
                e.Item.Parent = this;
                e.Item.ItemStateChanged += OnItemStateChanged;
                UpdateLayout();
            }
        }

        /// <summary>
        /// Called when a ListBoxStateMachineItem is removed from the items collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListBoxItemRemoved(object sender, ListBoxStateMachineItemEventArgs e)
        {
            if (e.Item != null)
            {
                e.Item.ResetItem();
                e.Item.ItemStateChanged -= OnItemStateChanged;
                UpdateLayout();
            }
        }

        /// <summary>
        /// Handles the ItemStateChanged event for all items in the list box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemStateChanged(object sender, ListBoxStateMachineItemEventArgs e)
        {
            OnItemStateChanged(e.Item);
        }

        /// <summary>
        /// Called from the <strong>OnItemStateChanged</strong> event handler to
        /// raise the <strong><see cref="ItemStateChanged"/></strong> event for clients of this 
        /// <strong><see cref="ListBoxStateMachine"/></strong> object.
        /// </summary>
        /// <param name="item">The item that changed state.</param>
        protected virtual void OnItemStateChanged(ListBoxStateMachineItem item)
        {
            GotItemStateChanged = true;
            EventHandler<ListBoxStateMachineItemEventArgs> temp = ItemStateChanged;

            if (temp != null)
            {
                temp(this, new ListBoxStateMachineItemEventArgs(item));
            }
        }

        #endregion

        /// <summary>
        /// Gets a value that represents the current <strong><see cref="CoreInteractionFramework.ListBoxMode"/></strong>
        /// mode based on the touches
        /// that are manipulating the list box.
        /// </summary>
        /// <returns>The current <strong>ListBoxMode</strong> enumeration value. The possible 
        /// values include <strong>Selection</strong> and <strong>Scrolling</strong>.</returns>
        /// <example>
        /// <para>
        ///  The following code example demonstrates the use of various <strong><see cref="CoreInteractionFramework.ListBoxStateMachine"/></strong> properties
        ///  including mode verification by using <strong>ListBoxMode</strong>.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs"
        ///  region="Listbox Item State" title="Listbox Item State" lang="cs" />
        /// </example>
        public ListBoxMode ListBoxMode { get; private set; }

        /// <summary>
        /// Gets a value that represents the collection of 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> objects
        /// in the <strong><see cref="ListBoxStateMachine"/></strong> state machine.
        /// </summary>
        /// <returns>The ListBoxStateMachine collection of items.</returns>
        /// <example>
        /// <para>
        ///  This example uses <stron>ListBoxStateMachineItemCollection</stron>
        ///  to validate state data for objects (weapons) associated with the ListBox items.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\WeaponSystems.cs"
        ///  region="Weapons Check" title="Weapons Check" lang="cs" />
        /// </example>
        public ListBoxStateMachineItemCollection Items
        {
            get { return items; }
        }

        /// <summary>
        /// Gets the collection of <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> objects
        /// in the <strong><see cref="ListBoxStateMachine"/></strong> object.
        /// </summary>
        /// <returns>The collection of selected list box items.</returns>
        /// <example>
        /// <para>
        ///  The following code example shows the <strong>SelectedItems.Count</strong> property used in a decision statement.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs"
        ///  region="Got Clicked Test" title="Got Clicked Test" lang="cs" />
        /// </example>
        public ListBoxStateMachineItemCollection SelectedItems
        {
            get { return selectedItems; }
        }

        /// <summary>
        /// Gets or sets a value that represents <strong><see cref="ListBoxStateMachine"/></strong> selection behavior. </summary>
        /// <remarks>The possible
        /// values include <strong>Single</strong> and <strong>Multiple</strong>. 
        /// <strong>SelectionMode.Single</strong> enables selecting
        /// only one list box item at a time, while <strong>SelectionMode.Multiple</strong> enables
        /// selecting all list box items.
        /// </remarks>
        /// <returns>The current <strong>SelectionMode</strong> enumeration value. </returns>
        /// <example>
        /// <para>
        ///  The following code example initializes some <strong>ListBoxStateMachine</strong> properties, including
        ///  changing the selection mode to <strong>Multiple</strong> (the default value is <strong>Single</strong>).
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\UI\ListBox.cs"
        ///  region="Initializing ListBox" title="Initializing ListBox" lang="cs" />
        /// </example>
        public SelectionMode SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
                selectionMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the orientation of items in the <strong><see cref="ListBoxStateMachine"/></strong> object.
        /// </summary>
        /// <returns>The current orientation enumeration value. The possible values include <strong>Vertical</strong>,
        /// <strong>Horizontal</strong>, and <strong>Both</strong>.</returns>
        /// <example>
        /// <para>
        ///  The following code example initializes some <strong>ListBoxStateMachine</strong> properties, including
        ///  changing <strong>Orientation</strong> to <strong>Vertical</strong>.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\UI\ListBox.cs"
        ///  region="Initializing ListBox" title="Initializing ListBox" lang="cs" />
        /// </example>
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                if (value == Orientation.Both)
                {
                    throw SurfaceCoreFrameworkExceptions.InvalidOrientationArgumentException("Orientation",
                                                                                              Orientation.Both);
                }
                orientation = value;
                LayoutItemsInExtentSpace();

                // When we change orientation, the ViewPortSize
                // in the non-scrolling direction should be 1f.
                if (orientation == Orientation.Horizontal)
                {
                    VerticalViewportSize = 1f;
                }
                else
                {
                    HorizontalViewportSize = 1f;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the number of horizontal pixels that the
        /// viewable list box occupies.
        /// <remarks>Changing the <strong>NumberOfPixelsInHorizontalAxis</strong> property causes the 
        /// size of the items in the
        /// list box to be recalculated so that they do not change when the size of the control
        /// changes.
        /// </remarks>
        /// </summary>
        /// <returns>The current number of pixels in the horizontal axis.</returns>
        /// <example>
        /// <para>
        ///  The following code example initializes <strong>NumberOfPixelsInHorizontalAxis</strong> and
        ///  <strong><see cref="NumberOfPixelsInVerticalAxis"/></strong>.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\UI\ScrollBar.cs"
        ///  region="Load ScrollBar Graphics" title="Load ScrollBar Graphics" lang="cs" />
        /// </example>
        public override int NumberOfPixelsInHorizontalAxis
        {
            get
            {
                return base.NumberOfPixelsInHorizontalAxis;
            }
            set
            {
                // Only update and notify if there has been a change.
                if (value == base.NumberOfPixelsInHorizontalAxis)
                {
                    return;
                }

                UpdateItemSize(value, base.NumberOfPixelsInHorizontalAxis, Orientation.Horizontal);
                base.NumberOfPixelsInHorizontalAxis = value;
                scrollAdapter.HorizontalScrollBarStateMachine.NumberOfPixelsInHorizontalAxis = value;

                if (Orientation == Orientation.Horizontal)
                {
                    UpdateLayout();                 
                }
            }
        }
        

        /// <summary>
        /// Gets or sets a value that represents the number of vertical pixels that the
        /// viewable list box occupies.
        /// <remarks>Changing the <strong>NumberOfPixelsInVerticalAxis</strong> property causes the 
        /// size of the items in the list box
        /// to be recalculated so that they do not change when the size of the control changes.
        /// </remarks>
        /// </summary>
        /// <returns>The current number of pixels in the horizontal axis.</returns>
        /// <example>
        /// <para>
        ///  The following code example initializes <strong><see cref="NumberOfPixelsInHorizontalAxis"/></strong> and
        ///  <strong>NumberOfPixelsInVerticalAxis</strong>.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\UI\ScrollBar.cs"
        ///  region="Load ScrollBar Graphics" title="Load ScrollBar Graphics" lang="cs" />
        /// </example>
        public override int NumberOfPixelsInVerticalAxis
        {
            get
            {
                return base.NumberOfPixelsInVerticalAxis;
            }
            set
            {
                // Only update and notify if there has been a change.
                if (value == base.NumberOfPixelsInVerticalAxis)
                {
                    return;
                }

                UpdateItemSize(value, base.NumberOfPixelsInVerticalAxis, Orientation.Vertical);
                base.NumberOfPixelsInVerticalAxis = value;
                scrollAdapter.VerticalScrollBarStateMachine.NumberOfPixelsInVerticalAxis = value;
                if (Orientation == Orientation.Vertical)
                {
                    UpdateLayout();
                }
            }
        }


        /// <summary>
        /// Gets the collection of 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> objects that
        /// are partially or completely visible.
        /// </summary>
        /// <returns>The list box items that are visible in the viewport.</returns>
        /// <example>
        /// <para>
        ///  In this code example, the <strong>GetVisibleItems</strong> method retrieves the
        ///  collection of visible list box items and iterates over them to determine their
        ///  respective selected states and take appropriate action.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs"
        ///  region="Button 2" title="Button 2" lang="cs" />
        /// </example>
        public ListBoxStateMachineItemCollection GetVisibleItems()
        {
            ListBoxStateMachineItemCollection visibleItems = new ListBoxStateMachineItemCollection(this);

            foreach (ListBoxStateMachineItem item in items)
            {
                if (item.IsVisible)
                {
                    visibleItems.Add(item);
                }
            }

            return visibleItems;
        }

        /// <summary>
        /// Gets the <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object 
        /// at the specified position in
        /// this <strong><see cref="ListBoxStateMachine"/></strong> object based on orientation.
        /// </summary>
        /// <param name="position">The normalized position that is relative to the viewport.</param>
        /// <returns>The <strong>ListBoxStateMachineItem</strong> that is hit.</returns>
        public ListBoxStateMachineItem HitTestWithin(float position)
        {
            if (Orientation == Orientation.Horizontal)
            {
                foreach (ListBoxStateMachineItem item in items)
                {
                    if (item.IsVisible)
                    {
                        if (position >= item.HorizontalStartPosition &&
                            position <= item.HorizontalStartPosition + item.HorizontalSize)
                        {
                            return item;
                        }
                    }
                }
            }
            else
            {
                foreach (ListBoxStateMachineItem item in items)
                {
                    if (item.IsVisible)
                    {
                        if (position >= item.VerticalStartPosition &&
                            position <= item.VerticalStartPosition + item.VerticalSize)
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a type that is equal to <strong><see cref="CoreInteractionFramework.ListBoxHitTestDetails"/></strong>
        /// that is
        /// the <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> type of a 
        /// <strong><see cref="ListBoxStateMachine"/></strong> object.
        /// </summary>
        /// <returns>A <strong>ListBoxHitTestDetails</strong> type.</returns>
        public override Type TypeOfHitTestDetails
        {
            get
            {
                return typeof(ListBoxHitTestDetails);
            }
        }

        #region ScrollAdapter Pass Through

        /// <summary>
        /// Gets or sets a value that represents the horizontal 
        /// <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> object.
        /// </summary>
        /// <returns>The current horizontal <strong>ScrollBarStateMachine</strong> object.</returns>
        public ScrollBarStateMachine HorizontalScrollBarStateMachine
        {
            get { return scrollAdapter.HorizontalScrollBarStateMachine; }
            set { scrollAdapter.HorizontalScrollBarStateMachine = value; }
        }

        /// <summary>
        /// Gets or sets the vertical <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> object.
        /// </summary>
        /// <returns>The current vertical <strong>ScrollBarStateMachine</strong> object.</returns>
        public ScrollBarStateMachine VerticalScrollBarStateMachine
        {
            get { return scrollAdapter.VerticalScrollBarStateMachine; }
            set { scrollAdapter.VerticalScrollBarStateMachine = value; }
        }

        /// <summary>
        /// Gets or sets a value that represents the left and right elastic margins.
        /// </summary>
        /// <returns>The current value of <strong>scrollAdapter.HorizontalElasticity</strong>.</returns>
        public float HorizontalElasticity
        {
            get { return scrollAdapter.HorizontalElasticity; }
            set { scrollAdapter.HorizontalElasticity = value; }
        }

        /// <summary>
        /// Gets or sets a value that represents the top and bottom elastic margins.
        /// </summary>
        /// <returns>The current value of <strong>scrollAdapter.VerticalElasticity</strong>.</returns>
        public float VerticalElasticity
        {
            get { return scrollAdapter.VerticalElasticity; }
            set { scrollAdapter.VerticalElasticity = value; }
        }

        /// <summary>
        /// Gets or sets a value that represents the horizontal normalized width of the viewport.
        /// </summary>
        /// <returns>The current value of <strong>scrollAdapter.HorizontalViewportSize</strong>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ViewportSize",
            Justification = "The intent is to use two words")]
        public float HorizontalViewportSize
        {
            get { return scrollAdapter.HorizontalViewportSize; }
            set { scrollAdapter.HorizontalViewportSize = value; }
        }

        /// <summary>
        /// Gets or sets the vertical normalized height of the viewport.
        /// </summary>
        /// <returns>The current value of <strong>scrollAdapter.VerticalViewportSize</strong>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ViewportSize",
            Justification = "The intent is to use two words")]
        public float VerticalViewportSize
        {
            get { return scrollAdapter.VerticalViewportSize; }
            set { scrollAdapter.VerticalViewportSize = value; }
        }

        /// <summary>
        /// Gets or sets a value that represents the starting normalized horizontal coordinate
        /// of the viewport.
        /// </summary>
        /// <returns>The current value of <strong>scrollAdapter.HorizontalViewportStartPosition</strong>.
        /// </returns>
        public float HorizontalViewportStartPosition
        {
            get { return scrollAdapter.HorizontalViewportStartPosition; }
            set
            {
               scrollAdapter.HorizontalViewportStartPosition = value;
               UpdateVisibleItems();
            }
        }

        /// <summary>
        /// Gets or sets the starting normalized vertical coordinate of
        /// the viewport.
        /// </summary>
        /// <returns>The value of <strong>scrollAdapter.VerticalViewportStartPosition</strong>.</returns>
        public float VerticalViewportStartPosition
        {
            get { return scrollAdapter.VerticalViewportStartPosition; }
            set
            {
                scrollAdapter.VerticalViewportStartPosition = value;
                UpdateVisibleItems();
            }
        }

        /// <summary>
        /// Gets a value that represents whether scrolling is occurring.
        /// </summary>
        /// <returns><strong>true</strong> if currently scrolling; otherwise, <strong>false</strong>.</returns>
        public bool IsScrolling
        {
            get { return scrollAdapter.IsScrolling; }
        }

        /// <summary>
        /// Scrolls the viewport to the specified values.
        /// </summary>
        /// <param name="viewportTopPosition">The y-coordinate to scroll to.</param>
        /// <param name="viewportLeftPosition">The x-coordinate to scroll to.</param>
        public void ScrollTo(float viewportTopPosition, float viewportLeftPosition)
        {
            scrollAdapter.ScrollTo(viewportTopPosition, viewportLeftPosition);
        }

        /// <summary>
        /// Causes the thumb to stop scrolling if it is currently being flicked.
        /// </summary>
        public void StopFlick()
        {
            scrollAdapter.StopFlick();
        }

        #endregion

        /// <summary>
        /// Called when any of the viewport properties change to 
        /// raise the <strong><see cref="ViewportChanged"/></strong> event.
        /// </summary>
        protected virtual void OnViewportChanged()
        {
            EventHandler viewportChanged = ViewportChanged;

            if (viewportChanged != null)
            {
                viewportChanged(this, EventArgs.Empty);
            }
        }

        #region UIController Events

        /// <summary>
        /// Handles <strong>TouchDown</strong> events.
        /// </summary>
        /// <param name="touchEvent">The list Box item touch that was added.</param>
        protected override void OnTouchDown(TouchTargetEvent touchEvent)
        {
            base.OnTouchDown(touchEvent);

            ListBoxMode = ListBoxMode.Selection;

            // Capture the touch to the ListBox
            Controller.Capture(touchEvent.Touch, this);

            // Get the item based on the touch details.
            ListBoxStateMachineItem item = GetItemHit(touchEvent.HitTestDetails as ListBoxHitTestDetails);

            // The item could be null if there are fewer items whose sizes sum to less than the size of the ListBox.
            if (item == null)
                return;

            // Put the touch on the listbox item.  Internal hit test to figure out which item it's over.
            capturedItemTouchIds.Add(touchEvent.Touch.Id, item);

            // Have the item process TouchDown.
            item.ProcessTouchDown(touchEvent.Touch.Id);

            // The item should change based on having a touch over it?
            // Store the position.
            touchTargetEventTouchIds.Add(touchEvent.Touch.Id, touchEvent);
        }

        /// <summary>
        /// Handles <strong>TouchUp</strong> events.
        /// </summary>
        /// <param name="touchEvent">The touch that is removed from the list box item.</param>
        protected override void OnTouchUp(TouchTargetEvent touchEvent)
        {
            if (TouchesCaptured.Contains(touchEvent.Touch.Id))
            {
                // Remove the items from tracking.
                if (capturedItemTouchIds.ContainsKey(touchEvent.Touch.Id))
                {
                    // Get the ListBoxItem this touch is "captured" to.
                    ListBoxStateMachineItem item = capturedItemTouchIds[touchEvent.Touch.Id];

                    if (ListBoxMode == ListBoxMode.Scrolling)
                    {
                        scrollAdapter.ProcessTouchUp(touchEvent);

                        // Set the pressed state to false for this item.
                        item.IsPressed = false;
                    }
                    else
                    {
                        // If in single selection mode set all the other SelectItems to unselected.
                        if (selectionMode == SelectionMode.Single)
                        {
                            for (int i = 0; i < SelectedItems.Count; i++)
                            {
                                if (SelectedItems[i] == item)
                                    continue;

                                SelectedItems[i].IsSelected = false;
                            }
                        }

                        // Only call process touch up when the touch is captured
                        // as it will cause the item to be select/deselected.
                        // Also item's IsPressed state will be cleared if scrolling begins.
                        item.ProcessCapturedTouchUp(touchEvent.Touch.Id);

                    }


                    // The touch is up so we don't need to track this item.
                    capturedItemTouchIds.Remove(touchEvent.Touch.Id);
                    touchTargetEventTouchIds.Remove(touchEvent.Touch.Id);
                    scrollAdapterTouches.Remove(touchEvent.Touch.Id);
                    touchChangeDeltaTouchIds.Remove(touchEvent.Touch.Id);
                }

                // Release the touch.
                Controller.Release(touchEvent.Touch);

                if (TouchesCaptured.Count == 0)
                {
                    ListBoxMode = ListBoxMode.Selection;
                }
            }

            // Select the item if in selection mode
            // Release capture on the touch.
            base.OnTouchUp(touchEvent);
        }

        /// <summary>
        /// Handles <strong>TouchMoved</strong> events.
        /// </summary>
        /// <param name="touchEvent">The list box item touch that changed.</param>
        protected override void OnTouchMoved(TouchTargetEvent touchEvent)
        {
            base.OnTouchMoved(touchEvent);

            if (ListBoxMode == ListBoxMode.Selection)
            {
                // Update the touch position and track total change.
                UpdateTouchPosition(touchEvent);

                // If the ListBox is in scroll mode then route this touch to the scrollAdapter.
                // If in selection mode check to see if the touch has moved 1/8"
                // and change to scrolling mode if it has.
                if (DidTouchMovePastThreshold(touchEvent.Touch.Id))
                {
                    ListBoxMode = ListBoxMode.Scrolling;

                    // Uncapture touches from items and send capture
                    // information to the ScrollViewer for every captured touches.
                    foreach (KeyValuePair<int, TouchTargetEvent> downTouch in touchTargetEventTouchIds)
                    {
                        // Get the ListBoxItem this touch is "captured" to.
                        ListBoxStateMachineItem item = capturedItemTouchIds[downTouch.Value.Touch.Id];

                        // Set the pressed state to false for this item.
                        item.IsPressed = false;
                        item.ChangeToScrolling();
                        item.capturedTouches.Clear();

                        if (!scrollAdapterTouches.Contains(downTouch.Key))
                        {
                            // Track which touch ids have been added to the scroll adapter.
                            scrollAdapterTouches.Add(downTouch.Key);

                            // Process each touch as a touch down.
                            scrollAdapter.ProcessTouchDown(downTouch.Value);
                        }
                    }
                }
            }
            else
            {
                // If the ListBox is scrollling then pass information to the scrollAdapter.
                scrollAdapter.ProcessTouchMoved(touchEvent);
            }
        }

        /// <summary>
        /// Reset the <strong><see cref="GotItemStateChanged"/></strong> state.
        /// </summary>
        /// <param name="sender">A <strong>UIController</strong> object.</param>
        /// <param name="e">Empty.</param>
        protected override void OnResetState(object sender, EventArgs e)
        {
            GotItemStateChanged = false;
            foreach (ListBoxStateMachineItem item in items)
            {
                item.ProcessResetState();
            }

            scrollAdapter.ProcessResetState(sender, e);
        }

        #endregion

        /// <summary>
        /// Informs the ListBox that an update has occured.
        /// </summary>
        internal void UpdateLayout()
        {
            LayoutItemsInExtentSpace();
        }

        /// <summary>
        /// Updates the size of the items so they don't change their render size.
        /// </summary>
        /// <param name="currentNumberOfPixelsInAxis"></param>
        /// <param name="newNumberOfPixelsInAxis"></param>
        /// <param name="orientationUpdated"></param>
        private void UpdateItemSize(int currentNumberOfPixelsInAxis,
                                    int newNumberOfPixelsInAxis,
                                    Orientation orientationUpdated)
        {
            if (currentNumberOfPixelsInAxis == 0 || items == null)
                return;

            float change = (float)newNumberOfPixelsInAxis / currentNumberOfPixelsInAxis;

            if (change == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                if (orientationUpdated == Orientation.Vertical)
                {
                    items[i].VerticalSize *= change;
                }
                else
                {
                    items[i].HorizontalSize *= change;
                }
            }
        }

        /// <summary>
        /// Lays out each StateMachineItem based on its size and position.
        /// </summary>
        private void LayoutItemsInExtentSpace()
        {
            if (Items == null) return;
            // Go through all of the items
            // based on orientation of the ListBox
            // set the start position of the item based on their size and position.
            // the positions are relative to the extent start not the
            // ListBox startPosition.

            // RenderSpace Always = 1 and starts at 0.  RenderSpace = NumberOfPixelsIn*Axis
            float itemPosition = 0;

            if (Orientation == Orientation.Vertical)
            {
                foreach (ListBoxStateMachineItem item in Items)
                {
                    item.VerticalExtentStartPosition = itemPosition;
                    itemPosition += item.VerticalSize;
                    item.IsVisible = false;
                }

                if (items.Count != 0)
                {
                    VerticalViewportSize = Math.Min(1f / GetVerticalExtentLength(), 1.0f);
                }
            }
            else
            {
                foreach (ListBoxStateMachineItem item in Items)
                {
                    item.HorizontalExtentStartPosition = itemPosition;
                    itemPosition += item.HorizontalSize;
                    item.IsVisible = false;
                }

                if (items.Count != 0)
                {
                    HorizontalViewportSize = Math.Min(1f / GetHorizontalExtentLength(), 1.0f);
                }
            }

            UpdateVisibleItems();
        }

        /// <summary>
        /// Calculates the total size of the extent in the horizontal axis.
        /// </summary>
        /// <returns></returns>
        private float GetHorizontalExtentLength()
        {
            if (items.Count == 0)
                return 0;

            return (items[items.Count - 1].HorizontalExtentStartPosition + items[items.Count - 1].HorizontalSize);
        }

        /// <summary>
        /// Calculates the total size of the extent in the vertical axis.
        /// </summary>
        /// <returns></returns>
        private float GetVerticalExtentLength()
        {
            if (items.Count == 0)
                return 0;

            return (items[items.Count - 1].VerticalExtentStartPosition + items[items.Count - 1].VerticalSize);
        }

        /// <summary>
        /// Based on the StateMachineItem.*ExtentStartPosition the
        /// visible items are determined and their StateMachineItem.*StartPosition property
        /// is set. Additionally an item's IsVisible property is set to true for all items
        /// that are currently in the Viewport.
        /// </summary>
        private void UpdateVisibleItems()
        {
            // The Viewport and the Extent are in the same space, but the viewport starts over at zero, so that
            // when rendering it's just an offset.  The ViewportSize is said to be 1 here as the items are based off
            // of that constent.

            if (Orientation == Orientation.Vertical)
            {
                float extentLength = GetVerticalExtentLength();

                foreach (ListBoxStateMachineItem item in Items)
                {
                    // Determine if any part of the item is in the viewport.
                    // Includes the case where the item is larger than the viewport.
                    // Is the Bottom of the item greater than (below) the start of the viewport?
                    // And is the Top of the item less than (above) the bottom of the viewport?
                   if (item.VerticalExtentStartPosition + item.VerticalSize > VerticalViewportStartPosition * extentLength &&
                        item.VerticalExtentStartPosition < VerticalViewportStartPosition * extentLength + 1)
                    {
                        item.VerticalStartPosition = item.VerticalExtentStartPosition - VerticalViewportStartPosition * extentLength;
                        item.IsVisible = true;
                    }
                    else
                    {
                        item.IsVisible = false;
                    }
                }
            }
            else
            {
                float extentLength = GetHorizontalExtentLength();

                foreach (ListBoxStateMachineItem item in Items)
                {
                    // Determine if any part of the item is in the viewport.
                    // Includes the case where the item is larger than the viewport.
                    // Is the Right edge of the item greater than the start of the viewport?
                    // And is the Left edge of the item less than the end of the viewport?
                    if (item.HorizontalExtentStartPosition + item.HorizontalSize > HorizontalViewportStartPosition * extentLength &&
                        item.HorizontalExtentStartPosition <  HorizontalViewportStartPosition * extentLength + 1)
                    {
                        item.HorizontalStartPosition = item.HorizontalExtentStartPosition - HorizontalViewportStartPosition*extentLength;
                        item.IsVisible = true;
                    }
                    else
                    {
                        item.IsVisible = false;
                    }
                }

            }
        }

        /// <summary>
        /// Gets the <strong>ListBoxStateMachineItem</strong> object based on the hit test information.
        /// </summary>
        /// <param name="listBoxHitTestDetails"></param>
        /// <returns></returns>
        private ListBoxStateMachineItem GetItemHit(ListBoxHitTestDetails listBoxHitTestDetails)
        {
            // Test the details based on the current orientation
            return HitTestWithin(Orientation == Orientation.Horizontal ?        // IsHorizontal?
                                    listBoxHitTestDetails.HorizontalPosition :  // Horizontal
                                    listBoxHitTestDetails.VerticalPosition);    // Vertical
        }

        /// <summary>
        /// Updates the position of the touch information in touchTargetEventTouchIds
        /// </summary>
        /// <param name="touchEvent"></param>
        private void UpdateTouchPosition(TouchTargetEvent touchEvent)
        {
            // Based on the down position in touchTargetEventTouchIds track how much this item has changed
            // and place it in touchChangeDeltaTouchIds
            int touchId = touchEvent.Touch.Id;

            if (!touchTargetEventTouchIds.ContainsKey(touchId))
            {
                return;
            }

            if (touchChangeDeltaTouchIds.ContainsKey(touchId))
            {
                ListBoxHitTestDetails originalDetails = (ListBoxHitTestDetails)touchTargetEventTouchIds[touchId].HitTestDetails;
                ListBoxHitTestDetails currentDetails = touchEvent.HitTestDetails as ListBoxHitTestDetails;

                if (currentDetails == null)
                {
                    return;
                }

                float original = 0f, current = 0f;

                if (Orientation == Orientation.Horizontal)
                {
                    current = currentDetails.HorizontalPosition;
                    original = originalDetails.HorizontalPosition;
                }
                else
                {
                    current = currentDetails.VerticalPosition;
                    original = originalDetails.VerticalPosition;
                }

                // Get the total distance this touch has moved since it went down.
                touchChangeDeltaTouchIds[touchId] = current - original;
            }
            else
            {
                touchChangeDeltaTouchIds.Add(touchId, 0f);
            }
        }

        /// <summary>
        /// Tests the touch position change to see if its past the threshold of 1/8".
        /// </summary>
        /// <param name="touchId"></param>
        /// <returns></returns>
        private bool DidTouchMovePastThreshold(int touchId)
        {
            if (touchChangeDeltaTouchIds.ContainsKey(touchId))
            {
                float delta = touchChangeDeltaTouchIds[touchId];

                // Check touchChangeDeltaTouchIds to see if the value is more than 1/8" in screen space.
                if (Math.Abs(GetScreenSpace(delta)) > DragDistanceLimit)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the number of pixels for the value specified.
        /// </summary>
        /// <param name="value">Value is in 0-to-1 extent space.</param>
        /// <returns></returns>
        private int GetScreenSpace(float value)
        {
            if (Orientation == Orientation.Horizontal)
            {
                return (int)(value * NumberOfPixelsInHorizontalAxis);
            }
            else
            {
                return (int)(value * NumberOfPixelsInVerticalAxis);
            }
        }

        /// <summary>
        /// When the viewport changes update the layout of the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollAdapterViewportChanged(object sender, EventArgs e)
        {
            UpdateVisibleItems();
            OnViewportChanged();
        }
    }
}
