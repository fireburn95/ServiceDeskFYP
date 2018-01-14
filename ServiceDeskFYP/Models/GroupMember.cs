using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class GroupMember
    {
        [Key]
        [Column(Order = 1)]
        public int Group_Id { get; set; }

        [Key]
        [StringLength(128)]
        [Column(Order = 2)]
        public string User_Id { get; set; }

        public bool Owner { get; set; }

        [ForeignKey("Group_Id")]
        public Group Group { get; set; }

        [ForeignKey("User_Id")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}