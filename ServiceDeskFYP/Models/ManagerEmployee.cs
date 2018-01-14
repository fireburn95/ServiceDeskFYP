using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class ManagerEmployee
    {
        [Key]
        [Column(Order = 1)]
        [StringLength(128)]
        public string ManagerUserId { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(128)]
        public string SubUserId { get; set; }

        [ForeignKey("ManagerUserId")]
        public virtual ApplicationUser ApplicationUserManager { get; set; }

        [ForeignKey("SubUserId")]
        public virtual ApplicationUser ApplicationUserSub { get; set; }
    }
}