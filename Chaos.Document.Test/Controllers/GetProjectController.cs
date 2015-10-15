using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Chaos.Document.Test.Controllers
{
    public class GetProjectController : Controller
    {
        // GET: GetProject
        public ActionResult Index(string projectId)
        {
            ViewBag.projectId = projectId ?? Guid.NewGuid().ToString();
            return View();
        }
    }
}