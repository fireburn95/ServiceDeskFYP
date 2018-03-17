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
            return RedirectToAction("EmployeeDashboard");
        }

        [Authorize(Roles = "Employee")]
        [Route("dashboard")]
        public ActionResult EmployeeDashboard()
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
            var L_Calls = _context.Call.Where(n => n.LockedToUserId!=null && n.LockedToUserId.Equals(LoggedInUserId));

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
                .Where(n => n.ToUserId.Equals(LoggedInUserId) && n.DismissedWhen == null)
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
            var LoggedInUsersCalls = _context.Call.Where(n => n.Closed==false && n.ResourceUserId!=null && n.ResourceUserId.Equals(LoggedInUserId));

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
            foreach(var call in UrgentCalls)
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

            //Pass to view
            return View("EmployeeDashboard", model);
        }
    }
}
