using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceDeskFYP.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Detail { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Datetime { get; set; }

        [StringLength(45)]
        public string PublicIP { get; set; }

        [StringLength(45)]
        public string LocalIP { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}