using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [StringLength(10)]
        public string Extension { get; set; }

        [StringLength(15)]
        [Display(Name = "Organisation Alias")]
        public string OrganisationAlias { get; set; }

        [StringLength(25)]
        public string Organisation { get; set; }

        [StringLength(20)]
        public string Department { get; set; }

        [Required]
        public bool Disabled { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedTimestamp { get; set; }

        //Specify 1-to-many links
        [InverseProperty("ApplicationUserResourceUserId")]
        public virtual ICollection<Call> Call_ResourceUserId { get; set; }
        [InverseProperty("ApplicationUserLockedId")]
        public virtual ICollection<Call> Call_LockedToUserId { get; set; }
        [InverseProperty("ApplicationUserForId")]
        public virtual ICollection<Call> Call_ForUserId { get; set; }
        public virtual ICollection<Action> Action { get; set; }
        public virtual ICollection<GroupMember> GroupMember { get; set; }
        public virtual ICollection<Knowledge> Knowledge { get; set; }
        [InverseProperty("ApplicationUserManager")]
        public virtual ICollection<ManagerEmployee> ManagerEmployee_ManagerUserId { get; set; }
        [InverseProperty("ApplicationUserSub")]
        public virtual ICollection<ManagerEmployee> ManagerEmployee_SubUserId { get; set; }
        [InverseProperty("ApplicationUserFrom")]
        public virtual ICollection<Alert> Alert_FromUserId { get; set; }
        [InverseProperty("ApplicationUserTo")]
        public virtual ICollection<Alert> Alert_ToUserId { get; set; }
        [InverseProperty("ApplicationUserDismissedBy")]
        public virtual ICollection<Alert> Alert_DismissedByUserId { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Group> Group { get; set; }
        public DbSet<SLAPolicy> SLAPolicy { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<GroupMember> GroupMember { get; set; }
        public DbSet<Knowledge> Knowledge { get; set; }
        public DbSet<Call> Call { get; set; }
        public DbSet<Action> Action { get; set; }
        public DbSet<Alert> Alert { get; set; }
        public DbSet<ManagerEmployee> ManagerEmployee { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}