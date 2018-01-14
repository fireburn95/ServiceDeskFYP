using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class Action
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(12)]
        public string CallReference { get; set; }

        [Required]
        [StringLength(128)]
        public string ActionedByUserId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; }

        [StringLength(200)]
        public string TypeDetails { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        [DataType(DataType.Upload)]
        public string Attachment { get; set; }

        [ForeignKey("CallReference")]
        public Call Call { get; set; }

        [ForeignKey("ActionedByUserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}