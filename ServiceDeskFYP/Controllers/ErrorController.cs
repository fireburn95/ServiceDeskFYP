using ServiceDeskFYP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskFYP.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult NotFound(string aspxerrorpath = null)
        {
            Helpers.LogEvent("Error", "The user navigated to a page which cannot be found (" + aspxerrorpath + ")");
            return View();
        }

        public ActionResult Error400(string aspxerrorpath = null)
        {
            Helpers.LogEvent("Error", "A Bad Request error has occured with the user");
            return View();
        }
    }
}