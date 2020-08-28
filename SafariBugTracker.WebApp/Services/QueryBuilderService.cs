using SafariBugTracker.WebApp.Extensions;
using SafariBugTracker.WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FilterOperator = SafariBugTracker.WebApp.Models.ODataFilter.ComparisonOperator;

namespace SafariBugTracker.WebApp.Services
{

    /// <summary>
    /// Provides a means of organizing, and constructing an OData compatible uri query string
    /// </summary>
    public class QueryBuilderService
    {
        /// <summary>
        /// Contains the complete query string based on the provided arguments
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// $top query option. Defines the count of the top n entities from a set to return
        /// </summary>
        public string Top { get; set; }

        /// <summary>
        /// $filter query option. Contains a list of parameters by which to narrow down the results
        /// </summary>
        public IList<ODataFilter> Filters { get; set; }

        /// <summary>
        /// $select query option. Contains the desired properties of an entity from the set
        /// </summary>
        public IEnumerable<string> Select   { get; set; }


        /// <summary>
        /// Packages, and saves the desired filter options
        /// </summary>
        /// <param name="paramName">Name of the parameter to filter by</param>
        /// <param name="paramOperator">Operator used to join the parameter name and value (e.g. filter={PROPERTY}%20{OPERATOR}%20'{VALUE}') </param>
        /// <param name="paramValue">Value to filter by </param>
        /// <returns>Void return, but adds the complete filter string to the Filter property </returns>
        public void AddFilterParameter(string paramName, string paramValue, FilterOperator paramOperator = FilterOperator.eq)
        {
            if (!string.IsNullOrEmpty(paramValue))
            {
                //Initialize the filter collection if its null
                Filters ??= new List<ODataFilter>();
                Filters.Add(new ODataFilter(paramName, paramOperator.ToString(), paramValue));
            }
        }


        /// <summary>
        /// Sets the top n records parameter for the query
        /// </summary>
        /// <param name="nRecords">Count of records</param>
        /// <returns>Void return, but sets the Top parameter to the nRecords count </returns>
        public void AddTopParameter(string nRecords)
        {
            if (!string.IsNullOrEmpty(nRecords))
            {
                Top = nRecords;
            }
        }


        /// <summary>
        /// Constructs a single query string using the classes properties. The supported query options are 
        /// $filter, $top, and $select. For more information, please see: 
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/querying-tables-and-entities#supported-query-options
        /// </summary>
        /// <returns>Void return, but sets the QueryString to the complete OData compatible query string, or an empty string if no query options are specified </returns>
        public void ConstructQuery()
        {
            //Example of complete query string: /directory?$filter={PROPERTY}%20{OPERATOR}%20'{VALUE}'%20&amp;%20$top={VALUE}
            const string baseFilterString = "filter=";
            const string baseTopString = "top=";
            const string queryWhitespace = "%20";
            const string queryJoiner = "&amp;";

            StringBuilder query = new StringBuilder(string.Empty);
            if ( (Filters.IsNullOrDefault() || Filters.Count() < 1) && Top.IsNullOrDefault() && Select.IsNullOrDefault())
            {
                //No query parameters were specified
                QueryString = query.ToString();
                return;
            }


            //At least one query parameter was specified
            query.Append("?");
            if (Filters != null)
            {
                /* 
                * Complete string: 
                * $filter={PROPERTY}%20{OPERATOR}%20'{VALUE}' 
                *   or 
                * $filter=LastName%20eq%20'Smith'%20and%20FirstName%20eq%20'John'  
                */
                query.Append(baseFilterString);

                int paramCount = Filters.Count();
                int i = 0;
                foreach (var filter in Filters)
                {
                    query.Append($"{filter.Property}{queryWhitespace}{filter.Operator}{queryWhitespace}'{filter.Value}'");
                    
                    //Check if theres any remaining filter params
                    if (i == paramCount - 1)
                    {
                        break;
                    }
                    query.Append($"{queryWhitespace}and{queryWhitespace}");
                    i++;
                }
            }

            if (Top != null)
            {
                //Complete string: %20&amp;%20$top={VALUE}
                query.Append($"{queryWhitespace}{queryJoiner}{queryWhitespace}{baseTopString}{Top}");
            }

            QueryString = query.ToString();
        }

    }//class
}//namespace