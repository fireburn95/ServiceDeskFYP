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

        [HttpGet]
        [Route("groups/{groupid}")]
        public ActionResult GroupHome(string groupid)
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
            if (Group==null)
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
         * Helpers
         * ***************/

        //Error and success messages
        public void HandleMessages()
        {
            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Check for a message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
        }
    }
}