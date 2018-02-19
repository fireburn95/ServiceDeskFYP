using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class Knowledge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public int Group_Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Summary { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Updated { get; set; }

        [Required]
        [StringLength(128)]
        public string LastUpdatedByUserId { get; set; }

        [ForeignKey("Group_Id")]
        public Group Group { set; get; }

        [ForeignKey("LastUpdatedByUserId")]
        public ApplicationUser ApplicationUser { set; get; }

        public virtual ICollection<Alert> Alert { get; set; }
    }
}