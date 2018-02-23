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
    public class AlertsController : Controller
    {
        //Variables
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public AlertsController()
        {
            _context = new ApplicationDbContext();
            userStore = new UserStore<ApplicationUser>(_context);
            userManager = new UserManager<ApplicationUser>(userStore);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        /*******************
         *     View Alerts
         ******************/

        // GET: View Alerts
        public ActionResult Index(string resource = null, string dismissed = null)
        {
            //Handle messages
            HandleMessages();

            //Get Logged in Users ID
            var LoggedInUserID = User.Identity.GetUserId();

            //If Logged in User
            if (string.IsNullOrEmpty(resource))
            {
                //Get all open calls
                var OpenCalls = _context.Call.Where(n => (n.ResourceUserId.Equals(LoggedInUserID)) && (n.Closed == false));

                //Get exceeded SLA calls
                int mins, exceededslacalls = 0;
                DateTime ExpiredDate;
                using (ApplicationDbContext dbcontext = new ApplicationDbContext())
                {
                    foreach (Call item in OpenCalls)
                    {
                        if (item.SlaLevel.Equals("Low"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).LowMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now) exceededslacalls++;
                        }
                        else if (item.SlaLevel.Equals("Medium"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).MedMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now) exceededslacalls++;
                        }
                        else if (item.SlaLevel.Equals("High"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).HighMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now) exceededslacalls++;
                        }
                    }
                }

                //Get required by dates of calls
                var requireddatescount = _context.Call.Where(n => (n.ResourceUserId.Equals(LoggedInUserID)) && (n.Closed == false) && (n.Required_By < DateTime.Now)).Count();

                //Get dismissed alerts
                IQueryable<Alert> Alerts;
                bool isDismissed;
                if (!String.IsNullOrEmpty(dismissed) && dismissed.Equals("true"))
                {
                    Alerts = _context.Alert.Where(n => (n.ToUserId.Equals(LoggedInUserID)) && (n.DismissedWhen != null));
                    isDismissed = true;
                }
                //Get Non dismissed alerts
                else
                {
                    Alerts = _context.Alert.Where(n => (n.ToUserId.Equals(LoggedInUserID)) && (n.DismissedWhen == null));
                    isDismissed = false;
                }

                //Convert Alerts into View Model Alerts
                List<ViewAlertsTemplateViewModel> AlertsVM = new List<ViewAlertsTemplateViewModel>();
                using (ApplicationDbContext dbcontext = new ApplicationDbContext())
                {
                    ApplicationUser FromUser, ToUser, DismissedByUser;
                    foreach (var item in Alerts)
                    {
                        FromUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.FromUserId));
                        ToUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.ToUserId));
                        DismissedByUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.DismissedByUserId));

                        AlertsVM.Add(new ViewAlertsTemplateViewModel()
                        {
                            Id = item.Id,
                            FromUserId = item.FromUserId,
                            FromUserName = FromUser?.UserName,
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
                            AssociatedKnowledgeId = item.AssociatedKnowledgeId
                        });
                    }
                }

                //Create the view model
                var model = new ViewAlertsPageViewModel()
                {
                    GroupId = null,
                    ExceededSLACalls = exceededslacalls,
                    PastRequiredDate = requireddatescount,
                    Alerts = AlertsVM.AsEnumerable(),
                    IsLoggedInUserGroupOwner = null,
                    IsDismissed = isDismissed,
                    IsGroup = false
                };

                //Pass to view
                return View(model);
            }







            //Else if a group
            else
            {
                //Check group id is a number then cast to int
                if (!int.TryParse(resource, out int GroupId))
                {
                    TempData["ErrorMessage"] = "Error: resource ID incorrect";
                    //TODO check corrdct passing of values in all these
                    return RedirectToAction("Index", new { resource = "", dismissed = "" });
                }

                //Check group id exists
                var Group = _context.Group.SingleOrDefault(n => n.Id == GroupId);
                if (Group == null)
                {
                    TempData["ErrorMessage"] = "Error: Group does not exist";
                    return RedirectToAction("Index", new { resource = "", dismissed = "" });
                }

                //Check logged in user is a member of group and owner
                var GroupMember = _context.GroupMember.SingleOrDefault(n => n.User_Id.Equals(LoggedInUserID) && n.Group_Id == Group.Id);
                if (GroupMember == null)
                {
                    TempData["ErrorMessage"] = "Sorry, you are not a member of the group '" + Group.Name + "'";
                    return RedirectToAction("Index", new { resource = "", dismissed = "" });
                }
                var GroupMemberOwner = GroupMember.Owner;

                //Get all open calls
                var OpenCalls = _context.Call.Where(n => (n.ResourceGroupId == GroupId) && (n.Closed==false));

                //Get exceeded SLA calls
                int mins, exceededslacalls = 0;
                DateTime ExpiredDate;
                using (ApplicationDbContext dbcontext = new ApplicationDbContext())
                {
                    foreach (Call item in OpenCalls)
                    {
                        if (item.SlaLevel.Equals("Low"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).LowMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now) exceededslacalls++;
                        }
                        else if (item.SlaLevel.Equals("Medium"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).MedMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now) exceededslacalls++;
                        }
                        else if (item.SlaLevel.Equals("High"))
                        {
                            //Get the Mins
                            mins = dbcontext.SLAPolicy.SingleOrDefault(n => (n.Id == item.SlaId)).HighMins;

                            //Add to SLA Date
                            ExpiredDate = item.SLAResetTime.Value.AddMinutes(mins);

                            //Check if Date is past now
                            if (ExpiredDate < DateTime.Now) exceededslacalls++;
                        }
                    }
                }

                //Get required by dates of calls
                var requireddatescount = _context.Call.Where(n => (n.ResourceUserId.Equals(LoggedInUserID)) && (n.Closed==false) && (n.Required_By < DateTime.Now)).Count();

                //Get dismissed alerts
                IQueryable<Alert> Alerts;
                bool isDismissed;
                if (!String.IsNullOrEmpty(dismissed) && dismissed.Equals("true"))
                {
                    Alerts = _context.Alert.Where(n => (n.ToGroupId == GroupId) && (n.DismissedWhen != null));
                    isDismissed = true;
                }
                //Get Non dismissed alerts
                else
                {
                    Alerts = _context.Alert.Where(n => (n.ToGroupId == GroupId) && (n.DismissedWhen == null));
                    isDismissed = false;
                }

                //Convert Alerts into View Model Alerts
                List<ViewAlertsTemplateViewModel> AlertsVM = new List<ViewAlertsTemplateViewModel>();
                using (ApplicationDbContext dbcontext = new ApplicationDbContext())
                {
                    ApplicationUser FromUser, DismissedByUser;
                    Group ToGroup;
                    foreach (var item in Alerts)
                    {
                        FromUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.FromUserId));
                        ToGroup = dbcontext.Group.SingleOrDefault(n => n.Id == item.ToGroupId);
                        DismissedByUser = dbcontext.Users.SingleOrDefault(n => n.Id.Equals(item.DismissedByUserId));

                        AlertsVM.Add(new ViewAlertsTemplateViewModel()
                        {
                            Id = item.Id,
                            FromUserId = item.FromUserId,
                            FromUserName = FromUser?.UserName,
                            ToUserId = null,
                            ToUserName = null,
                            ToGroupId = item.ToGroupId,
                            ToGroupName = ToGroup?.Name,
                            DismissedByUserId = item.DismissedByUserId,
                            DismissedByUserName = DismissedByUser?.UserName,
                            DismissedWhen = item.DismissedWhen,
                            Dismissed = item.DismissedWhen == null ? false : true,
                            Created = item.Created,
                            Text = item.Text,
                            AssociatedCallRef = item.AssociatedCallRef,
                            AssociatedKnowledgeId = item.AssociatedKnowledgeId
                        });
                    }
                }

                //Create the view model
                var model = new ViewAlertsPageViewModel()
                {
                    GroupId = null,
                    ExceededSLACalls = exceededslacalls,
                    PastRequiredDate = requireddatescount,
                    Alerts = AlertsVM.AsEnumerable(),
                    IsLoggedInUserGroupOwner = GroupMemberOwner,
                    IsDismissed = isDismissed,
                    IsGroup = true
                };

                //Pass to view
                return View(model);
            }
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
    }
}