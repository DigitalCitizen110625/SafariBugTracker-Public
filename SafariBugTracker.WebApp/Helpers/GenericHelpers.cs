using System.Collections.Generic;

namespace SafariBugTracker.WebApp.Helpers
{

    /// <summary>
    /// Contains a collection of small, generic, or functionally distinct helper methods 
    /// </summary>
    public static class GenericHelper
    {

        /// <summary>
        /// Creates an enumerable collection using an array of arguments
        /// </summary>
        /// <typeparam name="T"> Type of enumerable to create</typeparam>
        /// <param name="values">The collection of values used to initialize the enumerable </param>
        /// <returns>IEnumerable of a type matching that of the arguments </returns>
        public static IEnumerable<T> CreateEnumerable<T>(params T[] values) => values;
    }
}