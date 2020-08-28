using System.Collections.Generic;

namespace SafariBugTracker.IssueAPI.Models
{

    /// <summary>
    /// Provides a means of organizing the query options for incoming requests
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Top query option. Defines the count of the top n entities from a set to return
        /// </summary>
        public string Top { get; set; }

        /// <summary>
        /// Filter query option. Contains a list of parameters by which to narrow down the results
        /// </summary>
        public IList<QueryFilter> Filters { get; set; }

        /// <summary>
        /// Select query option. Contains the desired properties of an entity from the set
        /// </summary>
        public IEnumerable<string> Select   { get; set; }


        public Query(IList<QueryFilter> filters, string top, IEnumerable<string> selectParams)
        {
            Filters = filters;
            Top = top;
            Select =  selectParams;
        }

    }//class
}//namespace