namespace System
{
    /// <summary>
    /// Contains extension methods for working with numeric types.
    /// </summary>
    public static class NumberExtensions
    {
        #region ToNegative
        /// <summary>
        /// Converts <paramref name="number"/> to the negative value.
        /// </summary>
        /// <param name="number">Number to convert</param>
        /// <returns>A number equal or below 0</returns>
        public static int ToNegative(this int number)
        {
            return number > 0 ? 0 - number : number;
        }
        #endregion
    }
}
