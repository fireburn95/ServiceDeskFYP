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

        // GET: Desk
        public ActionResult Index()
        {
            return View();
        }

        /*****************
         * Create Call
         * ***************/

        //GET Create call
        [HttpGet]
        [Route("desk/create")]
        public ActionResult CreateCallGET()
        {
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

                //TODO add an initial action

                //Add to DB
                ApplicationDbContext _context3 = new ApplicationDbContext();
                _context3.Call.Add(NewCall);
                _context3.SaveChanges();

                //Return to Own Calls page or the actual Call TODO
                return RedirectToAction("index");

                
            }
            //Failed validation
            return View("CreateCall", model);
        }

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
        }
    }
}