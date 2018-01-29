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
    public class DeskController : Controller
    {
        //Variables
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public DeskController()
        {
            _context = new ApplicationDbContext();
            userStore = new UserStore<ApplicationUser>(_context);
            userManager = new UserManager<ApplicationUser>(userStore);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        /*****************
         * View Calls
         * ***************/

        // GET: Desk
        public ActionResult Index(string resource = null, string search = null)
        {
            //Handle Messages
            HandleMessages();

            //Get User ID of logged in user
            var UserId = User.Identity.GetUserId();

            //Create the View Model
            DeskPageViewModel model = new DeskPageViewModel { GSVM = null, VCVMList = null };

            //Get Groups of User
            var GroupsOfUser = _context.GroupMember
                .Where(n => n.User_Id == UserId)
                .Select(n => new { n.Group_Id, n.Group.Name })
                .AsEnumerable();
            List<GroupsSelectViewModel> GSVMList = new List<GroupsSelectViewModel>();
            foreach (var item in GroupsOfUser)
            {
                GSVMList.Add(new GroupsSelectViewModel { Id = item.Group_Id, Name = item.Name });
            }
            model.GSVM = GSVMList.AsEnumerable();

            //Validate resource string
            var IsResourceInt = Int32.TryParse(resource, out int ResourceGroupId);

            //If resource not set or is any other string
            IEnumerable<Call> Calls = null;
            if (!IsResourceInt)
            {
                //Get the calls of the user
                Calls = _context.Call.Where(n => n.ResourceUserId == UserId).AsEnumerable();
            }
            //Else Resource is a group
            else
            {
                //Check group exists
                var groupexists = _context.Group.Where(n => n.Id == ResourceGroupId).Any();
                if (!groupexists)
                {
                    TempData["ErrorMessage"] = "Sorry, an error has occured, group doesn't exist";
                    return RedirectToAction("Index");
                }

                //Check permissions to access group
                var authorised = _context.GroupMember.Where(n => n.User_Id == UserId && n.Group_Id == ResourceGroupId).Any();
                if (!authorised)
                {
                    TempData["ErrorMessage"] = "Sorry, You are not authorised to access this group, please contact an administrator for access";
                    return RedirectToAction("Index");
                }

                //Get the calls of the group
                Calls = _context.Call.Where(n => n.ResourceGroupId == ResourceGroupId).AsEnumerable();
            }

            //TODO respond to sort and filter
            if (!String.IsNullOrEmpty(search))
            {
                Calls = Calls.Where(n =>
                (!(String.IsNullOrEmpty(n.Reference)) && (n.Reference.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Category)) && (n.Category.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Summary)) && (n.Summary.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Description)) && (n.Description.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Email)) && (n.Email.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.FirstName)) && (n.FirstName.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Lastname)) && (n.Lastname.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.PhoneNumber)) && (n.PhoneNumber.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Extension)) && (n.Extension.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.OrganisationAlias)) && (n.OrganisationAlias.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Organisation)) && (n.Organisation.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Department)) && (n.Department.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.Regarding_Ref)) && (n.Regarding_Ref.ToLower().Contains(search.ToLower())))
                );
            }


            //Set Calls to View Models
            List<ViewCallsViewModel> VCVM = new List<ViewCallsViewModel>();
            foreach (var item in Calls)
            {
                VCVM.Add(new ViewCallsViewModel
                {
                    Reference = item.Reference,
                    SlaLevel = item.SlaLevel,
                    Category = item.Category,
                    Created = item.Created,
                    Required_By = item.Required_By,
                    Summary = item.Summary,
                    Closed = item.Closed,
                    FirstName = item.FirstName,
                    Lastname = item.Lastname
                });
            }
            model.VCVMList = VCVM.AsEnumerable();



            return View("ViewCalls", model);
        }

        /*****************
         * Create Call
         * ***************/

        //GET Create call
        [HttpGet]
        [Route("desk/create")]
        public ActionResult CreateCallGET()
        {
            //Handle messages
            HandleMessages();

            //Set up select boxes
            SetUpCreateCall();

            //Display the form
            return View("CreateCall");
        }

        //POST Create Call
        [HttpPost]
        [Route("desk/create")]
        public ActionResult CreateCallPOST(CreateCallViewModel model)
        {
            //Set up View incase of error
            SetUpCreateCall();
            ViewBag.Reference = model.Reference;

            //If model is valid
            if (ModelState.IsValid)
            {
                //Check for duplicate reference
                var Call = _context.Call.SingleOrDefault(n => n.Reference == model.Reference);
                if (Call != null)
                {
                    ViewBag.ErrorMessage = "Sorry, that reference code already exists";
                    return View("CreateCall", model);
                }

                //Check Required By is not in the past TODO
                if (model.Required_By < DateTime.Now)
                {
                    ViewBag.ErrorMessage = "Sorry, 'Required By' cannot be set in the past";
                    return View("CreateCall", model);
                }

                //Find SLA ID
                ApplicationDbContext _context2 = new ApplicationDbContext();
                var SLAIdPOST = _context.SLAPolicy.SingleOrDefault(n => n.Name == model.SlaName).Id;

                //View Model to Call Mode
                Call NewCall = new Call
                {
                    Reference = model.Reference,
                    ResourceUserId = User.Identity.GetUserId(),
                    SlaId = SLAIdPOST,
                    SlaLevel = model.SlaLevel,
                    Category = model.Category,
                    Created = DateTime.Now,
                    Required_By = model.Required_By,
                    Summary = model.Summary,
                    Description = model.Description,
                    Hidden = model.Hidden,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    Lastname = model.Lastname,
                    PhoneNumber = model.PhoneNumber,
                    Extension = model.Extension,
                    OrganisationAlias = model.OrganisationAlias,
                    Organisation = model.Organisation,
                    Department = model.Department,
                    Regarding_Ref = model.Regarding_Ref
                };

                //Create an action for the call
                var Action = new Models.Action
                {
                    CallReference = model.Reference,
                    ActionedByUserId = User.Identity.GetUserId(),
                    Created = DateTime.Now,
                    Type = "Opened Call"
                };

                //Add to DB
                ApplicationDbContext _context3 = new ApplicationDbContext();
                _context3.Call.Add(NewCall);
                _context3.SaveChanges();
                _context3.Action.Add(Action);
                _context3.SaveChanges();

                //Return to Own Calls page or the actual Call TODO
                return RedirectToAction("index");


            }
            //Failed validation
            return View("CreateCall", model);
        }

        //Sets ViewBags for pre-populated form data
        public void SetUpCreateCall()
        {
            //Get SLA's
            var SLAPolicies = _context.SLAPolicy.Select(n => n.Name).AsEnumerable();
            ViewBag.SLAPolicies = SLAPolicies;

            //Get SLA Levels
            string[] levels = { "Low", "Medium", "High", "Task", "On-Going" };
            List<string> SLALevels = new List<string>(levels);
            ViewBag.SLALevels = SLALevels.AsEnumerable();

            //Populate Category, read and remove empties
            string[] categories = System.IO.File.ReadAllLines(Server.MapPath(@"~/Content/CallCategories.txt"));
            categories = categories.Where(n => !string.IsNullOrEmpty(n)).ToArray();
            List<string> Categories = new List<string>(categories);
            ViewBag.Categories = Categories.AsEnumerable();

            //Populate Reference code
            ViewBag.Reference = DistinctCallReference();
        }

        //Returns a distinct call reference by checking the DB
        public string DistinctCallReference()
        {
            //Random Class
            Random random = new Random();

            //Database
            ApplicationDbContext _dbcontext = new ApplicationDbContext();

            //Alpha Numeric characters
            const string alphanums = "abcdefghijklmnopqrstuvwxyz0123456789";

            //Declaring vars
            string reference = null;
            bool duplicate = true;
            Call call = null;

            while (duplicate)
            {
                //Generate a random string
                reference = new string(Enumerable.Repeat(alphanums, 12)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                //Check if Duplicate
                call = _dbcontext.Call.SingleOrDefault(n => n.Reference == reference);
                if (call == null)
                    duplicate = false;
            }

            //Return the distinct reference code
            return reference;
        }





        /*******************
         *     HELPERS
         ******************/

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