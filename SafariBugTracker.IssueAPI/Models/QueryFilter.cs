using System.ComponentModel.DataAnnotations;

namespace SafariBugTracker.IssueAPI.Models
{

    /// <summary>
    /// Defines the properties of the filter query option.
    /// The basic structure of a filter query is as follows: 
    /// $filter={PROPERTY}%20{OPERATOR}%20'{VALUE}'
    /// </summary>
    public class QueryFilter
    {

        public enum ComparisonOperator
        {
            [Display(Name = "Equal")]
            eq,

            [Display(Name = "GreaterThan")]
            gt,

            [Display(Name = "GreaterThanOrEqual")]
            ge,

            [Display(Name = "LessThan")]
            lt,

            [Display(Name = "LessThanOrEqual")]
            le,

            [Display(Name = "NotEqual")]
            ne
        }

        /// <summary>
        /// Entity property to filter on
        /// </summary>
        public string Property  { get; set; }

        /// <summary>
        /// Comparison operator used to specify the criteria against which to filter the query results
        /// For a complete list of supported operators, 
        /// see: https://docs.microsoft.com/en-us/rest/api/storageservices/querying-tables-and-entities#supported-comparison-operators
        /// </summary>
        public string Operator  { get; set; }

        /// <summary>
        /// Value of the property to filter for
        /// </summary>
        public string Value     { get; set; }


        public QueryFilter(string property, string comparisonOperator, string value)
        {
            Property = property; 
            Operator = comparisonOperator;
            Value = value;
        }
    }
}