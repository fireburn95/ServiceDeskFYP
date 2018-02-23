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


        /*********************************
         * Index - View Subordinates list
         *********************************/

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

        /*********************************
         * View Employee and options
         *********************************/
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

        //View Subordinate's calls
        [Route("manager_centre/sub/{sub_username}/calls")]
        public ActionResult ViewSubordinateCalls(string sub_username, bool closed = false)
        {
            //Handle Messages
            HandleMessages();

            //Get logged in user id
            var LoggedInId = User.Identity.GetUserId();

            //Get Subordinate
            var Subordinate = _context.Users.SingleOrDefault(n => n.UserName.Equals(sub_username));

            //Check if exists
            if (Subordinate == null)
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

            //If No Closed parameter set, or is false
            IEnumerable<Call> Calls = null;
            if (!closed)
            {
                //Get the open calls of the subordinate
                Calls = _context.Call.Where(n => (n.ResourceUserId.Equals(Subordinate.Id)) && (n.Closed==false)).AsEnumerable();
            }
            else
            {
                //Get the closed calls of the subordinate
                Calls = _context.Call.Where(n => (n.ResourceUserId.Equals(Subordinate.Id)) && (n.Closed==true)).AsEnumerable();
            }

            //Pass Calls to model
            return View(Calls);
        }

        //Send an alert GET
        [HttpGet]
        [Route("manager_centre/sub/{sub_username}/alert")]
        public ActionResult SendAlertToSubGET(string sub_username)
        {
            //Handle Messages
            HandleMessages();

            //Get logged in user id
            var LoggedInId = User.Identity.GetUserId();

            //Get Subordinate
            var Subordinate = _context.Users.SingleOrDefault(n => n.UserName.Equals(sub_username));

            //Check if exists
            if (Subordinate == null)
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

            //Populate Username
            ViewBag.SubordinateUsername = sub_username;

            //Return view
            return View("SendAlertToSub");
        }

        //Send an alert POST
        [HttpPost]
        [Route("manager_centre/sub/{sub_username}/alert")]
        public ActionResult SendAlertToSubPOST(SendAlertToSubViewModel model, string sub_username)
        {
            //Populate Username
            ViewBag.SubordinateUsername = sub_username;

            if (ModelState.IsValid)
            {
                //Get logged in user id
                var LoggedInId = User.Identity.GetUserId();

                //Get Subordinate
                var Subordinate = _context.Users.SingleOrDefault(n => n.UserName.Equals(sub_username));

                //Check if exists
                if (Subordinate == null)
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

                //Create alert
                var Alert = new Alert()
                {
                    FromUserId = LoggedInId,
                    ToUserId = Subordinate.Id,
                    ToGroupId = null,
                    Text = model.Text,
                    AssociatedCallRef = null,
                    AssociatedKnowledgeId = null,
                    Created = DateTime.Now,
                    DismissedByUserId = null,
                    DismissedWhen = null
                };

                //Save alert
                _context.Alert.Add(Alert);
                _context.SaveChanges();

                //State success
                TempData["SuccessMessage"] = "Alert sent to " + Subordinate.UserName;
                return RedirectToAction("ViewSubordinate", new { sub_username = Subordinate.UserName });
            }

            //Failed validation so return view
            return View("SendAlertToSub", model);
        }


        /*********************************
         * Helpers
         *********************************/
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