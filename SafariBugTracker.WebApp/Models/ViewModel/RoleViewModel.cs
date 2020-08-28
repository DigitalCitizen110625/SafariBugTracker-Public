using System.ComponentModel.DataAnnotations;

namespace SafariBugTracker.WebApp.Models.ViewModels
{

    /// <summary>
    /// Defines the requirements for changing the role of a user account
    /// </summary>
    public class RoleViewModel
    {
        /// <summary>
        /// The id of the selected user
        /// </summary>
        [Required] 
        public string UserId   { get; set; }

        /// <summary>
        /// The new role for the selected user
        /// </summary>
        [Required]
        public string Role     { get; set; }
    }
}