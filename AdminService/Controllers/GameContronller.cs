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
                    //ongoingGames = (from game in data where (int)game.State==1 select game).ToList();
                }
                catch { ViewData["ErrCode"] = "-1"; return View(); }
            }
            return View(data);
        }

        [Route("/game/create")]
        [HttpGet]
        public IActionResult Create(string ErrMsg)
        {
            ViewData["ErrMsg"] = ErrMsg;
            Log.Information("Creation Page requested.");
            return View();
        }

        [BindProperty]
        public GameCreation GameOnCreate { get; set; }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/create";
            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(GameOnCreate);
                var res = new HttpResponseMessage();
                try { res = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json")); Log.Information("Creation Request started to StorageService: Game Id = " + GameOnCreate.FirstTeamCode); }
                catch
                {
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService"; ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;";
                    Log.Error("Connection to StorageService Failed on team creation request. SocketException: Connection refused;");
                    return View("Create");
                }
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains(":409,")) { ViewData["UpstreamResponse"] = "409 Error: An identical team already exists"; Log.Warning("StorageService reported 409 Error: Identical Team"); }
                //Request was successful if Response Conatins Newly Created Team as raw json 
                if (resContent.Contains("routerIpAddress")) { ViewData["SuccessRedirectCode"] = JsonConvert.DeserializeObject<Game>(resContent).Code; Log.Information("Game has been created with code "); return View("CreateSuccess"); }
                if (resContent.Contains(":415,")) { ViewData["UpstreamResponse"] = "415 Error: Unsupported Format"; Log.Warning("StorageService reported 415 Error: Unsupported Format"); }
                ViewData["UpstreamRawResponse"] = resContent;
                return View("Create");
            }
        }

        [Route("/game/StartGame/{Code}")]
        [HttpGet]
        public async Task<IActionResult> StartGame(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/start";
            using (HttpClient client = new HttpClient())
            {
                var res = new HttpResponseMessage();
                try
                {

                    var json = JsonConvert.SerializeObject(new GameStartRequest(Code));
                    res = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                    Log.Warning("A startGame request has been strated. Target Code: " + Code);
                }
                catch
                {
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService";
                    ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;";
                    Log.Error("Connection to StorageService Failed while processing Start request on Game " + Code + ". SocketException: Connection refused;");
                    return View();
                }
                string resContent = await res.Content.ReadAsStringAsync();
                if (resContent.Contains("winningRate")) { ViewData["UpstreamResponse"] = "Success. The team has been deleted."; Log.Information("The team " + Code + " has been deleted."); }
                if (resContent.Contains(":404,")) { ViewData["UpstreamResponse"] = "Error: The code is not vaild."; Log.Error("Invaild delete request on Team " + Code); }
                ViewData["UpstreamRawResponse"] = resContent;
                return View();
            }
        }
    }
}