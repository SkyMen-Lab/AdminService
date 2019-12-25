using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using AdminService.Models;
using GameStorage.Domain.Models;
using System.Net.Http;
namespace AdminService.Controllers
{
    public class TeamController : Controller
    {
        [Route("/team")]
        public async Task<IActionResult> Index()
        {
            string baseUrl = "http://localhost:5000/api/team";
            List<Team> data = new List<Team>();
            using (HttpClient client = new HttpClient())
            {
                try{
                using (HttpResponseMessage res = await client.GetAsync(baseUrl))
                {
                    string jsonContent = await res.Content.ReadAsStringAsync();
                    data = (JsonConvert.DeserializeObject<List<Team>>(jsonContent));
                }
                } catch {ViewData["TeamCountforDisplay"] = -1; return View(); }
            }
            ViewData["TeamCountforDisplay"] = data.Count();
            return View(data);
        }
        [Route("/team/create")]
        public async Task<IActionResult> Create(Team team){
            //only for testing
            team = new Team();
            team.Name = "Demo Post School";
            Config config = new Config();
            config.RouterIpAddress = "0.0.0.0";
            config.RouterPort = 8080;
            config.ConnectionType = 0;
            team.Config = config;
            //
            string baseUrl = "http://localhost:5000/api/team/create";
            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(team);
                var res = new HttpResponseMessage();
                try{res = await client.PostAsync(baseUrl,new StringContent(json, Encoding.UTF8, "application/json"));} 
                catch { ViewData["UpstreamResponse"] = "Failed to connect to GameStorageService"; ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;"; return View();}
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains(":409")) ViewData["UpstreamResponse"] = "409 Error: An identical team already exists";
                if (resContent.Contains(":201")) ViewData["UpstreamResponse"] = "Success";
                if (resContent.Contains(":415")) ViewData["UpstreamResponse"] = "415 Error: Unsupported Format";
                ViewData["UpstreamRawResponse"] = resContent;
                return View();
            }
        }
    }    
}