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
    public class DashboardController : Controller
    {
        //Variables
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        //Constructor
        public DashboardController()
        {
            _context = new ApplicationDbContext();
            userStore = new UserStore<ApplicationUser>(_context);
            userManager = new UserManager<ApplicationUser>(userStore);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");

        }

        [Route("dashboard")]
        public ActionResult Dashboard(string dismissed = null)
        {
            HandleMessages();

            if (User.IsInRole("Employee"))
            {
                return View("EmployeeDashboard", EmployeeDashboard());
            }
            else if (User.IsInRole("Client"))
            {
                return View("ClientDashboard", ClientDashboard(dismissed));
            }
            else
            {
                return RedirectToAction("Index", "Home", null);
            }
        }

        public EmployeeDashboardPageViewModel EmployeeDashboard()
        {
            //Get logged in user id
            var LoggedInUserId = User.Identity.GetUserId();

            /**************************
             * STATS
             **************************/

            var OpenCalls =
                    _context.Action
                .Where(n => n.ActionedByUserId.Equals(LoggedInUserId) && n.Type.Equals("Opened Call"))
                .Where(n => n.Created.Year == DateTime.Now.Year && n.Created.Month == DateTime.Now.Month && n.Created.Day == DateTime.Now.Day)
                .Count();

            var ClosedCalls =
                    _context.Action
                .Where(n => n.ActionedByUserId.Equals(LoggedInUserId) && n.Type.Equals("Call Closed"))
                .Where(n => n.Created.Year == DateTime.Now.Year && n.Created.Month == DateTime.Now.Month && n.Created.Day == DateTime.Now.Day)
                .GroupBy(n => n.CallReference)
                .Count();

            var Actions = _context.Action
                .Where(n => n.ActionedByUserId.Equals(LoggedInUserId))
                .Where(n => n.Created.Year == DateTime.Now.Year && n.Created.Month == DateTime.Now.Month && n.Created.Day == DateTime.Now.Day)
                .Count();

            /**************************
             * Locked Calls
             **************************/

            //Get calls
            var L_Calls = _context.Call.Where(n => n.LockedToUserId != null && n.LockedToUserId.Equals(LoggedInUserId));

            //To model
            List<EmployeeLockedCallsViewModel> LockedCalls = new List<EmployeeLockedCallsViewModel>();
            foreach (var call in L_Calls)
            {
                LockedCalls.Add(new EmployeeLockedCallsViewModel
                {
                    Reference = call.Reference,
                    Created = call.Created,
                    Summary = call.Summary
                });
            }

            /**************************
             * Count Alerts
             **************************/
            var AlertsCount = _context.Alert
                .Where(n => n.ToUserId != null && n.ToUserId.Equals(LoggedInUserId) && n.DismissedWhen == null)
                .Count();

            /**************************
             * Groups Count
             **************************/
            var GroupsCount = _context.GroupMember
                .Where(n => n.User_Id.Equals(LoggedInUserId))
                .Count();

            /**************************
             * Urgent Calls
             **************************/

            //Get open calls
            var LoggedInUsersCalls = _context.Call.Where(n => n.Closed == false && n.ResourceUserId != null && n.ResourceUserId.Equals(LoggedInUserId));

            //List of urgent calls
            List<Call> UrgentCalls = new List<Call>();

            //Get exceeded SLA calls
            int mins;
            DateTime ExpiredDate;
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                foreach (Call item in LoggedInUsersCalls)
                {
                    if (item.SlaLevel.Equals("Low"))
                    {
                        //Get the Mins
                        mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).LowMins;

                        //Add to SLA Date
                        ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                        //Check if Date is past now
                        if (ExpiredDate < DateTime.Now) UrgentCalls.Add(item);
                    }
                    else if (item.SlaLevel.Equals("Medium"))
                    {
                        //Get the Mins
                        mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).MedMins;

                        //Add to SLA Date
                        ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                        //Check if Date is past now
                        if (ExpiredDate < DateTime.Now) UrgentCalls.Add(item);
                    }
                    else if (item.SlaLevel.Equals("High"))
                    {
                        //Get the Mins
                        mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).HighMins;

                        //Add to SLA Date
                        ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                        //Check if Date is past now
                        if (ExpiredDate < DateTime.Now) UrgentCalls.Add(item);
                    }
                }
            }

            //Get calls which have exceeded required date
            var requiredcalls = _context.Call.Where(n => (n.ResourceUserId.Equals(LoggedInUserId)) && (n.Closed == false) && (n.Required_By < DateTime.Now));

            //Merge the two
            UrgentCalls = UrgentCalls.Union(requiredcalls).ToList();

            //To model
            List<EmployeeUrgentCallsViewModel> UrgentCallsModel = new List<EmployeeUrgentCallsViewModel>();
            foreach (var call in UrgentCalls)
            {
                UrgentCallsModel.Add(new EmployeeUrgentCallsViewModel
                {
                    Reference = call.Reference,
                    Created = call.Created,
                    Summary = call.Summary
                });
            }

            /**************************
             * Model
             **************************/

            //Create Model
            var model = new EmployeeDashboardPageViewModel
            {
                LockedCalls = LockedCalls,
                AlertsCount = AlertsCount,
                GroupsCount = GroupsCount,
                UrgentCalls = UrgentCallsModel,
                Stats = new EmployeeStatsViewModel
                {
                    OpenCalls = OpenCalls,
                    ClosedCalls = ClosedCalls,
                    Actions = Actions
                }
            };

            //Pass to action
            return model;
        }

        public ClientDashboardPageViewModel ClientDashboard(string dismissed = null)
        {
            //Get user id
            var LoggedInUserId = User.Identity.GetUserId();

            /**************************
             * My Associated Calls
             **************************/
            //Get calls
            var AssociatedClientCalls = _context.Call.Where(n => n.Closed == false && n.ForUserId != null && n.ForUserId.Equals(LoggedInUserId));

            //To View Model
            var AssociatedCallsModel = new List<ClientAssociatedCallViewModel>();
            foreach (var call in AssociatedClientCalls)
            {
                AssociatedCallsModel.Add(new ClientAssociatedCallViewModel
                {
                    Reference = call.Reference,
                    Created = call.Created,
                    Summary = call.Summary
                });
            }

            /**************************
             * List of Groups
             **************************/
            var Groups = _context.Group;

            /**************************
             * My Messages
             **************************/
            //Get dismissed alerts
            IQueryable<Alert> Alerts;
            bool isDismissed;
            if (!String.IsNullOrEmpty(dismissed) && dismissed.Equals("true"))
            {
                Alerts = _context.Alert.Where(n => (n.ToUserId.Equals(LoggedInUserId)) && (n.DismissedWhen != null) && (n.ToGroupId == null));
                isDismissed = true;
            }
            //Get Non dismissed alerts
            else
            {
                Alerts = _context.Alert.Where(n => (n.ToUserId.Equals(LoggedInUserId)) && (n.DismissedWhen == null) && (n.ToGroupId == null));
                isDismissed = false;
            }

            //Convert Alerts into View Model Alerts
            List<ClientAlertsViewModel> AlertsVM = new List<ClientAlertsViewModel>();
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                ApplicationUser FromUser, ToUser, DismissedByUser;
                Group FromGroup;

                foreach (var item in Alerts)
                {
                    FromUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.FromUserId));
                    FromGroup = dbcontext.Group.SingleOrDefault(n => n.Id == item.FromGroupId);
                    ToUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.ToUserId));
                    DismissedByUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.DismissedByUserId));

                    AlertsVM.Add(new ClientAlertsViewModel()
                    {
                        Id = item.Id,
                        FromUserId = item.FromUserId,
                        FromUserName = FromUser?.UserName,
                        FromGroupId = item.FromGroupId,
                        FromGroupName = FromGroup?.Name,
                        ToUserId = item.ToUserId,
                        ToUserName = ToUser?.UserName,
                        ToGroupId = null,
                        ToGroupName = null,
                        DismissedByUserId = item.DismissedByUserId,
                        DismissedByUserName = DismissedByUser?.UserName,
                        DismissedWhen = item.DismissedWhen,
                        Dismissed = item.DismissedWhen == null ? false : true,
                        Created = item.Created,
                        Text = item.Text,
                        AssociatedCallRef = item.AssociatedCallRef,
                        AssociatedKnowledgeId = item.AssociatedKnowledgeId,
                    });
                }
            }

            /**************************
             * MODELS
             **************************/

            var model = new ClientDashboardPageViewModel
            {
                AssociatedCalls = AssociatedCallsModel,
                GroupList = Groups,
                Alerts = AlertsVM
            };

            //Return view
            return model;
        }

        [HttpPost]
        [Route("dashboard")]
        public ActionResult SendMessagePOST(ClientDashboardPageViewModel model)
        {
            //Get user id
            var LoggedInUserId = User.Identity.GetUserId();

            /**************************
             * My Associated Calls
             **************************/
            //Get calls
            var AssociatedClientCalls = _context.Call.Where(n => n.Closed == false && n.ForUserId != null && n.ForUserId.Equals(LoggedInUserId));

            //To View Model
            var AssociatedCallsModel = new List<ClientAssociatedCallViewModel>();
            foreach (var call in AssociatedClientCalls)
            {
                AssociatedCallsModel.Add(new ClientAssociatedCallViewModel
                {
                    Reference = call.Reference,
                    Created = call.Created,
                    Summary = call.Summary
                });
            }
            model.AssociatedCalls = AssociatedCallsModel;

            /**************************
             * List of Groups
             **************************/
            var Groups = _context.Group;
            model.GroupList = Groups;

            /**************************
             * My Messages
             **************************/
            //Get dismissed alerts
            IQueryable<Alert> Alerts;

            //Get Non dismissed alerts
            Alerts = _context.Alert.Where(n => (n.ToUserId.Equals(LoggedInUserId)) && (n.DismissedWhen == null) && (n.ToGroupId == null));

            //Convert Alerts into View Model Alerts
            List<ClientAlertsViewModel> AlertsVM = new List<ClientAlertsViewModel>();
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                ApplicationUser FromUser, ToUser, DismissedByUser;
                Group FromGroup;

                foreach (var item in Alerts)
                {
                    FromUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.FromUserId));
                    FromGroup = dbcontext.Group.SingleOrDefault(n => n.Id == item.FromGroupId);
                    ToUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.ToUserId));
                    DismissedByUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.DismissedByUserId));

                    AlertsVM.Add(new ClientAlertsViewModel()
                    {
                        Id = item.Id,
                        FromUserId = item.FromUserId,
                        FromUserName = FromUser?.UserName,
                        FromGroupId = item.FromGroupId,
                        FromGroupName = FromGroup?.Name,
                        ToUserId = item.ToUserId,
                        ToUserName = ToUser?.UserName,
                        ToGroupId = null,
                        ToGroupName = null,
                        DismissedByUserId = item.DismissedByUserId,
                        DismissedByUserName = DismissedByUser?.UserName,
                        DismissedWhen = item.DismissedWhen,
                        Dismissed = item.DismissedWhen == null ? false : true,
                        Created = item.Created,
                        Text = item.Text,
                        AssociatedCallRef = item.AssociatedCallRef,
                        AssociatedKnowledgeId = item.AssociatedKnowledgeId,
                    });
                }
            }

            model.Alerts = AlertsVM;
            /**************************
             * MODEL
             **************************/
            if (ModelState.IsValid)
            {
                //Check group exists
                var ToGroup = _context.Group.SingleOrDefault(n => n.Name.Equals(model.Message.GroupName));
                if (ToGroup == null)
                {
                    ViewBag.ErrorMessage = "An error has occured regarding the selected group - Please contact an admin";
                    return View("ClientDashboard", model);
                }

                //Create Alert
                var Alert = new Alert
                {
                    FromUserId = LoggedInUserId,
                    ToGroupId = ToGroup.Id,
                    Text = model.Message.Message,
                    Created = DateTime.Now,
                };

                //Send Alert
                _context.Alert.Add(Alert);
                _context.SaveChanges();

                //Return to Dashboard
                TempData["SuccessMessage"] = "Message Sent";
                return RedirectToAction("Index");
            }
            return View("ClientDashboard", model);
        }

        public ActionResult DismissUndismissAlert(string alertid)
        {
            //Check Alert is not null
            if (string.IsNullOrEmpty(alertid))
            {
                TempData["ErrorMessage"] = "An error has occured";
                return RedirectToAction("Index");
            }

            //Check Alert is an int
            if (!int.TryParse(alertid, out int AlertIdInt))
            {
                TempData["ErrorMessage"] = "Error: Alert ID is incorrect";
                return RedirectToAction("Index");
            }

            //Check Alert exists
            var Alert = _context.Alert.SingleOrDefault(n => n.Id == AlertIdInt);
            if (Alert == null)
            {
                TempData["ErrorMessage"] = "Error: The alert you are replying to doesn't exist";
                return RedirectToAction("Index");
            }

            //Check Alert belongs to user
            if (Alert.ToUserId != User.Identity.GetUserId())
            {
                TempData["ErrorMessage"] = "Error: This alert you attempted to reply to does not belong to you";
                return RedirectToAction("Index");
            }

            //Change Alert Dismissed Property
            var isDismissed = Alert.DismissedWhen == null ? "" : "true";
            if (Alert.DismissedWhen == null)
            {
                Alert.DismissedWhen = DateTime.Now;
                Alert.DismissedByUserId = User.Identity.GetUserId();
            }
            else
            {
                Alert.DismissedWhen = null;
                Alert.DismissedByUserId = null;
            }
            _context.SaveChanges();

            //Return to view
            return RedirectToAction("Index", new { dismissed = isDismissed });
        }


        /*******************
         * Reply to Alerts
         ******************/
        [HttpGet]
        [Route("dashboard/reply/{alertid}")]
        public ActionResult ReplyToAlertGET(string alertid)
        {
            //Check Alert is not null
            if (string.IsNullOrEmpty(alertid))
            {
                TempData["ErrorMessage"] = "An error has occured";
                return RedirectToAction("Index");
            }

            //Check Alert is an int
            if (!int.TryParse(alertid, out int AlertIdInt))
            {
                TempData["ErrorMessage"] = "Error: Alert ID is incorrect";
                return RedirectToAction("Index");
            }

            //Check Alert exists
            var Alert = _context.Alert.SingleOrDefault(n => n.Id == AlertIdInt);
            if (Alert == null)
            {
                TempData["ErrorMessage"] = "Error: The alert you are replying to doesn't exist";
                return RedirectToAction("Index");
            }

            //Check if alert dismissed
            if (Alert.DismissedWhen != null)
            {
                TempData["ErrorMessage"] = "Error: The alert you are replying to has been dismissed";
                return RedirectToAction("Index");
            }

            //Check Alert belongs to user
            if (Alert.ToUserId != User.Identity.GetUserId())
            {
                TempData["ErrorMessage"] = "Error: This alert you attempted to reply to does not belong to you";
                return RedirectToAction("Index");
            }

            //Populate data for view
            ReplyAlertViewModel model;
            using (ApplicationDbContext dbcontext = new ApplicationDbContext())
            {
                Group ToGroup = dbcontext.Group.SingleOrDefault(n => n.Id == Alert.FromGroupId);
                ApplicationUser ToUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(Alert.FromUserId));

                model = new ReplyAlertViewModel()
                {
                    ReplyingToMessage = Alert.Text,
                    FromGroupName = null,
                    FromUserName = User.Identity.GetUserName(),
                    ReplyToGroupName = ToGroup?.Name,
                    ReplyToUserName = ToUser?.UserName,
                    Resource = null
                };
            }

            //Return to view
            return View("ReplyToAlert", model);


        }

        [HttpPost]
        [Route("dashboard/reply/{alertid}")]
        public ActionResult ReplyToAlertPOST(string alertid, ReplyAlertViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Check Alert is not null
                if (string.IsNullOrEmpty(alertid))
                {
                    TempData["ErrorMessage"] = "An error has occured";
                    return RedirectToAction("Index");
                }

                //Check Alert is an int
                if (!int.TryParse(alertid, out int AlertIdInt))
                {
                    TempData["ErrorMessage"] = "Error: Alert ID is incorrect";
                    return RedirectToAction("Index");
                }

                //Check Alert exists
                var Alert = _context.Alert.SingleOrDefault(n => n.Id == AlertIdInt);
                if (Alert == null)
                {
                    TempData["ErrorMessage"] = "Error: The alert you are replying to doesn't exist";
                    return RedirectToAction("Index");
                }

                //Check if alert dismissed
                if (Alert.DismissedWhen != null)
                {
                    TempData["ErrorMessage"] = "Error: The alert you are replying to has been dismissed";
                    return RedirectToAction("Index");
                }

                //Check Alert belongs to user
                if (Alert.ToUserId != User.Identity.GetUserId())
                {
                    TempData["ErrorMessage"] = "Error: This alert you attempted to reply to does not belong to you";
                    return RedirectToAction("Index");
                }

                //Create Alert
                var SendingAlert = new Alert()
                {
                    FromUserId = Alert.ToUserId,
                    FromGroupId = null,
                    ToUserId = Alert.FromUserId,
                    ToGroupId = Alert.FromGroupId,
                    Text = model.Text,
                    AssociatedCallRef = Alert.AssociatedCallRef,
                    AssociatedKnowledgeId = Alert.AssociatedKnowledgeId,
                    Created = DateTime.Now,
                    DismissedWhen = null,
                    DismissedByUserId = null
                };

                //Send
                _context.Alert.Add(SendingAlert);
                _context.SaveChanges();

                //Return to Alerts page
                TempData["SuccessMessage"] = "Alert Sent";
                return RedirectToAction("Index");

            }
            return View("ReplyToAlert", model);
        }


        //HELPERS
        //Error and success messages
        public void HandleMessages()
        {
            //Check for an error message from another action
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                TempData.Remove("ErrorMessage");
            }

            //Check for a message from another action
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                TempData.Remove("SuccessMessage");
            }

            //Remove Tempdata TODO do for all others
        }
    }
}
