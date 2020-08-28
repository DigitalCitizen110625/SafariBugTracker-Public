namespace SafariBugTracker.IssueAPI.Extensions
{
    /// <summary>
    /// Contains a collection of helper methods for converting object types
    /// </summary>
    public static class TypeConversionExtensions
    {

        /// <summary>
        /// Performs an unchecked() conversion on the integer value, into a string
        /// </summary>
        /// <param name="value">Int value to convert to a string</param>
        /// <returns>String version of the integer</returns>
        public static string TruncateToString(this long value)
        {
            return unchecked((int)value).ToString();
        }
    }
}