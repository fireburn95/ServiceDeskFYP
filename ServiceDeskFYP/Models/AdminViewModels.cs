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

    //Used in admin/employees/edit/{UserId}
    public class EditAnEmployeeViewModel
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

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    //Used in admin/groups/members/1
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

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    //Used in admin/employees/{UserId}/subordinates
    public class ViewAnEmployeeOfSubordinateViewModel
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

        public string ManagerId { get; set; }
    }

    //Used in admin/logs
    public class ViewLogsPageViewModel
    {
        public IEnumerable<ViewLogsViewModel> LogsList { get; set; }
        public IEnumerable<string> Types { get; set; }
    }
    public class ViewLogsViewModel
    {
        [Key]
        public int Id { get; set; }

        public string Type { get; set; }

        public string Detail { get; set; }

        [Display(Name = "Dated")]
        public DateTime Datetime { get; set; }

        [Display(Name = "Public IP")]
        public string PublicIP { get; set; }

        [Display(Name = "Local IP")]
        public string LocalIP { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }
    }

    //Used in admin/calls
    public class ViewCallsAdminViewModel
    {
        [Key]
        public string Reference { get; set; }

        public string ResourceUserId { get; set; }

        public string ResourceUserName { get; set; }

        public int? ResourceGroupId { get; set; }

        public string ResourceGroupName { get; set; }

        public int SlaId { get; set; }

        [Display(Name = "Level")]
        public string SlaLevel { get; set; }

        [StringLength(30)]
        public string Category { get; set; }

        [Display(Name = "Opened")]
        public DateTime Created { get; set; }

        [Display(Name = "Required By")]
        public DateTime? Required_By { get; set; }

        public DateTime? SLAResetTime { get; set; }

        public string Summary { get; set; }

        public string Description { get; set; }

        public string ForUserId { get; set; }

        public bool Closed { get; set; }

        public bool Hidden { get; set; }

        public string LockedToUserId { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string Lastname { get; set; }

        public string PhoneNumber { get; set; }

        public string Extension { get; set; }

        public string OrganisationAlias { get; set; }

        public string Organisation { get; set; }

        public string Department { get; set; }

        public string Regarding_Ref { get; set; }
    }
}


