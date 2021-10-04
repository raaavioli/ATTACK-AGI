using System;
using System.Collections.Generic;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Maintains a list of <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> objects.
    /// </summary>
    public class ListBoxStateMachineItemCollection : ICollection<ListBoxStateMachineItem>, IEnumerable<ListBoxStateMachineItem>
    {
        /// <summary>
        /// Occurs when a <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object is removed from the collection.
        /// </summary>
        public event EventHandler<ListBoxStateMachineItemEventArgs> ListBoxItemRemoved;

        /// <summary>
        /// Occurs when a <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object is added to the collection.
        /// </summary>
        public event EventHandler<ListBoxStateMachineItemEventArgs> ListBoxItemAdded; 

        readonly private List<ListBoxStateMachineItem> list;

        // The listbox which owns this collection.
        readonly private ListBoxStateMachine owner;

        internal ListBoxStateMachineItemCollection(ListBoxStateMachine owner)
        {
            this.owner = owner;
            list = new List<ListBoxStateMachineItem>();
        }

        #region ICollection<ListBoxItemState> Members

        /// <summary>
        /// Adds and item to the collection.
        /// </summary>
        /// <param name="item">The item to add to the collection.</param>
        /// <example>
        /// <para>
        ///  The following code example loads game content elements, including 
        ///  <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> objects by using
        ///  the <strong>Add</strong> method.
        /// </para>
        ///  <code source="Core\Framework\StarshipArsenal\MainGameFrame.cs" 
        ///  region="Load Content" title="Load Content" lang="cs" />
        /// </example>
        public void Add(ListBoxStateMachineItem item)
        {
            if (item == null)
            {
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("item");
            }

            if ((item.Parent != null && item.Parent != owner) || list.Contains(item))
            {
                throw SurfaceCoreFrameworkExceptions.ItemIsAlreadyInCollection();
            }

            list.Add(item);

            OnListBoxItemAdded(item);
        }

        /// <summary>
        /// Clears all the items in the collection.
        /// </summary>
        public void Clear()
        {
            foreach (ListBoxStateMachineItem item in list)
            {
                OnListBoxItemRemoved(item);
            }

            list.Clear();
            owner.UpdateLayout();
        }

        /// <summary>
        /// Gets the <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object at the specified position on 
        /// this collection.  
        /// </summary>
        /// <param name="index">The index location within the collection of the item to retrieve.</param>
        /// <returns>The <strong>ListBoxStateMachineItem</strong> at the specified index.</returns>
        public ListBoxStateMachineItem this[int index]
        {
            get
            {
                return list[index];
            }
        }

        /// <summary>
        /// Gets a Boolean value that indicates if the 
        /// <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong>
        /// is 
        /// in the collection.
        /// </summary>
        /// <param name="item">The item to look for in the collection.</param>
        /// <returns><strong>true</strong> if the item is in the collection.</returns>
        public bool Contains(ListBoxStateMachineItem item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// Copies all of the <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> objects
        /// in this collection from the specified list 
        /// position to the end of 
        /// the collection.
        /// </summary>
        /// <param name="array">The destination to copy this list box collection to.</param>
        /// <param name="arrayIndex">The index of this collection to begin copying from.</param>
        public void CopyTo(ListBoxStateMachineItem[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> elements 
        /// in this collection.
        /// </summary>
        /// <returns>The number of elements in the collection.</returns>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        /// Gets a Boolean value that indicates whether the <strong><see cref="ListBoxStateMachineItemCollection"/></strong> 
        /// object is read-only. 
        /// </summary>
        /// <remarks><note type="caution"> This property is always false.</note></remarks>
        /// <returns>Always <strong>false</strong>.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object
        /// from 
        /// the list.
        /// </summary>
        /// <param name="item">The item to remove from the collection.</param>
        /// <returns><strong>true</strong> if the item is removed successfully.</returns>
        public bool Remove(ListBoxStateMachineItem item)
        {
            if (item == null)
            {
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("item");
            }

            if (list.Remove(item))
            {
                OnListBoxItemRemoved(item);
                return true;
            }

            return false;
        }

        private void OnListBoxItemRemoved(ListBoxStateMachineItem item)
        {
            EventHandler<ListBoxStateMachineItemEventArgs> temp = ListBoxItemRemoved;

            if (temp != null)
            {
                temp(this, new ListBoxStateMachineItemEventArgs(item));
            }
        }

        private void OnListBoxItemAdded(ListBoxStateMachineItem item)
        {
            EventHandler<ListBoxStateMachineItemEventArgs> temp = ListBoxItemAdded;

            if (temp != null)
            {
                temp(this, new ListBoxStateMachineItemEventArgs(item));
            }
        }

        #endregion

        #region IEnumerable<ListBoxItemState> Members

        /// <summary>
        /// Gets an enumerator for the <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object that iterates 
        /// over the collection.
        /// </summary>
        /// <returns>A <strong>ListBoxStateMachineItem</strong> enumerator.</returns>
        public IEnumerator<ListBoxStateMachineItem> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator for the <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object that iterates through the 
        /// collection.
        /// </summary>
        /// <returns>A <strong>System.IEnumerator</strong>.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable<ListBoxItemState> Members

        /// <summary>
        /// Gets an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <strong>System.IEnumerator</strong>.</returns>
        IEnumerator<ListBoxStateMachineItem> IEnumerable<ListBoxStateMachineItem>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion


    }
}
