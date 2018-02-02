using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceDeskFYP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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
        public ActionResult Index(string resource = null, string search = null, string sortcategory = null, string sortdirection = null, string closed = null)
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

            //Check whether CLOSED calls
            if (!string.IsNullOrEmpty(closed) && closed.Equals("true"))
            {
                Calls = Calls.Where(n => n.Closed == true);
            }
            else
            {
                Calls = Calls.Where(n => n.Closed == false);
            }


            //SEARCH through results for matching term
            if (!String.IsNullOrEmpty(search))
            {
                Calls = Calls.Where(n =>
                (!(String.IsNullOrEmpty(n.Reference)) && (n.Reference.ToLower().Contains(search.ToLower()))) ||
                (!(String.IsNullOrEmpty(n.SlaLevel)) && (n.SlaLevel.ToLower().Contains(search.ToLower()))) ||
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

            //SORT results
            if (!string.IsNullOrEmpty(sortcategory))
            {
                //If direction is null, set to asc
                if (string.IsNullOrEmpty(sortdirection))
                    sortdirection = "asc";

                //If descending
                if (sortdirection.Equals("desc"))
                {
                    if (sortcategory.Equals("reference"))
                        Calls = Calls.OrderByDescending(n => n.Reference);
                    else if (sortcategory.Equals("slalevel"))
                        Calls = Calls.OrderByDescending(n => n.SlaLevel);
                    else if (sortcategory.Equals("category"))
                        Calls = Calls.OrderByDescending(n => n.Category);
                    else if (sortcategory.Equals("created"))
                        Calls = Calls.OrderByDescending(n => n.Created);
                    else if (sortcategory.Equals("requiredby"))
                        Calls = Calls.OrderByDescending(n => n.Required_By);
                    else if (sortcategory.Equals("summary"))
                        Calls = Calls.OrderByDescending(n => n.Summary);
                    else if (sortcategory.Equals("firstname"))
                        Calls = Calls.OrderByDescending(n => n.FirstName);
                    else if (sortcategory.Equals("lastname"))
                        Calls = Calls.OrderByDescending(n => n.Lastname);
                }
                //Else ascending
                else
                {
                    if (sortcategory.Equals("reference"))
                        Calls = Calls.OrderBy(n => n.Reference);
                    else if (sortcategory.Equals("slalevel"))
                        Calls = Calls.OrderBy(n => n.SlaLevel);
                    else if (sortcategory.Equals("category"))
                        Calls = Calls.OrderBy(n => n.Category);
                    else if (sortcategory.Equals("created"))
                        Calls = Calls.OrderBy(n => n.Created);
                    else if (sortcategory.Equals("requiredby"))
                        Calls = Calls.OrderBy(n => n.Required_By == null).ThenBy(n => n.Required_By);
                    else if (sortcategory.Equals("summary"))
                        Calls = Calls.OrderBy(n => n.Summary);
                    else if (sortcategory.Equals("firstname"))
                        Calls = Calls.OrderBy(n => string.IsNullOrEmpty(n.FirstName)).ThenBy(n => n.FirstName);
                    else if (sortcategory.Equals("lastname"))
                        Calls = Calls.OrderBy(n => string.IsNullOrEmpty(n.Lastname)).ThenBy(n => n.Lastname);
                }
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

        /*****************
         * View a Call
         * ***************/

        [HttpGet]
        [Route("desk/call/{Reference}")]
        public ActionResult ViewCall(string Reference)
        {
            //Handle error and success messages
            HandleMessages();

            //Check Reference exists
            var CallExists = _context.Call.Where(n => n.Reference.Equals(Reference)).Any();
            if (!CallExists)
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Get the Call
            Call Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Make Call Details model
            CallDetailsForACallViewModel CallDetails = new CallDetailsForACallViewModel
            {
                Reference = Call.Reference,
                ResourceUserId = Call.ResourceUserId,
                ResourceGroupId = Call.ResourceGroupId,
                SlaPolicy = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).Name,
                SlaLevel = Call.SlaLevel,
                SlaExpiry = Call.Created, //TODO calculate expiry, must also make slaresettime = created upon call creation
                Category = Call.Category,
                Created = Call.Created,
                Required_By = Call.Required_By,
                SLAResetTime = Call.SLAResetTime,
                Summary = Call.Summary,
                Description = Call.Description,
                ForUserId = Call.ForUserId,
                Closed = Call.Closed,
                Hidden = Call.Hidden,
                LockedToUserId = Call.LockedToUserId,
                Email = Call.Email,
                FirstName = Call.FirstName,
                Lastname = Call.Lastname,
                PhoneNumber = Call.PhoneNumber,
                Extension = Call.Extension,
                OrganisationAlias = Call.OrganisationAlias,
                Organisation = Call.Organisation,
                Department = Call.Department,
                Regarding_Ref = Call.Regarding_Ref
            };

            //Get the Actions
            var Actions = _context.Action.Where(n => n.CallReference.Equals(Reference));
            ApplicationDbContext _context2 = new ApplicationDbContext();
            List<ActionDetailsForACallViewModel> ActionList = new List<ActionDetailsForACallViewModel>();
            foreach (var item in Actions)
            {
                ActionList.Add(new ActionDetailsForACallViewModel
                {
                    Id = item.Id,
                    ActionedByUserId = item.ActionedByUserId,
                    ActionedByUserName = _context2.Users.SingleOrDefault(n => n.Id.Equals(item.ActionedByUserId)).UserName,
                    CallReference = item.CallReference,
                    Created = item.Created,
                    Type = item.Type,
                    TypeDetails = item.TypeDetails,
                    Comments = item.Comments,
                    Attachment = item.Attachment
                });
            }

            //Construct the View Model
            ViewCallPageViewModel model = new ViewCallPageViewModel()
            {
                ActionsList = ActionList.AsEnumerable(),
                CallDetails = CallDetails
            };

            return View("ViewCallAndActions", model);

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