namespace Sels.Core.Conversion.Serialization.Filters
{
    /// <summary>
    /// Filter that allows for modifying serialized or to be deserialized string values.
    /// </summary>
    public interface ISerializationFilter
    {
        /// <summary>
        /// Modifies the value from a property before it is deserialized.
        /// </summary>
        /// <param name="input">The value to modifiy</param>
        /// <returns>The modified value</returns>
        string ModifyOnRead(string input);
        /// <summary>
        /// Modifies the value from a property after it is serialized.
        /// </summary>
        /// <param name="input">The value to modifiy</param>
        /// <returns>The modified value</returns>
        string ModifyOnWrite(string input);
    }
}
