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
        [StringLength(12, MinimumLength = 12)]
        [Required]
        public string Reference { get; set; }

        [Required]
        [Display(Name = "SLA Policy")]
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

    /*
     * The following are used for displaying Calls in desk/,
     * as well as other details like group names.
     */

    public class ViewCallsViewModel
    {
        [Key]
        [StringLength(12)]
        public string Reference { get; set; }

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
        public DateTime? Required_By { get; set; }

        [Required]
        [StringLength(75)]
        public string Summary { get; set; }

        [Required]
        public bool Closed { get; set; }

        [StringLength(20)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(20)]
        [Display(Name = "Surname")]
        public string Lastname { get; set; }
    }

    public class GroupsSelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DeskPageViewModel
    {
        public IEnumerable<ViewCallsViewModel> VCVMList { get; set; }
        public IEnumerable<GroupsSelectViewModel> GSVM { get; set; }
    }

    /*
    * The following are used for viewing a specific call in desk/call/{reference}
    */

    public class CallDetailsForACallViewModel
    {
        [Key]
        [StringLength(12, MinimumLength = 12)]
        public string Reference { get; set; }

        [StringLength(128)]
        public string ResourceUserId { get; set; }

        public int? ResourceGroupId { get; set; }

        public string ResourceUserName { get; set; }

        public string ResourceGroupName { get; set; }

        public string SlaPolicy { get; set; }

        [Required]
        [StringLength(10)]
        public string SlaLevel { get; set; }

        public DateTime? SlaExpiry { get; set; }

        [Required]
        [StringLength(30)]
        public string Category { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
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

        [Required]
        public bool Closed { get; set; }

        [Required]
        public bool Hidden { get; set; }

        [StringLength(128)]
        public string LockedToUserId { get; set; }

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
        public string Regarding_Ref { get; set; }
    }

    public class ActionDetailsForACallViewModel
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
        public string TypeDetails { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        [DataType(DataType.Upload)]
        public string Attachment { get; set; }
    }

    public class ViewCallPageViewModel
    {
        public CallDetailsForACallViewModel CallDetails { get; set; }
        public IEnumerable<ActionDetailsForACallViewModel> ActionsList { get; set; }
        public Action ActionForCreate { get; set; }
    }

    /*
    * The following are used for creating an Action in desk/call/{reference}/action
    */

    public class CreateActionViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        [DataType(DataType.Upload)]
        public string Attachment { get; set; }
    }

    public class CreateActionPageViewModel
    {
        public CreateActionViewModel CreateAction { get; set; }
        public IEnumerable<String> ActionTypes { get; set; }
    }

    /*
    * The following are used for assigning a resource in desk/call/{reference}/assign
    */

    public class SelectResourceViewModel
    {
        public string Username { get; set; }
        public string GroupName { get; set; }
    }

    public class AssignResourcePageViewModel
    {
        public SelectResourceViewModel SelectResource { get; set; }
        public IEnumerable<ApplicationUser> UserList { get; set; }
        public IEnumerable<Group> GroupList { get; set; }
    }

    /*
    * The following are used for re-setting SLA in desk/call/{reference}/sla
    */

    public class ResetSLAForm
    {
        [Required]
        public string SLAPolicyName { get; set; }

        [Required]
        public string SLALevel { get; set; }
    }


    public class ResetSLAPageViewModel
    {
        public ResetSLAForm SLAForm { get; set; }
        public IEnumerable<SLAPolicy> SLAPolicies { get; set; }
        public IEnumerable<String> SLALevels { get; set; }
    }
}


