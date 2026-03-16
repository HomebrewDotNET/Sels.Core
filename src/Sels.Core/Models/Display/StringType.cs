namespace Sels.Core.Display
{
    /// <summary>
    /// Defines how a collection <see cref="object.ToString"/> method works.
    /// </summary>
    public enum StringType
    {
        /// <summary>
        /// The default <see cref="List{T}"/> <see cref="object.ToString"/>.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Formatted as GenericTypeName[Count].
        /// </summary>
        Formatted = 1,
        /// <summary>
        /// Formatted as GenericTypeName[Count] without including the namespace.
        /// </summary>
        FormattedShort = 2
    }
}
