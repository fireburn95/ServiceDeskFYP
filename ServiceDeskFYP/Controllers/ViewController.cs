using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ServiceDeskFYP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskFYP.Controllers
{
    public class ViewController : Controller
    {
        //Variables
        private ApplicationDbContext _context;
        private UserStore<ApplicationUser> userStore;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        //Constructor
        public ViewController()
        {
            _context = new ApplicationDbContext();
            userStore = new UserStore<ApplicationUser>(_context);
            userManager = new UserManager<ApplicationUser>(userStore);
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
        }

        // GET: View
        [HttpGet]
        [Route("view/call/{Reference}")]
        public ActionResult ViewCall(string Reference)
        {
            //Handle error and success messages
            HandleMessages();

            //Check Reference exists
            var CallExists = _context.Call.Where(n => n.Reference.Equals(Reference)).Any();
            if (!CallExists)
            {
                TempData["ErrorMessage"] = "Sorry, the call you attempted to access doesn't exist";
                return RedirectToAction("Index", "Home");
            }

            //Get the Call, and username and groupname for ease
            Call Call = _context.Call.SingleOrDefault(n => n.Reference.Equals(Reference));
            var resourceuser = _context.Users.SingleOrDefault(n => n.Id.Equals(Call.ResourceUserId));
            var resourcegroup = _context.Group.SingleOrDefault(n => n.Id == Call.ResourceGroupId);
            var foruser = _context.Users.SingleOrDefault(n => n.Id.Equals(Call.ForUserId));

            //Check if hidden
            if (Call.Hidden)
            {
                TempData["ErrorMessage"] = "Sorry, you can not access this call";
                return RedirectToAction("Index", "Home");
            }

            //Make Call Details model
            CallDetailsViewViewModel CallDetails = new CallDetailsViewViewModel
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
            List<ActionDetailsViewViewModel> ActionList = new List<ActionDetailsViewViewModel>();
            foreach (var item in Actions)
            {
                ActionList.Add(new ActionDetailsViewViewModel
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
            ViewCallPageViewViewModel model = new ViewCallPageViewViewModel()
            {
                ActionsList = ActionList.OrderByDescending(n => n.Created).AsEnumerable(),
                CallDetails = CallDetails
            };

            return View("ViewCallAndActions", model);
        }

        //Called by Index form to get the call
        [HttpPost]
        public ActionResult GetCallPOST(string reference)
        {
            //Go to call
            return RedirectToAction("ViewCall", new { Reference = reference });
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