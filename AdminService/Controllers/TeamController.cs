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
                }
                catch { ViewData["ErrCode"] = "-1"; return View(); }
            }
            return View(data);
        }

        [Route("/team/create")]
        [HttpGet]
        public IActionResult Create(string ErrMsg)
        {
            ViewData["ErrMsg"] = ErrMsg;
            return View();
        }
        [BindProperty]
        public Team team {get;set;}
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
                try { res = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json")); }
                catch 
                { 
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService"; ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;"; 
                    return View("Create"); 
                }
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains(":409,")) ViewData["UpstreamResponse"] = "409 Error: An identical team already exists";
                //Request was successful if Response Conatins Newly Created Team as raw json 
                if (resContent.Contains("routerIpAddress")) ViewData["UpstreamResponse"] = "Success. The team has been created.";
                if (resContent.Contains(":415,")) ViewData["UpstreamResponse"] = "415 Error: Unsupported Format";
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
                }
                catch
                {
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService";
                    ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;";
                    return View();
                }
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains("winningRate")) ViewData["UpstreamResponse"] = "Success. The team has been deleted.";
                if (resContent.Contains(":404,")) ViewData["UpstreamResponse"] = "Error: The code is not vaild.";
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
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    jsonContent = await res.Content.ReadAsStringAsync();
                    TeamDetail = (JsonConvert.DeserializeObject<Team>(jsonContent));
                }
                catch { ViewData["ErrCode"] = "-1"; return View(); }
            }
            if (jsonContent.Contains(":404")) ViewData["ErrCode"]="404";
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
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    jsonContent = await res.Content.ReadAsStringAsync();
                    TeamEdit = (JsonConvert.DeserializeObject<Team>(jsonContent));
                }
                catch { ViewData["ErrCode"] = "-1"; return View(); }
            }
            if (jsonContent.Contains(":404")) ViewData["ErrCode"] = "404";
            return View(TeamEdit);
        }
        [BindProperty]
        public Team TeamEdited {get;set;}
        [HttpPost]
        public async Task<IActionResult> Edit()
        {
            if (!ModelState.IsValid){
                ViewData["UpstreamResponse"] = "Invaild Model";
                return View(TeamEdited);
            }
                string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/update/" + TeamEdited.Code;
                string jsonContent;
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(TeamEdited);
                        HttpResponseMessage res = await client.PutAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                        jsonContent = await res.Content.ReadAsStringAsync();
                    }
                    catch { ViewData["ErrCode"] = "-1"; return View(); }
                }
                if (String.IsNullOrEmpty(jsonContent)) ViewData["UpstreamResponse"] = "Success, the team has been updated.";
                return View(TeamEdited);
        }
    }
}