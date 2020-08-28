using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SafariBugTracker.WebApp.Models
{
    public interface IIssue
    {
        public string Id                    { get; set; }
        public string Project               { get; set; }
        public string Team                  { get; set; }
        public string Product               { get; set; }
        public string Platform              { get; set; }
        public string Category              { get; set; }
        public string Version               { get; set; }
        public string ExpectedResults       { get; set; }
        public string ActualResults         { get; set; }
        public string StepsToReproduce      { get; set; }
        public string ResolveStatus         { get; set; }
        public byte[] Image                 { get; set; }
        public string ContentType           { get; set; }
        public DateTime SubmissionDate      { get; set; }
        public DateTime UpdatedDate         { get; set; }
        public List<Message> Messages       { get; set; }
        public IssueUser OriginalAuthor     { get; set; }
        public IssueUser AssignedTo         { get; set; }
    }

    /// <summary>
    /// Represents issue/bug entities in the database
    /// </summary>
    public class Issue : IIssue
    {

        [Display(Name = "Issue ID")]
        public string Id { get; set; }

        [Display(Name = "Project")]
        [StringLength(50)]
        public string Project { get; set; }

        [Display(Name = "Team")]
        [StringLength(50)]
        public string Team { get; set; }

        [Display(Name = "Product")]
        [StringLength(100)]
        public string Product { get; set; }

        [Display(Name = "Platform")]
        [StringLength(25)]
        public string Platform { get; set; }

        [Required]
        [Display(Name = "Category")]
        [StringLength(50)]
        public string Category { get; set; }

        [Display(Name = "Version")]
        [StringLength(50)]
        public string Version { get; set; }

        [Required]
        [Display(Name = "Expected Result")]
        [StringLength(2000)]
        public string ExpectedResults { get; set; }

        [Required]
        [Display(Name = "Actual Result")]
        [StringLength(2000)]
        public string ActualResults { get; set; }

        [Required]
        [Display(Name = "Steps To Reproduce")]
        [StringLength(2000)]
        public string StepsToReproduce { get; set; }

        [Display(Name = "Resolve Status")]
        [StringLength(50)]
        public string ResolveStatus { get; set; }

        public byte[] Image { get; set; }

        public string ContentType { get; set; }

        [Display(Name = "Date")]
        public DateTime SubmissionDate { get; set; }

        [Display(Name = "Updated On")]
        public DateTime UpdatedDate { get; set; }

        public List<Message> Messages { get; set; }

        [Required(ErrorMessage = "Issue Author Is Required")]
        [Display(Name = "Author")]
        public IssueUser OriginalAuthor { get; set; }

        [Required(ErrorMessage = "Issue Must Be Assigned To A User")]
        [Display(Name = "Assigned To")]
        public IssueUser AssignedTo { get; set; }
    }


    /// <summary>
    /// Defines a message posted by a user in relation to the submitted issue
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Id of the account which posted the message
        /// </summary>
        public string PosterUserID      { get; set; }

        /// <summary>
        /// Display name of the account at the time of posting
        /// </summary>
        public string PosterDisplayName { get; set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        [StringLength(256)]
        public string MessageContent    { get; set; }

        /// <summary>
        /// Date when the message was posted
        /// </summary>
        public DateTime PostDate        { get; set; }
    }


    /// <summary>
    /// Defines who was the original user who submitted the issue
    /// </summary>
    public class IssueUser
    {
        /// <summary>
        /// Id of the user who originally submitted the record
        /// </summary>
        public string UserID        { get; set; }

        /// <summary>
        /// Display name of the account at the time of submitting the record
        /// </summary>
        public string DisplayName   { get; set; }

        /// <summary>
        /// Date when the record was submitted
        /// </summary>
        public DateTime Date        { get; set; }
    }

}//namespace