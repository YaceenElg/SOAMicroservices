namespace JeuService.Models
{
    public class GameState
    {
        public int GameId { get; set; }
        public string GameType { get; set; } = string.Empty; 
        public string Status { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Level { get; set; }
        public Dictionary<string, object> GameData { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}
