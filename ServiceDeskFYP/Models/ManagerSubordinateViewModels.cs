using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ServiceDeskFYP.Models
{
    //Used in manager_centre/sub/{username}/alert
    public class SendAlertToSubViewModel
    {
        public string ToUsername { get; set; }

        [StringLength(500)]
        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
    }

    //Used in manager_centre/sub/{username}/report
    public class ManagerUserReportPageViewModel
    {
        public string ActionJsonDatapoints { get; set; }
        public ManagerUserStatisticsViewModel Statistics { get; set; }
    }

    [DataContract]
    public class ManagerUserActionsReportDataPointsViewModel
    {


        public ManagerUserActionsReportDataPointsViewModel(string label, double y)
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

    public class ManagerUserStatisticsViewModel
    {
        public int OpenCalls { get; set; }
        public int ClosedCalls { get; set; }
        public int Actions { get; set; }
        public int ClosedBeforeSla { get; set; }
        public int ClosedAfterSla { get; set; }

    }

    //Used in manager_centre/report
    public class ManagerReportPageViewModel
    {
        public string PieJsonDatapoints { get; set; }
        public IEnumerable<ManagerReportCompareTableViewModel> stats { get; set; }
    }

    [DataContract]
    public class ManagerReportDataPointsViewModel
    {


        public ManagerReportDataPointsViewModel(string label, double y)
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

    public class ManagerReportCompareTableViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public int OpenCalls { get; set; }
        public int ClosedCalls { get; set; }
        public int Actions { get; set; }
        public int ClosedBeforeSla { get; set; }
        public int ClosedAfterSla { get; set; }
    }
}

