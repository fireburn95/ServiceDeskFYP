using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
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

        //GET: Manager_Centre view employees
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

        //View Manager Report of subordinates
        [Route("manager_centre/report")]
        public ActionResult ViewManagerReport(int lastxdays = 7)
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

            //Check if last x days is less than 1
            if (lastxdays < 1)
            {
                lastxdays = 7;
            }

            //Todays day minus param date
            var CompareDate = DateTime.Now.AddDays(-lastxdays);

            //Create List of Stats
            var ReportStats = new List<ManagerReportCompareTableViewModel>();

            /*******************
             * User Populate
             *******************/
            foreach (var user in SubordinatesList)
            {
                ReportStats.Add(new ManagerReportCompareTableViewModel
                {
                    UserId = user.Id,
                    Username = user.UserName
                });
            }

            /*******************
            * Open Calls
            *******************/

            foreach (var item in ReportStats)
            {
                item.OpenCalls =
                    _context.Action
                .Where(n => n.ActionedByUserId.Equals(item.UserId) && n.Type.Equals("Opened Call"))
                .Where(n => n.Created > CompareDate)
                .Count();
            }

            /*******************
            * Closed Calls
            *******************/

            foreach (var item in ReportStats)
            {
                item.ClosedCalls =
                    _context.Action
                .Where(n => n.ActionedByUserId.Equals(item.UserId) && n.Type.Equals("Call Closed"))
                .Where(n => n.Created > CompareDate)
                .GroupBy(n => n.CallReference)
                .Count();
            }

            /*******************
            * Actions
            *******************/

            foreach (var item in ReportStats)
            {
                item.Actions = _context.Action
                .Where(n => n.ActionedByUserId.Equals(item.UserId))
                .Where(n => n.Created > CompareDate)
                .Count();
            }

            /*********************
             * Calls Opened and Closed before and after SLA
             * ******************/
             foreach(var item in ReportStats)
            {
                var CallsClosedAfterSla = 0;
                var CallsClosedBeforeSla = 0;

                //Get all Closed calls created by user
                var UsersClosedCalls = _context.Call.Where(n => n.Closed && n.ResourceUserId != null && n.ResourceUserId.Equals(item.UserId));

                using (ApplicationDbContext dbcontext = new ApplicationDbContext())
                {
                    //For each call
                    foreach (var call in UsersClosedCalls)
                    {
                        //Get the actions from the past x days
                        var actions = dbcontext.Action.Where(n => n.CallReference.Equals(call.Reference) && n.ActionedByUserId.Equals(item.UserId) && n.Created > CompareDate && n.Type.Equals("Call Closed"))
                                      .OrderBy(n => n.Created);

                        //If no action then skip this call
                        if (actions == null || !actions.Any())
                        {
                            continue;
                        }

                        //Get the last action date
                        var LastActionDate = actions.AsEnumerable().Last().Created;

                        //Get SLA Expiry date
                        DateTime? SlaExpiry = GetSLAExpiryDateTime(call.Reference);

                        //If date is before SLA, increment count
                        if (SlaExpiry == null) continue;
                        if (LastActionDate < SlaExpiry)
                            CallsClosedBeforeSla++;
                        else
                            CallsClosedAfterSla++;
                    }

                    item.ClosedBeforeSla = CallsClosedBeforeSla;
                    item.ClosedAfterSla = CallsClosedAfterSla;
                }
            }
            

            /*******************
             * PIE Chart of Actions
             *******************/
            //Datapoints list
            List<ManagerReportDataPointsViewModel> datapoints = new List<ManagerReportDataPointsViewModel>();

            //For each subordinate
            foreach (var user in SubordinatesList)
            {
                var ActionsCount =
                    _context.Action
                    .Where(n => n.ActionedByUserId.Equals(user.Id))
                    .Where(n => n.Created > CompareDate)
                    .Count();

                datapoints.Add(new ManagerReportDataPointsViewModel(user.UserName, ActionsCount));
            }

            //Convert to json
            string JsonPieData = JsonConvert.SerializeObject(datapoints);

            /*******************
             * Model
             *******************/

            //Create the model
            var model = new ManagerReportPageViewModel
            {
                PieJsonDatapoints = JsonPieData,
                stats = ReportStats
            };

            return View("ViewManagerReport", model);
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
                Calls = _context.Call.Where(n => (n.ResourceUserId.Equals(Subordinate.Id)) && (n.Closed == false)).AsEnumerable();
            }
            else
            {
                //Get the closed calls of the subordinate
                Calls = _context.Call.Where(n => (n.ResourceUserId.Equals(Subordinate.Id)) && (n.Closed == true)).AsEnumerable();
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

            //Populate data
            SendAlertToSubViewModel model = new SendAlertToSubViewModel()
            {
                ToUsername = Subordinate.UserName
            };

            //Return view
            return View("SendAlertToSub", model);
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

        [HttpGet]
        [Route("manager_centre/sub/{sub_username}/report")]
        public ActionResult ViewSubordinateReport(string sub_username, int lastxdays = 7)
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

            //Check if last x days is less than 1
            if (lastxdays < 1)
            {
                lastxdays = 7;
            }

            //Todays day minus param date
            var CompareDate = DateTime.Now.AddDays(-lastxdays);

            /*********************
             * Opened Calls
             * ******************/

            var OpenCallsPastXDays = _context.Action
                .Where(n => n.ActionedByUserId.Equals(Subordinate.Id) && n.Type.Equals("Opened Call"))
                .Where(n => n.Created > CompareDate)
                .Count();

            /*********************
             * Closed Calls
             * ******************/

            var ClosedCallsPastXDays = _context.Action
                .Where(n => n.ActionedByUserId.Equals(Subordinate.Id) && n.Type.Equals("Call Closed"))
                .Where(n => n.Created > CompareDate)
                .GroupBy(n => n.CallReference)
                .Count();

            /*********************
             * Total Actions
             * ******************/

            var TotalActionsPastXDays = _context.Action
                .Where(n => n.ActionedByUserId.Equals(Subordinate.Id))
                .Where(n => n.Created > CompareDate)
                .Count();

            /*********************
             * Calls Opened and Closed before and after SLA
             * ******************/
            var CallsClosedBeforeSla = 0;
            var CallsClosedAfterSla = 0;

            //Get all Closed calls created by user
            var UsersClosedCalls = _context.Call.Where(n => n.Closed && n.ResourceUserId != null && n.ResourceUserId.Equals(Subordinate.Id));

            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                //For each call
                foreach (var call in UsersClosedCalls)
                {
                    //Get the actions from the past x days
                    var actions = dbcontext.Action.Where(n => n.CallReference.Equals(call.Reference) && n.ActionedByUserId.Equals(Subordinate.Id) && n.Created > CompareDate && n.Type.Equals("Call Closed"))
                                  .OrderBy(n => n.Created);

                    //If no action then skip this call
                    if (actions == null || !actions.Any())
                    {
                        continue;
                    }

                    //Get the last action date
                    var LastActionDate = actions.AsEnumerable().Last().Created;

                    //Get SLA Expiry date
                    DateTime? SlaExpiry = GetSLAExpiryDateTime(call.Reference);

                    //If date is before SLA, increment count
                    if (SlaExpiry == null) continue;
                    if (LastActionDate < SlaExpiry)
                        CallsClosedBeforeSla++;
                    else
                        CallsClosedAfterSla++;
                }
            }

            /*********************
             * Actions for last x days
             * ******************/

            //Get all of users Actions
            var Actions = _context.Action.Where(n => n.ActionedByUserId.Equals(Subordinate.Id));

            //Get last x days
            List<DateTime> LastXDates =
                Enumerable.Range(0, lastxdays)
                .Select(n => DateTime.Now.Date.AddDays(-n))
                .Reverse()
                .ToList();

            //Get the number of days per count
            List<ManagerUserActionsReportDataPointsViewModel> actionsDatapoints = new List<ManagerUserActionsReportDataPointsViewModel>();
            foreach (var fulldate in LastXDates)
            {
                var date = fulldate.Date;

                actionsDatapoints.Add(new ManagerUserActionsReportDataPointsViewModel(
                    date.ToString("dd-MM-yy"),
                    Actions.Where(n => n.Created.Year == fulldate.Year &&
                                  n.Created.Month == fulldate.Month &&
                                  n.Created.Day == fulldate.Day).Count()
                ));

            }

            //Encode to JSON
            string actionsjsondatapoints = JsonConvert.SerializeObject(actionsDatapoints);


            /*********************
             * MODEL
             * ******************/

            //Make model
            var model = new ManagerUserReportPageViewModel()
            {
                ActionJsonDatapoints = actionsjsondatapoints,
                Statistics = new ManagerUserStatisticsViewModel
                {
                    OpenCalls = OpenCallsPastXDays,
                    ClosedCalls = ClosedCallsPastXDays,
                    Actions = TotalActionsPastXDays,
                    ClosedBeforeSla = CallsClosedBeforeSla,
                    ClosedAfterSla = CallsClosedAfterSla
                },

            };

            //Pass to view
            return View("ViewSubordinateReport", model);

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
                mins = dbcontext.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).LowMins;
                hasSla = true;
            }
            else if (Call.SlaLevel.Equals("Medium"))
            {
                mins = dbcontext.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).MedMins;
                hasSla = true;
            }
            else if (Call.SlaLevel.Equals("High"))
            {
                mins = dbcontext.SLAPolicy.SingleOrDefault(n => n.Id == Call.SlaId).HighMins;
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