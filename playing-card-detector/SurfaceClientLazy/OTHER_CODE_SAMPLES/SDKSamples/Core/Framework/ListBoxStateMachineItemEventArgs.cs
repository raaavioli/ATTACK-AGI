using System;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Specifies event details for list box item events.
    /// </summary>
    public class ListBoxStateMachineItemEventArgs : EventArgs
    {
        private readonly ListBoxStateMachineItem item;

        internal ListBoxStateMachineItemEventArgs(ListBoxStateMachineItem listBoxItemStateMachine)
        {
            this.item = listBoxItemStateMachine;
        }

        /// <summary>
        /// Gets the <strong><see cref="CoreInteractionFramework.ListBoxStateMachineItem"/></strong> object.
        /// </summary>
        public ListBoxStateMachineItem Item
        {
            get { return item; }
        }

    }
}
