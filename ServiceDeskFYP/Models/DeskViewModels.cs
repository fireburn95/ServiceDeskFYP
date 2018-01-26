using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{
    //Used in desk/create
    public class CreateCallViewModel
    {
        [Key]
        [StringLength(12)]
        [Required]
        public string Reference { get; set; }

        [Required]
        [Display(Name ="SLA Policy")]
        public string SlaName { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "SLA Priority")]
        public string SlaLevel { get; set; }

        [Required]
        [StringLength(30)]
        public string Category { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Required_By { get; set; }

        [Required]
        [StringLength(75)]
        public string Summary { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        public bool Hidden { get; set; }

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
    }

}


