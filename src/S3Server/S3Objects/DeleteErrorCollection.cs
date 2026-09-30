namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// XML serialization surface for the Error entries of a DeleteResult.
    /// Consumers should use DeleteResult.Errors rather than this type directly.
    /// When bound to a DeleteResult, the collection is a snapshot for serialization, and the Error wrapped by every item
    /// added to it (as XmlSerializer does during deserialization) is also added to the owner's Errors list.
    /// Not thread-safe.
    /// </summary>
    public class DeleteErrorCollection : Collection<DeleteError>
    {
        #region Private-Members

        private DeleteResult _Owner = null;
        private bool _Loading = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate an unbound collection.
        /// </summary>
        public DeleteErrorCollection()
        {

        }

        /// <summary>
        /// Instantiate a collection bound to a DeleteResult.
        /// </summary>
        /// <param name="owner">Owning result.  Cannot be null.</param>
        /// <param name="errors">Errors to serialize, in order.  Null is treated as empty, and null entries are skipped.</param>
        /// <exception cref="ArgumentNullException">Thrown if owner is null.</exception>
        public DeleteErrorCollection(DeleteResult owner, IEnumerable<Error> errors)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            _Loading = true;

            if (errors != null)
            {
                foreach (Error error in errors)
                {
                    if (error != null) Add(new DeleteError(error));
                }
            }

            _Loading = false;
            _Owner = owner;
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Insert an item, routing its Error to the owning result when bound.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="item">Item.</param>
        protected override void InsertItem(int index, DeleteError item)
        {
            base.InsertItem(index, item);

            if (_Loading || _Owner == null || item == null) return;

            _Owner.Errors.Add(item.Error);
        }

        #endregion
    }
}
