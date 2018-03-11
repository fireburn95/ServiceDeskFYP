using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Runtime.Serialization;

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
        [Display(Name = "Required By")]
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
        [Display(Name = "Regarding Reference")]
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
        [Display(Name = "Sla Level")]
        public string SlaLevel { get; set; }

        [Required]
        [StringLength(30)]
        public string Category { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Required By")]
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

        public bool Urgent { get; set; }

        public DateTime? SLAResetTime { get; set; }

        public int SlaId { get; set; }
    }

    public class GroupsSelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DeskPageViewModel
    {
        public string GroupName { get; set; }
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
        [Display(Name = "Details")]
        public string TypeDetails { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        public bool Attachment { get; set; }
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
        public HttpPostedFileBase Attachment { get; set; }

        [Display(Name = "Send Email Update To Client")]
        public bool SendEmail { get; set; }
    }

    public class CreateActionPageViewModel
    {
        public string CallSummary { get; set; }
        public CreateActionViewModel CreateAction { get; set; }
        public IEnumerable<String> ActionTypes { get; set; }
    }

    /*
    * The following are used for assigning a resource in desk/call/{reference}/assign
    */

    public class SelectResourceViewModel
    {
        public string Username { get; set; }
        [Display(Name = "Group")]
        public string GroupName { get; set; }
    }

    public class AssignResourcePageViewModel
    {
        public string CallSummary { get; set; }
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
        [Display(Name = "Policy")]
        public string SLAPolicyName { get; set; }

        [Required]
        [Display(Name = "Level")]
        public string SLALevel { get; set; }
    }


    public class ResetSLAPageViewModel
    {
        public string CallSummary { get; set; }
        public ResetSLAForm SLAForm { get; set; }
        public IEnumerable<SLAPolicy> SLAPolicies { get; set; }
        public IEnumerable<String> SLALevels { get; set; }
    }

    /*
    * The following are used for Editing a call SLA in desk/call/{reference}/edit
    */

    public class EditCallViewModel
    {
        [Key]
        [StringLength(12, MinimumLength = 12)]
        [Required]
        public string Reference { get; set; }

        [Required]
        [StringLength(30)]
        public string Category { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Required By")]
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
        [Display(Name = "Regarding Reference")]
        public string Regarding_Ref { get; set; }

        [Required(ErrorMessage = "Please specify the purpose for the changes made")]
        [DataType(DataType.MultilineText)]
        [Display(Name ="Reason for changes? (for Action)")]
        public string EditComments { get; set; }
    }

    public class EditCallPageViewModel
    {
        public string CallSummary { get; set; }
        public IEnumerable<String> Categories { get; set; }
        public EditCallViewModel EditCall { get; set; }
    }

    /*
     * 
     */

    public class NotifyViewModel
    {
        public string Username { get; set; }
        [Display(Name = "Group")]
        public string GroupName { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }

    public class NotifyPageViewModel
    {
        public string CallSummary { get; set; }
        public NotifyViewModel Notify { get; set; }
        public IEnumerable<ApplicationUser> UserList { get; set; }
        public IEnumerable<Group> GroupList { get; set; }
    }

    /*
     * The following are used for associating a client in desk/call/{reference}/client
     */

    public class AssociateClientViewModel
    {
        public string Username { get; set; }

        [Display(Name = "Update Call Contact Information")]
        public bool UpdateCallDetails { get; set; }
    }

    public class AssociateClientPageViewModel
    {
        public string CallSummary { get; set; }
        public AssociateClientViewModel AssociateClient { get; set; }
    }

    /*
     * The following are used for viewing a call report in desk/call/{reference}/report
     */

    public class ReportPageViewModel
    {
        public string ActionedByJsonData { get; set; }
    }

    [DataContract]
    public class ActionedByGraphDataPointViewModel
    {
        public ActionedByGraphDataPointViewModel(string label, double y)
        {
            this.Label = label;
            this.Y = y;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
        public string Label = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;
    }

}


