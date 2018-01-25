using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{
    //Used in admin/employees/{UserId}
    public class ViewAnEmployeeViewModel
    {
        [Key]
        public string Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string Extension { get; set; }

        public string Department { get; set; }

        public bool Admin { get; set; }

        public bool Disabled { get; set; }
    }

    //Used in admin/employees/create
    public class CreateEmployeeViewModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Only alphanumeric characters allowed in username")]
        [Display(Name = "Username")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "{0} must be between {2} and {1} characters")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[A-z]+$", ErrorMessage = "Only alphabetical characters allowed in first name")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[A-z]+$", ErrorMessage = "Only alphabetical characters allowed in surname")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters")]
        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "{0} must be between {2} and {1} characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(10)]
        public string Extension { get; set; }

        [StringLength(20)]
        public string Department { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedTimestamp { get; set; }
    }

    public class ManageGroupMembersViewModel
    {
        [Key]
        [Column(Order = 1)]
        public int Group_Id { get; set; }

        [Key]
        [StringLength(128)]
        [Column(Order = 2)]
        public string User_Id { get; set; }

        public bool Owner { get; set; }

        public string GroupName { get; set; }

        public string UserName { get; set; }
    }

    //Used in admin/clients/{UserId}
    public class ViewAClientViewModel
    {
        [Key]
        public string Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string Extension { get; set; }

        public string Department { get; set; }

        [StringLength(15)]
        [Display(Name = "Organisation Alias")]
        public string OrganisationAlias { get; set; }

        [StringLength(25)]
        public string Organisation { get; set; }

        public bool Disabled { get; set; }
    }

    //Used in admin/clients/edit/{UserId}
    public class EditAClientViewModel
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Only alphanumeric characters allowed in username")]
        [Display(Name = "Username")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "{0} must be between {2} and {1} characters")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[A-z]+$", ErrorMessage = "Only alphabetical characters allowed in first name")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[A-z]+$", ErrorMessage = "Only alphabetical characters allowed in surname")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {2} and {1} characters")]
        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(10)]
        public string Extension { get; set; }

        [StringLength(15)]
        [Display(Name = "Organisation Alias")]
        public string OrganisationAlias { get; set; }

        [StringLength(25)]
        public string Organisation { get; set; }

        [StringLength(20)]
        public string Department { get; set; }
    }


}


