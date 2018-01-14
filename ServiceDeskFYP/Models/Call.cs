using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class Call
    {
        [Key]
        [StringLength(12)]
        public string Reference { get; set; }

        [StringLength(128)]
        public string ResourceUserId { get; set; }

        public int? ResourceGroupId { get; set; }

        public int SlaId { get; set; }

        [Required]
        [StringLength(10)]
        public string SlaLevel { get; set; }

        [Required]
        [StringLength(30)]
        public string Category { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Required_By { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SLAResetTime { get; set; }

        [Required]
        [StringLength(75)]
        public string Summary { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(128)]
        public string ForUserId { get; set; }

        [Required]
        public bool Closed { get; set; }

        [Required]
        public bool Hidden { get; set; }

        [StringLength(128)]
        public string LockedToUserId { get; set; }

        [EmailAddress]
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
        public string Regarding_Ref { get; set; }

        [ForeignKey("ResourceUserId")]
        public ApplicationUser ApplicationUserResourceUserId { get; set; }

        [ForeignKey("ResourceGroupId")]
        public Group Group { get; set; }

        [ForeignKey("SlaId")]
        public SLAPolicy SLAPolicy { get; set; }

        [ForeignKey("ForUserId")]
        public ApplicationUser ApplicationUserForId { get; set; }

        [ForeignKey("LockedToUserId")]
        public ApplicationUser ApplicationUserLockedId { get; set; }



        public ICollection<Action> Action { set; get; }
        public ICollection<Alert> Alert { set; get; }
    }
}