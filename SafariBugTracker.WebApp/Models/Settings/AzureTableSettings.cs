namespace SafariBugTracker.WebApp.Models.Settings
{

    /// <summary>
    /// Defines the settings that all services must have when interacting with an Azure storage account
    /// </summary>
    public interface IAzureStorageSettings
    {
        public string AccountName   { get; set; }
        public string SasToken      { get; set; }
    }

    /// <summary>
    /// Defines the settings that all services must have when interacting with Azures table storage
    /// </summary>
    public interface IAzureTableStorage
    {
        public string TableName { get; set; }
    }


    /// <summary>
    /// Defines the settings that must be entered for the custom table service to function
    /// </summary>
    public class AzureTableSettings : IAzureStorageSettings, IAzureTableStorage
    {
        public string AccountName    { get; set; }
        public string SasToken       { get; set; }
        public string TableName      { get; set; }
    }
}