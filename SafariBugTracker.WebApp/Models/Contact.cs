using System.ComponentModel.DataAnnotations;

namespace SafariBugTracker.WebApp.Models
{
    /// <summary>
    /// Defines the input fields used for the "contact us" type messages
    /// </summary>
    public class ContactMessage
    {
        [Required(ErrorMessage = "Please enter your name")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 2)]
        public string ContactName   { get; set; }

        [Required(ErrorMessage = "Please enter a an email address")]
        [DataType(DataType.EmailAddress)]
        [StringLength(150, MinimumLength = 6)]
        public string ContactEmail  { get; set; }

        [Required(ErrorMessage = "Please enter a message before submitting")]
        [StringLength(1000)]
        public string Message       { get; set; }
    }
}