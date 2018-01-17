using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceDeskFYP.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ServiceDeskFYP.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        //Variables
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        //Constructor
        public AdminController()
        {
            _context = new ApplicationDbContext();
            userStore = new UserStore<ApplicationUser>(_context);
            userManager = new UserManager<ApplicationUser>(userStore);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        /*
         * Pages
         */

        // GET Admin Homepage
        public ActionResult Index()
        {
            return View();
        }

        //GET Employee Management Homepage
        //Shows the list of employees
        //If search GET param set, then filters list
        public ActionResult Employees()
        {
            //Check for an error message from another action
            if (TempData["ErrorMessage"]!=null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Get the role ID for the Employee
            var roleId = roleManager.FindByName("Employee").Id;

            //Check the Users in the Roles table with the matching ID using LINQ
            var Employees = _context.Users.Where(n => n.Roles.Select(r => r.RoleId).Contains(roleId)).ToList();

            //If GET Parameter 'search' is set
            if (Request.QueryString["search"] != null)
            {
                //TODO sanitise 'search'

                //Find employees that match search pattern
                var search = Request.QueryString["search"];
                var SearchResults = Employees.Where(n => 
                                n.UserName.ToLower().Contains(search.ToLower()) ||
                                n.FirstName.ToLower().Contains(search.ToLower()) ||
                                n.LastName.ToLower().Contains(search.ToLower()) ||
                                n.Email.ToLower().Contains(search.ToLower())
                                );

                //Return filtered list of employees
                return View(SearchResults);
            }
            else
            {
                //Return the full list of employees
                return View(Employees);
            }
        }

        // GET View Details of a single employee
        [Route("admin/employees/{UserId}")]
        public ActionResult ViewAnEmployee(string UserId)
        {
            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //If last visited page is available, store into viewbag
            if (Request.UrlReferrer != null)
                ViewBag.Referrer = Request.UrlReferrer.ToString();

            //Get the employee
            var Employee = _context.Users.FirstOrDefault(n => n.Id == UserId);

            //Check if doesn't exist
            if (Employee == null)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, the user you have attempted to access does not exist";

                //Redirect to Employees list
                return RedirectToAction("Employees");
            }
                

            //Check if admin
            var isAdmin = false;
            if(userManager.IsInRole(UserId, "Admin"))
                isAdmin = true;

            //Create ViewModel
            var ViewEmployee = new ViewAnEmployeeViewModel()
            {
                Id = Employee.Id, UserName = Employee.UserName,
                FirstName = Employee.FirstName, LastName = Employee.LastName,
                Department = Employee.Department, Email = Employee.Email,
                Extension = Employee.Extension, PhoneNumber = Employee.PhoneNumber,
                Admin = isAdmin, Disabled = Employee.Disabled
            };

            //Pass Employee View Model to View
            return View(ViewEmployee);
        }


        /*
         * Action links
         */

        // Set/Unset a given user as admin
        public ActionResult SetUnsetAdmin(string PassedId)
        {
            //Check if parameter exists
            if (String.IsNullOrEmpty(PassedId))
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, an error has occured";

                //Redirect to Employees list
                return RedirectToAction("Employees");
            }
            
            //Check if trying to change own account
            if (User.Identity.GetUserId() == PassedId)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, you cannot unset your own admin status";

                //Redirect to Employees list
                return RedirectToAction("ViewAnEmployee", new { UserId = PassedId });
            }

            //Find user
            var UserClass = _context.Users.SingleOrDefault(n => n.Id == PassedId);

            //Check if doesn't exist
            if (UserClass == null)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, the user you have attempted to access does not exist";

                //Redirect to Employees list
                return RedirectToAction("Employees");
            }

            //Change Admin setting
            //If admin
            if (userManager.IsInRole(PassedId, "Admin"))
            {
                userManager.RemoveFromRole(PassedId, "Admin");
            }
            //Else if not admin
            else
            {
                userManager.AddToRole(PassedId, "Admin");
            }

            //Return to page
            return RedirectToAction("ViewAnEmployee", new { UserId = PassedId });
        }

        // Disable/Enable a given user
        public ActionResult DisableEnable(string PassedId)
        {
            //Check if parameter exists
            if (String.IsNullOrEmpty(PassedId))
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, an error has occured";

                //Redirect to Employees list
                return RedirectToAction("Employees");
            }

            //Check if trying to change own account
            if (User.Identity.GetUserId() == PassedId)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, you cannot disable yourself";

                //Redirect to Employees list
                return RedirectToAction("ViewAnEmployee", new { UserId = PassedId });
            }

            //Find user
            var UserClass = _context.Users.SingleOrDefault(n => n.Id == PassedId);

            //Check if doesn't exist
            if (UserClass == null)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, the user you have attempted to access does not exist";

                //Redirect to Employees list
                return RedirectToAction("Employees");
            }

            //Change Disable
            //If Disabled
            if (UserClass.Disabled)
            {
                UserClass.Disabled = false;
            }
            //Else if not Disabled
            else
            {
                UserClass.Disabled = true;
            }

            //TODO, there might be more to do when you disable a user such as Delete Managerships
            //TODO, need to actually block disabled logins

            //Update Database
            _context.SaveChanges();

            //Return to page
            return RedirectToAction("ViewAnEmployee", new { UserId = PassedId });
        }
    }
}