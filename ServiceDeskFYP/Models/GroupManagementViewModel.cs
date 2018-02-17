using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDeskFYP.Models
{
    /*
     * Manage Group Members
     */

    //Manage group members
    public class ManageGroupMembersForOwnersViewModel
    {
        [Key]
        [Column(Order = 1)]
        public int Group_Id { get; set; }

        [Key]
        [StringLength(128)]
        [Column(Order = 2)]
        public string User_Id { get; set; }

        [Display(Name = "Type")]
        public bool Owner { get; set; }

        public string GroupName { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }
    }

    //Used in groups/{groupid}/members
    public class ViewManageGroupMembersViewModel
    {
        public IEnumerable<ManageGroupMembersForOwnersViewModel> GroupMembers { get; set; }
        public bool IsLoggedInUserOwner { get; set; }
    }




}


