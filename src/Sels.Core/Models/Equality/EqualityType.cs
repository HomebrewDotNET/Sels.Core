namespace Sels.Core.Equality
{
    /// <summary>
    /// Defines how an a list equality is performed.
    /// </summary>
    public enum EqualityType
    {
        /// <summary>
        /// The default <see cref="List{T}"/> equality.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The elements in the list are compared, the order of the elements matters.
        /// </summary>
        Sequence = 1,
        /// <summary>
        /// The elements in the list are compared, the order of the elements doesn't matter.
        /// </summary>
        SequenceUnordered = 2,
        /// <summary>
        /// No equality checks are done. Will always return true.
        /// </summary>
        None = 3
    }
}
