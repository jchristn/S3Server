namespace S3ServerLibrary.S3Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// XML serialization surface for the interleaved Version and DeleteMarker elements of a ListVersionsResult.
    /// Consumers should use ListVersionsResult.Entries, Versions, and DeleteMarkers rather than this type directly.
    /// When bound to a ListVersionsResult, the collection is a snapshot for serialization, and every item added to it
    /// (as XmlSerializer does during deserialization) is also added to the owner's Entries list and to its Versions
    /// or DeleteMarkers list according to the item's type.
    /// Not thread-safe.
    /// </summary>
    public class ListVersionsEntryCollection : Collection<VersionedEntity>
    {
        #region Private-Members

        private ListVersionsResult _Owner = null;
        private bool _Loading = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate an unbound collection.
        /// </summary>
        public ListVersionsEntryCollection()
        {

        }

        /// <summary>
        /// Instantiate a collection bound to a ListVersionsResult.
        /// </summary>
        /// <param name="owner">Owning result.  Cannot be null.</param>
        /// <param name="items">Items to serialize, in order.  Null is treated as empty.</param>
        /// <exception cref="ArgumentNullException">Thrown if owner is null.</exception>
        public ListVersionsEntryCollection(ListVersionsResult owner, IEnumerable<VersionedEntity> items)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            _Loading = true;

            if (items != null)
            {
                foreach (VersionedEntity item in items)
                {
                    if (item != null) Add(item);
                }
            }

            _Loading = false;
            _Owner = owner;
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Insert an item, routing it to the owning result when bound.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="item">Item.</param>
        protected override void InsertItem(int index, VersionedEntity item)
        {
            base.InsertItem(index, item);

            if (_Loading || _Owner == null || item == null) return;

            _Owner.Entries.Add(item);

            if (item is DeleteMarker marker) _Owner.DeleteMarkers.Add(marker);
            else if (item is ObjectVersion version) _Owner.Versions.Add(version);
        }

        #endregion
    }
}
