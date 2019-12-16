using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace AdminService.Controllers
{
    public class SchoolController : Controller
    {
        //
        // GET: /school/
        public string Index()
        {
            return "Default view";
        }
        public string detail(string name){
            return HtmlEncoder.Default.Encode($"Infomation regarding to school {name}:");
        }
    }
}