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
using GameStorage.Domain.Models;

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
                catch { ViewData["TeamCountforDisplay"] = -1; return View(); }
            }
            ViewData["TeamCountforDisplay"] = data.Count();
            return View(data);
        }
        [Route("/team/create")]
        public ActionResult Create(){
            ViewData["Submitted?"]="false";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateTeam()
        {
            //only for testing
            Team team = new Team();
            team.Name = HttpContext.Request.Form["Name"].ToString();
            Config config = new Config();
            config.RouterIpAddress = HttpContext.Request.Form["IP"].ToString();
            config.RouterPort = Convert.ToInt32(HttpContext.Request.Form["Port"].ToString());
            //config.ConnectionType = 0;
            team.Config = config;
            //
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
                //Request was successful Response Conatins Newly Created Team as raw json 
                if (resContent.Contains("routerIpAddress")) ViewData["UpstreamResponse"] = "Success. The team has been created.";
                if (resContent.Contains(":415,")) ViewData["UpstreamResponse"] = "415 Error: Unsupported Format";
                ViewData["UpstreamRawResponse"] = resContent;
                return View("Create");
            }
        }
        [Route("/team/delete")]
        public ActionResult Delete()
        {
            ViewData["Submitted?"] = "false";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTeam()
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/team/delete/" + HttpContext.Request.Form["ID"].ToString();
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
                    return View("Delete");
                }
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains("winningRate")) ViewData["UpstreamResponse"] = "Success. The team has been deleted.";
                if (resContent.Contains(":404,")) ViewData["UpstreamResponse"] = "Error: The code is not vaild.";
                ViewData["UpstreamRawResponse"] = resContent;
                return View("Delete");
            }
        }
    }
}