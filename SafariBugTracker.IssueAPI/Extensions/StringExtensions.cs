namespace SafariBugTracker.IssueAPI
{
    /// <summary>
    /// Contains helpful extension methods for interacting with string type objects
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Capitalizes the first char of the target string
        /// </summary>
        /// <param name="str">Target string </param>
        /// <returns>String with the first char capitalized</returns>
        public static string FirstLetterToUpper(this string str)
        {
            if (str == null) { return null; }

            if (str.Length > 1) { return char.ToUpper(str[0]) + str.Substring(1); }

            return str.ToUpper();
        }
    }
}