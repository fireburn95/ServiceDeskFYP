using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{
    /*
* The following are used for viewing a specific call in desk/call/{reference}
*/

    public class CallDetailsViewViewModel
    {
        [Key]
        [StringLength(12, MinimumLength = 12)]
        public string Reference { get; set; }

        [StringLength(128)]
        public string ResourceUserId { get; set; }

        public int? ResourceGroupId { get; set; }

        [Display(Name = "Employee Resource")]
        public string ResourceUserName { get; set; }

        [Display(Name = "Group Resource")]
        public string ResourceGroupName { get; set; }

        [Display(Name = "SLA Policy")]
        public string SlaPolicy { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "SLA Level")]
        public string SlaLevel { get; set; }

        [Display(Name = "SLA Expiry")]
        public DateTime? SlaExpiry { get; set; }

        [Required]
        [StringLength(30)]
        public string Category { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Call Opened")]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Required By")]
        public DateTime? Required_By { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? SLAResetTime { get; set; }

        [Required]
        [StringLength(75)]
        public string Summary { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(128)]
        public string ForUserId { get; set; }

        [Display(Name = "Associated Client")]
        public string ForUserName { get; set; }

        [Required]
        [Display(Name = "Closed?")]
        public bool Closed { get; set; }

        [Required]
        [Display(Name = "Hidden?")]
        public bool Hidden { get; set; }

        [StringLength(128)]
        public string LockedToUserId { get; set; }

        [Display(Name = "Locked to")]
        public string LockedToUsername { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(20)]
        [Display(Name = "Surname")]
        public string Lastname { get; set; }

        [DataType(DataType.PhoneNumber)]
        [StringLength(40)]
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

        [StringLength(30)]
        [Display(Name = "Regarding Reference")]
        public string Regarding_Ref { get; set; }
    }

    public class ActionDetailsViewViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(12)]
        public string CallReference { get; set; }

        [Required]
        [StringLength(128)]
        public string ActionedByUserId { get; set; }

        [Display(Name = "Actioned By")]
        public string ActionedByUserName { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; }

        [StringLength(200)]
        [Display(Name = "Details")]
        public string TypeDetails { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        public bool Attachment { get; set; }
    }

    public class ViewCallPageViewViewModel
    {
        public CallDetailsViewViewModel CallDetails { get; set; }
        public IEnumerable<ActionDetailsViewViewModel> ActionsList { get; set; }
        public Action ActionForCreate { get; set; }
    }
}


