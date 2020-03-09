using System;
using System.Net;
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

        public async Task<List<Team>> getTeamObject()
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
                    Log.Error("Connection to StorageService Failed on team info request. SocketException: Connection refused;");
                }
            }
            return data;
        }

        [Route("/game/create")]
        [HttpGet]
        public IActionResult Create(string ErrMsg)
        {
            Log.Information("Creation Page requested. Team index requested.");
            ViewData["TeamObject"]= getTeamObject().Result;
            return View(onCreate);
        }

        [BindProperty]
        public GameCreation onCreate {get; set;}

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            //Truncate Code to 5 character
            onCreate.FirstTeamCode = onCreate.FirstTeamCode.Substring(0, 5);
            onCreate.SecondTeamCode = onCreate.SecondTeamCode.Substring(0, 5);
            if (onCreate.FirstTeamCode == onCreate.SecondTeamCode)
            {
                this.ModelState.AddModelError("FirstTeamCode","The team can not be identical");
                this.ModelState.AddModelError("SecondTeamCode", "The team can not be identical");
                ViewData["TeamObject"] = getTeamObject().Result;
                return View("Create");
            }
            var timeFromNow = Convert.ToDateTime(onCreate.Date) - DateTime.Now;
            if (timeFromNow.TotalMinutes<1)
            {
                this.ModelState.AddModelError("Date","Past event is not permitted.");
                ViewData["TeamObject"] = getTeamObject().Result;
                return View("Create");
            }
            if (!ModelState.IsValid)
            {
                //fetch team data for retry  
                
                return View("Create");
            }
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/create";
            using (HttpClient client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(onCreate);
                var res = new HttpResponseMessage();
                try 
                { 
                    res = await client.PostAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json")); 
                    Log.Information("Creation Request started to StorageService: Game Id = " + onCreate.FirstTeamCode); 
                }
                catch
                {
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService"; ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;";
                    Log.Error("Connection to StorageService Failed on team creation request. SocketException: Connection refused;");
                    return View("Create");
                }
                string resContent = await res.Content.ReadAsStringAsync();
                //Success
                if (res.StatusCode==HttpStatusCode.Created) 
                { 
                    ViewData["SuccessRedirectCode"] = JsonConvert.DeserializeObject<Game>(resContent).Code; 
                    Log.Information("Game has been created with code "); 
                    return View("CreateSuccess"); 
                }
                //Error
                ViewData["UpstreamResponse"] = "BadRequest 400";
                ViewData["UpstreamRawResponse"] = resContent;
                ViewData["TeamObject"] = getTeamObject().Result;
                return View();
            }
        }

        [Route("/game/Detail/{code}")]
        [HttpGet]
        public async Task<IActionResult> Detail(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/" + Code;
            Game GameDetail;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Started detail request of Game " + Code);
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    jsonContent = await res.Content.ReadAsStringAsync();
                    GameDetail = (JsonConvert.DeserializeObject<Game>(jsonContent));
                    if (res.StatusCode==HttpStatusCode.NotFound) 
                    { 
                        ViewData["ErrCode"] = "404"; 
                        Log.Warning("Detail request of Game " + Code + " was not found."); 
                    }
                }
                catch
                {
                    ViewData["ErrCode"] = "-1";
                    Log.Error("Connection to StorageService Failed while processing detail request on Game " + Code + ". SocketException: Connection refused;");
                    return View();
                }
            }
            return View(GameDetail);
        }

        [Route("/game/edit/{Code}")]
        [HttpGet]
        public async Task<IActionResult> Edit(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/" + Code;
            Game GameEdit;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Edit request started on Game " + Code);
                    using HttpResponseMessage res = await client.GetAsync(baseUrl);
                    jsonContent = await res.Content.ReadAsStringAsync();
                    GameEdit = (JsonConvert.DeserializeObject<Game>(jsonContent));
                }
                catch { ViewData["ErrCode"] = "-1"; return View(); }
            }
            if (jsonContent.Contains(":404"))
            {
                ViewData["ErrCode"] = "404";
                Log.Error("Game " + Code + " was not found on StorageService while processing Game edit request.");
            }
            return View(GameEdit);
        }

        [BindProperty]
        public Game GameEdited { get; set; }

        [HttpPost]
        public async Task<IActionResult> Edit()
        {
            if (!ModelState.IsValid)
            {
                ViewData["UpstreamResponse"] = "Invaild Model";
                Log.Warning("Received invalid game info on edit request");
                return View(GameEdited);
            }
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/update/" + GameEdited.Code;
            string jsonContent;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Log.Information("Started edit request to StorageService of game " + GameEdited.Code);
                    var json = JsonConvert.SerializeObject(GameEdited);
                    HttpResponseMessage res = await client.PutAsync(baseUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                    jsonContent = await res.Content.ReadAsStringAsync();
                }
                catch
                {
                    ViewData["ErrCode"] = "-1";
                    Log.Error("Connection to StorageService Failed while processing edit request on Game " + GameEdited.Code + ". SocketException: Connection refused;");
                    return View();
                }
            }
            if (String.IsNullOrEmpty(jsonContent))
            {
                ViewData["SuccessRedirectCode"] = GameEdited.Code;
                Log.Information("The game " + GameEdited.Code + " has been edited.");
                return View("EditSuccess");
            }
            return View(GameEdited);
        }

        [Route("/game/StartGame/{Code}")]
        [HttpGet]
        public async Task<IActionResult> StartGame(string Code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/start/"+Code;
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
                ViewData["UpstreamRawResponse"] = resContent;
                Log.Information("StartGameRequestResponse:{0}",resContent);
                return View();
            }
        }

        [Route("/game/delete/{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            string baseUrl = _configuration["ServerAddress:StorageServerAddress"] + "/api/game/delete/" + code;
            using (HttpClient client = new HttpClient())
            {
                var res = new HttpResponseMessage();
                try
                {
                    Log.Warning("A game delete request has been strated. Target Code: " + code);
                    res = await client.DeleteAsync(baseUrl);
                    if (res.StatusCode==HttpStatusCode.OK) 
                    { 
                        ViewData["UpstreamResponse"] = "Success. The Game has been deleted."; 
                        Log.Information("The Game " + code + " has been deleted."); 
                    }
                    if (res.StatusCode==HttpStatusCode.NotFound) 
                    { 
                        ViewData["UpstreamResponse"] = "Error: The code is not vaild."; 
                        Log.Error("Invaild delete request on Game " + code); 
                    }
                }
                catch
                {
                    ViewData["UpstreamResponse"] = "Failed to connect to StorageService";
                    ViewData["UpstreamRawResponse"] = "An unhandled exception occurred while processing the request. SocketException: Connection refused;";
                    Log.Error("Connection to StorageService Failed while processing delete request on Game " + code + ". SocketException: Connection refused;");
                    return View();
                }
                string resContent = await res.Content.ReadAsStringAsync();
                ViewData["UpstreamRawResponse"] = resContent;
                return View();
            }
        }
    }
}