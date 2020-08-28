namespace SafariBugTracker.WebApp.Models
{

    /// <summary>
    /// Model that defines all possible fields by which a user can search the database for an issue/record
    /// </summary>
    public class IssueSearchParameters
    {
        public string Keyword           { get; set; }
        public string Id                { get; set; }
        public string Author            { get; set; }
        public string AssignedTo        { get; set; }
        public string StartDate         { get; set; }
        public string DateSearchType    { get; set; }
        public string EndDate           { get; set; }
        public string Platform          { get; set; }
        public string Product           { get; set; }
        public string Category          { get; set; }
        public string StartVersion      { get; set; }
        public string VersionSearchType { get; set; }
        public string EndVersion        { get; set; }
        public string ResolveStatus     { get; set; }
    }
}//namespace