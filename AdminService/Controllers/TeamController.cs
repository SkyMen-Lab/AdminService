using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
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
                using (HttpResponseMessage res = await client.GetAsync(baseUrl))
                {
                    string jsonContent = await res.Content.ReadAsStringAsync();
                    data = (JsonConvert.DeserializeObject<List<Team>>(jsonContent));
                }
            }
            ViewData["TeamCountforDisplay"] = data.Count();
            return View(data);
        }
    }    
}