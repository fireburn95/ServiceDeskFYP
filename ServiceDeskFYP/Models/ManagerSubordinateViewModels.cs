using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{
    //Used in manager_centre/sub/{username}/alert
    public class SendAlertToSubViewModel
    {
        [StringLength(500)]
        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
    }


}


