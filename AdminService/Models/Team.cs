using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameStorage.Domain.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Code { get; set; }
        [MinLength(2, ErrorMessage = "Team name must at least 3 characters")]
        public string Name { get; set; }
        public int Rank { get; set; }
        public double WinningRate { get; set; }
        public int ConfigId { get; set; }
        [Required(ErrorMessage = "Config properties for connection must be specified")]
        public Config Config { get; set; }
        public List<Game> GamesWon { get; set; }
        public List<TeamGameSummary> TeamGameSummaries { get; set; }
    }
}