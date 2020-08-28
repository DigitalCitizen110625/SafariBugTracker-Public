using System;
using System.Collections.Generic;

namespace SafariBugTracker.IssueAPI.Models
{
    /// <summary>
    ///  Contains a collection of metrics usable on the dashboard.
    ///  each metric corresponds to a separate time period for the selected team and project 
    /// </summary>
    [Serializable]
    public class DashboardKPIViewModel
    {
        /// <summary>
        /// Count of issues submitted in the last 24 hours
        /// </summary>
        public string DailyNewCount { get; set; }

        /// <summary>
        /// Count of issues closed in the last 24 hours
        /// </summary>
        public string DailyClosedCount { get; set; }

        /// <summary>
        /// Count of submitted issues in the last 24 hours that have been started
        /// </summary>
        public string DailyInProgressCount { get; set; }

        /// <summary>
        /// Collection of dates for the previous 30 days, where element[0] corresponds to the date 30 days ago, element[1] is 29 days ago, and so on./
        /// These will be the labels for the x axis on the monthly chart
        /// </summary>
        public string[] MonthlyChartXLabels { get; set; }

        /// <summary>
        /// Collection of distinct categories found in the records submitted in the past 30 days.
        /// These will be the labels for each dataset
        /// </summary>
        public string[] MonthlyCategoryLabels { get; set; }

        /// <summary>
        /// Collection of counts for each distinct category found in the records submitted in the past 30 days. 
        /// NOTE: Each array contains 30 elements, where each element gives the count for a category on a specific day. 
        /// So element[0] gives the count for the category held in MonthlyCategoryLabels[0] 30 days back. Element[1] gives the count 
        /// 29 days back, and so on.
        /// </summary>
        public List<string[]> MonthlyCategoryValues { get; set; }

        /// <summary>
        /// Count of all issues in the database for the selected team/project
        /// </summary>
        public string LifelongTotal { get; set; }

        /// <summary>
        /// Count of all issues with the closed resolve status
        /// </summary>
        public string LifelongClosed { get; set; }

        /// <summary>
        /// Count of all issues that are in progress of being completed (totalCount - closedCount - newCount)
        /// </summary>
        public string LifelongInProgress { get; set; }

        /// <summary>
        /// Collection of all distinct categories found in the database
        /// </summary>
        public string[] LifelongCategoryLables { get; set; }

        /// <summary>
        /// Count of all records in each distinct category
        /// NOTE: The order of each value matches the order of the labels in the 
        /// LifelongCategoryLables property
        /// </summary>
        public string[] LifelongCategoryValues { get; set; }

        /// <summary>
        /// Collection of all distinct resolve statuses found in the database
        /// </summary>
        public string[] LifelongResolveStatusLabels { get; set; }

        /// <summary>
        /// Count of all records with each distinct resolve status
        /// NOTE: The order of each value matches the order of the labels in the 
        /// LifelongResolveStatusLabels property
        /// </summary>
        public string[] LifelongResolveStatusValues { get; set; }
    }
}