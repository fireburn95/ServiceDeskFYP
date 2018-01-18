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

        // GET Admin Homepage
        public ActionResult Index()
        {
            return View();
        }

        /**************************
         *     Manage Employees   *
         * ***********************/

        //GET Employee Management Homepage
        //Shows the list of employees
        //If search GET param set, then filters list
        public ActionResult Employees()
        {
            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
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
            if (userManager.IsInRole(UserId, "Admin"))
                isAdmin = true;

            //Create ViewModel
            var ViewEmployee = new ViewAnEmployeeViewModel()
            {
                Id = Employee.Id,
                UserName = Employee.UserName,
                FirstName = Employee.FirstName,
                LastName = Employee.LastName,
                Department = Employee.Department,
                Email = Employee.Email,
                Extension = Employee.Extension,
                PhoneNumber = Employee.PhoneNumber,
                Admin = isAdmin,
                Disabled = Employee.Disabled
            };

            //Pass Employee View Model to View
            return View(ViewEmployee);
        }

        //GET Create Employee
        [HttpGet]
        [Route("admin/employees/create")]
        public ActionResult CreateEmployeeGET()
        {
            return View("CreateEmployee");
        }

        //POST Create Employee
        [HttpPost]
        [Route("admin/employees/create")]
        public ActionResult CreateEmployeePOST(CreateEmployeeViewModel model)
        {
            //If model validation passes
            if (ModelState.IsValid)
            {
                //Capitalising names correctly
                model.FirstName = ValidationHelpers.FirstLetterTOUpper(model.FirstName.ToLower());
                model.LastName = ValidationHelpers.FirstLetterTOUpper(model.LastName.ToLower());

                //Create User model
                var UserModel = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Extension = model.Extension,
                    Department = model.Department,
                    CreatedTimestamp = DateTime.Now
                };

                //Create User in DB
                var result = userManager.Create(UserModel, model.Password);


                if (result.Succeeded)
                {
                    //Add as an employee
                    userManager.AddToRole(UserModel.Id, "Employee");
                    return RedirectToAction("Employees");
                }
                else
                {
                    //Temp data
                    ViewBag.ErrorMessage = "Error - Cannot create.. Duplicate username/email";
                }
            }
            // If we got this far, something failed, redisplay form
            return View("CreateEmployee", model);
        }

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

        /**************************
         *     Manage Groups   *
         * ***********************/

        //View Groups
        public ActionResult Groups()
        {
            //Handle errors from other action methods
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Handle success message from other action methods
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Get all groups
            var Groups = _context.Group;

            //Pass groups to model
            return View(Groups);
        }

        //Create group page
        [HttpGet]
        [Route("admin/groups/create")]
        public ActionResult CreateGroupGET()
        {
            return View("CreateGroup");
        }

        //Create group POST page
        [HttpPost]
        [Route("admin/groups/create")]
        public ActionResult CreateGroupPOST(Group model)
        {
            if (ModelState.IsValid)
            {
                //Check if duplicate name
                var GroupFound = _context.Group.SingleOrDefault(n => n.Name == model.Name);
                if (GroupFound != null)
                {
                    //It's a duplicate, so error
                    ViewBag.ErrorMessage = "Sorry, the group name already exists";

                    //Return the View
                    return View("CreateGroup", model);
                }

                //Add to DB Group
                _context.Group.Add(model);
                _context.SaveChanges();

                return RedirectToAction("Groups");
            }
            else
            {
                //Failed validation
                return View("CreateGroup", model);
            }

        }

        //Edit Group GET page
        [HttpGet]
        [Route("admin/groups/edit/{GroupId}")]
        public ActionResult EditGroupGET(int GroupId)
        {
            //Get the group
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupId);

            //Check if doesnt exist
            if (Group == null)
            {
                //Create temp data session
                TempData["ErrorMessage"] = "Sorry, the group you attempted to access doesn't exist";

                //Return to and pass it to the action
                return RedirectToAction("Groups");
            }

            //Return view
            return View("EditGroup", Group);
        }

        //Edit Group POST page
        [HttpPost]
        [Route("admin/groups/edit/{GroupId}")]
        public ActionResult EditGroupPOST(Group model)
        {
            if (ModelState.IsValid)
            {
                //Check if duplicate name
                var GroupFound = _context.Group.SingleOrDefault(n => n.Name == model.Name);
                if (GroupFound != null)
                {
                    if(GroupFound.Id != model.Id)
                    {
                        //It's a duplicate, so error
                        ViewBag.ErrorMessage = "Sorry, the group name already exists";

                        //Return the View
                        return View("EditGroup", model);
                    }

                }

                //Update row in DB
                GroupFound.Name = model.Name;
                GroupFound.Description = model.Description;
                _context.SaveChanges();

                //Return to Groups page
                TempData["SuccessMessage"] = "Thank you, " + model.Name + " has been updated";
                return RedirectToAction("Groups");

            }

            //Validation failed so return
            return View("EditGroup", model);
        }

    }
}