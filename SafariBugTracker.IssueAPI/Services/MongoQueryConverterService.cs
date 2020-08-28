using SafariBugTracker.IssueAPI.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SafariBugTracker.IssueAPI.Services
{

    /// <summary>
    /// Contains methods to extract the search parameters from the query string
    /// </summary>
    public class MongoQueryConverterService
    {

        /// <summary>
        /// Gets the values for the filter query option
        /// </summary>
        /// <param name="queryString">Unaltered query string from the client</param>
        /// <returns>Collection of queryFilter objects with each containing the property name, comparison 
        /// operator,and property value used to filter the records
        /// </returns>
        private IList<QueryFilter> ExtractFilterParameters(string queryString)
        {
            var filterCollection = new List<QueryFilter>();

            //Pattern to get  the values from the filter groups
            const string filterGroupValuePattern = @"([A-Za-z]+)\s([A-Za-z]{2})\s'([A-Za-z0-9_&*.,'""()!`~-]+)'";

            /* Each filter option should have three components
            * ex: product eq 'SafariBugTracker'
            * • Property name      (product)
            * • Comparison operator(eq)
            * • Property value     (SafariBugTracker)
            */
            const int paramsPerFilter = 3;
            Regex filterGroupsRegex = new Regex(@filterGroupValuePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = filterGroupsRegex.Matches(queryString);

            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;

                /* Currently, there are four matches for each matched filter parameter
                *   Ex: The query filter=keyword eq 'TestKeyword' yields
                *   groups[0] = keyword eq 'TestKeyword'
                *   groups[1] = keyword
                *   groups[2] = eq
                *   groups[3] = TestKeyword
                */
                int filterGroups = groups.Count - 1;
                if (filterGroups % paramsPerFilter == 0)
                {
                    string property = string.Empty;
                    string comparisonOperator = string.Empty;
                    string propertyValue = string.Empty;

                    //We don't want group[0] (as shown above), just the latter three
                    for (int i = 1; i < groups.Count; i++)
                    {
                        //Property name
                        if (i == 1)
                        {
                            property = groups[i].Value.FirstLetterToUpper();
                        }
                        //Comparison operator
                        else if (i == 2)
                        {
                            if (Enum.IsDefined(typeof(QueryFilter.ComparisonOperator), groups[i].Value))
                            {
                                comparisonOperator = groups[i].Value;
                            }
                            else
                            {
                                throw new ArgumentException($"Request Parameter: filter was poorly formed");
                            }
                        }
                        //Property value
                        else if (i == 3)
                        {
                            propertyValue = groups[i].Value;
                            filterCollection.Add(new QueryFilter(property, comparisonOperator, propertyValue));
                        }
                    }
                }
                else
                {
                    //Some portion of the filter queries were poorly formed
                    throw new ArgumentException($"Request Parameter: filter was poorly formed");
                }
            }

            return filterCollection;
        }


        /// <summary>
        /// Gets the value for the ?top query option
        /// </summary>
        /// <param name="queryString">Unaltered query string from the client</param>
        /// <returns>String value of the top n records to return from the query</returns>
        private string ExtractTopParameter(string queryString)
        {
            //Check if the ?top parameter was specified
            string topValue = null;
            if (queryString.Contains("[?]top="))
            {
                const string topValuePattern = @"(?<=[?]top=)\d+";
                Regex topValueRegex = new Regex(@topValuePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                Match match = topValueRegex.Match(queryString);
                if (string.IsNullOrEmpty(match.Value))
                {
                    //The query included the top query option but with no value
                    throw new ArgumentException($"Request Parameter: top was poorly formed");
                }
                topValue = match.Value;
            }
            return topValue;
        }

        /// <summary>
        /// Gets the values for the filter, top, query options
        /// </summary>
        /// <param name="query">The unaltered uri query string from the client application</param>
        /// <returns>Query object containing the separated filter, and top parameters</returns>
        public Query ExtractQueryParameters(string query)
        {
            var filters = ExtractFilterParameters(query);
            var topParam = ExtractTopParameter(query);
            return new Query(filters, topParam, null);
        }

    }//class
}//namespace