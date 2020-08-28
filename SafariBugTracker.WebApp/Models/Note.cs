namespace SafariBugTracker.WebApp.Models
{
    /// <summary>
    /// Defines the properties that all records saved to Azure Table Storage must have
    /// </summary>
    public interface IAzureTableRecord
    {
        public string PartitionKey  { get; set; }
        public string RowKey        { get; set; }
        public string TimeStamp     { get; set; }
    }

    /// <summary>
    /// Defines the additional properties of records saved to the UserNotes table
    /// </summary>
    public interface INote
    {
        public string Title     { get; set; }
        public string Content   { get; set; }
    }

    /// <summary>
    /// Concrete implementation of a user note object
    /// </summary>
    public class Note : IAzureTableRecord, INote
    {
        /// <summary>
        /// The key used to find the correct partition where the note is saved, in the Azure Table storage
        /// </summary>
        public string PartitionKey  { get; set; }

        /// <summary>
        /// Essentially the id used to find a specific note in the partition
        /// </summary>
        public string RowKey        { get; set; }

        /// <summary>
        /// Title of the note in storage, and when displayed in the view
        /// </summary>
        public string Title         { get; set; }
        
        /// <summary>
        /// The message content of the note
        /// </summary>
        public string Content       { get; set; }
        
        /// <summary>
        /// The datetime when the note was created
        /// </summary>
        public string TimeStamp     { get; set; }
    }
}