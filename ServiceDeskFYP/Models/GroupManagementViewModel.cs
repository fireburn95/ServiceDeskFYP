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

    /*
     * Used in viewing knowledges
     */

    public class ViewKnowledgesPageGroupViewModel
    {
        public IEnumerable<Knowledge> Knowledges { get; set; }
        public bool IsLoggedInUserOwner { get; set; }
    }

    /*
     * Used in Creating knowledges
     */
    public class CreateKnowledgeGroupViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Summary { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }

    /*
    * Used in Viewing knowledges
    */
    public class ViewKnowledgeGroupViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public int Group_Id { get; set; }

        [Display(Name = "For Group")]
        public string GroupName { get; set; }

        [Required]
        [StringLength(150)]
        public string Summary { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Last Updated")]
        public DateTime Updated { get; set; }

        [Required]
        [StringLength(128)]
        public string LastUpdatedByUserId { get; set; }

        [Display(Name ="Updated by")]
        public string LastUpdatedByUserName { get; set; }

        public bool IsLoggedInUserOwner { get; set; }
    }

    /*
     * Used in Updating knowledges
     */
    public class UpdateKnowledgeGroupViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Summary { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }

}


