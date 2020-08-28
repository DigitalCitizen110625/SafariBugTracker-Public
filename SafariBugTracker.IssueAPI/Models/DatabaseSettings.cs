namespace SafariBugTracker.IssueAPI.Models
{
    public interface IDatabaseSettings
    {
        public string DatabaseName          { get; set; }
        public string IssueCollectionName    { get; set; }
        public string ConnectionString      { get; set; }
    }

    /// <summary>
    /// Defines the customizable properties for the database service
    /// </summary>
    public class DatabaseSettings : IDatabaseSettings
    {
        /// <summary>
        /// Name of the target database
        /// </summary>
        public string DatabaseName          { get; set; }

        /// <summary>
        /// Name of the target entity (SQL Server)/collection (Mongodb) in the database
        /// </summary>
        public string IssueCollectionName   { get; set; }

        /// <summary>
        /// String used to open a connection to the target database
        /// </summary>
        public string ConnectionString      { get; set; }
    }
}
