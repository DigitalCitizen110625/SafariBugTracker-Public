using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using SafariBugTracker.IssueAPI.Models;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SafariBugTracker.IssueAPI.Helpers
{

    /// <summary>
    /// Contains methods for checking the health of the mongodb database where the issue records are kept
    /// </summary>
    public class MongoDbHealthCheck : IHealthCheck
    {

        /// <summary>
        /// Reference to the database where the issue collection is stored
        /// </summary>
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Reference to the collection where the issue records are kept
        /// </summary>
        private readonly IMongoCollection<Issue> _issues;

        public MongoDbHealthCheck(string collectionName, string databaseName, string connectionString)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            _issues = _database.GetCollection<Issue>(collectionName);
        }


        /// <summary>
        /// Performs a health check by pinging the mongodb issue database, and retrieving a record from the target collection
        /// </summary>
        /// <param name="context">HealthCheckContext context in which the health check is performed </param>
        /// <param name="cancellationToken">CancellationToken allowing the task to be canceled</param>
        /// <returns> Task of type HealthCheckResult indicating the result of the health check</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                //Ping the database to ensure it's online
                const string successfulPingResult = @"{ ""ok"" : 1.0 }";
                var result = _database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                if (result.ToString() != successfulPingResult)
                {
                    throw new Exception($"Database: {nameof(_issues.Database)}, unreachable. Ping returned {result.ToString()}");
                }


                //Ensure theres at least one record in the database, then get a single record to ensure the data is readable
                //Note: This doesn't check write permissions, only that the api can read from the endpoint
                var recordCount = _issues.CountDocuments(new BsonDocument());
                if(recordCount > 0)
                {
                    var issue = _issues.Find<Issue>(new BsonDocument()).Limit(1).ToList<Issue>();
                    if (issue == null || issue.Count< 1)
                    {
                        throw new Exception($"Database: {nameof(_issues.Database)}, Collection: {nameof(_issues.CollectionNamespace)} cannot retrieve records");
                    }
                }

                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception e)
            {
                Log.Error(e, $"{nameof(MongoDbHealthCheck)} failed: {e.Message}");
                return Task.FromResult(HealthCheckResult.Unhealthy($"MongoDB Issue database reported error: ", e));
            }
        }

    }//class
}//namespace