using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceDeskFYP.Models
{
    public class SLAPolicy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(25)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Low Priority mins")]
        public int LowMins { get; set; }

        [Required]
        [Display(Name = "Medium Priority mins")]
        public int MedMins { get; set; }

        [Required]
        [Display(Name = "High Priority mins")]
        public int HighMins { get; set; }

        public virtual ICollection<Call> Call { get; set; }
    }
}