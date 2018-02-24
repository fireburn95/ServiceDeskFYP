using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(25)]
        public string Name { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 10)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public virtual ICollection<Call> Call { set; get; }
        public virtual ICollection<GroupMember> GroupMember { set; get; }
        public virtual ICollection<Knowledge> Knowledge { set; get; }

        [InverseProperty("GroupFrom")]
        public virtual ICollection<Alert> Alert_FromGroupId { get; set; }
        [InverseProperty("GroupTo")]
        public virtual ICollection<Alert> Alert_ToGroupId { get; set; }
    }
}