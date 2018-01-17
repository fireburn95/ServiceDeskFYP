using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceDeskFYP.Models
{
    //Used in admin/employees/{UserId}
    public class ViewAnEmployeeViewModel
    {
        [Key]
        public string Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string Extension { get; set; }

        public string Department { get; set; }
        
        public bool Admin { get; set; }

        public bool Disabled { get; set; }
    }
}
