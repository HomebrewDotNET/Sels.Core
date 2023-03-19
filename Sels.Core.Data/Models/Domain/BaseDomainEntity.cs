using System;

namespace Sels.Core.Data.Domain
{
    /// <summary>
    /// Base class for a domain entity containing common shared properties.
    /// </summary>
    /// <typeparam name="TId">Type of the primary id</typeparam>
    public abstract class BaseDomainEntity<TId>
    {
        /// <summary>
        /// The unique id of this entity.
        /// </summary>
        public TId Id { get; set; }
        /// <summary>
        /// Key of the entity (User, background job, ...) that created this entity.
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// The time when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Key of the entity (User, background job, ...) that modified this entity last.
        /// </summary>
        public string ModifiedBy { get; set; }
        /// <summary>
        /// THe last time this entity was modified.
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }
}
