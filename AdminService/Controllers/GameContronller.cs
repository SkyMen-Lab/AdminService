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
        IConfiguration _configuration;
        public GameController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("/game")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/list/1";
            List<Game> data = new List<Game>();
            List<Game> ongoingGames = new List<Game>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    string jsonContent = await res.Content.ReadAsStringAsync();
                    data = (JsonConvert.DeserializeObject<List<Game>>(jsonContent));
                    ongoingGames = (from game in data where (int)game.State==1 select game).ToList();
                }
                catch { ViewData["ErrCode"] = "-1"; return View(); }
            }
            return View(ongoingGames);
        }
    }
}