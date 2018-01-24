using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceDeskFYP.Models;
using System;
using System.Collections.Generic;
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

            //Update Database
            _context.SaveChanges();

            //Return to page
            return RedirectToAction("ViewAnEmployee", new { UserId = PassedId });
        }

        /**************************
         *     Manage Groups      *
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
                    if (GroupFound.Id != model.Id)
                    {
                        //It's a duplicate, so error
                        ViewBag.ErrorMessage = "Sorry, the group name already exists";

                        //Return the View
                        return View("EditGroup", model);
                    }
                }

                //Update row in DB
                ApplicationDbContext _context2 = new ApplicationDbContext();
                var Group = _context2.Group.SingleOrDefault(n => n.Id == model.Id);
                Group.Name = model.Name;
                Group.Description = model.Description;
                _context2.SaveChanges();

                //Return to Groups page
                TempData["SuccessMessage"] = "Thank you, " + model.Name + " has been updated";
                return RedirectToAction("Groups");
            }

            //Validation failed so return
            return View("EditGroup", model);
        }

        //Manage members GET page
        [HttpGet]
        [Route("admin/groups/members/{GroupId}")]
        public ActionResult ManageGroupMembers(int GroupId)
        {
            //Get the group
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupId);

            //Handle Message from another action
            if (TempData["SuccessMessage"] != null)
            {
                //Pass the message to ViewBag
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Handle Error Message from another action
            if (TempData["ErrorMessage"] != null)
            {
                //Pass the message to ViewBag
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Check if doesnt exist
            if (Group == null)
            {
                //Create temp data session
                TempData["ErrorMessage"] = "Sorry, the group you attempted to access doesn't exist";

                //Return to and pass it to the action
                return RedirectToAction("Groups");
            }

            //Get all group members
            var GroupMembers = _context.GroupMember.Where(n => n.Group_Id == GroupId);

            //If empty
            if (GroupMembers.Any() == false)
            {
                //Return the empty list view
                return View();
            }

            //Assign to View Model TODO if possible, make Username retrieved by using foreign key (wasnt working
            ApplicationDbContext _context2 = new ApplicationDbContext();
            List<ManageGroupMembersViewModel> GroupMembersVMList = new List<ManageGroupMembersViewModel>();
            ManageGroupMembersViewModel MGMVM;
            foreach (GroupMember row in GroupMembers)
            {
                MGMVM =
                    new ManageGroupMembersViewModel
                    {
                        Group_Id = row.Group_Id,
                        User_Id = row.User_Id,
                        Owner = row.Owner,
                        UserName = _context2.Users.SingleOrDefault(n => n.Id == row.User_Id).UserName,
                        GroupName = row.Group.Name
                    };

                GroupMembersVMList.Add(MGMVM);
            }

            //Return the view
            return View(GroupMembersVMList);
        }

        [HttpPost]
        [Route("admin/groups/members/{GroupId}")]
        public ActionResult AddMemberToGroup(int GroupId)
        {
            //Check if username is empty
            string UserName = null;
            if (Request["username"] == null)
            {
                TempData["ErrorMessage"] = "No username entered";
                return RedirectToAction("ManageGroupMembers", new { GroupId });
            }
            //Else not empty so save into var
            else
            {
                UserName = Request["username"].ToLower();
            }

            //Check username exists
            var User = _context.Users.SingleOrDefault(n => n.UserName.ToLower().Equals(UserName));
            if (User == null)
            {
                TempData["ErrorMessage"] = "Sorry, the user entered doesn't exist";
                return RedirectToAction("ManageGroupMembers", new { GroupId });
            }

            //Check if not an employee
            if (!userManager.IsInRole(User.Id, "Employee"))
            {
                TempData["ErrorMessage"] = "This user is not an employee, so cannot be added here";
                return RedirectToAction("ManageGroupMembers", new { GroupId });
            }

            //Check if already in Group
            var GroupMember = _context.GroupMember.SingleOrDefault(n => (n.User_Id == User.Id) && (n.Group_Id == GroupId));
            if (GroupMember != null)
            {
                TempData["ErrorMessage"] = "User is already a member of the group";
                return RedirectToAction("ManageGroupMembers", new { GroupId });
            }

            //Add to group
            _context.GroupMember.Add(new GroupMember { Group_Id = GroupId, User_Id = User.Id, Owner = false });
            _context.SaveChanges();

            //Return Redirect
            TempData["SuccessMessage"] = "User added to group";
            return RedirectToAction("ManageGroupMembers", new { GroupId });

        }

        public ActionResult RemoveMemberFromGroup(string UserId, int GroupId)
        {
            //Get the Group Member
            var GroupMember = _context.GroupMember.SingleOrDefault(n => (n.User_Id == UserId) && (n.Group_Id == GroupId));

            //Check combo actually exists
            if (GroupMember == null)
            {
                //Create temp data session
                TempData["ErrorMessage"] = "Sorry, the Group member you attempted to remove does not exist for the given group";

                //Return to and pass it to the action
                return RedirectToAction("Groups");
            }

            //Remove
            _context.GroupMember.Remove(GroupMember);
            _context.SaveChanges();

            //Return to same page TODO message
            //Create temp data session
            TempData["SuccessMessage"] = "User successfully removed from group";

            //Return to and pass it to the action
            return RedirectToAction("ManageGroupMembers", new { GroupId });
        }

        public ActionResult SetUnsetGroupOwner(string UserId, int GroupId)
        {
            //Get the Group Member
            var GroupMember = _context.GroupMember.SingleOrDefault(n => (n.User_Id == UserId) && (n.Group_Id == GroupId));

            //Check combo actually exists
            if (GroupMember == null)
            {
                //Create temp data session
                TempData["ErrorMessage"] = "Sorry, the Group member you attempted to remove does not exist for the given group";

                //Return to and pass it to the action
                return RedirectToAction("Groups");
            }

            //If Owner
            if (GroupMember.Owner == true)
            {
                GroupMember.Owner = false;
            }
            //Else if not an owner
            else
            {
                GroupMember.Owner = true;
            }

            //Save to DB
            _context.SaveChanges();

            //Return to Action
            TempData["SuccessMessage"] = "User set to owner";
            return RedirectToAction("ManageGroupMembers", new { GroupId });
        }

        /**************************
         *     Manage Clients     *
         * ***********************/

        //Manage Clients homepage
        public ActionResult Clients()
        {
            //Check for a Success message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Get the role ID for the Clients
            var roleId = roleManager.FindByName("Client").Id;

            //Check the Users in the Roles table with the matching ID using LINQ
            var Clients = _context.Users.Where(n => n.Roles.Select(r => r.RoleId).Contains(roleId)).ToList();


            //If GET Parameter 'search' is set
            if (Request.QueryString["search"] != null)
            {
                //Find clients that match search pattern
                var search = Request.QueryString["search"];
                var SearchResults = Clients.Where(n =>
                                n.UserName.ToLower().Contains(search.ToLower()) ||
                                n.FirstName.ToLower().Contains(search.ToLower()) ||
                                n.LastName.ToLower().Contains(search.ToLower()) ||
                                n.Email.ToLower().Contains(search.ToLower())
                                );

                //Return filtered list of Clients
                return View(SearchResults);
            }
            else
            {
                //Return the full list of clients
                return View(Clients);
            }
        }

        //GET View Details of a single Client
        [Route("admin/clients/{UserId}")]
        public ActionResult ViewAClient(string UserId)
        {
            //Check for a Success message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //If last visited page is available, store into viewbag
            if (Request.UrlReferrer != null)
                ViewBag.Referrer = Request.UrlReferrer.ToString();

            //Get the Client
            var Client = _context.Users.FirstOrDefault(n => n.Id == UserId);

            //Check if doesn't exist
            if (Client == null)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, the user you have attempted to access does not exist";

                //Redirect to Clients list
                return RedirectToAction("Clients");
            }

            //Create ViewModel
            var ViewClient = new ViewAClientViewModel()
            {
                Id = Client.Id,
                UserName = Client.UserName,
                FirstName = Client.FirstName,
                LastName = Client.LastName,
                Department = Client.Department,
                Email = Client.Email,
                Extension = Client.Extension,
                PhoneNumber = Client.PhoneNumber,
                Disabled = Client.Disabled,
                Organisation = Client.Organisation,
                OrganisationAlias = Client.OrganisationAlias
            };

            //Pass Client View Model to View
            return View(ViewClient);
        }

        //Disable/Enable a client
        public ActionResult DisableEnableClient(string PassedId)
        {
            //Check if parameter exists
            if (String.IsNullOrEmpty(PassedId))
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, an error has occured";

                //Redirect to Clients list
                return RedirectToAction("Clients");
            }

            //Find user
            var UserClass = _context.Users.SingleOrDefault(n => n.Id == PassedId);

            //Check if doesn't exist
            if (UserClass == null)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, the user you have attempted to access does not exist";

                //Redirect to Clients list
                return RedirectToAction("Clients");
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

            //TODO, may have to deal with assigned calls/alerts etc. or ignore?

            //Update Database
            _context.SaveChanges();

            //Return to page
            return RedirectToAction("ViewAClient", new { UserId = PassedId });
        }

        //Edit a client GET
        [HttpGet]
        [Route("admin/clients/edit/{UserId}")]
        public ActionResult EditClientGET(string UserId)
        {
            //Check for a Success message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Get the Client
            var Client = _context.Users.FirstOrDefault(n => n.Id == UserId);

            //Check if doesn't exist
            if (Client == null)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, the user you have attempted to access does not exist";

                //Redirect to Clients list
                return RedirectToAction("Clients");
            }

            //Convert to View Model
            var ClientVM = new EditAClientViewModel
            {
                Id = Client.Id,
                UserName = Client.UserName,
                Email = Client.Email,
                FirstName = Client.FirstName,
                LastName = Client.LastName,
                Extension = Client.Extension,
                Department = Client.Department,
                Organisation = Client.Organisation,
                OrganisationAlias = Client.OrganisationAlias,
                PhoneNumber = Client.PhoneNumber
            };

            //Pass view to model
            return View("EditClient", ClientVM);
        }

        //Edit a client POST
        [HttpPost]
        [Route("admin/clients/edit/{UserId}")]
        public ActionResult EditClientPOST(EditAClientViewModel model)
        {
            //If model passes validation
            if (ModelState.IsValid)
            {
                //Check user does not exist
                var Client = _context.Users.SingleOrDefault(n => n.Id.Equals(model.Id));
                if (Client == null)
                {
                    //Error message
                    ViewBag.ErrorMessage = "Sorry, an error has occured, user does not exist";

                    //Return to view
                    return View("EditClient", model);
                }

                //Check for duplicate username
                ApplicationDbContext _context2 = new ApplicationDbContext();
                var checkUsername = _context2.Users.SingleOrDefault(n => n.UserName == model.UserName);
                if (checkUsername != null)
                {
                    if (checkUsername.Id != model.Id)
                    {
                        //Duplicate so error
                        ViewBag.ErrorMessage = "Sorry, that username already exists";

                        return View("EditClient", model);
                    }
                }

                //Check for duplicate emails
                ApplicationDbContext _context3 = new ApplicationDbContext();
                var checkEmail = _context2.Users.SingleOrDefault(n => n.Email == model.Email);
                if (checkEmail != null)
                {
                    if (checkEmail.Id != model.Id)
                    {
                        //Duplicate so error
                        ViewBag.ErrorMessage = "Sorry, that email already exists";

                        return View("EditClient", model);
                    }
                }

                //Validation on fields
                model.FirstName = ValidationHelpers.FirstLetterTOUpper(model.FirstName.ToLower());
                model.LastName = ValidationHelpers.FirstLetterTOUpper(model.LastName.ToLower());

                //Update the model
                Client.UserName = model.UserName;
                Client.Email = model.Email;
                Client.FirstName = model.FirstName;
                Client.LastName = model.LastName;
                Client.PhoneNumber = model.PhoneNumber;
                Client.Extension = model.Extension;
                Client.OrganisationAlias = model.OrganisationAlias;
                Client.Organisation = model.Organisation;
                Client.Department = model.Department;
                _context.SaveChanges();

                //Return to view with a message
                TempData["SuccessMessage"] = "User successfully updated";
                return RedirectToAction("ViewAClient", new { UserId = model.Id });
            }

            //An error has occured/failed validation, return to view
            return View("EditClient", model);

        }

        /**************************
         *     Manage SLA's       *
         * ***********************/

        //SLA homepage
        public ActionResult Sla()
        {
            //Check for a Success message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Get the SLA's
            var SLAs = _context.SLAPolicy;

            //Pass to the view
            return View(SLAs);
        }

        //Create SLA GET
        [HttpGet]
        [Route("admin/sla/create")]
        public ActionResult CreateSLAGET()
        {
            //Check for a Success message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View("CreateSLA");
        }

        //Create SLA POST
        [HttpPost]
        [Route("admin/sla/create")]
        public ActionResult CreateSLAPOST(SLAPolicy model)
        {
            //If model passes validation
            if (ModelState.IsValid)
            {
                //Check name isn't duplicate
                var SLA = _context.SLAPolicy.SingleOrDefault(n => n.Name.ToLower() == model.Name.ToLower());
                if (SLA != null)
                {
                    ViewBag.ErrorMessage = "Sorry, that name is already taken";
                    return View("CreateSLA");
                }

                //Check if not low>med>high
                if (!(model.LowMins > model.MedMins) ||
                    !(model.MedMins > model.HighMins) ||
                    !(model.LowMins > model.HighMins))
                {
                    ViewBag.ErrorMessage = "Error: your ordering of minutes is incorrect";
                    return View("CreateSLA");
                }

                //Check if any values are negatives
                if(model.LowMins <= 0 || model.MedMins <= 0 || model.HighMins <= 0)
                {
                    ViewBag.ErrorMessage = "Error: Non-positive integers are not allowed";
                    return View("CreateSLA");
                }

                //Create and save
                ApplicationDbContext _context2 = new ApplicationDbContext();
                _context2.SLAPolicy.Add(model);
                _context2.SaveChanges();

                //Return to view SLA's
                return RedirectToAction("Sla");
            }

            //Not valid, return
            return View("CreateSLA");
        }

        //Edit SLA GET
        [HttpGet]
        [Route("admin/sla/edit/{SlaId}")]
        public ActionResult EditSLAGET(int SlaId)
        {
            //Check for a Success message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            //Check if SLA exists
            var SLA = _context.SLAPolicy.SingleOrDefault(n => n.Id == SlaId);
            if(SLA == null)
            {
                TempData["ErrorMessage"] = "Sorry, the SLA Policy you tried to access doesn't exist";
                return RedirectToAction("SLA");
            }

            //Pass to view
            return View("EditSLA", SLA);
        }

        //Edit SLA POST
        [HttpPost]
        [Route("admin/sla/edit/{SlaId}")]
        public ActionResult EditSLAPOST(SLAPolicy model)
        {
            //If model passes validation
            if (ModelState.IsValid)
            {
                //Check name isn't duplicate
                var SLA = _context.SLAPolicy.SingleOrDefault(n => n.Name.ToLower() == model.Name.ToLower());
                if (SLA != null)
                {
                    if(model.Id != SLA.Id)
                    {
                        ViewBag.ErrorMessage = "Sorry, that name is already taken";
                        return View("EditSLA", model);
                    }

                }

                //Check if not low>med>high
                if (!(model.LowMins > model.MedMins) ||
                    !(model.MedMins > model.HighMins) ||
                    !(model.LowMins > model.HighMins))
                {
                    ViewBag.ErrorMessage = "Error: your ordering of minutes is incorrect";
                    return View("EditSLA", model);
                }

                //Check if any values are negatives
                if (model.LowMins <= 0 || model.MedMins <= 0 || model.HighMins <= 0)
                {
                    ViewBag.ErrorMessage = "Error: Non-positive integers are not allowed";
                    return View("EditSLA", model);
                }

                //Update and save
                SLA.Name = model.Name;
                SLA.LowMins = model.LowMins;
                SLA.MedMins = model.MedMins;
                SLA.HighMins = model.HighMins;
                _context.SaveChanges();

                //Return to view SLA's
                return RedirectToAction("Sla");
            }

            //Not valid, return
            return View("EditSLA", model);
        }
    }
}