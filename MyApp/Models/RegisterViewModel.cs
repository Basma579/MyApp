
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace MyApp.Models
{
    public class RegisterViewModel 
    {
        [Required]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller: "Account")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "length between 4 than 20 characters.")]
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)).+$",
            ErrorMessage = "Password Must Contain  Digits , Lower , Upper and Special Characters.")]
        public string Password { get; set; }

        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}
