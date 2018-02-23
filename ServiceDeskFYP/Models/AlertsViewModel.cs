using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{

    //Used in /alerts
    public class ViewAlertsPageViewModel
    {
        public string GroupId { get; set; }

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

}


