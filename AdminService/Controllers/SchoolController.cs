using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using AdminService.Models;
namespace AdminService.Controllers
{
    public class SchoolController : Controller
    {
        //
        // GET: /school/
        public IActionResult Index(string name, int repTimes)
        {
            ViewData["SchName"] = name;
            ViewData["repTimes"] = repTimes;
            return View();
        }
        public IActionResult showDemoInfo()
        {
            var bedford = new SchoolModel()
            {
                Name = "Bedford School",
                Description = "Host School",
                Website = "https://www.bedfordschool.org.uk",
                State = "Bedford",
                PostalCode = "MK40 2TT"
            };
            return View(bedford);
        }
    }
}