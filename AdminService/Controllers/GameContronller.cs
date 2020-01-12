using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AdminService.Models;


namespace AdminService.Controllers
{
    public class GameController: Controller
    {
        [Route("/game")]
        public async Task<IActionResult> Index(){
            return View();
        }
    }
}