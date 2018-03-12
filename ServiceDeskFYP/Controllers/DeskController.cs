using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using ServiceDeskFYP.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

//DEBUG LINE System.Diagnostics.Debug.WriteLine("HELLO");
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
        public ActionResult Index(string resource = null, string search = null, string sortcategory = null, string sortdirection = null, string closed = null, string urgent = null)
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

                model.GroupName = _context.Group.SingleOrDefault(n => n.Id == ResourceGroupId).Name;
            }

            //Check whether urgent calls TODO

            if (!string.IsNullOrEmpty(urgent) && urgent.Equals("true"))
            {
                List<Call> SlaExceededCalls = new List<Call>();
                //Getting SLA Exceeded calls
                using (ApplicationDbContext dbcontext = new ApplicationDbContext())
                {
                    int mins;
                    DateTime ExpiredDate;
                    foreach (Call item in Calls)
                    {
                        if (item.SlaLevel.Equals("Low"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).LowMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now)
                                SlaExceededCalls.Add(item);
                        }
                        else if (item.SlaLevel.Equals("Medium"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).MedMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now)
                                SlaExceededCalls.Add(item);
                        }
                        else if (item.SlaLevel.Equals("High"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).HighMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now)
                                SlaExceededCalls.Add(item);
                        }
                    }

                }

                //Exceeded required calls
                var ExceededRequiredCalls = Calls.Where(n => n.Required_By < DateTime.Now);

                //Add together and remove any duplicates
                Calls = SlaExceededCalls.Concat(ExceededRequiredCalls).Distinct();
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
                    Lastname = item.Lastname,
                    SlaId = item.SlaId,
                    SLAResetTime = item.SLAResetTime,
                });
            }

            //Mark Calls as urgent
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                int mins;
                DateTime ExpiredDate;
                foreach (var item in VCVM)
                {
                    //Check required by date
                    if (item.Required_By < DateTime.Now)
                    {
                        item.Urgent = true;
                    }
                    else
                    {
                        //Check SLA exceeded
                        if (item.SlaLevel.Equals("Low"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).LowMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now)
                                item.Urgent = true;
                        }
                        else if (item.SlaLevel.Equals("Medium"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).MedMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now)
                                item.Urgent = true;
                        }
                        else if (item.SlaLevel.Equals("High"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).HighMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now)
                                item.Urgent = true;
                        }
                    }

                }
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

                //Check Required By is not in the past
                if (model.Required_By < DateTime.Now)
                {
                    ViewBag.ErrorMessage = "Sorry, 'Required By' cannot be set in the past";
                    return View("CreateCall", model);
                }

                //Find SLA ID
                ApplicationDbContext _context2 = new ApplicationDbContext();
                var SLAIdPOST = _context.SLAPolicy.SingleOrDefault(n => n.Name == model.SlaName).Id;

                //Check SLA Level is set
                bool slaSet = false;
                if (model.SlaLevel.Equals("Low") || model.SlaLevel.Equals("Medium") || model.SlaLevel.Equals("High"))
                    slaSet = true;

                //View Model to Call Mode
                var DateTimeNow = DateTime.Now;
                Call NewCall = new Call
                {
                    Reference = model.Reference,
                    ResourceUserId = User.Identity.GetUserId(),
                    SlaId = SLAIdPOST,
                    SlaLevel = model.SlaLevel,
                    Category = model.Category,
                    Created = DateTimeNow,
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
                    Regarding_Ref = model.Regarding_Ref,
                };

                //Set SLAResetTime if necessary
                if (slaSet)
                    NewCall.SLAResetTime = DateTimeNow;

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

        //View Call page showing Call details and Actions for the call
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

            //Get the Call, and username and groupname for ease
            Call Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
            var resourceuser = _context.Users.SingleOrDefault(n => n.Id.Equals(Call.ResourceUserId));
            var resourcegroup = _context.Group.SingleOrDefault(n => n.Id == Call.ResourceGroupId);
            var foruser = _context.Users.SingleOrDefault(n => n.Id.Equals(Call.ForUserId));

            //Make Call Details model
            CallDetailsForACallViewModel CallDetails = new CallDetailsForACallViewModel
            {
                Reference = Call.Reference,
                ResourceUserId = Call.ResourceUserId,
                ResourceUserName = resourceuser?.UserName,
                ResourceGroupId = Call.ResourceGroupId,
                ResourceGroupName = resourcegroup?.Name,
                SlaPolicy = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).Name,
                SlaLevel = Call.SlaLevel,
                Category = Call.Category,
                Created = Call.Created,
                Required_By = Call.Required_By,
                SLAResetTime = Call.SLAResetTime,
                Summary = Call.Summary,
                Description = Call.Description,
                ForUserId = Call.ForUserId,
                ForUserName = foruser?.UserName,
                Closed = Call.Closed,
                Hidden = Call.Hidden,
                LockedToUserId = Call.LockedToUserId,
                LockedToUsername = Call.LockedToUserId != null ? _context.Users.SingleOrDefault(n => n.Id.Equals(Call.LockedToUserId)).UserName : null,
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

            //Get the number of minutes
            double mins = 0;
            bool hasSla = false;
            if (Call.SlaLevel.Equals("Low"))
            {
                mins = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).LowMins;
                hasSla = true;
            }
            else if (Call.SlaLevel.Equals("Medium"))
            {
                mins = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).MedMins;
                hasSla = true;
            }
            else if (Call.SlaLevel.Equals("High"))
            {
                mins = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).HighMins;
                hasSla = true;
            }

            //If there is an SLA Policy of low-high, set SLA Expiry time for view model
            if (hasSla)
                CallDetails.SlaExpiry = Call.SLAResetTime.Value.AddMinutes(mins);
            else
                CallDetails.SlaExpiry = null;

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
                    Attachment = item.Attachment != null ? true : false //bool so as to not pass file
                });
            }

            //Construct the View Model
            ViewCallPageViewModel model = new ViewCallPageViewModel()
            {
                ActionsList = ActionList.OrderByDescending(n => n.Created).AsEnumerable(),
                CallDetails = CallDetails
            };

            return View("ViewCallAndActions", model);

        }

        //Download file
        public ActionResult DownloadFileFromAction(int actionid, string Reference)
        {
            Models.Action Action = null;
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                //Get action
                Action = dbcontext.Action.SingleOrDefault(n => n.Id == actionid);
            }

            //Check action exists
            if (Action == null)
            {
                TempData["ErrorMessage"] = "Error: File not found";
                return RedirectToAction("ViewCall", new { Reference });
            }

            //Get path
            var path = Action.Attachment;

            //Check path string has something in it
            if (string.IsNullOrEmpty(path))
            {
                TempData["ErrorMessage"] = "Error: File not found";
                return RedirectToAction("ViewCall", new { Reference });
            }

            //Download
            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                string fileName = Path.GetFileName(path);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Error: File not found";
                return RedirectToAction("ViewCall", new { Reference });
            }


        }

        /*****************
         * Action a Call
         * ***************/

        //GET page for actioning a call
        [HttpGet]
        [Route("desk/call/{Reference}/action")]
        public ActionResult ActionCallGET(string Reference)
        {
            //Handle messages
            HandleMessages();

            //Check Reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call closed
            if (IsCallClosed(Reference))
            {
                TempData["ErrorMessage"] = "This call is closed";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call locked
            if (IsCallLockedToSomeoneElse(Reference))
            {
                var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                return RedirectToAction("call/" + Reference);
            }

            //Create Model
            CreateActionPageViewModel model = new CreateActionPageViewModel();

            //Populate Call Summary
            model.CallSummary = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).Summary;

            //Populate Action Types select box
            string[] categories = System.IO.File.ReadAllLines(Server.MapPath(@"~/Content/ActionTypes.txt"));
            model.ActionTypes = categories.Where(n => !string.IsNullOrEmpty(n)).ToArray().AsEnumerable();

            //Return view
            return View("Call_Action", model);
        }

        //POST page for actioning a call
        [HttpPost]
        [Route("desk/call/{Reference}/action")]
        public ActionResult ActionCallPOST(CreateActionPageViewModel model, string Reference)
        {
            //Fill Types
            string[] categories = System.IO.File.ReadAllLines(Server.MapPath(@"~/Content/ActionTypes.txt"));
            model.ActionTypes = categories.Where(n => !string.IsNullOrEmpty(n)).ToArray().AsEnumerable();

            //If model passes validation
            if (ModelState.IsValid)
            {
                //Check Reference exists
                if (!CheckReferenceExists(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                    return RedirectToAction("Index");
                }

                //Authorise access/permissions
                if (!ModifyCallAuthorisation(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call closed
                if (IsCallClosed(Reference))
                {
                    TempData["ErrorMessage"] = "This call is closed";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call locked
                if (IsCallLockedToSomeoneElse(Reference))
                {
                    var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                    var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                    TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                    return RedirectToAction("call/" + Reference);
                }

                //Check if there is an attachment
                string newPath = null;
                if ((model.CreateAction.Attachment != null) && (model.CreateAction.Attachment.ContentLength > 0))
                {
                    //Get the file
                    HttpPostedFileBase file = model.CreateAction.Attachment;

                    //Get the filename without extension
                    var filename = Path.GetFileNameWithoutExtension(file.FileName);

                    //Get the filename with extension
                    var filenameandextension = Path.GetFileName(file.FileName);

                    //Get the full path without extension
                    var path = Path.Combine(Server.MapPath("~/Content/actionfiles"), filename);

                    //Get the full path with extension
                    var pathwithextension = Path.Combine(Server.MapPath("~/Content/actionfiles"), filenameandextension);

                    //Get directory of file
                    var directory = Path.GetDirectoryName(path);

                    //Checking if file exists, if so append new number
                    int count = 1;
                    string tempfilename = null;
                    string extension = Path.GetExtension(pathwithextension);
                    newPath = pathwithextension;
                    if (System.IO.File.Exists(newPath))
                    {
                        while (System.IO.File.Exists(newPath))
                        {
                            tempfilename = string.Format("{0}({1})", filename, count++);
                            newPath = Path.Combine(directory, tempfilename + extension);
                        }
                    }

                    //Save the file in that path
                    file.SaveAs(newPath);
                }

                //Create the Action
                var loggedInUser = User.Identity.GetUserId();
                var Action = new Models.Action()
                {
                    Id = model.CreateAction.Id,
                    ActionedByUserId = loggedInUser,
                    CallReference = Reference,
                    Created = DateTime.Now,
                    Type = model.CreateAction.Type,
                    TypeDetails = null,
                    Comments = model.CreateAction.Comments,
                    Attachment = newPath ?? null,
                };

                //Add to DB
                _context.Action.Add(Action);
                _context.SaveChanges();

                //Get the call
                var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

                //If 'Send email' is true
                if (model.CreateAction.SendEmail == true)
                {
                    //Send email
                    string UrlOfCall = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + "/view/call/" + Reference;
                    string message =
                    "Your call has been updated<br /><br />" +
                    "<a href='" + UrlOfCall + "'>Please click here to view the details</a>";

                    //If 'email' column in Call is set
                    if (!string.IsNullOrEmpty(Call.Email))
                    {
                        SendEmail(Call.Email, "Your Call " + Reference + " has been updated", message);
                    }

                    //Otherwise there is no one to send an email to so ignore
                }

                //Redirect back to call
                TempData["SuccessMessage"] = "Your Action has been saved";
                return RedirectToAction("call/" + Reference);
            }

            //Error so return view
            return View("Call_Action", model);
        }

        /*****************
         * Assign a Call
         * ***************/

        //GET page for re-assigning a call
        [HttpGet]
        [Route("desk/call/{Reference}/assign")]
        public ActionResult AssignResourceGET(string Reference)
        {
            //Check reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call closed
            if (IsCallClosed(Reference))
            {
                TempData["ErrorMessage"] = "This call is closed";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call locked
            if (IsCallLockedToSomeoneElse(Reference))
            {
                var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                return RedirectToAction("call/" + Reference);
            }

            //Create View Model
            AssignResourcePageViewModel model = new AssignResourcePageViewModel()
            {
                UserList = GetNonDisabledEmployees(),
                GroupList = GetGroups(),
                CallSummary = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).Summary,
            };

            //Pass into view
            return View("Call_Assign", model);

        }

        //POST page for re-assigning a call
        [HttpPost]
        [Route("desk/call/{Reference}/assign")]
        public ActionResult AssignResourcePOST(AssignResourcePageViewModel model, string Reference)
        {
            //Populate model list fields
            model.UserList = GetNonDisabledEmployees();
            model.GroupList = GetGroups();

            //If model fields pass validation
            if (ModelState.IsValid)
            {
                //Check reference exists
                if (!CheckReferenceExists(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                    return RedirectToAction("Index");
                }

                //Authorise access/permissions
                if (!ModifyCallAuthorisation(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, you do not have the permissions to re-assign this call, please contact the resource";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call closed
                if (IsCallClosed(Reference))
                {
                    TempData["ErrorMessage"] = "This call is closed";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call locked
                if (IsCallLockedToSomeoneElse(Reference))
                {
                    var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                    var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                    TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                    return RedirectToAction("call/" + Reference);
                }

                //Check if both fields set
                if ((!string.IsNullOrEmpty(model.SelectResource.Username)) && (!string.IsNullOrEmpty(model.SelectResource.GroupName)))
                {
                    ViewBag.ErrorMessage = "Please only select one resource";
                    return View("Call_Assign", model);
                }

                //Check if both fields empty
                if ((string.IsNullOrEmpty(model.SelectResource.Username)) && (string.IsNullOrEmpty(model.SelectResource.GroupName)))
                {
                    ViewBag.ErrorMessage = "Please select a resource";
                    return View("Call_Assign", model);
                }

                //If User is set
                if (!string.IsNullOrEmpty(model.SelectResource.Username))
                {
                    //Get the user ID
                    var ResourceUserId = _context.Users.SingleOrDefault(n => n.UserName.Equals(model.SelectResource.Username)).Id;

                    //Check if resource already same
                    var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
                    if (Call.ResourceUserId == ResourceUserId)
                    {
                        ViewBag.ErrorMessage = "The selected user is already assigned to this call";
                        return View("Call_Assign", model);
                    }

                    //Clear current call resource
                    Call.ResourceUserId = null;
                    Call.ResourceGroupId = null;

                    //Set the resource
                    Call.ResourceUserId = ResourceUserId;

                    //Save
                    _context.SaveChanges();

                    //Create Action
                    var ActionMade = new Models.Action()
                    {
                        CallReference = Reference,
                        ActionedByUserId = User.Identity.GetUserId(),
                        Created = DateTime.Now,
                        Attachment = null,
                        Type = "Assigned",
                        TypeDetails = model.SelectResource.Username,
                        Comments = null
                    };

                    //Save action
                    _context.Action.Add(ActionMade);
                    _context.SaveChanges();
                }
                //Else Group is set
                else
                {
                    //Get the group ID
                    var ResourceGroupId = _context.Group.SingleOrDefault(n => n.Name.Equals(model.SelectResource.GroupName)).Id;

                    //Check if resource already same
                    var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
                    if (Call.ResourceGroupId.Equals(ResourceGroupId))
                    {
                        ViewBag.ErrorMessage = "The selected group is already assigned to this call";
                        return View("Call_Assign", model);
                    }

                    //Clear current resources
                    Call.ResourceUserId = null;
                    Call.ResourceGroupId = null;

                    //Set the resource
                    Call.ResourceGroupId = ResourceGroupId;

                    //Save
                    _context.SaveChanges();

                    //Create Action
                    var ActionMade = new Models.Action()
                    {
                        CallReference = Reference,
                        ActionedByUserId = User.Identity.GetUserId(),
                        Created = DateTime.Now,
                        Attachment = null,
                        Type = "Assigned",
                        TypeDetails = model.SelectResource.GroupName,
                        Comments = null
                    };

                    //Save action
                    _context.Action.Add(ActionMade);
                    _context.SaveChanges();
                }

                //Return to Call
                TempData["SuccessMessage"] = "Call assigned to new resource";
                return RedirectToAction("call/" + Reference);

            }

            //Failed validation
            return View("Call_Assign", model);
        }

        /*****************
         * Notify User/Group
         * ***************/

        //GET page for notifying a user/group
        [HttpGet]
        [Route("desk/call/{Reference}/notify")]
        public ActionResult NotifyGET(string Reference)
        {
            //Check reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Create View Model
            NotifyPageViewModel model = new NotifyPageViewModel()
            {
                UserList = GetNonDisabledEmployees(),
                GroupList = GetGroups(),
                CallSummary = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).Summary,
            };

            //Pass into view
            return View("Call_Notify", model);

        }

        //POST page for notifying a user/group
        [HttpPost]
        [Route("desk/call/{Reference}/notify")]
        public ActionResult NotifyPOST(NotifyPageViewModel model, string Reference)
        {
            //Populate model list fields
            model.UserList = GetNonDisabledEmployees();
            model.GroupList = GetGroups();

            //If model fields pass validation
            if (ModelState.IsValid)
            {
                //Check reference exists
                if (!CheckReferenceExists(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                    return RedirectToAction("Index");
                }

                //Check if both fields set
                if ((!string.IsNullOrEmpty(model.Notify.Username)) && (!string.IsNullOrEmpty(model.Notify.GroupName)))
                {
                    ViewBag.ErrorMessage = "Please only select one resource";
                    return View("Call_Notify", model);
                }

                //Check if both fields empty
                if ((string.IsNullOrEmpty(model.Notify.Username)) && (string.IsNullOrEmpty(model.Notify.GroupName)))
                {
                    ViewBag.ErrorMessage = "Please select a resource";
                    return View("Call_Notify", model);
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
                        AssociatedCallRef = Reference,
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
                        AssociatedCallRef = Reference,
                        Created = DateTime.Now,
                        DismissedWhen = null,
                        DismissedByUserId = null,

                    };

                    _context.Alert.Add(Alert);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = model.Notify.GroupName + " notified";

                }

                //Return to Call
                return RedirectToAction("call/" + Reference);

            }

            //Failed validation
            return View("Call_Notify", model);
        }

        /*****************
         * Reset SLA
         * ***************/

        //GET page for re-setting an SLA
        [HttpGet]
        [Route("desk/call/{Reference}/sla")]
        public ActionResult ResetSlaGET(string Reference)
        {
            //Check reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call closed
            if (IsCallClosed(Reference))
            {
                TempData["ErrorMessage"] = "This call is closed";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call locked
            if (IsCallLockedToSomeoneElse(Reference))
            {
                var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                return RedirectToAction("call/" + Reference);
            }

            //Create View Model
            ResetSLAPageViewModel model = new ResetSLAPageViewModel
            {
                SLAPolicies = _context.SLAPolicy.AsEnumerable(),
                SLALevels = new List<String> { "Low", "Medium", "High", "Task", "On-Going" },
                CallSummary = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).Summary
            };

            //Return to view
            return View("Call_ResetSLA", model);
        }

        //POST page for re-setting an SLA
        [HttpPost]
        [Route("desk/call/{Reference}/sla")]
        public ActionResult ResetSlaPOST(ResetSLAPageViewModel model, string Reference)
        {
            //Populate SLA Policies in model
            model.SLAPolicies = _context.SLAPolicy.AsEnumerable();

            //Populate SLA Levels in model
            model.SLALevels = new List<String> { "Low", "Medium", "High", "Task", "On-Going" };

            //If model passes validation
            if (ModelState.IsValid)
            {
                //Check reference exists
                if (!CheckReferenceExists(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                    return RedirectToAction("Index");
                }

                //Authorise access/permissions
                if (!ModifyCallAuthorisation(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call closed
                if (IsCallClosed(Reference))
                {
                    TempData["ErrorMessage"] = "This call is closed";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call locked
                if (IsCallLockedToSomeoneElse(Reference))
                {
                    var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                    var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                    TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                    return RedirectToAction("call/" + Reference);
                }

                //Get the SLA ID
                var SlaId = _context.SLAPolicy.SingleOrDefault(n => n.Name.Equals(model.SLAForm.SLAPolicyName)).Id;

                //Get the call
                ApplicationDbContext _context2 = new ApplicationDbContext();
                var Call = _context2.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

                //Update relevant fields
                var CurrentPolicy = _context.SLAPolicy.Single(n => n.Id == SlaId).Name;
                var CurrentLevel = Call.SlaLevel;
                Call.SlaId = SlaId;
                Call.SlaLevel = model.SLAForm.SLALevel;
                Call.SLAResetTime = DateTime.Now;

                //Save changes
                _context2.SaveChanges();

                //Create Action
                var ActionMade = new Models.Action
                {
                    CallReference = Reference,
                    ActionedByUserId = User.Identity.GetUserId(),
                    Created = DateTime.Now,
                    Type = "SLA Reset",
                    TypeDetails = "From [" + CurrentPolicy + " - " + CurrentLevel + "] to [" + model.SLAForm.SLAPolicyName + " - " + model.SLAForm.SLALevel + "]",
                    Comments = null,
                    Attachment = null,
                };

                //Add Alert to DB and Save changes
                _context.Action.Add(ActionMade);
                _context.SaveChanges();

                //Return to Call
                TempData["SuccessMessage"] = "SLA has been reset";
                return RedirectToAction("call/" + Reference);
            }

            //Failed validation
            return View("Call_ResetSLA", model);
        }

        /*****************
         * Close/Open a Call
         * ***************/

        //GET page for re-setting an SLA
        [HttpGet]
        public ActionResult CloseOpenCall(string Reference)
        {
            //Check reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to close this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call locked
            if (IsCallLockedToSomeoneElse(Reference))
            {
                var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                return RedirectToAction("call/" + Reference);
            }

            //Get the call
            var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Update closed field and save
            if (Call.Closed == true)
                Call.Closed = false;
            else
                Call.Closed = true;
            _context.SaveChanges();

            //Create Action and save
            var ActionMade = new Models.Action
            {
                CallReference = Reference,
                Created = DateTime.Now,
                Type = Call.Closed ? "Call Closed" : "Call Re-Opened",
                TypeDetails = null,
                Comments = null,
                Attachment = null,
                ActionedByUserId = User.Identity.GetUserId()
            };
            _context.Action.Add(ActionMade);
            _context.SaveChanges();

            //Success Message
            TempData["SuccessMessage"] = Call.Closed ? "Call closed" : "Call-Reopened";

            //Return to call
            return RedirectToAction("call/" + Reference);
        }

        /*****************
         * Edit Call
         * ***************/

        //GET page for Editing a Call
        [HttpGet]
        [Route("desk/call/{Reference}/edit")]
        public ActionResult EditCallGET(string Reference)
        {
            //Handle Messages
            HandleMessages();

            //Check reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to close this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call closed
            if (IsCallClosed(Reference))
            {
                TempData["ErrorMessage"] = "This call is closed";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call locked
            if (IsCallLockedToSomeoneElse(Reference))
            {
                var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                return RedirectToAction("call/" + Reference);
            }

            //Create the view model
            EditCallPageViewModel model = new EditCallPageViewModel();

            //Populate Category, read and remove empties
            string[] categories = System.IO.File.ReadAllLines(Server.MapPath(@"~/Content/CallCategories.txt"));
            categories = categories.Where(n => !string.IsNullOrEmpty(n)).ToArray();
            model.Categories = categories.AsEnumerable();

            //Get the call
            var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Populate Edit Call fields
            model.EditCall = new EditCallViewModel
            {
                Reference = Call.Reference,
                Category = Call.Category,
                Required_By = Call.Required_By,
                Summary = Call.Summary,
                Description = Call.Description,
                Hidden = Call.Hidden,
                Email = Call.Email,
                FirstName = Call.FirstName,
                Lastname = Call.Lastname,
                PhoneNumber = Call.PhoneNumber,
                Extension = Call.Extension,
                OrganisationAlias = Call.OrganisationAlias,
                Organisation = Call.Organisation,
                Department = Call.Department,
                Regarding_Ref = Call.Regarding_Ref,
                EditComments = null
            };

            //Set Summary
            model.CallSummary = Call.Summary;

            //Pass to view
            return View("Call_EditCall", model);
        }

        //POST page for Editing a Call
        [HttpPost]
        [Route("desk/call/{Reference}/edit")]
        public ActionResult EditCallPOST(EditCallPageViewModel model, string Reference)
        {
            //Populate Category, read and remove empties
            string[] categories = System.IO.File.ReadAllLines(Server.MapPath(@"~/Content/CallCategories.txt"));
            categories = categories.Where(n => !string.IsNullOrEmpty(n)).ToArray();
            model.Categories = categories.AsEnumerable();

            //If validated
            if (ModelState.IsValid)
            {
                //Check reference exists
                if (!CheckReferenceExists(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                    return RedirectToAction("Index");
                }

                //Authorise access/permissions
                if (!ModifyCallAuthorisation(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, you do not have the permissions to close this call, please contact the resource";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call closed
                if (IsCallClosed(Reference))
                {
                    TempData["ErrorMessage"] = "This call is closed";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call locked
                if (IsCallLockedToSomeoneElse(Reference))
                {
                    var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                    var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                    TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                    return RedirectToAction("call/" + Reference);
                }

                //Check Required By is not in the past
                if (model.EditCall.Required_By < DateTime.Now)
                {
                    ViewBag.ErrorMessage = "Sorry, 'Required By' cannot be set in the past";
                    return View("Call_EditCall", model);
                }

                //Get the call
                var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

                //Update the fields and save
                Call.Category = model.EditCall.Category;
                Call.Required_By = model.EditCall.Required_By;
                Call.Summary = model.EditCall.Summary;
                Call.Description = model.EditCall.Description;
                Call.Hidden = model.EditCall.Hidden;
                Call.Email = model.EditCall.Email;
                Call.FirstName = model.EditCall.FirstName;
                Call.Lastname = model.EditCall.Lastname;
                Call.PhoneNumber = model.EditCall.Extension;
                Call.Extension = model.EditCall.Extension;
                Call.OrganisationAlias = model.EditCall.OrganisationAlias;
                Call.Organisation = model.EditCall.Organisation;
                Call.Department = model.EditCall.Department;
                Call.Regarding_Ref = model.EditCall.Regarding_Ref;
                _context.SaveChanges();

                //Create Action
                var ActionMade = new Models.Action
                {
                    CallReference = Reference,
                    Type = "Edited Details",
                    TypeDetails = null,
                    Comments = model.EditCall.EditComments,
                    ActionedByUserId = User.Identity.GetUserId(),
                    Created = DateTime.Now,
                    Attachment = null
                };
                _context.Action.Add(ActionMade);
                _context.SaveChanges();

                //Return to Call
                TempData["SuccessMessage"] = "Changes saved";
                return RedirectToAction("call/" + Reference);
            }

            //Failed validation
            return View("Call_EditCall", model);
        }

        /*****************
         * Lock
         * ***************/

        public ActionResult LockCall(string Reference)
        {
            //Check reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to close this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call closed
            if (IsCallClosed(Reference))
            {
                TempData["ErrorMessage"] = "This call is closed";
                return RedirectToAction("call/" + Reference);
            }

            //Get the call
            var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Check if it is already locked
            if (Call.LockedToUserId != null)
            {
                TempData["ErrorMessage"] = "This call is already locked";
                return RedirectToAction("call/" + Reference);
            }

            //Lock it and save
            Call.LockedToUserId = User.Identity.GetUserId();
            _context.SaveChanges();

            //Return to call
            TempData["SuccessMessage"] = "Call Locked";
            return RedirectToAction("ViewCall", new { Reference });
        }

        [HttpGet]
        public ActionResult ClearLock(string Reference)
        {
            //Check reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Get the call
            ApplicationDbContext _context2 = new ApplicationDbContext();
            var Call = _context2.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Check permissions
            var permissions = false;
            if (Call.LockedToUserId != null)
            {
                var LoggedInUser = User.Identity.GetUserId();

                //If it's the logged in user
                if (Call.LockedToUserId.Equals(LoggedInUser)) permissions = true;

                //Else if current user is admin
                else if (User.IsInRole("admin")) permissions = true;

                //Else if it's a group call, and it's the group owner
                else if
                (
                    //Group call
                    Call.ResourceGroupId != null &&
                    //Logged in user is in group
                    _context.GroupMember.Where(n => n.User_Id.Equals(LoggedInUser) && n.Group_Id == Call.ResourceGroupId).Any() &&
                    //Logged in user is owner of group
                    _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInUser) && n.Group_Id == Call.ResourceGroupId).Owner == true
                )
                    permissions = true;

            }

            //If they have permissions to clear lock, then clear
            if (permissions == true)
            {
                Call.LockedToUserId = null;
                _context2.SaveChanges();
                TempData["SuccessMessage"] = "Lock cleared";
                return RedirectToAction("call/" + Reference);
            }
            else
            {
                TempData["ErrorMessage"] = "Sorry, you need to be either the user, the group owner or an admin to clear the lock";
                return RedirectToAction("call/" + Reference);
            }

        }

        /*****************
         * Associate Client to call
         * ***************/

        [HttpGet]
        [Route("desk/call/{Reference}/client")]
        public ActionResult AssociateClientGET(string Reference)
        {
            //Handle messages
            HandleMessages();

            //Check Reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call closed
            if (IsCallClosed(Reference))
            {
                TempData["ErrorMessage"] = "This call is closed";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call locked
            if (IsCallLockedToSomeoneElse(Reference))
            {
                var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                return RedirectToAction("call/" + Reference);
            }

            //Check if call already associated
            var CheckCall = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
            if (CheckCall.ForUserId != null)
            {
                TempData["ErrorMessage"] = "This call is already associated to a client";
                return RedirectToAction("call/" + Reference);
            }

            //Get the call
            var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Create the model
            AssociateClientPageViewModel model = new AssociateClientPageViewModel();
            model.CallSummary = Call.Summary;

            //Return view
            return View("Call_AssociateClient", model);
        }

        [HttpPost]
        [Route("desk/call/{Reference}/client")]
        public ActionResult AssociateClientPOST(string Reference, AssociateClientPageViewModel model)
        {
            //If model passes validation
            if (ModelState.IsValid)
            {
                //Check Reference exists
                if (!CheckReferenceExists(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                    return RedirectToAction("Index");
                }

                //Authorise access/permissions
                if (!ModifyCallAuthorisation(Reference))
                {
                    TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call closed
                if (IsCallClosed(Reference))
                {
                    TempData["ErrorMessage"] = "This call is closed";
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call locked
                if (IsCallLockedToSomeoneElse(Reference))
                {
                    var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                    var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                    TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                    return RedirectToAction("call/" + Reference);
                }

                //Check if call already associated
                var CheckCall = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
                if (CheckCall.ForUserId != null)
                {
                    TempData["ErrorMessage"] = "This call is already associated to a client";
                    return RedirectToAction("call/" + Reference);
                }

                //Check the entered user exists
                var EnteredUser = _context.Users.SingleOrDefault(n => n.UserName.ToLower().Equals(model.AssociateClient.Username.ToLower()));
                if (EnteredUser == null)
                {
                    ViewBag.ErrorMessage = "Sorry, username doesn't exists";
                    return View("Call_AssociateClient", model);
                }

                //Check user is a client
                if (!userManager.IsInRole(EnteredUser.Id, "Client"))
                {
                    ViewBag.ErrorMessage = "Sorry, that user is not a client";
                    return View("Call_AssociateClient", model);
                }

                //Check if update details set
                Call Call;
                if (model.AssociateClient.UpdateCallDetails == true)
                {
                    //Get the call
                    Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

                    //Update the call
                    Call.ForUserId = EnteredUser.Id;
                    Call.FirstName = EnteredUser.FirstName;
                    Call.Lastname = EnteredUser.LastName;
                    Call.Email = EnteredUser.Email;
                    Call.PhoneNumber = EnteredUser.PhoneNumber;
                    Call.Extension = EnteredUser.Extension;
                    Call.OrganisationAlias = EnteredUser.OrganisationAlias;
                    Call.Organisation = EnteredUser.Organisation;
                    Call.Department = EnteredUser.Department;
                }
                //Otherwise if not set
                else
                {
                    //Get the call
                    Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

                    //Update the call
                    Call.ForUserId = EnteredUser.Id;
                }

                //Save changes
                _context.SaveChanges();

                //Return to page
                TempData["SuccessMessage"] = EnteredUser.UserName + " now associated to call";
                return RedirectToAction("ViewCall", new { Reference });
            }

            return View("Call_AssociateClient", model);
        }

        [HttpGet]
        public ActionResult ClearAssociation(string Reference)
        {
            //Check Reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Authorise access/permissions
            if (!ModifyCallAuthorisation(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, you do not have the permissions to action this call, please contact the resource";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call closed
            if (IsCallClosed(Reference))
            {
                TempData["ErrorMessage"] = "This call is closed";
                return RedirectToAction("call/" + Reference);
            }

            //Check if call locked
            if (IsCallLockedToSomeoneElse(Reference))
            {
                var LockedId = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference)).LockedToUserId;
                var LockedUsername = _context.Users.SingleOrDefault(n => n.Id.Equals(LockedId)).UserName;
                TempData["ErrorMessage"] = "This call is locked to " + LockedUsername;
                return RedirectToAction("call/" + Reference);
            }

            //Get the call and clear it
            var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
            Call.ForUserId = null;
            _context.SaveChanges();

            //Return to page
            TempData["SuccessMessage"] = "Client Association cleared";
            return RedirectToAction("ViewCall", new { Reference });
        }

        /*****************
         * Call Statistics
         * ***************/

        [HttpGet]
        [Route("desk/call/{Reference}/report")]
        public ActionResult ViewCallReport(string Reference)
        {
            //Check Reference exists
            if (!CheckReferenceExists(Reference))
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index");
            }

            //Get the call and actions
            var Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
            var Actions = _context.Action.Where(n => n.CallReference.Equals(Reference));

            /********************
             * Time between Open to Close
             * ******************/
            TimeSpan? OpenToCloseTime;

            //If Call is closed
            if (Call.Closed)
            {
                var CallCreatedTime = Call.Created;
                var ClosedTime = Actions.AsEnumerable().OrderBy(n => n.Created).Last().Created;
                OpenToCloseTime = ClosedTime.Subtract(CallCreatedTime);
            }
            //Else Call is open so not applicable
            else
            {
                OpenToCloseTime = null;
            }

            /********************
             * Closed within SLA?
             * ******************/
            bool? ClosedWithinSLA = null;

            //Get SLA Expiry
            DateTime? SlaExpiry = GetSLAExpiryDateTime(Reference);

            //If Call closed
            if (Call.Closed)
            {
                //If null then there was no dated SLA
                if (SlaExpiry != null)
                {
                    //Get the last action
                    var ClosedTime = Actions.AsEnumerable().OrderBy(n => n.Created).Last().Created;

                    //If closed before SLA Expired
                    if (ClosedTime < SlaExpiry)
                        ClosedWithinSLA = true;
                    //Else it closed after SLA expired
                    else
                        ClosedWithinSLA = false;
                }
                //Else no SLA Set
                else
                {
                    ClosedWithinSLA = null;
                }
            }
            //Else Call not Closed
            else
            {
                ClosedWithinSLA = null;
            }

            /********************
             * No Of times SLA Reset
             * ******************/

            //Count Number of times SLA Reset
            var SlaResetCount = Actions.Where(n => n.Type.Equals("SLA Reset")).Count();

            /********************
             * Timespan Call opened for
             * ******************/
            TimeSpan? CallOpenedForTime;

            //If call open
            if (!Call.Closed)
            {
                var CallCreated = Call.Created;
                var TimeNow = DateTime.Now;
                CallOpenedForTime = TimeNow.Subtract(CallCreated);
            }
            //else call closed
            else
            {
                CallOpenedForTime = null;
            }

            /********************
             * Actioned By Graph
             * ******************/

            //Get Distinct category 'actioned by' from Action
            var DistinctActionedBy = Actions.Select(n => n.ActionedByUserId).Distinct();

            //Create the list
            var ActionedByList = new List<ActionedByGraphDataPointViewModel>();

            //For each user count the number of actions in the call
            using (ApplicationDbContext _context2 = new ApplicationDbContext())
            {
                foreach (var userid in DistinctActionedBy)
                {
                    ActionedByList.Add(new ActionedByGraphDataPointViewModel(
                        _context2.Users.SingleOrDefault(n => n.Id.Equals(userid)).UserName,
                        _context2.Action.Where(n => n.ActionedByUserId.Equals(userid) && n.CallReference.Equals(Reference)).Count()
                        ));
                }
            }

            //Json convert serialise object
            string ActionedByJsonData = JsonConvert.SerializeObject(ActionedByList);

            /********************
             * Model
             * ******************/

            //Create Model
            var model = new ReportPageViewModel()
            {
                ActionedByJsonData = ActionedByJsonData,
                Statistics = new CallReportStatisticsViewModel
                {
                    OpenToCloseTime = OpenToCloseTime,
                    ClosedWithinSLA = ClosedWithinSLA,
                    SlaResetCount = SlaResetCount,
                    CallOpenedForTime = CallOpenedForTime
                }
            };

            //Return to View
            return View("Call_Report", model);
        }

        /*******************
         *     HELPERS
         ******************/

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

        //Check if the reference for a call supplied exists
        public bool CheckReferenceExists(string Reference)
        {
            ApplicationDbContext dbcontext = new ApplicationDbContext();

            //Check Reference exists
            var CallExists = dbcontext.Call.Where(n => n.Reference.Equals(Reference)).Any();
            if (!CallExists)
            {
                return false;
            }
            //Else return true
            return true;
        }

        //Check if the logged in user has authority to modify the call
        public bool ModifyCallAuthorisation(string Reference)
        {
            //Make db access
            ApplicationDbContext dbcontext = new ApplicationDbContext();

            //Get the call
            var Call = dbcontext.Call.SingleOrDefault(n => n.Reference == Reference);

            //Check if resource User ID of call is same as logged in user
            var permissions = false;
            var loggedInUser = User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(Call.ResourceUserId))
            {
                if (Call.ResourceUserId.Equals(loggedInUser))
                    permissions = true;
            }

            //Check if resource Group Id of call is same as one which logged in user is a member of
            else
            {
                //If call is associated to a group which the logged in user is a member of
                var GroupMembers = dbcontext.GroupMember.SingleOrDefault(n => (n.Group_Id == Call.ResourceGroupId) && (n.User_Id.Equals(loggedInUser)));
                if (GroupMembers != null)
                    permissions = true;
            }

            //Return true or false
            return permissions;
        }

        //Check if call closed
        public bool IsCallClosed(string Reference)
        {
            //Create db access
            ApplicationDbContext dbcontext = new ApplicationDbContext();

            //Get the call
            var Call = dbcontext.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Check if closed
            return Call.Closed;
        }

        //Check if call locked
        public bool IsCallLockedToSomeoneElse(string Reference)
        {
            //Create db access
            ApplicationDbContext dbcontext = new ApplicationDbContext();

            //Get the call
            var Call = dbcontext.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Check if locked to someone else
            return Call.LockedToUserId != null && !Call.LockedToUserId.Equals(User.Identity.GetUserId());
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

        //Send Message
        void SendEmail(string ToEmail, string Subject, string HtmlMessage)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["SendFromEmail"].ToString());
            msg.To.Add(new MailAddress(ToEmail));
            msg.Subject = Subject;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(HtmlMessage, null, MediaTypeNames.Text.Html));
            msg.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SendFromEmail"].ToString(), ConfigurationManager.AppSettings["SendFromEmailPassword"].ToString());
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);

        }

        public DateTime? GetSLAExpiryDateTime(string Reference)
        {
            //Get the call
            ApplicationDbContext dbcontext = new ApplicationDbContext();
            var Call = dbcontext.Call.SingleOrDefault(n => n.Reference.Equals(Reference));

            //Get the number of minutes
            double mins = 0;
            bool hasSla = false;
            if (Call.SlaLevel.Equals("Low"))
            {
                mins = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).LowMins;
                hasSla = true;
            }
            else if (Call.SlaLevel.Equals("Medium"))
            {
                mins = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).MedMins;
                hasSla = true;
            }
            else if (Call.SlaLevel.Equals("High"))
            {
                mins = _context.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).HighMins;
                hasSla = true;
            }

            //If there is an SLA Policy of low-high, set SLA Expiry time for view model
            if (hasSla)
                return Call.SLAResetTime.Value.AddMinutes(mins);
            else
                return null;
        }
    }
}