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
    public class Manager_CentreController : Controller
    {
        //Helper vars
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        //Initialise via constructor
        public Manager_CentreController()
        {
            _context = new ApplicationDbContext();
            userStore = new UserStore<ApplicationUser>(_context);
            userManager = new UserManager<ApplicationUser>(userStore);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }


        /*
         * Index - View Subordinates list
         */

        // GET: Manager_Centre view employees
        public ActionResult Index()
        {
            //Handle messages
            HandleMessages();

            //Get logged in Users ID
            var LoggedInID = User.Identity.GetUserId();

            //Get subordinates Id
            var SubordinatesId = _context.ManagerEmployee.Where(n => n.ManagerUserId.Equals(LoggedInID)).Select(n => n.SubUserId);

            //Make List of Users
            List<ApplicationUser> SubordinatesList = new List<ApplicationUser>();

            //Get subordinate users
            using (ApplicationDbContext _context2 = new ApplicationDbContext())
            {
                foreach (var id in SubordinatesId)
                {
                    SubordinatesList.Add(_context2.Users.SingleOrDefault(n => n.Id.Equals(id)));
                }
            }

            //Pass to model
            return View(SubordinatesList.AsEnumerable());
        }

        /*
         * View Employee and options
         */
         // Handles no username specified
        [Route("manager_centre/sub")]
        public ActionResult Sub()
        {
            return RedirectToAction("Index");
        }

        //View Subordinate details page
        [Route("manager_centre/sub/{sub_username}")]
        public ActionResult ViewSubordinate(string sub_username)
        {
            //Handle Messages
            HandleMessages();

            //Get logged in user id
            var LoggedInId = User.Identity.GetUserId();

            //Get Subordinate
            var Subordinate = _context.Users.SingleOrDefault(n => n.UserName.Equals(sub_username));

            //Check if exists
            if(Subordinate == null)
            {
                TempData["ErrorMessage"] = "Sorry, the user you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Check if actually a subordinate
            var ManagerEmployee = _context.ManagerEmployee.SingleOrDefault(n => n.ManagerUserId.Equals(LoggedInId) && n.SubUserId.Equals(Subordinate.Id));
            if (ManagerEmployee == null)
            {
                TempData["ErrorMessage"] = "Sorry, you are not authorised to access this user";
                return RedirectToAction("Index");
            }

            //Pass to model
            return View(Subordinate);
        }

        /*
         * Helpers
         */
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