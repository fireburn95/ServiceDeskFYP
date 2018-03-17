using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ServiceDeskFYP.Models
{
    //Employee Dashboard page
    public class EmployeeDashboardPageViewModel
    {
        public EmployeeStatsViewModel Stats { get; set; }
        public IEnumerable<EmployeeLockedCallsViewModel> LockedCalls { get; set; }
        public IEnumerable<EmployeeUrgentCallsViewModel> UrgentCalls { get; set; }
        public int AlertsCount { get; set; }
        public int GroupsCount { get; set; }
    }

    public class EmployeeStatsViewModel
    {
        public int OpenCalls { get; set; }
        public int ClosedCalls { get; set; }
        public int Actions { get; set; }
    }

    public class EmployeeLockedCallsViewModel
    {
        public string Reference { get; set; }
        public DateTime Created { get; set; }
        public string Summary { get; set; }
    }

    public class EmployeeUrgentCallsViewModel
    {
        public string Reference { get; set; }
        public DateTime Created { get; set; }
        public string Summary { get; set; }
    }

    //Client Dashboard page
    public class ClientDashboardPageViewModel
    {
        public IEnumerable<ClientAssociatedCallViewModel> AssociatedCalls { get; set; }
        public IEnumerable<ClientAlertsViewModel> Alerts { get; set; }
        public IEnumerable<Group> GroupList { get; set; }
        public ClientSendAlertViewModel Message { get; set; }
    }

    public class ClientAssociatedCallViewModel
    {
        public string Reference { get; set; }
        public DateTime Created { get; set; }
        public string Summary { get; set; }
    }

    public class ClientAlertsViewModel
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        public string FromUserId { get; set; }

        public string FromUserName { get; set; }

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

    public class ClientSendAlertViewModel
    {
        [Display(Name = "Group")]
        public string GroupName { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }
}

