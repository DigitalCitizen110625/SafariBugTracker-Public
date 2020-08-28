using System.ComponentModel.DataAnnotations;

namespace SafariBugTracker.WebApp.Models
{

    /// <summary>
    /// Defines the properties of an OData compatible filter query
    /// The basic structure of a filter query is as follows: 
    /// $filter={PROPERTY}%20{OPERATOR}%20'{VALUE}'
    /// </summary>
    public class ODataFilter
    {
        /// <summary>
        /// Defines the comparison operator applied to the query/filter
        /// </summary>
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
            ne,

            [Display(Name = "Between")]
            be
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


        public ODataFilter(string property, string comparisonOperator, string value)
        {
            Property = property; 
            Operator = comparisonOperator;
            Value = value;
        }
    }
}