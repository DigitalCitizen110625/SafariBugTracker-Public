using System.ComponentModel.DataAnnotations;

namespace SafariBugTracker.WebApp.Models.ViewModels
{
    /// <summary>
    /// Defines the basic structure, and contents of a user when displayed in a view
    /// </summary>
    public class UserViewModel
    {
        public string Id        { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50 ,MinimumLength = 1)]
        public string LastName { get; set; }

        public string UserName { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        [StringLength(25, MinimumLength = 3)]
        public string DisplayName   { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        [StringLength(50, MinimumLength = 6)]
        public string Email         { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 12)]
        public string Password      { get; set; }

        [Display(Name = "Position")]
        [StringLength(50, MinimumLength = 0)]
        public string Position      { get; set; }

        [Display(Name = "Role")]
        [StringLength(50, MinimumLength = 0)]
        public string Role          { get; set; }

        [Display(Name = "Project")]
        [StringLength(50, MinimumLength = 0)]
        public string Project       { get; set; }

        [Display(Name = "Team")]
        [StringLength(50, MinimumLength = 0)]
        public string Team          { get; set; }

        [Display(Name = "Profile Image")]
        public byte[] ProfileImage  { get; set; }

        public string ContentType   { get; set; }
    }
}