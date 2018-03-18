using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceDeskFYP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

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
            //Handle Messages
            HandleMessages();

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
            //Handle Messages
            HandleMessages();

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
            HandleMessages();

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
                model.FirstName = Helpers.FirstLetterTOUpper(model.FirstName.ToLower());
                model.LastName = Helpers.FirstLetterTOUpper(model.LastName.ToLower());

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
                    CreatedTimestamp = DateTime.Now,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                };

                //Create User in DB
                var result = userManager.Create(UserModel, model.Password);


                if (result.Succeeded)
                {
                    //Add as an employee
                    Helpers.LogEvent("Admin Action", "User has created the new employee '" + UserModel.UserName + "'");
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
                Helpers.LogEvent("Admin Action", "User has removed ' " + UserClass.UserName + "'s ' administrative privileges");
                userManager.RemoveFromRole(PassedId, "Admin");
            }
            //Else if not admin
            else
            {
                Helpers.LogEvent("Admin Action", "User has set ' " + UserClass.UserName + "' as an admin");
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
                Helpers.LogEvent("Admin Action", "User has re-enabled ' " + UserClass.UserName + "'s ' employee account");
                UserClass.Disabled = false;
            }
            //Else if not Disabled
            else
            {
                Helpers.LogEvent("Admin Action", "User has disabled ' " + UserClass.UserName + "'s ' employee account");
                UserClass.Disabled = true;
            }

            //TODO, there might be more to do when you disable a user such as Delete Managerships

            //Update Database
            _context.SaveChanges();

            //Return to page
            return RedirectToAction("ViewAnEmployee", new { UserId = PassedId });
        }

        // View Employees(manager's) subordinates
        [HttpGet]
        [Route("admin/employees/{UserId}/subordinates")]
        public ActionResult ManageSubordinates(string UserId)
        {
            //Handle Messages
            HandleMessages();

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

            //Get the list of users that are a subordinate
            var subUsers = _context.ManagerEmployee.Where(n => n.ManagerUserId.Equals(UserId)).AsEnumerable();
            List<ViewAnEmployeeOfSubordinateViewModel> Subordinates = new List<ViewAnEmployeeOfSubordinateViewModel>();

            //Save into a list
            using (ApplicationDbContext _context2 = new ApplicationDbContext())
            {
                ApplicationUser UserClass = null;
                foreach (var user in subUsers)
                {
                    UserClass = _context2.Users.SingleOrDefault(n => n.Id.Equals(user.SubUserId));

                    Subordinates.Add(new ViewAnEmployeeOfSubordinateViewModel
                    {
                        Id = UserClass.Id,
                        UserName = UserClass.UserName,
                        FirstName = UserClass.FirstName,
                        LastName = UserClass.LastName,
                        Email = UserClass.Email,
                        Department = UserClass.Department,
                        Disabled = UserClass.Disabled,
                        ManagerId = UserId
                    });
                }
            }

            //Pass to view
            return View("ManageSubordinates", Subordinates.AsEnumerable());
        }

        // POST add sub manager relationship record
        [HttpPost]
        [Route("admin/employees/{UserId}/subordinates")]
        public ActionResult AddSubordinateToManager(string UserId, string username = null)
        {
            //Check if username is empty
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "No username entered";
                return RedirectToAction("ManageSubordinates", new { UserId });
            }

            //Check username exists
            var user = _context.Users.SingleOrDefault(n => n.UserName.ToLower().Equals(username));
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sorry, the user entered doesn't exist";
                return RedirectToAction("ManageSubordinates", new { UserId });
            }

            //Check if not an employee
            if (!userManager.IsInRole(user.Id, "Employee"))
            {
                TempData["ErrorMessage"] = "This user is not an employee, so cannot be added here";
                return RedirectToAction("ManageSubordinates", new { UserId });
            }

            //Check if already a subordinate
            var ManagerEmployee = _context.ManagerEmployee.SingleOrDefault(n => (n.ManagerUserId == UserId) && (n.SubUserId == user.Id));
            if (ManagerEmployee != null)
            {
                TempData["ErrorMessage"] = "User is already a subordinate";
                return RedirectToAction("ManageSubordinates", new { UserId });
            }

            //Check if adding self
            if (UserId.Equals(user.Id))
            {
                TempData["ErrorMessage"] = "You cannot set a user to manage himself";
                return RedirectToAction("ManageSubordinates", new { UserId });
            }

            //Get the manager
            var ManagerUser = _context.Users.SingleOrDefault(n => n.Id.Equals(UserId));

            //Add to ManagerEmployee
            _context.ManagerEmployee.Add(new ManagerEmployee { ManagerUserId = UserId, SubUserId = user.Id });
            _context.SaveChanges();

            //Return Redirect
            Helpers.LogEvent("Admin Action", "User has added '" + user.UserName + "' as a subordinate to '" + ManagerUser.UserName + "'");
            TempData["SuccessMessage"] = "User is now a subordinate";
            return RedirectToAction("ManageSubordinates", new { UserId });
        }

        //Remove sub manager record
        public ActionResult RemoveSubordinate(string managerId, string subId)
        {
            //Get the ManagerEmployee Record
            var ManagerEmployee = _context.ManagerEmployee.SingleOrDefault(n => (n.ManagerUserId.Equals(managerId)) && (n.SubUserId.Equals(subId)));

            //Check combo actually exists
            if (ManagerEmployee == null)
            {
                //Create temp data session
                TempData["ErrorMessage"] = "Sorry, the Employee/Manager combo you attempted to remove does not exist";

                //Return to and pass it to the action
                return RedirectToAction("ManageSubordinates", new { UserId = managerId });
            }

            //Get the subordinate
            var subuser = _context.Users.SingleOrDefault(n => n.Id.Equals(subId));

            //Get the manager
            var manageruser = _context.Users.SingleOrDefault(n => n.Id.Equals(managerId));

            //Remove
            _context.ManagerEmployee.Remove(ManagerEmployee);
            _context.SaveChanges();

            //Create temp data session
            TempData["SuccessMessage"] = "Subordinate removed";
            Helpers.LogEvent("Admin Action", "User has removed '" + subuser.UserName + "' as a subordinate to '" + manageruser.UserName + "'");

            //Return to and pass it to the action
            return RedirectToAction("ManageSubordinates", new { UserId = managerId });
        }

        //Edit an employee GET
        [HttpGet]
        [Route("admin/employees/edit/{UserId}")]
        public ActionResult EditEmployeeGET(string UserId)
        {
            //Handle Messages
            HandleMessages();

            //Get the employee
            var Employee = _context.Users.FirstOrDefault(n => n.Id == UserId);

            //Check if doesn't exist
            if (Employee == null)
            {
                //Create temp session message
                TempData["ErrorMessage"] = "Sorry, the user you have attempted to access does not exist";

                //Redirect to Employee list
                return RedirectToAction("Employees");
            }

            //Check if employee is self
            if (Employee.Id.Equals(User.Identity.GetUserId()))
            {
                TempData["ErrorMessage"] = "Sorry, you cannot edit yourself via admin tools";
                return RedirectToAction("ViewAnEmployee", new { UserId });
            }

            //Convert to View Model
            var model = new EditAnEmployeeViewModel
            {
                Id = Employee.Id,
                UserName = Employee.UserName,
                Email = Employee.Email,
                FirstName = Employee.FirstName,
                LastName = Employee.LastName,
                Extension = Employee.Extension,
                Department = Employee.Department,
                Organisation = Employee.Organisation,
                OrganisationAlias = Employee.OrganisationAlias,
                PhoneNumber = Employee.PhoneNumber
            };

            //Pass view to model
            return View("EditEmployee", model);
        }

        //Edit an employee POST
        [HttpPost]
        [Route("admin/employees/edit/{UserId}")]
        public ActionResult EditEmployeePOST(string UserId, EditAnEmployeeViewModel model)
        {
            //If model passes validation
            if (ModelState.IsValid)
            {
                //Check user does not exist
                var Employee = _context.Users.SingleOrDefault(n => n.Id.Equals(model.Id));
                if (Employee == null)
                {
                    //Error message
                    ViewBag.ErrorMessage = "Sorry, an error has occured, user does not exist";

                    //Return to view
                    return View("EditEmployee", model);
                }

                //Check if employee is self
                if (Employee.Id.Equals(User.Identity.GetUserId()))
                {
                    TempData["ErrorMessage"] = "Sorry, you cannot edit yourself via admin tools";
                    return RedirectToAction("ViewAnEmployee", new { UserId });
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

                        return View("EditEmployee", model);
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

                        return View("EditEmployee", model);
                    }
                }

                //Validation on fields
                model.FirstName = Helpers.FirstLetterTOUpper(model.FirstName.ToLower());
                model.LastName = Helpers.FirstLetterTOUpper(model.LastName.ToLower());

                //Check if password set
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    //Hash and update the password
                    Employee.PasswordHash = userManager.PasswordHasher.HashPassword(model.NewPassword);

                }

                //Update the model
                Employee.UserName = model.UserName;
                Employee.Email = model.Email;
                Employee.FirstName = model.FirstName;
                Employee.LastName = model.LastName;
                Employee.PhoneNumber = model.PhoneNumber;
                Employee.Extension = model.Extension;
                Employee.OrganisationAlias = model.OrganisationAlias;
                Employee.Organisation = model.Organisation;
                Employee.Department = model.Department;
                _context.SaveChanges();

                //Return to view with a message
                Helpers.LogEvent("Admin Action", "User has edited the employee '" + model.UserName + "'");
                TempData["SuccessMessage"] = "User successfully updated";
                return RedirectToAction("ViewAnEmployee", new { UserId = model.Id });
            }

            //An error has occured/failed validation, return to view
            return View("EditEmployee", model);

        }

        /**************************
         *     Manage Groups      *
         * ***********************/

        //View Groups
        public ActionResult Groups()
        {
            //Handle Messages
            HandleMessages();

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
            HandleMessages();

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
                Helpers.LogEvent("Admin Action", "User has created the group '" + model.Name + "'");
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
            //Handle messages
            HandleMessages();

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
                Helpers.LogEvent("Admin Action", "User has edited the group '" + model.Name + "'");
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
            //Handle messages
            HandleMessages();

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

            //Get group
            var Group = _context.Group.SingleOrDefault(n => n.Id == GroupId);

            //Add to group
            Helpers.LogEvent("Admin Action", "User has added '" + User.UserName + "' to group '" + Group.Name + "'");
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

            //Get user
            var user = _context.Users.SingleOrDefault(n => n.Id.Equals(UserId));

            //Get group
            var group = _context.Group.SingleOrDefault(n => n.Id == GroupId);

            //Remove
            _context.GroupMember.Remove(GroupMember);
            _context.SaveChanges();

            //Return to same page TODO message
            //Create temp data session
            TempData["SuccessMessage"] = "User successfully removed from group";
            Helpers.LogEvent("Admin Action", "User has removed '" + user.UserName + "' from group '" + group.Name + "'");

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

            //Get user
            var user = _context.Users.SingleOrDefault(n => n.Id.Equals(UserId));

            //Get group
            var group = _context.Group.SingleOrDefault(n => n.Id == GroupId);

            //If Owner
            if (GroupMember.Owner == true)
            {
                Helpers.LogEvent("Admin Action", "User has unset '" + user.UserName + "' as an owner from group '" + group.Name + "'");
                GroupMember.Owner = false;
            }
            //Else if not an owner
            else
            {
                Helpers.LogEvent("Admin Action", "User has set '" + user.UserName + "' as an owner to group '" + group.Name + "'");
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
            //Handle messages
            HandleMessages();

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
            //Handle Messages
            HandleMessages();

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
                Helpers.LogEvent("Admin Action", "User has re-enabled the client '" + UserClass.UserName + "'");
                UserClass.Disabled = false;
            }
            //Else if not Disabled
            else
            {
                Helpers.LogEvent("Admin Action", "User has disabled the client '" + UserClass.UserName + "'");
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
            //Handle Messages
            HandleMessages();

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
                model.FirstName = Helpers.FirstLetterTOUpper(model.FirstName.ToLower());
                model.LastName = Helpers.FirstLetterTOUpper(model.LastName.ToLower());

                //Check if password set
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    //Hash and update the password
                    Client.PasswordHash = userManager.PasswordHasher.HashPassword(model.NewPassword);

                }

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
                Helpers.LogEvent("Admin Action", "User has edited the client '" + model.UserName + "'");
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
            //Handle Messages
            HandleMessages();

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
            //Handle Messages
            HandleMessages();

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
                if (model.LowMins <= 0 || model.MedMins <= 0 || model.HighMins <= 0)
                {
                    ViewBag.ErrorMessage = "Error: Non-positive integers are not allowed";
                    return View("CreateSLA");
                }

                //Create and save
                ApplicationDbContext _context2 = new ApplicationDbContext();
                _context2.SLAPolicy.Add(model);
                _context2.SaveChanges();
                Helpers.LogEvent("Admin Action", "User has created the SLA Policy '" + model.Name + "'");

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
            //Handle messages
            HandleMessages();

            //Check if SLA exists
            var SLA = _context.SLAPolicy.SingleOrDefault(n => n.Id == SlaId);
            if (SLA == null)
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
                    if (model.Id != SLA.Id)
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
                Helpers.LogEvent("Admin Action", "User has edited the SLA Policy '" + model.Name + "'");

                //Return to view SLA's
                return RedirectToAction("Sla");
            }

            //Not valid, return
            return View("EditSLA", model);
        }

        /**************************
         *     Manage Categories  *
         * ***********************/

        //View Categories GET
        [HttpGet]
        [Route("admin/categories")]
        public ActionResult CategoriesGET()
        {
            /****************************
             * Categories
             ****************************/
            //Read Categories from text file
            string[] categories = System.IO.File.ReadAllLines(Server.MapPath(@"~/Content/CallCategories.txt"));

            //Remove empties from Categories
            categories = categories.Where(n => !string.IsNullOrEmpty(n)).ToArray();

            //TODO Remove beginning and end whitespace from each

            /****************************
             * Action Types
             ****************************/
            //Read Action Types from text file
            string[] actiontypes = System.IO.File.ReadAllLines(Server.MapPath(@"~/Content/ActionTypes.txt"));

            //Remove empties from Categories
            actiontypes = actiontypes.Where(n => !string.IsNullOrEmpty(n)).ToArray();

            //TODO Remove beginning and end whitespace from each

            /******************************
             * Save into Jagged Array
             *******************************/
            string[][] data = new string[2][];
            data[0] = categories;
            data[1] = actiontypes;

            //Pass text to view
            return View("Categories", data);
        }

        //View Categories POST
        [HttpPost]
        [Route("admin/categories")]
        public ActionResult CategoriesPOST(string categories, string actiontypes)
        {
            //Check if POST data exists
            if (string.IsNullOrEmpty(categories))
            {
                ViewBag.ErrorMessage = "Error: No categories have been set";
                return View("Categories");
            }

            if (string.IsNullOrEmpty(actiontypes))
            {
                ViewBag.ErrorMessage = "Error: No action types have been set";
                return View("Categories");
            }

            /****************************
            * Categories
            ****************************/

            //Split into array
            string[] CategoryArray = categories.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            //Remove empties
            CategoryArray = CategoryArray.Where(n => !string.IsNullOrEmpty(n)).ToArray();

            //Remove Duplicates
            CategoryArray = CategoryArray.Distinct().ToArray();

            //Write to text file
            System.IO.File.WriteAllLines(Server.MapPath(@"~/Content/CallCategories.txt"), CategoryArray);

            //TODO remove empties from both sides in each line

            /****************************
            * Action Types
            ****************************/

            //Split into array
            string[] ActionTypesArray = actiontypes.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            //Remove empties
            ActionTypesArray = ActionTypesArray.Where(n => !string.IsNullOrEmpty(n)).ToArray();

            //Remove Duplicates
            ActionTypesArray = ActionTypesArray.Distinct().ToArray();

            //Write to text file
            System.IO.File.WriteAllLines(Server.MapPath(@"~/Content/ActionTypes.txt"), ActionTypesArray);

            //TODO remove empties from both sides in each line

            /****************************
            * Return to view
            ****************************/
            //Put data into array
            string[][] data = new string[2][];
            data[0] = CategoryArray;
            data[1] = ActionTypesArray;

            //Return
            ViewBag.SuccessMessage = "Changes made";
            Helpers.LogEvent("Admin Action", "User has updated the categories");
            return View("Categories", data);
        }

        /**************************
         *     Manage Logs        *
         * ***********************/

        //View Logs GET
        public ActionResult Logs(int page = 1, string type = null, string user = null)
        {
            //Handle messages
            HandleMessages();

            //Specify row count
            int rowcount = 15;

            //Handle negative page numbers
            if (page < 1)
            {
                page = 1;
            }

            //Get all the logs
            var Logs = _context.Log.OrderByDescending(n => n.Datetime).AsEnumerable();

            //Get a list of the types
            var Types = Logs.Select(n => n.Type).Distinct();

            //Filter type
            if (!string.IsNullOrEmpty(type))
            {
                Logs = Logs.Where(n => n.Type.Equals(type)).AsEnumerable();
            }

            //List of logs
            var LogsListForVM = new List<ViewLogsViewModel>();
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                ApplicationUser appuser = null;
                foreach (var item in Logs)
                {
                    appuser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.UserId));
                    LogsListForVM.Add(new ViewLogsViewModel
                    {
                        Id = item.Id,
                        Datetime = item.Datetime,
                        UserId = item.UserId,
                        Username = appuser?.UserName,
                        LocalIP = item.LocalIP,
                        PublicIP = item.PublicIP,
                        Type = item.Type,
                        Detail = item.Detail
                    });
                }
            }

            //Filter username
            if (!string.IsNullOrEmpty(user))
            {
                //Get rid of nulls
                LogsListForVM = LogsListForVM.Where(n => n.Username != null).ToList();

                //Matching fields
                LogsListForVM = LogsListForVM.Where(n => n.Username.ToLower().Equals(user.ToLower())).ToList();
            }

            //Limit
            LogsListForVM = LogsListForVM.Skip((page - 1) * rowcount).Take(rowcount).ToList();

            //Create model
            var model = new ViewLogsPageViewModel()
            {
                LogsList = LogsListForVM,
                Types = Types
            };

            //Return view
            return View(model);
        }

        /**************************
         *     Manage Calls        *
         * ***********************/

        public ActionResult Calls(string username = null, string groupname = null, string closed = null)
        {
            //Handle messages
            HandleMessages();

            //Create the empty View Model TODO
            List<ViewCallsAdminViewModel> model = new List<ViewCallsAdminViewModel>();

            IEnumerable<Call> Calls = null;

            //If both set
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(groupname))
            {
                ViewBag.ErrorMessage = "Error: both username and groupname set";
                return View(model);
            }
            //If both empty
            else if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(groupname))
            {
                return View(model);
            }
            //If username set
            else if (!string.IsNullOrEmpty(username))
            {
                //Check if user exists
                var User = _context.Users.SingleOrDefault(n => n.UserName.ToLower().Equals(username.ToLower()));
                if (User == null)
                {
                    ViewBag.ErrorMessage = "User doesn't exist";
                    return View(model);
                }

                //Check if user client
                if (userManager.IsInRole(User.Id, "client"))
                {
                    ViewBag.ErrorMessage = "User is a client";
                    return View(model);
                }

                //Get all Calls
                Calls = _context.Call;

                //Handle closed and open
                if (!string.IsNullOrEmpty(closed) && closed.Equals("true"))
                {
                    Calls = Calls.Where(n => n.Closed);
                }
                else
                {
                    Calls = Calls.Where(n => n.Closed == false);
                }

                //Filter by user
                Calls = Calls.Where(n => n.ResourceUserId != null && n.ResourceUserId.Equals(User.Id));
            }
            //If groupname set
            else if (!string.IsNullOrEmpty(groupname))
            {
                //Check if group exists
                var Group = _context.Group.SingleOrDefault(n => n.Name.ToLower().Equals(groupname.ToLower()));
                if (Group == null)
                {
                    ViewBag.ErrorMessage = "Group doesn't exist";
                    return View(model);
                }

                //Get all Calls
                Calls = _context.Call;

                //Handle closed and open
                if (!string.IsNullOrEmpty(closed) && closed.Equals("true"))
                {
                    Calls = Calls.Where(n => n.Closed);
                }
                else
                {
                    Calls = Calls.Where(n => n.Closed == false);
                }

                //Filter by group
                Calls = Calls.Where(n => n.ResourceGroupId != null && n.ResourceGroupId == Group.Id);
            }

            //To viewmodel
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                foreach (var call in Calls)
                {
                    model.Add(new ViewCallsAdminViewModel
                    {
                        Reference = call.Reference,
                        Created = call.Created,
                        SlaLevel = call.SlaLevel,
                        Summary = call.Summary,
                        Category = call.Category,
                        ResourceUserId = call.ResourceUserId,
                        ResourceGroupId = call.ResourceGroupId,
                        ResourceUserName = call.ResourceUserId == null ? null : dbcontext.Users.SingleOrDefault(n => n.Id.Equals(call.ResourceUserId)).UserName,
                        ResourceGroupName = call.ResourceGroupId == null ? null : dbcontext.Group.SingleOrDefault(n => n.Id == call.ResourceGroupId).Name,
                        
                        
                    });
                }
            }


            //Return view
            return View(model);



        }


        /**************************
         *     Helpers            *
         * ***********************/

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