using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace AdminService.Controllers
{
    public class MatchController: Controller
    {
        //
        // GET: /match/
        public string Index(){
            return HtmlEncoder.Default.Encode($"Default Match View");
        }
    }
}