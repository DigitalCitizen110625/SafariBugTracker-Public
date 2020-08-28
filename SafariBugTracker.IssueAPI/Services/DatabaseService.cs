using MongoDB.Bson;
using MongoDB.Driver;
using SafariBugTracker.IssueAPI.Extensions;
using SafariBugTracker.IssueAPI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafariBugTracker.IssueAPI.Services
{
    public interface IDatabaseService
    {
        public Task<List<Issue>>              Find();
        public Issue                          Find(string ID);
        public Task<List<Issue>>              Find(Query query);
        public IEnumerable<Issue>             Find(IEnumerable<string> issueIds);
        public Task                           Create(Issue issue);
        public Task<ReplaceOneResult>         Update(string id, Issue issue);
        public Task<DeleteResult>             Remove(string id);
        public Task<DashboardKPIViewModel>    GetDailyMetrics(DashboardKPIViewModel model, string project);
        public Task<DashboardKPIViewModel>    GetMonthlyMetrics(DashboardKPIViewModel model, string project);
        public Task<DashboardKPIViewModel>    GetLifelongMetrics(DashboardKPIViewModel model, string project);
    }

    public class DatabaseService : IDatabaseService
    {

        /// <summary>
        /// Reference to the mongodb database where the issue collection is stored
        /// </summary>
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Primary collection containing the issue documents/entities
        /// </summary>
        private readonly IMongoCollection<Issue> _issues;


        public DatabaseService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
            _issues = _database.GetCollection<Issue>(settings.IssueCollectionName);
        }



        #region PrivateMethods



        /// <summary>
        /// Gets a list of all distinct values for the target property, and queries the database for the count of records matching each distinct value
        /// </summary>
        /// <param name="property">The field/property name to query for</param>
        /// <returns>Task of a dictionary containing the collection of distinct property values, and the count of records matching those values</returns>
        private async Task<Dictionary<string, string>> GetCountOfRecordsInEachDistinctCategory(string property)
        {
            const char quotationMark = '\u0022';
            FilterDefinition<Issue> filter = new BsonDocument();
            var propertyBreakdown = new Dictionary<string, string>();

            var cursor = _issues.Distinct<string>(property, Builders<Issue>.Filter.Empty);
            while (await cursor.MoveNextAsync())
            {
                foreach (var status in cursor.Current)
                {
                    if (string.IsNullOrEmpty(status))
                    {
                        filter = @"{" + property + ": null}";
                    }
                    else
                    {
                        filter = @"{" + property + ":" + quotationMark + status + quotationMark + "}";
                    }
                    string count = _issues.CountDocuments(filter).TruncateToString();
                    propertyBreakdown.Add(status ?? "Not Set", count);
                }
            }
            return propertyBreakdown;
        }


        /// <summary>
        /// Gets a count of all documents in the default collection, using the provided filter
        /// </summary>
        /// <param name="filter">Filter definition used to provide the parameters for selecting the documents to count</param>
        /// <returns>Task showing the count result as a string</returns>
        private async Task<string> GetCount(FilterDefinition<Issue> filter)
        {
            var longCount = await _issues.CountDocumentsAsync(filter);
            return longCount.TruncateToString();
        }



        #endregion
        #region IDatabaseServiceImplementation


        /// <summary>
        /// Get a complete list of all the registered issues in the database
        /// </summary>
        /// <returns>Collection of all issues from the database</returns>
        public async Task<List<Issue>> Find() => await _issues.Find(issue => true).ToListAsync();


        /// <summary>
        /// Get a single record that matches the search parameters
        /// </summary>
        /// <param name="Id"> ID property of the IIssue model to search for </param>
        /// <returns> Single record with a  matching ID property </returns>
        public Issue Find(string Id) => _issues.Find(_ => _.Id == Id).FirstOrDefault();


        /// <summary>
        /// Get a list of records that match the search parameters
        /// </summary>
        /// <param name="query"> Contains the parameters by which the records will be filtered</param>
        /// <returns> List of issues matching the provided parameters </returns>
        public async Task<List<Issue>> Find(Query query)
        {
            var dynamicQuery = new Dictionary<string, dynamic>();

            if (query.Filters != null)
            {
                foreach(var filter in query.Filters)
                {
                    if (filter.Property == "Id")
                    {
                        dynamicQuery.Add("_id", new BsonDocument("$" + filter.Operator, ObjectId.Parse(filter.Value)));
                    }
                    else if(filter.Property == "Keyword")
                    {
                        dynamicQuery.Add("$text", new BsonDocument("$search", filter.Value));
                    }
                    else if (filter.Property == "ResolveStatus" && filter.Value == "All")
                    {
                        dynamicQuery.Add(filter.Property, new BsonDocument("$ne", "null"));
                    }
                    else
                    {
                        dynamicQuery.Add(filter.Property, new BsonDocument("$" + filter.Operator, filter.Value));
                    }
                }
            }
            if(query.Top != null)
            {
                dynamicQuery.Add("$limit", query.Top);
            }

            var queryResults = await _issues.Find(new BsonDocument(dynamicQuery)).ToListAsync();
            return queryResults;
        }


        /// <summary>
        /// Find the matching entities with the same ids
        /// </summary>
        /// <param name="issueIds"> Enumerable collection of ids to look for</param>
        /// <returns>Collection of issues matching the provided ids</returns>
        public IEnumerable<Issue> Find(IEnumerable<string> issueIds)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Creates a new record with the supplied details
        /// </summary>
        /// <param name="issue"> Details of the record to create</param>
        /// <returns> True if the record was inserted without error, false otherwise </returns>
        public async Task Create(Issue issue) => await _issues.InsertOneAsync(issue);


        /// <summary>
        /// Finds a record with the matching ID, and updates all its properties to match the passed in object
        /// </summary>
        /// <param name="id"> ID of the record to update </param>
        /// <param name="issue"> New values for the matched record </param>
        /// <returns> Object containing the number of updated records, and their count of modified values  </returns>
        public async Task<ReplaceOneResult> Update(string id, Issue issue) => await _issues.ReplaceOneAsync(_ => _.Id == id, issue);


        /// <summary>
        /// Finds a record with the matching ID, and updates all its properties to match the passed in object
        /// </summary>
        /// <param name="id"> ID of the record to update </param>
        /// <param name="issue"> New values for the matched record </param>
        /// <returns> Object containing the number of updated records, and their count of modified values  </returns>
        public async Task<DeleteResult> Remove(string id) => await _issues.DeleteOneAsync(_ => _.Id == id);


        /// <summary>
        /// Calculate the daily metrics using records that were submitted within the last 24 hours
        /// </summary>
        /// <param name="team">Use records with the same teams designation</param>
        /// <returns>Task of the DashboardKPIViewModel object containing the complete set of information on each defined daily metric</returns>
        public async Task<DashboardKPIViewModel> GetDailyMetrics(DashboardKPIViewModel model, string project)
        {
            /*
             * Unfortunately, the .Net mongo drivers, and LINQ don't work to well when comparing strings
             * For example: Normally in the mongo shell you can do the equivalent of Builders<Issue>.Filter.Where(_ => _.SubmissionDate >= date);
             * but the .Net driver doesn't allow a >= comparison for string types. Thus, we must convert it to a BSON query instead
             */
            const char quotationMark = '\u0022';
            string projectSubQuery = @"{Project:" + quotationMark + project + quotationMark + "}";

            var queryDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
            string dateSubQuery = @"{SubmissionDate: {$gte: new Date(" + quotationMark + queryDate + quotationMark + ")}}";
            string compoundQuery = @"{$and:[" + projectSubQuery + ", " + dateSubQuery + "]}";

            var totalIssuesFromPastDay = await GetCount(compoundQuery);

            var newIssueCount = await GetCount("{$and:[{Project:" + quotationMark + project + quotationMark + "}, {SubmissionDate: {$gte: new Date(" + quotationMark + queryDate + quotationMark + ")}},{ResolveStatus:" + quotationMark + "New" + quotationMark + "}]}");
            var closedIssueCount = await GetCount("{$and:[{Project:" + quotationMark + project + quotationMark + "}, {SubmissionDate: {$gte: new Date(" + quotationMark + queryDate + quotationMark + ")}},{ResolveStatus:" + quotationMark + "Closed" + quotationMark + "}]}");
            var issuesInProgressCount = (int.Parse(totalIssuesFromPastDay) - int.Parse(newIssueCount) - int.Parse(closedIssueCount)).ToString();

            model.DailyNewCount = newIssueCount;
            model.DailyClosedCount = closedIssueCount;
            model.DailyInProgressCount = issuesInProgressCount;
            return model;
        }


        /// <summary>
        /// Calculate the monthly metrics using issues that were submitted within the last 30 days
        /// </summary>
        /// <param name="team">Use records with the same teams designation</param>
        /// <returns>Task of the DashboardKPIViewModel object containing the complete set of information on each defined monthly metric</returns>
        public async Task<DashboardKPIViewModel> GetMonthlyMetrics(DashboardKPIViewModel model, string project)
        {
            const char quotationMark = '\u0022';


            //Here we want to create a list of dates that starts 30 days ago, and ends with todays date
            //  We will apply a custom formatter so that our dates follow the same pattern as used in the database (i.e. yyyy-MM-dd)
            DateTimeFormatInfo dtfi = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
            dtfi.DateSeparator = "-";
            dtfi.ShortDatePattern = @"MM-dd";

            DateTime start = DateTime.Now.Date.AddDays(-30);
            DateTime end = DateTime.Now;

            model.MonthlyChartXLabels = Enumerable.Range(0, 30)
                .Select(offset => start.AddDays(offset).ToString("d", dtfi))
                .ToArray();


            //Get all the records that were submitted in the last 30 days
            var startDate = DateTime.Now.AddDays(-30).Date.ToString("yyyy-MM-dd");
            var endDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var cursor = await _issues.FindAsync<Issue>(@"{$and:[{Project:" + quotationMark + project + quotationMark + "}, {SubmissionDate: {$gte: new Date(" + quotationMark + startDate + quotationMark + ")}}, {SubmissionDate: {$lt: new Date(" + quotationMark + endDate + quotationMark + ")}} ]}");
            var issuesFromPastMonth = await cursor.ToListAsync();


            //Get all the distinct categories (these will be used as the labels for each dataset in the monthly chart)
            model.MonthlyCategoryLabels = issuesFromPastMonth.Select(_ => _.Category).Distinct().ToArray();

            
            //This will contain the count of records with the same category for each day
            var datasetValues = new List<string>();
            model.MonthlyCategoryValues = new List<string[]>();

            foreach (var category in model.MonthlyCategoryLabels)
            {
                //Of all records submitted in the past 30 days, find all with a matching category
                var issuesWithMatchingCategory = issuesFromPastMonth.Where(_ => _.Category == category);
                
                
                foreach (var date in model.MonthlyChartXLabels)
                {
                    //Get all records that were submitted on the current date (starting at 30 days in the past, then 29, and so on...)
                    var issuesOnDate = issuesWithMatchingCategory.Where(_ => _.SubmissionDate.Date.ToString("d", dtfi) == date);
                    if(issuesOnDate.Count() < 1)
                    {
                        //No records with the current category were submitted on this specific date
                        datasetValues.Add("0");
                    }
                    else
                    {
                        //Some records were submitted that day => records the count, and move to the next day
                        datasetValues.Add(issuesOnDate.Count().ToString());
                    }
                }

                model.MonthlyCategoryValues.Add(datasetValues.ToArray());
                datasetValues.Clear();
            }

            return model;
        }

        /// <summary>
        /// Calculate the life long metrics using all issues ever submitted to the database
        /// </summary>
        /// <param name="team">Use records with the same teams designation</param>
        /// <returns>Task of the DashboardKPIViewModel object containing the complete set of information on each defined lifelong metric</returns>
        public async Task<DashboardKPIViewModel> GetLifelongMetrics(DashboardKPIViewModel model, string project)
        {     
            //Get the count of all issues, those with a resolve status of "closed", and those that are still in progress
            const char quotationMark = '\u0022';
            model.LifelongTotal = await GetCount(@"{Project:" + quotationMark + project + quotationMark + "}");
            model.LifelongClosed = await GetCount(@"{$and:[{Project:" + quotationMark + project + quotationMark + "}, { ResolveStatus : " + quotationMark + "Closed" + quotationMark + " }]}");
            
            var newIssueCount = await GetCount(@"{$and:[{Project:" + quotationMark + project + quotationMark + "}, { ResolveStatus : " + quotationMark + "New" + quotationMark + " }]}");
            model.LifelongInProgress = (int.Parse(model.LifelongTotal) - int.Parse(model.LifelongClosed) - int.Parse(newIssueCount)).ToString();


            var labelList = new List<string>();
            var valueList = new List<string>();


            //Get a count of all records in each distinct category
            var categoryBreakdown = await GetCountOfRecordsInEachDistinctCategory("Category");
            foreach (var item in categoryBreakdown)
            {
                labelList.Add(item.Key);    //Distinct name of the issues category
                valueList.Add(item.Value);  //The count of records in that category
            }

            model.LifelongCategoryLables = labelList.ToArray();
            model.LifelongCategoryValues = valueList.ToArray();

            labelList.Clear();
            valueList.Clear();


            //We'll do the same as above, but for the resolve status of each record. 
            var resolveStatusBreakdown = await GetCountOfRecordsInEachDistinctCategory("ResolveStatus");
            foreach (var item in resolveStatusBreakdown)
            {
                labelList.Add(item.Key);    //Distinct name of the resolve status
                valueList.Add(item.Value);  //The count of records with that status
            }
            model.LifelongResolveStatusLabels = labelList.ToArray();
            model.LifelongResolveStatusValues = valueList.ToArray();

            return model;
        }


        /// <summary>
        /// Builds a query string usable by the mongodb database
        /// </summary>
        /// <param name="property">Name of the property to query</param>
        /// <param name="value">Value of the property to find</param>
        /// <param name="comparisonOperator">Defines the type of comparison to apply on the property values (e.g. eq, lt, gt etc.)</param>
        /// <returns>Complete query string </returns>
        private string BuildFilter(string property, string value, string comparisonOperator)
        {
            const char quotationMark = '\u0022';
            const string baseFilterPattern = @"{[PROPERTY]: [QUOTE][VALUE][QUOTE]}";
            const string filterPatternWithComparison = @"{[PROPERTY]: { [COMPARISON_OPERATOR]: [QUOTE][VALUE][QUOTE]}}";
            StringBuilder filterBuilder = null;

            if(comparisonOperator == null)
            {
                filterBuilder = new StringBuilder(baseFilterPattern);
            }
            else
            {
                filterBuilder = new StringBuilder(filterPatternWithComparison);
                filterBuilder.Replace("[COMPARISON_OPERATOR]", comparisonOperator);
            }
            filterBuilder.Replace("[PROPERTY]", property);
            filterBuilder.Replace("[VALUE]", value);
            filterBuilder.Replace("[QUOTE]", quotationMark.ToString());
            return filterBuilder.ToString();
        }


        #endregion
    }//class
}//namespace