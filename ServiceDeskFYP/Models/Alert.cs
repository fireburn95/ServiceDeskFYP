using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class Alert
    {
        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        public string FromUserId { get; set; }

        public int? FromGroupId { get; set; }

        [StringLength(128)]
        public string ToUserId { get; set; }

        public int? ToGroupId { get; set; }

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

        [ForeignKey("FromUserId")]
        public ApplicationUser ApplicationUserFrom { get; set; }

        [ForeignKey("ToUserId")]
        public ApplicationUser ApplicationUserTo { get; set; }

        [ForeignKey("DismissedByUserId")]
        public ApplicationUser ApplicationUserDismissedBy { get; set; }

        [ForeignKey("ToGroupId")]
        public Group GroupTo { get; set; }

        [ForeignKey("FromGroupId")]
        public Group GroupFrom { get; set; }

        [ForeignKey("AssociatedCallRef")]
        public Call Call { get; set; }

        [ForeignKey("AssociatedKnowledgeId")]
        public Knowledge Knowledge { get; set; }
    }
}