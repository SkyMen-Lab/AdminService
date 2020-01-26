using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace AdminService.Controllers
{
    public class RouterController : Controller
    {
        //
        // GET: /router/
        public string Index()
        {
            return HtmlEncoder.Default.Encode($"Under Construction.");
        }
    }
}