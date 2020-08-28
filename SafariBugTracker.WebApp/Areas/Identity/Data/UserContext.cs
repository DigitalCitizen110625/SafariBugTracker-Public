using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SafariBugTracker.WebApp.Areas.Identity.Data
{
    /// <summary>
    /// Defines a user account in the database, and the additional (i.e. non-Identity) data to store on the account
    /// </summary>
    public class UserContext : IdentityUser
    {
        [Required]
        [PersonalData]
        [StringLength(50)]
        [Column(TypeName ="nvarchar(50)")]
        public string FirstName { get; set; }

        [Required]
        [PersonalData]
        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string LastName { get; set; }

        [Required]
        [PersonalData]
        [StringLength(25, MinimumLength = 3)]
        [Column(TypeName = "nvarchar(25)")]
        public string DisplayName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 6)]
        [Column(TypeName = "nvarchar(50)")]
        public new string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 12)]
        [Column(TypeName = "nvarchar(50)")]
        public string Password { get; set; }

        [StringLength(50, MinimumLength = 0)]
        [Column(TypeName = "nvarchar(50)")]
        public string Position { get; set; }

        [StringLength(50, MinimumLength = 0)]
        [Column(TypeName = "nvarchar(50)")]
        public string  Role { get; set; }

        [StringLength(50, MinimumLength = 0)]
        [Column(TypeName = "nvarchar(50)")]
        public string Project { get; set; }

        [StringLength(50, MinimumLength = 0)]
        [Column(TypeName = "nvarchar(50)")]
        public string Team { get; set; }
        
        [Column(TypeName = "Varbinary(max)")]
        public byte[] ProfileImage { get; set; }

        [StringLength(50, MinimumLength = 0)]
        [Column(TypeName = "nvarchar(50)")]
        public string ContentType { get; set; }

        public DateTime RegisterDate { get; set; }

        public bool VerifiedAccount { get; set; }
    }
}