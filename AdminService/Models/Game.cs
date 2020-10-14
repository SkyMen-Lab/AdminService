using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminService.Models
{
    public class Game
    {
        public int Id { get; set; }
        [MaxLength(10, ErrorMessage = "Code length must be below 10 chars")]
        [MinLength(4, ErrorMessage = "Code length must be above 4 chars")]
        public string Code { get; set; }
        public GameState State { get; set; } = GameState.Created;
        public List<TeamGameSummary> TeamGameSummaries { get; set; }
        public String WinnerCode { get; set; } = "None";
        public DateTime Date { get; set; }
        [Range(5, 1440, ErrorMessage = "Duration must be within 5 - 1440 minutes")]
        public int DurationMinutes { get; set; }
        //TODO: Replace by User object
        public string CreatedBy { get; set; }
    }

    public enum GameState
    {
        Created,
        Going,
        Finished,
    }

    public class GameCreation
    {
        public string FirstTeamCode { get; set; }
        public string SecondTeamCode { get; set; }
        public string Date { get; set; }
        public int DurationMinutes { get; set; }
    }
    public class OnCreateObject
    {
        public List<Team> Team {get; set;}
        public GameCreation Game {get; set;}
    }
    public class GameRequest
    {
        public string Code { get; set; }
        public GameRequest(string Code) {this.Code = Code;}
    }
}