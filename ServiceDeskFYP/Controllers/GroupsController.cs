using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceDeskFYP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskFYP.Controllers
{
    [Authorize(Roles = "Employee")]
    public class GroupsController : Controller
    {
        //Helper vars
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        //Initialise via constructor
        public GroupsController()
        {
            _context = new ApplicationDbContext();
            userStore = new UserStore<ApplicationUser>(_context);
            userManager = new UserManager<ApplicationUser>(userStore);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        /*****************
        * Select Group
        * ***************/

        // GET Select group
        public ActionResult Index()
        {
            //Handle messages
            HandleMessages();

            //Get logged in user ID
            var LoggedInId = User.Identity.GetUserId();

            //Get list of Group user is member of
            var Groups = _context.GroupMember.Where(n => n.User_Id.Equals(LoggedInId)).Select(n => n.Group);

            //Pass to view
            return View(Groups);
        }

        // POST Select group
        [HttpPost]
        public ActionResult Index(string groupid = null)
        {
            //Get logged in user ID
            var LoggedInId = User.Identity.GetUserId();

            //Check value has been posted exists
            if (groupid == null)
            {
                TempData["ErrorMessage"] = "An error has occured, Group ID specified doesn't exist";
                return RedirectToAction("Index");
            }

            //Go to action
            return RedirectToAction("GroupHome", new { groupid });
        }

        /*****************
        * Group Home
        * ***************/

        //GET Homepage for groups
        [HttpGet]
        [Route("groups/{groupid}")]
        public ActionResult GroupHome(string groupid)
        {
            //Handle messages
            HandleMessages();

            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Go to view and pass whether is owner
            return View(GroupMember);
        }

        /*****************
        * Manage Members
        * ***************/

        //GET View and Manage Members
        [HttpGet]
        [Route("groups/{groupid}/members")]
        public ActionResult ManageAndViewMembers(string groupid)
        {
            //Handle messages
            HandleMessages();

            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Get list of Group Members
            var GroupMembers = _context.GroupMember.Where(n => n.Group_Id == GroupIdInt);

            //Get the Users
            List<ManageGroupMembersForOwnersViewModel> GroupMembersUserList = new List<ManageGroupMembersForOwnersViewModel>();
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                foreach (var member in GroupMembers)
                {
                    GroupMembersUserList.Add(new ManageGroupMembersForOwnersViewModel()
                    {
                        User_Id = member.User_Id,
                        Group_Id = GroupIdInt,
                        GroupName = Group.Name,
                        Owner = member.Owner,
                        UserName = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(member.User_Id)).UserName,
                    });
                }
            }

            //Create View Model
            ViewManageGroupMembersViewModel model = new ViewManageGroupMembersViewModel()
            {
                GroupMembers = GroupMembersUserList.AsEnumerable(),
                IsLoggedInUserOwner = GroupMember.Owner
            };

            //Pass to view
            return View(model);
        }

        //POST Add member to group
        [HttpPost]
        [Route("groups/{groupid}/members")]
        public ActionResult AddMemberToGroup(string groupid)
        {
            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Check if username is empty
            string UserName = null;
            if (Request["username"] == null)
            {
                TempData["ErrorMessage"] = "No username entered";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }
            //Else not empty so save into var
            else
            {
                UserName = Request["username"].ToLower();
            }

            //Check username exists
            var AppUser = _context.Users.SingleOrDefault(n => n.UserName.ToLower().Equals(UserName));
            if (AppUser == null)
            {
                TempData["ErrorMessage"] = "Sorry, the user entered doesn't exist";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }

            //Check if not an employee
            if (!userManager.IsInRole(AppUser.Id, "Employee"))
            {
                TempData["ErrorMessage"] = "This user is not an employee, so cannot be added here";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }

            //Check if already in Group
            var GroupMemberAdd = _context.GroupMember.SingleOrDefault(n => (n.User_Id == AppUser.Id) && (n.Group_Id == GroupIdInt));
            if (GroupMemberAdd != null)
            {
                TempData["ErrorMessage"] = "User is already a member of the group";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }

            //Add to group
            _context.GroupMember.Add(new GroupMember { Group_Id = GroupIdInt, User_Id = AppUser.Id, Owner = false });
            _context.SaveChanges();

            //Return Redirect
            TempData["SuccessMessage"] = "User added to group";
            return RedirectToAction("ManageAndViewMembers", new { groupid });

        }

        //GET Remove a member from group
        [Route("groups/{groupid}/members/remove/{userid}")]
        public ActionResult RemoveMemberFromGroup(string UserId, string groupid)
        {
            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Get the Group Member
            var ToRemove = _context.GroupMember.SingleOrDefault(n => (n.User_Id == UserId) && (n.Group_Id == GroupIdInt));

            //Check combo actually exists
            if (ToRemove == null)
            {
                TempData["ErrorMessage"] = "Sorry, the Group member you attempted to remove does not exist for the given group";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }

            //Check not removing self
            if (ToRemove.User_Id.Equals(LoggedInId))
            {
                TempData["ErrorMessage"] = "Sorry, you cannot remove yourself";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }

            //Remove
            _context.GroupMember.Remove(ToRemove);
            _context.SaveChanges();

            //Return to same page TODO message
            //Create temp data session
            TempData["SuccessMessage"] = "User successfully removed from group";

            //Return to and pass it to the action
            return RedirectToAction("ManageAndViewMembers", new { groupid });
        }

        //GET Set or unset a user as owner
        [Route("groups/{groupid}/members/owner/{userid}")]
        public ActionResult SetUnsetGroupOwner(string UserId, string groupid)
        {
            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }


            //Get the Group Member
            var LookupUser = _context.GroupMember.SingleOrDefault(n => (n.User_Id == UserId) && (n.Group_Id == GroupIdInt));

            //Check combo actually exists
            if (LookupUser == null)
            {
                TempData["ErrorMessage"] = "Sorry, the Group member you attempted to remove does not exist for the given group";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }

            //Check not unsetting self
            if (LookupUser.User_Id.Equals(LoggedInId))
            {
                TempData["ErrorMessage"] = "Sorry, you cannot set or unset yourself";
                return RedirectToAction("ManageAndViewMembers", new { groupid });
            }

            //If Owner
            if (LookupUser.Owner == true)
            {
                LookupUser.Owner = false;
            }
            //Else if not an owner
            else
            {
                LookupUser.Owner = true;
            }

            //Save to DB
            _context.SaveChanges();

            //Return to Action
            TempData["SuccessMessage"] = "User set to owner";
            return RedirectToAction("ManageAndViewMembers", new { groupid });
        }

        /*****************
        * Knowledge Base
        * ***************/

        //GET View Knowledges of a group
        [HttpGet]
        [Route("groups/{groupid}/kbase/")]
        public ActionResult ViewKnowledges(string groupid)
        {
            //Handle messages
            HandleMessages();

            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Create the model
            var model = new ViewKnowledgesPageGroupViewModel()
            {
                Knowledges = _context.Knowledge.Where(n => n.Group_Id == GroupIdInt).AsEnumerable(),
                IsLoggedInUserOwner = GroupMember.Owner,
            };

            //Pass to view
            return View(model);
        }

        //GET Create Knowledge for a group
        [HttpGet]
        [Route("groups/{groupid}/kbase/create")]
        public ActionResult CreateKnowledgeGET(string groupid)
        {
            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //GO to view
            return View("CreateKnowledge");
        }

        //POST Create knowledge for a group
        [HttpPost]
        [Route("groups/{groupid}/kbase/create")]
        public ActionResult CreateKnowledgePOST(CreateKnowledgeGroupViewModel model, string groupid)
        {
            //If model valid
            if (ModelState.IsValid)
            {
                //Check group id is not null
                if (String.IsNullOrEmpty(groupid))
                {
                    TempData["ErrorMessage"] = "Error, no group has been specified";
                    return RedirectToAction("Index");
                }

                //Check group id is a number then cast to int
                if (!int.TryParse(groupid, out int GroupIdInt))
                {
                    TempData["ErrorMessage"] = "Error: Group ID incorrect";
                    return RedirectToAction("Index");
                }

                //Check group id exists
                var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
                if (Group == null)
                {
                    TempData["ErrorMessage"] = "Error: Group does not exist";
                    return RedirectToAction("Index");
                }

                //Check logged in user is a member of group
                var LoggedInId = User.Identity.GetUserId();
                var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
                if (GroupMember == null)
                {
                    TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                    return RedirectToAction("Index");
                }

                //Create the knowledge
                var DateTimeNow = DateTime.Now;
                var Knowledge = new Knowledge()
                {
                    Created = DateTimeNow,
                    Updated = DateTimeNow,
                    Description = model.Description,
                    Group_Id = GroupIdInt,
                    LastUpdatedByUserId = User.Identity.GetUserId(),
                    Summary = model.Summary
                };

                //Save the knowledge
                _context.Knowledge.Add(Knowledge);
                _context.SaveChanges();

                //Redirect
                TempData["SuccessMessage"] = "Knowledge Created";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }
            //Model not valid
            return View("CreateKnowledge", model);
        }

        //GET View a knowledge
        [Route("groups/{groupid}/kbase/view/{knowledgeid}")]
        public ActionResult ViewAKnowledge(string groupid, string knowledgeid)
        {
            //Handle Messages
            HandleMessages();

            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Check knowledge id is not null
            if (String.IsNullOrEmpty(knowledgeid))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check knowledge id is a number then cast to int
            if (!int.TryParse(knowledgeid, out int KnowledgeIdInt))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Get the knowledge
            var Knowledge = _context.Knowledge.SingleOrDefault(n => n.Id == KnowledgeIdInt);

            //Check if Knowledge exists
            if (Knowledge == null)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge doesn't exist";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check if knowledge is in group
            if (Knowledge.Group_Id != GroupIdInt)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge is not part of this group";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Create View Model
            var model = new ViewKnowledgeGroupViewModel()
            {
                Created = Knowledge.Created,
                Description = Knowledge.Description,
                GroupName = Knowledge.Group.Name,
                Group_Id = Knowledge.Group_Id,
                Id = Knowledge.Id,
                LastUpdatedByUserId = Knowledge.LastUpdatedByUserId,
                Summary = Knowledge.Summary,
                Updated = Knowledge.Updated,
                LastUpdatedByUserName = _context.Users.SingleOrDefault(n => n.Id.Equals(Knowledge.LastUpdatedByUserId)).UserName,
                IsLoggedInUserOwner = GroupMember.Owner
            };

            //Pass to view
            return View(model);
        }

        //GET Update Knowledge
        [HttpGet]
        [Route("groups/{groupid}/kbase/update/{knowledgeid}")]
        public ActionResult UpdateKnowledgeGET(string groupid, string knowledgeid)
        {
            //Handle Messages
            HandleMessages();

            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Check knowledge id is not null
            if (String.IsNullOrEmpty(knowledgeid))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check knowledge id is a number then cast to int
            if (!int.TryParse(knowledgeid, out int KnowledgeIdInt))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Get the knowledge
            var Knowledge = _context.Knowledge.SingleOrDefault(n => n.Id == KnowledgeIdInt);

            //Check if Knowledge exists
            if (Knowledge == null)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge doesn't exist";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check if knowledge is in group
            if (Knowledge.Group_Id != GroupIdInt)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge is not part of this group";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Create the view model
            var model = new UpdateKnowledgeGroupViewModel()
            {
                Id = Knowledge.Id,
                Summary = Knowledge.Summary,
                Description = Knowledge.Description
            };

            //Pass to view
            return View("UpdateKnowledge", model);
        }

        //POST Update Knowledge
        [HttpPost]
        [Route("groups/{groupid}/kbase/update/{knowledgeid}")]
        public ActionResult UpdateKnowledgePOST(string groupid, string knowledgeid, UpdateKnowledgeGroupViewModel model)
        {
            //If model valid
            if (ModelState.IsValid)
            {
                //Check group id is not null
                if (String.IsNullOrEmpty(groupid))
                {
                    TempData["ErrorMessage"] = "Error, no group has been specified";
                    return RedirectToAction("Index");
                }

                //Check group id is a number then cast to int
                if (!int.TryParse(groupid, out int GroupIdInt))
                {
                    TempData["ErrorMessage"] = "Error: Group ID incorrect";
                    return RedirectToAction("Index");
                }

                //Check group id exists
                var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
                if (Group == null)
                {
                    TempData["ErrorMessage"] = "Error: Group does not exist";
                    return RedirectToAction("Index");
                }

                //Check logged in user is a member of group
                var LoggedInId = User.Identity.GetUserId();
                var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
                if (GroupMember == null)
                {
                    TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                    return RedirectToAction("Index");
                }

                //Check knowledge id is not null
                if (String.IsNullOrEmpty(knowledgeid))
                {
                    TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Check knowledge id is a number then cast to int
                if (!int.TryParse(knowledgeid, out int KnowledgeIdInt))
                {
                    TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Get the knowledge
                var Knowledge = _context.Knowledge.SingleOrDefault(n => n.Id == KnowledgeIdInt);

                //Check if Knowledge exists
                if (Knowledge == null)
                {
                    TempData["ErrorMessage"] = "Error: That Knowledge doesn't exist";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Check if knowledge is in group
                if (Knowledge.Group_Id != GroupIdInt)
                {
                    TempData["ErrorMessage"] = "Error: That Knowledge is not part of this group";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Update Knowledge
                Knowledge.Summary = model.Summary;
                Knowledge.Description = model.Description;
                Knowledge.LastUpdatedByUserId = LoggedInId;
                Knowledge.Updated = DateTime.Now;

                //Save
                _context.SaveChanges();

                //Return to knowledge details
                TempData["SuccessMessage"] = "Knowledge Updated";
                return RedirectToAction("ViewAKnowledge", new { groupid, knowledgeid });
            }
            //Failed validation
            return View("UpdateKnowledge", model);
        }

        //GET Remove Knowledge
        [HttpGet]
        [Route("groups/{groupid}/kbase/remove/{knowledgeid}")]
        public ActionResult RemoveKnowledge(string groupid, string knowledgeid)
        {
            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Check knowledge id is not null
            if (String.IsNullOrEmpty(knowledgeid))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check knowledge id is a number then cast to int
            if (!int.TryParse(knowledgeid, out int KnowledgeIdInt))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Get the knowledge
            var Knowledge = _context.Knowledge.SingleOrDefault(n => n.Id == KnowledgeIdInt);

            //Check if Knowledge exists
            if (Knowledge == null)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge doesn't exist";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check if knowledge is in group
            if (Knowledge.Group_Id != GroupIdInt)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge is not part of this group";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check if owner
            if (!GroupMember.Owner)
            {
                TempData["ErrorMessage"] = "Error: Only an owner can remove knowledges from this group";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Remove from table
            _context.Knowledge.Remove(Knowledge);
            _context.SaveChanges();

            //Return to list
            TempData["SuccessMessage"] = "Knowledge removed";
            return RedirectToAction("ViewKnowledges", new { groupid });
        }

        //GET Notify Knowledge
        [HttpGet]
        [Route("groups/{groupid}/kbase/notify/{knowledgeid}")]
        public ActionResult NotifyKnowledgeGET(string groupid, string knowledgeid)
        {
            //Handle Messages
            HandleMessages();

            //Check group id is not null
            if (String.IsNullOrEmpty(groupid))
            {
                TempData["ErrorMessage"] = "Error, no group has been specified";
                return RedirectToAction("Index");
            }

            //Check group id is a number then cast to int
            if (!int.TryParse(groupid, out int GroupIdInt))
            {
                TempData["ErrorMessage"] = "Error: Group ID incorrect";
                return RedirectToAction("Index");
            }

            //Check group id exists
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
            if (Group == null)
            {
                TempData["ErrorMessage"] = "Error: Group does not exist";
                return RedirectToAction("Index");
            }

            //Check logged in user is a member of group
            var LoggedInId = User.Identity.GetUserId();
            var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
            if (GroupMember == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                return RedirectToAction("Index");
            }

            //Check knowledge id is not null
            if (String.IsNullOrEmpty(knowledgeid))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check knowledge id is a number then cast to int
            if (!int.TryParse(knowledgeid, out int KnowledgeIdInt))
            {
                TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Get the knowledge
            var Knowledge = _context.Knowledge.SingleOrDefault(n => n.Id == KnowledgeIdInt);

            //Check if Knowledge exists
            if (Knowledge == null)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge doesn't exist";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Check if knowledge is in group
            if (Knowledge.Group_Id != GroupIdInt)
            {
                TempData["ErrorMessage"] = "Error: That Knowledge is not part of this group";
                return RedirectToAction("ViewKnowledges", new { groupid });
            }

            //Create View Model
            var model = new NotifyKnowledgeViewModelPage()
            {
                GroupList = GetGroups(),
                UserList = GetNonDisabledEmployees()
            };

            //Return to view
            return View("NotifyKnowledge", model);
        }

        //POST Notify Knowledge
        [HttpPost]
        [Route("groups/{groupid}/kbase/notify/{knowledgeid}")]
        public ActionResult NotifyKnowledgePOST(string groupid, string knowledgeid, NotifyKnowledgeViewModelPage model)
        {
            //Populate View Model
            model.GroupList = GetGroups();
            model.UserList = GetNonDisabledEmployees();

            if (ModelState.IsValid)
            {
                //Check group id is not null
                if (String.IsNullOrEmpty(groupid))
                {
                    TempData["ErrorMessage"] = "Error, no group has been specified";
                    return RedirectToAction("Index");
                }

                //Check group id is a number then cast to int
                if (!int.TryParse(groupid, out int GroupIdInt))
                {
                    TempData["ErrorMessage"] = "Error: Group ID incorrect";
                    return RedirectToAction("Index");
                }

                //Check group id exists
                var Group = _context.Group.SingleOrDefault(n => n.Id == GroupIdInt);
                if (Group == null)
                {
                    TempData["ErrorMessage"] = "Error: Group does not exist";
                    return RedirectToAction("Index");
                }

                //Check logged in user is a member of group
                var LoggedInId = User.Identity.GetUserId();
                var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInId) && n.Group_Id == Group.Id);
                if (GroupMember == null)
                {
                    TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                    return RedirectToAction("Index");
                }

                //Check knowledge id is not null
                if (String.IsNullOrEmpty(knowledgeid))
                {
                    TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Check knowledge id is a number then cast to int
                if (!int.TryParse(knowledgeid, out int KnowledgeIdInt))
                {
                    TempData["ErrorMessage"] = "An error has occured regarding the Knowledge ID";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Get the knowledge
                var Knowledge = _context.Knowledge.SingleOrDefault(n => n.Id == KnowledgeIdInt);

                //Check if Knowledge exists
                if (Knowledge == null)
                {
                    TempData["ErrorMessage"] = "Error: That Knowledge doesn't exist";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Check if knowledge is in group
                if (Knowledge.Group_Id != GroupIdInt)
                {
                    TempData["ErrorMessage"] = "Error: That Knowledge is not part of this group";
                    return RedirectToAction("ViewKnowledges", new { groupid });
                }

                //Check if both fields set
                if ((!string.IsNullOrEmpty(model.Notify.Username)) && (!string.IsNullOrEmpty(model.Notify.GroupName)))
                {
                    ViewBag.ErrorMessage = "Please only select one resource";
                    return View("NotifyKnowledge", model);
                }

                //Check if both fields empty
                if ((string.IsNullOrEmpty(model.Notify.Username)) && (string.IsNullOrEmpty(model.Notify.GroupName)))
                {
                    ViewBag.ErrorMessage = "Please select a resource";
                    return View("NotifyKnowledge", model);
                }

                //If User is set
                if (!string.IsNullOrEmpty(model.Notify.Username))
                {
                    //Get the user ID
                    var ResourceUserId = _context.Users.SingleOrDefault(n => n.UserName.Equals(model.Notify.Username)).Id;

                    //Create Alert
                    var Alert = new Alert
                    {
                        FromUserId = User.Identity.GetUserId(),
                        ToUserId = ResourceUserId,
                        ToGroupId = null,
                        Text = model.Notify.Message,
                        AssociatedKnowledgeId = KnowledgeIdInt,
                        Created = DateTime.Now,
                        DismissedByUserId = null,
                    };
                    _context.Alert.Add(Alert);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = model.Notify.Username + " notified";

                }
                //Else Group is set
                else
                {
                    //Get the group ID
                    var ResourceGroupId = _context.Group.SingleOrDefault(n => n.Name.Equals(model.Notify.GroupName)).Id;

                    //Create Alert
                    var Alert = new Alert
                    {
                        FromUserId = User.Identity.GetUserId(),
                        ToUserId = null,
                        ToGroupId = ResourceGroupId,
                        Text = model.Notify.Message,
                        AssociatedKnowledgeId = KnowledgeIdInt,
                        Created = DateTime.Now,
                        DismissedWhen = null,
                        DismissedByUserId = null,

                    };

                    _context.Alert.Add(Alert);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = model.Notify.GroupName + " notified";

                }

                //Return to Knowledge
                return RedirectToAction("ViewAKnowledge", new { groupid, knowledgeid });
            }
            //Error so return to view
            return View("NotifyKnowledge", model);


        }

        /*****************
         * Helpers
         * ***************/

        //Error and success messages
        public void HandleMessages()
        {
            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                TempData.Remove("ErrorMessage");
            }

            //Check for a message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                TempData.Remove("SuccessMessage");
            }

            //Remove Tempdata TODO do for all others
        }

        [Route("groups/findaknowledge/{knowledgeid}")]
        public ActionResult FindKnowledgeURL(int knowledgeid)
        {
            //Get the knowledge
            var Knowledge = _context.Knowledge.SingleOrDefault(n => n.Id == knowledgeid);

            //Check it exists
            if(Knowledge == null)
            {
                return new HttpNotFoundResult();
            }

            //Get the group
            var GroupId = Knowledge.Group_Id;

            //GO to action
            return RedirectToAction("ViewAKnowledge", new { groupid = GroupId + "", knowledgeid });


        }

        //Get non-disabled employees as IEnumerable
        public IEnumerable<ApplicationUser> GetNonDisabledEmployees()
        {
            //Get the role ID for the Employee
            var roleId = roleManager.FindByName("Employee").Id;

            //Check the Users in the Roles table with the matching ID using LINQ
            var Employees = _context.Users.Where(n => n.Roles.Select(r => r.RoleId).Contains(roleId)).AsEnumerable();

            //Filter out disabled users and return
            return Employees.Where(n => n.Disabled == false);
        }

        //Get groups as IEnumberable
        public IEnumerable<Group> GetGroups()
        {
            return _context.Group.AsEnumerable();
        }
    }
}