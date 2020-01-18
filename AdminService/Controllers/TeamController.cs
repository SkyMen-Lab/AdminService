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
using Serilog;
using Serilog.Sinks.File;

namespace AdminService.Controllers
{
    public class TeamController : Controller
    {
        IConfiguration _configuration;
        public TeamController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("/team")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team";
            List<Team> data = new List<Team>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    string jsonContent = await res.Content.ReadAsStringAsync();
                    data = (JsonConvert.DeserializeObject<List<Team>>(jsonContent));
                    Log.Information("Team index requested. Team Count: " + data.Count);
                }
                catch
                {
                    ViewData["ErrCode"] = "-1";
                    Log.Error("Connection to StorageService Failed on team info request. SocketException: Connection refused;");
                    return View();
                }
            }
            return View(data);
        }

        [Route("/team/create")]
        [HttpGet]
        public IActionResult Create(string ErrMsg)
        {
            ViewData["ErrMsg"] = ErrMsg;
            Log.Information("Creation Page requested.");
            return View();
        }

        [BindProperty]
        public Team team { get; set; }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/create";
            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(team);
                var res = new HttpResponseMessage();
                try { res = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json")); Log.Information("Creation Request started to StorageService: Team Name = " + team.Name); }
                catch
                {
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService"; ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;";
                    Log.Error("Connection to StorageService Failed on team creation request. SocketException: Connection refused;");
                    return View("Create");
                }
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains(":409,")) { ViewData["UpstreamResponse"] = "409 Error: An identical team already exists"; Log.Warning("StorageService reported 409 Error: Identical Team"); }
                //Request was successful if Response Conatins Newly Created Team as raw json 
                if (resContent.Contains("routerIpAddress")) { ViewData["SuccessRedirectCode"] = JsonConvert.DeserializeObject<Team>(resContent).Code; Log.Information("Team " + team.Name + " has been created with code " + team.Code); return View("CreateSuccess"); }
                if (resContent.Contains(":415,")) { ViewData["UpstreamResponse"] = "415 Error: Unsupported Format"; Log.Warning("StorageService reported 415 Error: Unsupported Format"); }
                ViewData["UpstreamRawResponse"] = resContent;
                return View("Create");
            }
        }

        [Route("/team/delete/{Code}")]
        public async Task<IActionResult> Delete(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/delete/" + Code;
            using (HttpClient client = new HttpClient())
            {
                var res = new HttpResponseMessage();
                try
                {
                    res = await client.DeleteAsync(baseUrl);
                    Log.Warning("A delete request has been strated. Target Code: " + Code);
                }
                catch
                {
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService";
                    ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;";
                    Log.Error("Connection to StorageService Failed while processing delete request on Team " + Code + ". SocketException: Connection refused;");
                    return View();
                }
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains("winningRate")) { ViewData["UpstreamResponse"] = "Success. The team has been deleted."; Log.Information("The team " + Code + " has been deleted."); }
                if (resContent.Contains(":404,")) { ViewData["UpstreamResponse"] = "Error: The code is not vaild."; Log.Error("Invaild delete request on Team " + Code); }
                ViewData["UpstreamRawResponse"] = resContent;
                return View();
            }
        }

        [Route("/team/detail/{Code}")]
        public async Task<IActionResult> Detail(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/code/" + Code;
            Team TeamDetail;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Started detail request of Team " + Code);
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    jsonContent = await res.Content.ReadAsStringAsync();
                    TeamDetail = (JsonConvert.DeserializeObject<Team>(jsonContent));
                }
                catch
                {
                    ViewData["ErrCode"] = "-1";
                    Log.Error("Connection to StorageService Failed while processing detail request on Team " + Code + ". SocketException: Connection refused;");
                    return View();
                }
            }
            if (jsonContent.Contains(":404")) { ViewData["ErrCode"] = "404"; Log.Warning("Detail request of Team " + Code + " was not found."); }
            return View(TeamDetail);
        }

        [Route("/team/edit/{Code}")]
        [HttpGet]
        public async Task<IActionResult> Edit(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/code/" + Code;
            Team TeamEdit;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Edit request started on Team " + Code);
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    jsonContent = await res.Content.ReadAsStringAsync();
                    TeamEdit = (JsonConvert.DeserializeObject<Team>(jsonContent));
                }
                catch { ViewData["ErrCode"] = "-1"; return View(); }
            }
            if (jsonContent.Contains(":404"))
            {
                ViewData["ErrCode"] = "404";
                Log.Error("Team " + Code + " was not found on StorageService while processing team edit request.");
            }
            return View(TeamEdit);
        }

        [BindProperty]
        public Team TeamEdited { get; set; }

        [HttpPost]
        public async Task<IActionResult> Edit()
        {
            if (!ModelState.IsValid)
            {
                ViewData["UpstreamResponse"] = "Invaild Model";
                Log.Warning("Received invalid team info on edit request");
                return View(TeamEdited);
            }
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/update/" + TeamEdited.Code;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Started edit request to StorageService of Team " + TeamEdited.Code);
                    var json = JsonConvert.SerializeObject(TeamEdited);
                    HttpResponseMessage res = await client.PutAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                    jsonContent = await res.Content.ReadAsStringAsync();
                }
                catch
                {
                    ViewData["ErrCode"] = "-1";
                    Log.Error("Connection to StorageService Failed while processing edit request on Team " + TeamEdited.Code + ". SocketException: Connection refused;");
                    return View();
                }
            }
            if (String.IsNullOrEmpty(jsonContent))
            {
                ViewData["SuccessRedirectCode"] = TeamEdited.Code;
                Log.Information("The team " + TeamEdited.Code + " has been edited.");
                return View("EditSuccess");
            }
            return View(TeamEdited);
        }

        [Route("/team/editconfig/{Code}")]
        [HttpGet]
        public async Task<IActionResult> EditConfig(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/code/" + Code;
            Team TeamEdit;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Started edit config request to StorageService of Team " + Code);
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    jsonContent = await res.Content.ReadAsStringAsync();
                    TeamEdit = (JsonConvert.DeserializeObject<Team>(jsonContent));
                }
                catch
                {
                    ViewData["ErrCode"] = "-1";
                    Log.Error("Connection to StorageService Failed while processing edit config request on Team " + Code + ". SocketException: Connection refused;");
                    return View();
                }
            }
            if (jsonContent.Contains(":404"))
            {
                ViewData["ErrCode"] = "404";
                Log.Error("Team " + Code + " was not found on StorageService while processing team edit config request.");
            }
            return View(TeamEdit);
        }

        [BindProperty]
        public Team TeamConfigEdited { get; set; }

        [HttpPost]
        public async Task<IActionResult> EditConfig()
        {
            if (!ModelState.IsValid)
            {
                return View(TeamConfigEdited);
            }
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/config/update/" + TeamEdited.Code;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Started edit request to StorageService of Team " + TeamConfigEdited.Code);
                    var json = JsonConvert.SerializeObject(TeamConfigEdited.Config);
                    HttpResponseMessage res = await client.PutAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                    jsonContent = await res.Content.ReadAsStringAsync();
                }
                catch
                {
                    ViewData["ErrCode"] = "-1";
                    Log.Error("Connection to StorageService Failed while processing edit config request on Team " + TeamConfigEdited.Code + ". SocketException: Connection refused;");
                    return View(TeamConfigEdited);
                }
            }
            if (String.IsNullOrEmpty(jsonContent))
            {
                ViewData["SuccessRedirectCode"] = TeamConfigEdited.Code;
                Log.Information("The team config " + TeamEdited.Code + " has been edited.");
                return View("EditSuccess");
            }
            if (jsonContent.Contains(":400"))
            {
                ViewData["UpstreamResponse"] = "BadRequest: Router and IP are already in use or invalid";
                Log.Error("Bad request on team config edit " + TeamConfigEdited.Code + ": Router and IP are already in use or invalid");
            }
            return View(TeamConfigEdited);
        }
    }
}