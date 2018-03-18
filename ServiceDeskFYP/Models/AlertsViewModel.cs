using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{

    /*
     * The following are used by alerts/ 
     */
    public class ViewAlertsPageViewModel
    {
        public int? GroupId { get; set; }

        public string GroupName { get; set; }

        public int ExceededSLACalls { get; set; }

        public int PastRequiredDate { get; set; }

        public IEnumerable<ViewAlertsTemplateViewModel> Alerts { get; set; }

        public bool? IsLoggedInUserGroupOwner { get; set; }

        public bool IsGroup { get; set; }

        public bool IsDismissed { get; set; }
    }

    public class ViewAlertsTemplateViewModel
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        public string FromUserId { get; set; }

        public string FromUserName { get; set; }

        public bool FromClient { get; set; }

        public int? FromGroupId { get; set; }

        public string FromGroupName { get; set; }

        [StringLength(128)]
        public string ToUserId { get; set; }

        public string ToUserName { get; set; }

        public int? ToGroupId { get; set; }

        public string ToGroupName { get; set; }

        [StringLength(500)]
        [Required]
        public string Text { get; set; }

        [StringLength(12)]
        public string AssociatedCallRef { get; set; }

        public int? AssociatedKnowledgeId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public DateTime? DismissedWhen { get; set; }

        [StringLength(128)]
        public string DismissedByUserId { get; set; }

        public string DismissedByUserName { get; set; }

        public bool Dismissed { get; set; }
    }

    /*
     * The following are used by alerts/reply/{alertid}
     */

    public class ReplyAlertViewModel
    {
        public string Resource { get; set; }

        public string ReplyingToMessage { get; set; }

        public string ReplyToGroupName { get; set; }

        public string ReplyToUserName { get; set; }

        public string FromUserName { get; set; }

        public string FromGroupName { get; set; }

        [StringLength(500)]
        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
    }

}


