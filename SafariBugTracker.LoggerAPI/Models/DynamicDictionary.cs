using System.Collections.Generic;
using System.Dynamic;

namespace SafariBugTracker.LoggerAPI.Models
{

    /// <summary>
    /// SOURCE: https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.dynamicobject?view=netcore-3.1
    /// </summary>
    public class DynamicDictionary : DynamicObject
    {
        /// <summary>
        /// Returns the number of elements in the inner dictionary
        /// </summary>
        public int Count => dictionary.Count;

        /// <summary>
        /// DEBUG
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> GetKeyValuePairs
        {
            get
            {
                foreach (var item in dictionary)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// DEBUG
        /// </summary>
        private readonly Dictionary<string, object> dictionary;

        public DynamicDictionary(Dictionary<string, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        /// <summary>
        /// DEBUG
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return dictionary.TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// DEBUG
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dictionary[binder.Name] = value;
            return true;
        }
    }
}