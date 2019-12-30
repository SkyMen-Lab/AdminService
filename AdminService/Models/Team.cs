using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace AdminService.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Code {get; set;}
        [Required, MinLength(3, ErrorMessage = "Team name must at least 3 characters")]
        public string Name {get;set;}
        public int Rank { get; set; }
        public double WinningRate { get; set; }
        public int ConfigId { get; set; }
        [Required]
        public Config Config {get;set;}
        public List<Game> GamesWon { get; set; }
        public List<TeamGameSummary> TeamGameSummaries { get; set; }
    }
    public class Config
    {
        public int Id { get; set; }
        [Required]
        public string RouterIpAddress { get; set; }
        [Required, Range(1025, 65535, ErrorMessage = "Port is out of the range (1025, 65535")]
        public int RouterPort { get; set; }
        [DefaultValue(0), Range(0, 1, ErrorMessage = "Connection type must either 0 (UDP) or 1 (TCP)")]
        public int ConnectionType { get; set; }
        //public Team Team { get; set; }
    }
}