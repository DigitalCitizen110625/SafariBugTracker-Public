namespace SafariBugTracker.WebApp.Extensions
{

    /// <summary>
    /// Contains a collection of helpful generic extension methods
    /// </summary>
    public static class GenericExtensions
    {

        /// <summary>
        /// Checks if the arguments value is null, or the default value of it's type
        /// </summary>
        /// <typeparam name="T">Type of the object to check</typeparam>
        /// <param name="value">Reference to the object</param>
        /// <returns>True if the argument is null, or the default value of its type, false otherwise </returns>
        public static bool IsNullOrDefault<T>(this T value)
        {
            if(value == null || object.Equals(value, default(T)))
            {
                return true;
            }
            return false;
        }
    }
}