using System.Collections;
using System.Collections.Generic;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Specifies a read-only dictionary of touches to 
    /// <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> objects.
    /// </summary>
    /// <example>
    /// <para>The following code example shows captured touch and uncaptured touch 
    /// versions of <strong>ReadOnlyHitTestResultCollection</strong> in a 
    ///  <strong><see cref="CoreInteractionFramework.HitTestCallback"/></strong> 
    /// implementation for <strong><see cref="CoreInteractionFramework.ButtonStateMachine"/></strong>.
    /// </para>
    /// <note type="caution"> <strong>ButtonStateMachine</strong> does not require hit test details.
    /// </note>
    ///  <code source="Core\Framework\StarshipArsenal\UI\ButtonControl.cs" 
    ///  region="Button Hit Test" title="Button Hit Test" lang="cs" />
    /// </example>
    public class ReadOnlyHitTestResultCollection : IEnumerable<HitTestResult>
    {
        List<HitTestResult> hitTestResults;

        internal ReadOnlyHitTestResultCollection(Dictionary<TouchTargetEvent, IInputElementStateMachine> dict)
        {
            this.hitTestResults = new List<HitTestResult>();

            foreach (KeyValuePair<TouchTargetEvent, IInputElementStateMachine> dictItem in dict)
            {
                HitTestResult htr = new HitTestResult(dictItem.Key, dictItem.Value);

                this.hitTestResults.Add(htr);
            }
        }

        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator that checks all possible 
        /// <strong><see cref="CoreInteractionFramework.TouchTargetEvent"/></strong> objects in this 
        /// <strong><see cref="ReadOnlyHitTestResultCollection"/></strong>.
        /// </summary>
        /// <returns>The enumerator for this hit test collection.</returns>
        public IEnumerator<HitTestResult> GetEnumerator()
        {
            return this.hitTestResults.GetEnumerator();           
        }

        /// <summary>
        /// Gets a value that represents a 
        /// <strong><see cref="CoreInteractionFramework.HitTestResult"/></strong> object
        /// that is referenced by the
        /// <paramref name="index"/> parameter.
        /// </summary>
        /// <param name="index">The <strong>HitTestResult</strong> position in the collection.
        /// </param>
        /// <returns>The <strong>HitTestResult</strong> object at the specified index.</returns>
        public HitTestResult this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw SurfaceCoreFrameworkExceptions.ArgumentOutOfRangeException("index");
                }

                return this.hitTestResults[index];                
            }
        }

        /// <summary>
        /// Gets the number of items in its 
        /// <strong><see cref="ReadOnlyHitTestResultCollection"/></strong> collection.
        /// </summary>
        /// <returns>The number of items in this collection.</returns>
        public int Count
        {
            get
            {
                return this.hitTestResults.Count;
            }
        }

        #endregion


        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator to iterate over this collection.
        /// </summary>
        /// <returns>An enumerator value.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)(this.hitTestResults)).GetEnumerator();
        }

        #endregion
    }
}
