using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    }
}