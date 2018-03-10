using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskFYP.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            HandleMessages();

            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Desk", null);
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
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