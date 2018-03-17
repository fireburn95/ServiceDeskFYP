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
}

