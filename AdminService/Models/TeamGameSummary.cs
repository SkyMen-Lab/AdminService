using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStorage.Domain.Models
{
    public class TeamGameSummary
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        [Required(ErrorMessage = "Team object must be specifies")]
        public Team Team { get; set; }
        public bool IsWinner { get; set; }
        [Range(0, Int32.MaxValue, ErrorMessage = "Score must be non-negative and below int32.maxvalue")]
        public int Score { get; set; }
        public int NumberOfPlayers { get; set; }
        public int GameId { get; set; }
        [Required(ErrorMessage = "GameRepository object must be specifies")]
        public Game Game { get; set; }
    }
}